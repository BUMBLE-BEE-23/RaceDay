using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class EventsController : Controller
{
    private readonly RaceDayApiService _apiService;

    public EventsController(RaceDayApiService apiService)
    {
        _apiService = apiService;
    }

    private bool IsOrganiser()
    {
        return HttpContext.Session.GetString("Role") == "Organiser";
    }

    // Display all events.
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _apiService.SendAsync(
                HttpMethod.Get, "api/events");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = "Unable to load events.";
                return View(new List<EventViewModel>());
            }

            var events = await response.Content
                .ReadFromJsonAsync<List<EventViewModel>>();

            return View(events ?? new List<EventViewModel>());
        }
        catch (HttpRequestException)
        {
            ViewBag.ErrorMessage = "Unable to connect to RaceDay API.";
            return View(new List<EventViewModel>());
        }
    }

    // Display event details.
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var response = await _apiService.SendAsync(
                HttpMethod.Get, $"api/events/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var raceEvent = await response.Content
                .ReadFromJsonAsync<EventViewModel>();

            if (raceEvent == null)
                return NotFound();

            return View(raceEvent);
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, "Unable to connect to RaceDay API.");
        }
    }

    // Display Create form.
    [HttpGet]
    public IActionResult Create()
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        return View(new EventViewModel());
    }

    // Create event through API.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventViewModel model)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
            return View(model);

        if (!new[] { "Run", "Walk", "Cycle" }.Contains(model.EventType))
        {
            ModelState.AddModelError(
                nameof(model.EventType),
                "Select Run, Walk or Cycle.");

            return View(model);
        }

        try
        {
            var request = new
            {
                model.Name,
                model.Description,
                model.EventDate,
                model.Location,
                model.DistanceKm,
                model.EventType
            };

            var response = await _apiService.SendAsync(
                HttpMethod.Post,
                "api/events",
                JsonContent.Create(request));

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The API could not create the event.");

                return View(model);
            }

            TempData["SuccessMessage"] = "Event created successfully!";

            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to connect to RaceDay API.");

            return View(model);
        }
    }

    // Display Edit form.
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        try
        {
            var response = await _apiService.SendAsync(
                HttpMethod.Get, $"api/events/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return NotFound();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode);

            var raceEvent = await response.Content
                .ReadFromJsonAsync<EventViewModel>();

            if (raceEvent == null)
                return NotFound();

            return View(raceEvent);
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, "Unable to connect to RaceDay API.");
        }
    }

    // Update event through API.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventViewModel model)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        if (!new[] { "Run", "Walk", "Cycle" }.Contains(model.EventType))
        {
            ModelState.AddModelError(
                nameof(model.EventType),
                "Select Run, Walk or Cycle.");

            return View(model);
        }

        try
        {
            var request = new
            {
                model.Name,
                model.Description,
                model.EventDate,
                model.Location,
                model.DistanceKm,
                model.EventType
            };

            var response = await _apiService.SendAsync(
                HttpMethod.Put,
                $"api/events/{id}",
                JsonContent.Create(request));

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return Forbid();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
                return NotFound();

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to update this event.");

                return View(model);
            }

            TempData["SuccessMessage"] = "Event updated successfully!";

            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to connect to RaceDay API.");

            return View(model);
        }
    }

    // Display Delete confirmation.
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        try
        {
            var response = await _apiService.SendAsync(
                HttpMethod.Get, $"api/events/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var raceEvent = await response.Content
                .ReadFromJsonAsync<EventViewModel>();

            if (raceEvent == null)
                return NotFound();

            return View(raceEvent);
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, "Unable to connect to RaceDay API.");
        }
    }

    // Delete event through API.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        try
        {
            var response = await _apiService.SendAsync(
                HttpMethod.Delete, $"api/events/{id}");

            if (response.StatusCode == HttpStatusCode.Forbidden)
                return Forbid();

            if (response.StatusCode == HttpStatusCode.NotFound)
                return NotFound();

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Unable to delete this event.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Event deleted successfully!";

            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            TempData["ErrorMessage"] = "Unable to connect to RaceDay API.";
            return RedirectToAction(nameof(Index));
        }
    }
}