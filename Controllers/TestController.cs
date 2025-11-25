using Microsoft.AspNetCore.Mvc;

namespace Learning_Management_System.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Ping()
        {
            return Content("Pong");
        }
    }
}
