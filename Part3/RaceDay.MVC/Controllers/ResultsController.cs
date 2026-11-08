
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class ResultsController : Controller
{
    private readonly RaceDayApiService _apiService;

    public ResultsController(RaceDayApiService apiService)
    {
        _apiService = apiService;
    }

    private bool IsOrganiser() =>
        HttpContext.Session.GetString("Role") == "Organiser";

    private bool IsParticipant() =>
        HttpContext.Session.GetString("Role") == "Participant";

    // Organiser: select an event.
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

    // Organiser: view participants and their results.
    [HttpGet]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var enrolmentResponse = await _apiService.SendAsync(
            HttpMethod.Get,
            $"api/events/{eventId}/enrolments");

        if (!enrolmentResponse.IsSuccessStatusCode)
        {
            ViewBag.ErrorMessage = "Unable to load enrolled participants.";
            return View(new List<ResultViewModel>());
        }

        var enrolments = await enrolmentResponse.Content
            .ReadFromJsonAsync<List<EnrolmentViewModel>>()
            ?? new List<EnrolmentViewModel>();

        var resultResponse = await _apiService.SendAsync(
            HttpMethod.Get,
            $"api/events/{eventId}/results");

        var results = resultResponse.IsSuccessStatusCode
            ? await resultResponse.Content
                .ReadFromJsonAsync<List<ResultViewModel>>()
                ?? new List<ResultViewModel>()
            : new List<ResultViewModel>();

        var viewModels = enrolments.Select(enrolment =>
        {
            var existing = results.FirstOrDefault(
                result => result.ParticipantId == enrolment.ParticipantId);

            return new ResultViewModel
            {
                Id = existing?.Id ?? 0,
                EnrolmentId = enrolment.Id,
                EventId = eventId,
                EventName = enrolment.EventName,
                ParticipantId = enrolment.ParticipantId,
                ParticipantName = enrolment.ParticipantName,
                FinishingPosition = existing?.FinishingPosition ?? 1,
                FinishTime = existing?.FinishTime ?? TimeSpan.Zero,
                RecordedUtc = existing?.RecordedUtc ?? DateTime.MinValue
            };
        }).ToList();

        ViewBag.EventId = eventId;

        if (enrolments.Count > 0)
            ViewBag.EventName = enrolments[0].EventName;

        return View(viewModels);
    }

    // Organiser: record or update a participant's result.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Record(ResultViewModel model)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        if (model.FinishingPosition < 1)
        {
            TempData["ErrorMessage"] =
                "Finishing position must be greater than zero.";

            return RedirectToAction(
                "ForEvent", new { eventId = model.EventId });
        }

        if (model.FinishTime < TimeSpan.Zero)
        {
            TempData["ErrorMessage"] =
                "Finish time cannot be negative.";

            return RedirectToAction(
                "ForEvent", new { eventId = model.EventId });
        }

        var request = new
        {
            EnrolmentId = model.EnrolmentId,
            FinishingPosition = model.FinishingPosition,
            FinishTime = model.FinishTime
        };

        var response = await _apiService.SendAsync(
            HttpMethod.Post,
            "api/results",
            JsonContent.Create(request));

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] =
                "Race result saved successfully.";
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();

            TempData["ErrorMessage"] =
                $"Unable to save result ({(int)response.StatusCode}): {error}";
        }

        return RedirectToAction(
            "ForEvent", new { eventId = model.EventId });
    }

    // Participant: view personal race results.
    [HttpGet]
    public async Task<IActionResult> MyResults()
    {
        if (!IsParticipant())
            return RedirectToAction("Login", "Account");

        var response = await _apiService.SendAsync(
            HttpMethod.Get,
            "api/results/me");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.ErrorMessage = "Unable to load your results.";
            return View(new List<ResultViewModel>());
        }

        var results = await response.Content
            .ReadFromJsonAsync<List<ResultViewModel>>()
            ?? new List<ResultViewModel>();

        return View(results);
    }
}
