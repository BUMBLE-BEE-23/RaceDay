
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class CategoriesController : Controller
{
    private readonly RaceDayApiService _apiService;

    public CategoriesController(RaceDayApiService apiService)
    {
        _apiService = apiService;
    }

    private bool IsOrganiser()
    {
        return HttpContext.Session.GetString("Role") == "Organiser";
    }

    [HttpGet]
    public async Task<IActionResult> Index(int eventId)
    {
        if (eventId <= 0)
            return BadRequest();

        ViewBag.EventId = eventId;

        var response = await _apiService.SendAsync(
            HttpMethod.Get,
            $"api/events/{eventId}/categories");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return NotFound();

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.ErrorMessage = "Unable to load categories.";
            return View(new List<CategoryViewModel>());
        }

        var categories = await response.Content
            .ReadFromJsonAsync<List<CategoryViewModel>>()
            ?? new List<CategoryViewModel>();

        return View(categories);
    }

    [HttpGet]
    public IActionResult Create(int eventId)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        return View(new CategoryViewModel
        {
            EventId = eventId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryViewModel model)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
            return View(model);

        var request = new
        {
            model.Name,
            model.CategoryType,
            model.MinimumAge,
            model.MaximumAge,
            model.DistanceKm
        };

        var response = await _apiService.SendAsync(
            HttpMethod.Post,
            $"api/events/{model.EventId}/categories",
            JsonContent.Create(request));

        if (!response.IsSuccessStatusCode)
        {
            var apiError = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError("",
                $"API Error ({(int)response.StatusCode}): {apiError}");

            return View(model);
        }

        return RedirectToAction(nameof(Index),
            new { eventId = model.EventId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, int eventId)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var response = await _apiService.SendAsync(
            HttpMethod.Get,
            $"api/events/{eventId}/categories");

        if (!response.IsSuccessStatusCode)
            return NotFound();

        var categories = await response.Content
            .ReadFromJsonAsync<List<CategoryViewModel>>()
            ?? new List<CategoryViewModel>();

        var category = categories.FirstOrDefault(x => x.Id == id);

        if (category == null)
            return NotFound();

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryViewModel model)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var request = new
        {
            model.Name,
            model.CategoryType,
            model.MinimumAge,
            model.MaximumAge,
            model.DistanceKm
        };

        var response = await _apiService.SendAsync(
            HttpMethod.Put,
            $"api/categories/{id}",
            JsonContent.Create(request));

        if (!response.IsSuccessStatusCode)
        {
            var apiError = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError("",
                $"API Error ({(int)response.StatusCode}): {apiError}");

            return View(model);
        }

        return RedirectToAction(nameof(Index),
            new { eventId = model.EventId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, int eventId)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var response = await _apiService.SendAsync(
            HttpMethod.Get,
            $"api/events/{eventId}/categories");

        if (!response.IsSuccessStatusCode)
            return NotFound();

        var categories = await response.Content
            .ReadFromJsonAsync<List<CategoryViewModel>>()
            ?? new List<CategoryViewModel>();

        var category = categories.FirstOrDefault(x => x.Id == id);

        if (category == null)
            return NotFound();

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, int eventId)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var response = await _apiService.SendAsync(
            HttpMethod.Delete,
            $"api/categories/{id}");

        if (!response.IsSuccessStatusCode &&
            response.StatusCode != HttpStatusCode.NoContent)
        {
            var apiError = await response.Content.ReadAsStringAsync();

            return BadRequest(
                $"Unable to delete category. API Error: {apiError}");
        }

        return RedirectToAction(nameof(Index),
            new { eventId });
    }
}
