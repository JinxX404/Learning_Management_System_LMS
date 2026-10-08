using System.Collections.Generic;
using Learning_Management_System.Models;
namespace Learning_Management_System.ViewModels.Admin.Response
{
    public class UsersResponseViewModel
    {
        public List<User> Users { get; set; } = new();
        public string? SearchString { get; set; }
    }
}
