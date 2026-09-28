using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers;

// Ren visningsside - ingen innlogging skjer her, kun HTML/CSS.
public class AccountController : Controller
{
    public IActionResult Login()
    {
        return View();
    }
}
