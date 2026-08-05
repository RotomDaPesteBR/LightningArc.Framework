using System.Net;
using System.Net.Http.Json;
using LightningArc.Results.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using TUnit.AspNetCore;

namespace LightningArc.Results.AspNetCore.Tests;

public class WebIntegrationTests
    : WebApplicationTest<
        ResultsWebApplicationFactory,
        LightningArc.Results.AspNetCore.Tests.Server.Program
    >
{
    [Test]
    public async Task Get_TestResultById_0_ShouldReturnOk()
    {
        // Arrange
        HttpClient client = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/v1/Results/Test/0");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        // Since wrapSuccessResponses is true and no builder is provided, it returns SuccessDetail
        SuccessDetail? content = await response.Content.ReadFromJsonAsync<SuccessDetail>();
        await Assert.That(content).IsNotNull();
        // The data is boxed as object in SuccessDetail, we can cast or use JsonElement
        await Assert.That(content!.Data?.ToString()).IsEqualTo("Test");
    }

    [Test]
    public async Task Get_TestResultById_1_ShouldReturnCreated()
    {
        // Arrange
        HttpClient client = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/v1/Results/Test/1");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);

        SuccessDetail? content = await response.Content.ReadFromJsonAsync<SuccessDetail>();
        await Assert.That(content).IsNotNull();
        await Assert.That(content!.Message).Contains("Test created successfully");
    }

    [Test]
    public async Task Get_TestResultById_2_ShouldReturnInternalServerError()
    {
        // Arrange
        HttpClient client = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/v1/Results/Test/2");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);

        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        await Assert.That(problem).IsNotNull();
        await Assert.That(problem!.Status).IsEqualTo((int)HttpStatusCode.InternalServerError);
    }

    [Test]
    public async Task Get_TestResultById_Negative1_ShouldReturnBadRequestWithDetails()
    {
        // Arrange
        HttpClient client = Factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/v1/Results/Test/-1");

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        string json = await response.Content.ReadAsStringAsync();
        await Assert.That(json).Contains("Id");
        await Assert.That(json).Contains("-1");
    }
}
