
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class ParticipantController : Controller
{
    private readonly RaceDayApiService _apiService;

    public ParticipantController(RaceDayApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        // Only Participants may access this dashboard.
        if (HttpContext.Session.GetString("Role") != "Participant")
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.ParticipantName =
            HttpContext.Session.GetString("UserName");

        ViewBag.UpcomingEvents = 0;
        ViewBag.TotalEnrolments = 0;
        ViewBag.TotalResults = 0;

        try
        {
            // Load all events from the API.
            var eventsResponse = await _apiService.SendAsync(
                HttpMethod.Get, "api/events");

            if (eventsResponse.IsSuccessStatusCode)
            {
                var events = await eventsResponse.Content
                    .ReadFromJsonAsync<List<EventViewModel>>();

                ViewBag.UpcomingEvents = events?
                    .Count(x => x.EventDate >= DateTime.Now) ?? 0;
            }

            // Load this participant's enrolments.
            var enrolmentsResponse = await _apiService.SendAsync(
                HttpMethod.Get, "api/enrolments/me");

            if (enrolmentsResponse.IsSuccessStatusCode)
            {
                var enrolments = await enrolmentsResponse.Content
                    .ReadFromJsonAsync<List<EnrolmentViewModel>>();

                ViewBag.TotalEnrolments = enrolments?.Count ?? 0;
            }

            // Load this participant's race results.
            var resultsResponse = await _apiService.SendAsync(
                HttpMethod.Get, "api/results/me");

            if (resultsResponse.IsSuccessStatusCode)
            {
                var results = await resultsResponse.Content
                    .ReadFromJsonAsync<List<ResultViewModel>>();

                ViewBag.TotalResults = results?.Count ?? 0;
            }
        }
        catch (HttpRequestException)
        {
            ViewBag.ErrorMessage =
                "Unable to connect to the RaceDay API. Please try again.";
        }

        return View();
    }
}
