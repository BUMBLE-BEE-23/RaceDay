
using Microsoft.AspNetCore.Authorization;
using RaceDay.API.Controllers;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests;

public class EndpointAccessTests
{
    [Theory]
    [InlineData(typeof(ProfileController), "GetMyProfile")]
    [InlineData(typeof(ProfileController), "UpdateMyProfile")]
    public void ProfileEndpoints_RequireAuthentication(
        Type controllerType, string actionName)
    {
        var controllerAttributes = controllerType.GetCustomAttributes(
            typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>();

        var method = controllerType.GetMethod(actionName);

        Assert.NotNull(method);

        var methodAttributes = method!.GetCustomAttributes(
            typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>();

        Assert.True(
            controllerAttributes.Any() || methodAttributes.Any(),
            $"{actionName} must require authentication.");
    }

    [Fact]
    public void GetMyResults_RequiresParticipantRole()
    {
        var method = typeof(ResultsController)
            .GetMethod(nameof(ResultsController.GetMyResults));

        Assert.NotNull(method);

        var attributes = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>();

        Assert.Contains(attributes, attribute =>
            attribute.Roles?.Split(',')
                .Contains(UserRoles.Participant) == true);
    }

    [Theory]
    [InlineData("Record")]
    [InlineData("GetForEvent")]
    public void OrganiserResultEndpoints_RequireOrganiserRole(
        string actionName)
    {
        var method = typeof(ResultsController).GetMethod(actionName);

        Assert.NotNull(method);

        var attributes = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>();

        Assert.Contains(attributes, attribute =>
            attribute.Roles?.Split(',')
                .Contains(UserRoles.Organiser) == true);
    }
}
