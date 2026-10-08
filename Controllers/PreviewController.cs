using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Learning_Management_System.ViewModels.Preview.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Learning_Management_System.Controllers
{
    [Route("preview")]
    public class PreviewController : Controller
    {

    [HttpGet("view")]
        public IActionResult ViewPage([FromQuery] ViewPageRequestViewModel model)
        {
if (string.IsNullOrWhiteSpace(model.Name))
            return BadRequest("name query required, e.g. ?name=AddCourse");

        // return the exact cshtml file from Views/Admin
        var viewPath = $"~/Views/Admin/{model.Name}.cshtml";
            return View(viewPath);
                }

        private readonly ILogger<PreviewController> _logger;

        public PreviewController(ILogger<PreviewController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}