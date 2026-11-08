using Microsoft.AspNetCore.Mvc;

namespace RaceDay.MVC.Controllers;

public class ParticipantController : Controller
{
    public IActionResult Index()
    {
        // Only Participants may access this dashboard.
        if (HttpContext.Session.GetString("Role") != "Participant")
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.ParticipantName =
            HttpContext.Session.GetString("UserName");

        return View();
    }
}