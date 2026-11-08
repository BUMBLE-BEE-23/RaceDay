
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class ProfileController : Controller
{
    private readonly RaceDayApiService _apiService;
    private readonly IWebHostEnvironment _environment;

    private const long MaxFileSize = 5 * 1024 * 1024;

    private static readonly string[] AllowedExtensions =
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    public ProfileController(
        RaceDayApiService apiService,
        IWebHostEnvironment environment)
    {
        _apiService = apiService;
        _environment = environment;
    }

    private bool IsLoggedIn()
    {
        var role = HttpContext.Session.GetString("Role");

        return role == "Organiser" || role == "Participant";
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn())
            return RedirectToAction("Login", "Account");

        var response = await _apiService.SendAsync(
            HttpMethod.Get,
            "api/profile/me");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.ErrorMessage = "Unable to load your profile.";
            return View(new ProfileViewModel());
        }

        var profile = await response.Content
            .ReadFromJsonAsync<ProfileViewModel>();

        if (profile == null)
        {
            ViewBag.ErrorMessage = "Profile information was not found.";
            return View(new ProfileViewModel());
        }

        profile.ProfilePictureUrl = GetProfilePictureUrl(profile.Id);

        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        if (!IsLoggedIn())
            return RedirectToAction("Login", "Account");

        // Retrieve the authenticated user's profile.
        var profileResponse = await _apiService.SendAsync(
            HttpMethod.Get,
            "api/profile/me");

        if (!profileResponse.IsSuccessStatusCode)
        {
            return RedirectToAction("Login", "Account");
        }

        var currentProfile = await profileResponse.Content
            .ReadFromJsonAsync<ProfileViewModel>();

        if (currentProfile == null)
        {
            ViewBag.ErrorMessage = "Unable to verify your account.";
            return View(model);
        }

        // Never trust the user ID submitted by the browser.
        model.Id = currentProfile.Id;
        model.Email = currentProfile.Email;
        model.Role = currentProfile.Role;

        ModelState.Remove(nameof(model.Email));
        ModelState.Remove(nameof(model.Role));
        ModelState.Remove(nameof(model.ProfilePicture));

        if (model.ProfilePicture != null)
        {
            var uploadResult = await SaveProfilePicture(
                currentProfile.Id,
                model.ProfilePicture);

            if (!uploadResult.Success)
            {
                ModelState.AddModelError(
                    nameof(model.ProfilePicture),
                    uploadResult.ErrorMessage!);
            }
            else
            {
                model.ProfilePictureUrl = uploadResult.Url;
            }
        }

        if (!ModelState.IsValid)
        {
            model.ProfilePictureUrl =
                GetProfilePictureUrl(currentProfile.Id);

            return View(model);
        }

        var request = new
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber
        };

        var response = await _apiService.SendAsync(
            HttpMethod.Put,
            "api/profile/me",
            JsonContent.Create(request));

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            ViewBag.ErrorMessage =
                $"Unable to update profile. API Error ({(int)response.StatusCode}): {error}";

            model.ProfilePictureUrl =
                GetProfilePictureUrl(currentProfile.Id);

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Your profile was updated successfully!";

        return RedirectToAction("Index");
    }

    private async Task<(bool Success, string? Url, string? ErrorMessage)>
        SaveProfilePicture(int userId, IFormFile file)
    {
        if (file.Length == 0)
            return (false, null, "Please select an image.");

        if (file.Length > MaxFileSize)
            return (false, null, "Image size cannot exceed 5 MB.");

        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            return (
                false,
                null,
                "Only JPG, JPEG, PNG and WebP images are allowed.");
        }

        var webRoot = _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");
        }

        var uploadFolder = Path.Combine(
            webRoot,
            "uploads",
            "profiles");

        Directory.CreateDirectory(uploadFolder);

        var fileName = $"profile_{userId}{extension}";
        var filePath = Path.Combine(uploadFolder, fileName);

        // Remove an older picture if its extension has changed.
        foreach (var oldExtension in AllowedExtensions)
        {
            var oldPath = Path.Combine(
                uploadFolder,
                $"profile_{userId}{oldExtension}");

            if (System.IO.File.Exists(oldPath) &&
                !string.Equals(
                    oldPath,
                    filePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                System.IO.File.Delete(oldPath);
            }
        }

        await using (var stream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write))
        {
            await file.CopyToAsync(stream);
        }

        var url = $"/uploads/profiles/{fileName}";

        return (true, url, null);
    }

    private string? GetProfilePictureUrl(int userId)
    {
        var webRoot = _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");
        }

        var uploadFolder = Path.Combine(
            webRoot,
            "uploads",
            "profiles");

        foreach (var extension in AllowedExtensions)
        {
            var fileName = $"profile_{userId}{extension}";

            var filePath = Path.Combine(
                uploadFolder,
                fileName);

            if (System.IO.File.Exists(filePath))
            {
                return $"/uploads/profiles/{fileName}";
            }
        }

        return null;
    }
}
