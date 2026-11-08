
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class EnrolmentsController : Controller
{
    private readonly RaceDayApiService _apiService;

    public EnrolmentsController(RaceDayApiService apiService)
    {
        _apiService = apiService;
    }

    private bool IsOrganiser() =>
        HttpContext.Session.GetString("Role") == "Organiser";

    private bool IsParticipant() =>
        HttpContext.Session.GetString("Role") == "Participant";

    // ORGANISER: View events and select one to see its enrolments.
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var response = await _apiService.SendAsync(
            HttpMethod.Get, "api/events");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.ErrorMessage = "Unable to load events.";
            return View(new List<EventViewModel>());
        }

        var events = await response.Content
            .ReadFromJsonAsync<List<EventViewModel>>()
            ?? new List<EventViewModel>();

        return View(events);
    }

    // ORGANISER: View participants enrolled in a particular event.
    [HttpGet]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var response = await _apiService.SendAsync(
            HttpMethod.Get, $"api/events/{eventId}/enrolments");

        if (!response.IsSuccessStatusCode)
        {
            var apiError = await response.Content.ReadAsStringAsync();

            ViewBag.ErrorMessage =
                $"Unable to load enrolments. API Error ({(int)response.StatusCode}): {apiError}";

            return View(new List<EnrolmentViewModel>());
        }

        var enrolments = await response.Content
            .ReadFromJsonAsync<List<EnrolmentViewModel>>()
            ?? new List<EnrolmentViewModel>();

        ViewBag.EventId = eventId;

        return View(enrolments);
    }

    // PARTICIPANT: Browse categories before enrolling in an event.
    [HttpGet]
    public async Task<IActionResult> Browse(int eventId)
    {
        if (!IsParticipant())
            return RedirectToAction("Login", "Account");

        if (eventId <= 0)
            return BadRequest();

        var eventResponse = await _apiService.SendAsync(
            HttpMethod.Get, $"api/events/{eventId}");

        if (!eventResponse.IsSuccessStatusCode)
            return NotFound();

        var raceEvent = await eventResponse.Content
            .ReadFromJsonAsync<EventViewModel>();

        if (raceEvent == null)
            return NotFound();

        var categoryResponse = await _apiService.SendAsync(
            HttpMethod.Get, $"api/events/{eventId}/categories");

        if (!categoryResponse.IsSuccessStatusCode)
        {
            ViewBag.ErrorMessage = "Unable to load event categories.";
            return View(new List<CategoryViewModel>());
        }

        var categories = await categoryResponse.Content
            .ReadFromJsonAsync<List<CategoryViewModel>>()
            ?? new List<CategoryViewModel>();

        ViewBag.Event = raceEvent;

        return View(categories);
    }

    // PARTICIPANT: Submit an enrolment.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        if (!IsParticipant())
            return RedirectToAction("Login", "Account");

        var request = new
        {
            EventId = eventId,
            CategoryId = categoryId
        };

        var response = await _apiService.SendAsync(
            HttpMethod.Post,
            "api/enrolments",
            JsonContent.Create(request));

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            TempData["ErrorMessage"] =
                "You are already enrolled in this event.";
        }
        else if (!response.IsSuccessStatusCode)
        {
            var apiError = await response.Content.ReadAsStringAsync();

            TempData["ErrorMessage"] =
                $"Unable to enrol. API Error ({(int)response.StatusCode}): {apiError}";
        }
        else
        {
            TempData["SuccessMessage"] =
                "You have successfully enrolled in this event!";
        }

        return RedirectToAction("Index", "Events");
    }

    // PARTICIPANT: View all their own enrolments.
    [HttpGet]
    public async Task<IActionResult> MyEnrolments()
    {
        if (!IsParticipant())
            return RedirectToAction("Login", "Account");

        var response = await _apiService.SendAsync(
            HttpMethod.Get, "api/enrolments/me");

        if (!response.IsSuccessStatusCode)
        {
            var apiError = await response.Content.ReadAsStringAsync();

            ViewBag.ErrorMessage =
                $"Unable to load your enrolments. API Error ({(int)response.StatusCode}): {apiError}";

            return View(new List<EnrolmentViewModel>());
        }

        var enrolments = await response.Content
            .ReadFromJsonAsync<List<EnrolmentViewModel>>()
            ?? new List<EnrolmentViewModel>();

        return View(enrolments);
    }
}
