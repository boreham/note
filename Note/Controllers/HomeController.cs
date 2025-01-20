using Microsoft.AspNetCore.Mvc;

namespace Note.Controllers;
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
