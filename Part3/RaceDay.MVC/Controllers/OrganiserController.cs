using Microsoft.AspNetCore.Mvc;

namespace RaceDay.MVC.Controllers;

public class OrganiserController : Controller
{
    public IActionResult Dashboard()
    {
        // Only Organisers may access this dashboard.
        if (HttpContext.Session.GetString("Role") != "Organiser")
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.OrganiserName =
            HttpContext.Session.GetString("UserName");

        return View();
    }
}