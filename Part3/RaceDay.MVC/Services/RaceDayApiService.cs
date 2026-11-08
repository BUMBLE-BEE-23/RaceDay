
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using RaceDay.MVC.Models;

namespace RaceDay.MVC.Services;

public class RaceDayApiService
{
    private const string CookieSessionKey = "RaceDay.ApiCookies";

    private readonly HttpClient _client;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RaceDayApiService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _client = httpClientFactory.CreateClient("RaceDayApi");
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session =>
        _httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("No active HTTP session.");

    public async Task<HttpResponseMessage> LoginAsync(LoginViewModel model)
    {
        var request = new
        {
            model.Email,
            model.Password
        };

        return await SendAsync(
            HttpMethod.Post,
            "api/auth/login",
            JsonContent.Create(request));
    }

    public async Task<HttpResponseMessage> RegisterAsync(RegisterViewModel model)
    {
        var request = new
        {
            model.FirstName,
            model.LastName,
            model.Email,
            model.Password,
            model.Role,
            model.PhoneNumber
        };

        return await SendAsync(
            HttpMethod.Post,
            "api/auth/register",
            JsonContent.Create(request));
    }

    public async Task<HttpResponseMessage> LogoutAsync()
    {
        var response = await SendAsync(
            HttpMethod.Post,
            "api/auth/logout");

        if (response.IsSuccessStatusCode)
        {
            Session.Remove(CookieSessionKey);
        }

        return response;
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string endpoint,
        HttpContent? content = null)
    {
        using var request = new HttpRequestMessage(method, endpoint);

        if (content != null)
        {
            request.Content = content;
        }

        var cookies = Session.GetString(CookieSessionKey);

        if (!string.IsNullOrWhiteSpace(cookies))
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookies);
        }

        var response = await _client.SendAsync(request);

        SaveApiCookies(response);

        return response;
    }

    private void SaveApiCookies(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var headers))
        {
            return;
        }

        var cookies = new Dictionary<string, string>(
            StringComparer.Ordinal);

        var existing = Session.GetString(CookieSessionKey);

        if (!string.IsNullOrWhiteSpace(existing))
        {
            foreach (var cookie in existing.Split(
                ';', StringSplitOptions.RemoveEmptyEntries))
            {
                var separator = cookie.IndexOf('=');

                if (separator > 0)
                {
                    cookies[cookie[..separator].Trim()] =
                        cookie[(separator + 1)..].Trim();
                }
            }
        }

        foreach (var header in headers)
        {
            var cookie = header.Split(';', 2)[0].Trim();
            var separator = cookie.IndexOf('=');

            if (separator > 0)
            {
                cookies[cookie[..separator].Trim()] =
                    cookie[(separator + 1)..].Trim();
            }
        }

        Session.SetString(
            CookieSessionKey,
            string.Join("; ", cookies.Select(
                cookie => $"{cookie.Key}={cookie.Value}")));
    }
}
