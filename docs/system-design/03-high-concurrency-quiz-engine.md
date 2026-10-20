# Track 03 — High-Concurrency Quiz Engine: Auto-Save Buffering & Atomic Terminal State Machine

**Rank:** 03  
**Real-World Analogy:** LeetCode contest final submission under deadline, Google Docs auto-save, Online Proctored Exam.  
**Core Question:** When 1,000 students take an exam with auto-save triggered every 5 seconds, how do you prevent 12,000 writes/minute from melting SQL Server, and guarantee that when a student clicks "Submit", their score is computed exactly once—even if they double-click or lose Wi-Fi mid-submit?

---

## Level 0: The Current Naive Baseline (What Exists Today)

### The Code in Academix
Look at [StudentController.cs:404-442](file:///p:/Projects/Learning%20Management%20System/Controllers/StudentController.cs#L404-L442) (`SaveQuizProgress`):
```csharp
public async Task<IActionResult> SaveQuizProgress(SaveQuizProgressRequestViewModel model)
{
    var userId = GetCurrentUserId();
    // 1. Fetch Attempt
    var attempt = await _context.QuizAttempts
        .FirstOrDefaultAsync(a => a.AttemptId == model.AttemptId && a.UserId == userId);

    if (attempt == null || attempt.SubmittedAt != null)
        return BadRequest("Invalid attempt or already submitted.");

    // 2. Fetch Response
    var response = await _context.QuizResponses
        .FirstOrDefaultAsync(r => r.AttemptId == model.AttemptId && r.QuestionId == model.QuestionId);

    if (response == null) {
        response = new QuizResponse { AttemptId = model.AttemptId, QuestionId = model.QuestionId };
        _context.QuizResponses.Add(response);
    }

    // 3. Update & Save
    response.SelectedOptionId = optionId;
    await _context.SaveChangesAsync();
    return Ok();
}
```

And look at [StudentController.cs:603-674](file:///p:/Projects/Learning%20Management%20System/Controllers/StudentController.cs#L603-L674) (`SubmitQuiz`):
```csharp
// Mark as submitted
attempt.SubmittedAt = DateTime.Now;
await _context.SaveChangesAsync(); // <-- First Commit!

// Stored Procedure called OUTSIDE any transaction!
await _context.Database.ExecuteSqlRawAsync(
    "EXEC sp_SubmitQuizAttempt @p0",
    attempt.AttemptId
);
return RedirectToAction("QuizResult", new { id = attempt.AttemptId });
```

### Why Level 0 Fails
1. **The "Flash Crowd" Database Thrashing:** Every individual question change issues two `SELECT` queries and one `INSERT`/`UPDATE` to SQL Server. If 500 students are typing answers, the database receives thousands of roundtrips per minute, saturating the connection pool.
2. **The "Orphaned Terminal State" Bug:** If `SaveChangesAsync()` commits `SubmittedAt = DateTime.Now`, but the database server experiences a transient deadlock or network timeout before `sp_SubmitQuizAttempt` executes, the student is permanently locked out (`SubmittedAt != null`), but their score was never computed and no `Grade` record exists!
3. **Submit vs. Auto-Save Race Condition:** A slow auto-save HTTP request delayed by 200ms in transit can arrive *after* `SubmitQuiz` finished, mutating answers on a closed exam!
4. **Client-Authoritative Timer:** There is no server-side deadline validation; a student can keep the browser tab open for hours past the deadline and still submit.

---

## Level 1: The First Immediate Fix (In-Process Debouncing & Explicit SQL Transactions)

### The Solution
1. **Wrap `SubmitQuiz` in an Explicit Transaction:**
```csharp
using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
try
{
    // Re-verify attempt is not submitted with UPDLOCK to prevent racing submits
    var attempt = await _context.QuizAttempts
        .FromSqlRaw("SELECT * FROM QuizAttempts WITH (UPDLOCK) WHERE AttemptId = {0}", model.AttemptId)
        .FirstOrDefaultAsync();

    if (attempt.SubmittedAt != null)
        return BadRequest("Quiz already submitted.");

    attempt.SubmittedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    // Call scoring stored procedure inside the SAME transaction
    await _context.Database.ExecuteSqlInterpolatedAsync($"EXEC sp_SubmitQuizAttempt {attempt.AttemptId}");

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

2. **Add Server-Authoritative Clock:**
Inject `TimeProvider` and check:
```csharp
var now = _timeProvider.GetUtcNow().UtcDateTime;
var deadline = attempt.StartedAt.AddMinutes(quiz.TimeLimitMinutes);
if (now > deadline.AddSeconds(15)) // 15-second grace period for network jitter
{
    return BadRequest("Exam deadline has expired.");
}
```

### The Emerging Bottleneck in Level 1
1. **Auto-Save Load Remains Synchronous:** Even if `SubmitQuiz` is transactional, the database is still being bombarded with thousands of synchronous auto-save requests.
2. **Lock Contention on Submit:** If an auto-save request is writing to `QuizResponses` while `SubmitQuiz` is deleting and scoring responses, SQL Server lock escalation occurs, leading to `SqlException: Transaction (Process ID ...) was deadlocked on lock resources with another process and has been chosen as the deadlock victim`.

---

## Level 2: The Architectural Refactor (Write-Behind Buffering via In-Memory Channels)

### The Concept
Instead of synchronously writing every auto-save keystroke to SQL Server, push the auto-save payload into an in-memory queue ([`System.Threading.Channels.Channel<T>`](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels)). A dedicated background worker consumes answers in batches and flushes them efficiently using table-valued parameters or bulk updates.

```mermaid
flowchart LR
    Browser[Student Browser] -->|POST /SaveProgress| Web[ASP.NET Web Controller]
    Web -->|O(1) Memory Enqueue| Channel[Channel&lt;QuizResponseBuffer&gt;]
    Web -->>|Immediate 200 OK| Browser
    
    subgraph Background Processing
        Channel -->|Batch of 100 or every 1 sec| Worker[QuizAutoSaveBackgroundWorker]
        Worker -->|Single Batch Bulk Insert/Update| SQL[(SQL Server)]
    end
```

### In-Memory Bounded Channel
```csharp
public record AutoSaveItem(int AttemptId, int QuestionId, int? SelectedOptionId, string? ResponseText, DateTime Timestamp);

public class QuizAutoSaveChannel
{
    private readonly Channel<AutoSaveItem> _channel = Channel.CreateBounded<AutoSaveItem>(new BoundedChannelOptions(10_000)
    {
        FullMode = BoundedChannelFullMode.Wait
    });

    public ValueTask EnqueueAsync(AutoSaveItem item, CancellationToken ct) => _channel.Writer.WriteAsync(item, ct);
    public ChannelReader<AutoSaveItem> Reader => _channel.Reader;
}
```

### Background Worker Batching
```csharp
public class QuizAutoSaveBackgroundWorker : BackgroundService
{
    private readonly QuizAutoSaveChannel _channel;
    private readonly IServiceScopeFactory _scopeFactory;

    public QuizAutoSaveBackgroundWorker(QuizAutoSaveChannel channel, IServiceScopeFactory scopeFactory)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<AutoSaveItem>();
        while (!stoppingToken.IsCancellationRequested)
        {
            // Read next available item or wait up to 1 second
            if (await _channel.Reader.WaitToReadAsync(stoppingToken))
            {
                while (batch.Count < 200 && _channel.Reader.TryRead(out var item))
                {
                    batch.Add(item);
                }

                if (batch.Any())
                {
                    await FlushBatchToDatabaseAsync(batch);
                    batch.Clear();
                }
            }
        }
    }
}
```

### The Emerging Bottleneck in Level 2
1. **Volatile In-Memory Loss:** If the web server crashes or restarts (e.g., during a deployment) while answers are sitting in the in-memory `Channel`, student answers that were acknowledged as `200 OK` are permanently lost!
2. **Dual-Node Race on Submit:** If Student A auto-saves on Node 1, and immediately clicks "Submit" on Node 2, Node 2's SQL transaction might execute before Node 1 flushes its in-memory buffer. The quiz gets graded without the final answers!

---

## Level 3: Production-Grade Architecture (Distributed Write-Behind with Redis & Idempotent Submission)

### The Architecture

```mermaid
sequenceDiagram
    autonumber
    actor Student
    participant Web as Web Server
    participant Redis as Redis Cache
    participant SQL as SQL Server Primary

    rect rgb(240, 248, 255)
        Note over Student,Redis: High-Frequency Auto-Save (Every 3s)
        Student->>Web: POST /quiz/autosave (AttemptId, QuestionId, OptionId)
        Web->>Redis: HSET quiz:answers:{attemptId} question:{qId} option:{optId}
        Web-->>Student: 200 OK (Instantaneous &lt; 5ms)
    end

    rect rgb(255, 240, 245)
        Note over Student,SQL: Final Submit (Atomic & Idempotent)
        Student->>Web: POST /quiz/submit (Idempotency-Key: X-UUID-123)
        Web->>SQL: BEGIN TRANSACTION (SERIALIZABLE)
        Web->>SQL: Check Attempt State == InProgress
        Web->>Redis: HGETALL quiz:answers:{attemptId} (Fetch all cached answers)
        Web->>SQL: Bulk UPSERT QuizResponses
        Web->>SQL: EXEC sp_SubmitQuizAttempt (Compute Grade)
        Web->>SQL: UPDATE QuizAttempts SET Status = 'Graded', SubmittedAt = UTC
        Web->>SQL: COMMIT TRANSACTION
        Web->>Redis: DEL quiz:answers:{attemptId}
        Web-->>Student: 200 OK (Result: Grade = 94%)
    end
```

### Key Production Enhancements
1. **Write-Ahead Cache in Redis:**
   - Every auto-save writes to a fast Redis Hash (`HSET quiz:attempt:105 question:12 option:3`).
   - Latency drops from 35ms (SQL Server disk I/O) to <2ms (in-memory Redis).
   - If the web server restarts, answers are safe in Redis.
2. **Explicit State Machine:**
   - [QuizAttempt.cs](file:///p:/Projects/Learning%20Management%20System/Models/QuizAttempt.cs) uses strict statuses:
     - `InProgress`: Auto-saves accepted.
     - `Submitting`: Locked; rejects further auto-saves.
     - `Graded`: Terminal state; score immutable.
3. **Submission Idempotency Key:**
   - If the student's browser retries the submit button due to a network glitch, the server checks the idempotency token and returns the already computed grade receipt without scoring twice.

---

## Hands-on Failure Injection & Verification Challenge

### Exercise: The Raced Submit-While-Saving Experiment
Write an integration test simulating a student who clicks "Submit" while two auto-save background requests are still in-flight.

```csharp
[Fact]
public async Task AutoSave_Arriving_After_Submit_Is_Strictly_Rejected()
{
    // Arrange
    int attemptId = await CreateOpenQuizAttempt();

    // Act: Submit the quiz
    var submitTask = PostSubmitQuiz(attemptId, idempotencyKey: Guid.NewGuid());
    
    // Simulate delayed in-flight auto-save arriving 50ms later
    await Task.Delay(50);
    var lateAutoSaveTask = PostAutoSave(attemptId, questionId: 1, selectedOptionId: 4);

    await Task.WhenAll(submitTask, lateAutoSaveTask);

    // Assert
    var submitResponse = await submitTask;
    var autoSaveResponse = await lateAutoSaveTask;

    Assert.True(submitResponse.IsSuccessStatusCode);
    Assert.Equal(HttpStatusCode.BadRequest, autoSaveResponse.StatusCode); // Must reject late writes!
}
```

### Verification Questions to Answer
1. Why does an in-memory `Channel<T>` buffer introduce data-loss risk during ungraceful server shutdowns?
2. How does using `UPDLOCK` inside `SubmitQuiz` prevent deadlocks with concurrent background writes?
3. What is the difference between client-measured time and server-measured time when evaluating quiz deadlines?
