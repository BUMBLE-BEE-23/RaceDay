
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class OrganiserController : Controller
{
    private readonly RaceDayApiService _apiService;

    public OrganiserController(RaceDayApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Dashboard()
    {
        // Only Organisers may access this dashboard.
        if (HttpContext.Session.GetString("Role") != "Organiser")
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.OrganiserName =
            HttpContext.Session.GetString("UserName");

        var events = new List<EventViewModel>();

        try
        {
            // Load only events belonging to the logged-in organiser.
            var response = await _apiService.SendAsync(
                HttpMethod.Get,
                "api/events/mine");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage =
                    "Unable to load your events from the API.";

                return View(events);
            }

            events = await response.Content
                .ReadFromJsonAsync<List<EventViewModel>>()
                ?? new List<EventViewModel>();

            // Load enrolment totals for each of the organiser's events.
            var enrolmentCounts = new Dictionary<int, int>();

            foreach (var raceEvent in events)
            {
                var enrolmentResponse = await _apiService.SendAsync(
                    HttpMethod.Get,
                    $"api/events/{raceEvent.Id}/enrolments");

                if (enrolmentResponse.IsSuccessStatusCode)
                {
                    var enrolments = await enrolmentResponse.Content
                        .ReadFromJsonAsync<List<EnrolmentViewModel>>();

                    enrolmentCounts[raceEvent.Id] =
                        enrolments?.Count ?? 0;
                }
                else
                {
                    enrolmentCounts[raceEvent.Id] = 0;
                }
            }

            ViewBag.EnrolmentCounts = enrolmentCounts;
            ViewBag.TotalEvents = events.Count;
            ViewBag.TotalEnrolments = enrolmentCounts.Values.Sum();
            ViewBag.UpcomingEvents = events.Count(x =>
                x.EventDate >= DateTime.Now);
        }
        catch (HttpRequestException)
        {
            ViewBag.ErrorMessage =
                "Unable to connect to the RaceDay API.";

            ViewBag.EnrolmentCounts = new Dictionary<int, int>();
            ViewBag.TotalEvents = 0;
            ViewBag.TotalEnrolments = 0;
            ViewBag.UpcomingEvents = 0;
        }

        return View(events);
    }
}
