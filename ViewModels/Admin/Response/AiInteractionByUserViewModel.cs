using System;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class AiInteractionByUserViewModel
    {
        public int InteractionId { get; set; }
        public string UserMessage { get; set; } = null!;
        public string? Airesponse { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string ModelName { get; set; } = null!;
    }
}
