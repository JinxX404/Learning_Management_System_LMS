-- Add IsDeleted column to Quizzes table if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Quizzes' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    ALTER TABLE Quizzes ADD IsDeleted bit NOT NULL DEFAULT 0;
    PRINT 'IsDeleted column added successfully';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists';
END
