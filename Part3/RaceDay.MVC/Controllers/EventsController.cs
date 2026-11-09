
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class EventsController : Controller
{
    private readonly RaceDayApiService _apiService;
    private readonly IWebHostEnvironment _environment;

    private const long MaxBannerSize = 5 * 1024 * 1024;

    private static readonly Dictionary<string, string> AllowedBannerTypes = new()
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".webp", "image/webp" }
    };

    public EventsController(
        RaceDayApiService apiService,
        IWebHostEnvironment environment)
    {
        _apiService = apiService;
        _environment = environment;
    }

    private bool IsOrganiser()
    {
        return HttpContext.Session.GetString("Role") == "Organiser";
    }

    private bool ValidateBanner(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return true;

        if (file.Length > MaxBannerSize)
        {
            ModelState.AddModelError(
                nameof(EventViewModel.BannerFile),
                "The banner image cannot exceed 5 MB.");

            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedBannerTypes.TryGetValue(extension, out var expectedType) ||
            !string.Equals(file.ContentType, expectedType, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                nameof(EventViewModel.BannerFile),
                "Upload a valid JPG, JPEG, PNG or WebP image.");

            return false;
        }

        return true;
    }

    private async Task<string> SaveBannerAsync(IFormFile file)
    {
        var webRoot = _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");

        var uploadFolder = Path.Combine(webRoot, "uploads", "events");

        Directory.CreateDirectory(uploadFolder);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"event_{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return "/uploads/events/" + fileName;
    }

    private void DeleteBannerFile(string? bannerUrl)
    {
        if (string.IsNullOrWhiteSpace(bannerUrl) ||
            !bannerUrl.StartsWith(
                "/uploads/events/",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fileName = Path.GetFileName(bannerUrl);

        if (string.IsNullOrWhiteSpace(fileName))
            return;

        var webRoot = _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");

        var filePath = Path.Combine(
            webRoot, "uploads", "events", fileName);

        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? eventType,
        string? location,
        DateTime? eventDate)
    {
        ViewBag.SelectedEventType = eventType;
        ViewBag.SelectedLocation = location;
        ViewBag.SelectedEventDate = eventDate?.ToString("yyyy-MM-dd");

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

            var filteredEvents = events ?? new List<EventViewModel>();

            if (!IsOrganiser())
            {
                filteredEvents = filteredEvents
                    .Where(x => x.EventDate.Date >= DateTime.Today)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(eventType))
            {
                filteredEvents = filteredEvents
                    .Where(x => string.Equals(
                        x.EventType,
                        eventType,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                filteredEvents = filteredEvents
                    .Where(x => !string.IsNullOrEmpty(x.Location) &&
                        x.Location.Contains(
                            location,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (eventDate.HasValue)
            {
                filteredEvents = filteredEvents
                    .Where(x => x.EventDate.Date == eventDate.Value.Date)
                    .ToList();
            }

            return View(filteredEvents);
        }
        catch (HttpRequestException)
        {
            ViewBag.ErrorMessage = "Unable to connect to RaceDay API.";
            return View(new List<EventViewModel>());
        }
    }

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

    [HttpGet]
    public IActionResult Create()
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        return View(new EventViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventViewModel model)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        if (!ValidateBanner(model.BannerFile))
            return View(model);

        if (!ModelState.IsValid)
            return View(model);

        if (!new[] { "Run", "Walk", "Cycle" }.Contains(model.EventType))
        {
            ModelState.AddModelError(
                nameof(model.EventType),
                "Select Run, Walk or Cycle.");

            return View(model);
        }

        string? savedBannerUrl = null;

        try
        {
            if (model.BannerFile != null && model.BannerFile.Length > 0)
            {
                savedBannerUrl = await SaveBannerAsync(model.BannerFile);
                model.BannerUrl = savedBannerUrl;
            }

            var request = new
            {
                model.Name,
                model.Description,
                model.EventDate,
                model.Location,
                model.DistanceKm,
                model.EventType,
                model.BannerUrl
            };

            var response = await _apiService.SendAsync(
                HttpMethod.Post,
                "api/events",
                JsonContent.Create(request));

            if (!response.IsSuccessStatusCode)
            {
                DeleteBannerFile(savedBannerUrl);

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
            DeleteBannerFile(savedBannerUrl);

            ModelState.AddModelError(
                string.Empty,
                "Unable to connect to RaceDay API.");

            return View(model);
        }
        catch (IOException)
        {
            DeleteBannerFile(savedBannerUrl);

            ModelState.AddModelError(
                string.Empty,
                "Unable to save the banner image.");

            return View(model);
        }
    }

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventViewModel model)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        if (id != model.Id)
            return BadRequest();

        if (!ValidateBanner(model.BannerFile))
            return View(model);

        if (!ModelState.IsValid)
            return View(model);

        if (!new[] { "Run", "Walk", "Cycle" }.Contains(model.EventType))
        {
            ModelState.AddModelError(
                nameof(model.EventType),
                "Select Run, Walk or Cycle.");

            return View(model);
        }

        string? savedBannerUrl = null;

        try
        {
            var existingResponse = await _apiService.SendAsync(
                HttpMethod.Get, $"api/events/{id}");

            if (existingResponse.StatusCode == HttpStatusCode.NotFound)
                return NotFound();

            if (!existingResponse.IsSuccessStatusCode)
                return StatusCode((int)existingResponse.StatusCode);

            var existingEvent = await existingResponse.Content
                .ReadFromJsonAsync<EventViewModel>();

            if (existingEvent == null)
                return NotFound();

            model.BannerUrl = existingEvent.BannerUrl;

            if (model.BannerFile != null && model.BannerFile.Length > 0)
            {
                savedBannerUrl = await SaveBannerAsync(model.BannerFile);
                model.BannerUrl = savedBannerUrl;
            }

            var request = new
            {
                model.Name,
                model.Description,
                model.EventDate,
                model.Location,
                model.DistanceKm,
                model.EventType,
                model.BannerUrl
            };

            var response = await _apiService.SendAsync(
                HttpMethod.Put,
                $"api/events/{id}",
                JsonContent.Create(request));

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                DeleteBannerFile(savedBannerUrl);
                return Forbid();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                DeleteBannerFile(savedBannerUrl);
                return NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                DeleteBannerFile(savedBannerUrl);

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to update this event.");

                return View(model);
            }

            if (savedBannerUrl != null)
                DeleteBannerFile(existingEvent.BannerUrl);

            TempData["SuccessMessage"] = "Event updated successfully!";

            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            DeleteBannerFile(savedBannerUrl);

            ModelState.AddModelError(
                string.Empty,
                "Unable to connect to RaceDay API.");

            return View(model);
        }
        catch (IOException)
        {
            DeleteBannerFile(savedBannerUrl);

            ModelState.AddModelError(
                string.Empty,
                "Unable to save the banner image.");

            return View(model);
        }
    }

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
