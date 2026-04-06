using LightningArc.Primitives.ValueObjects;
using LightningArc.Results;
using LightningArc.Results.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace LightningArc.Samples
{
    public class UserDto(string email, string name);

    /// <summary>
    /// Demonstrates best practices for using the LightningArc ecosystem.
    /// </summary>
    public class UserService
    {
        public async TaskResult<UserDto> RegisterUserAsync(string emailRaw, string name)
        {
            // 1. Validation using Error Aggregation (+)
            Error? validationErrors = null;
            if (string.IsNullOrWhiteSpace(name))
                validationErrors += Error.Validation.MissingField("Name is required");

            if (validationErrors != null)
                return validationErrors;

            // 2. Value Object Creation and Functional Chaining
            return await Email.Create(emailRaw) // Returns Result<Email>
                .BindAsync(email => CheckDatabaseUniqueness(email))
                .Map(email => new UserDto(email, name))
                .Tap(user => NotifyAdmin(user));
        }

        private async TaskResult<Email> CheckDatabaseUniqueness(Email email)
        {
            await Task.Delay(10); // Simulate DB
            return email; // Success
        }

        private void NotifyAdmin(UserDto user) { /* Side effect */ }
    }

    /// <summary>
    /// Demonstrates seamless Web API integration.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(UserService service) : ControllerBase
    {
        [HttpPost]
        public async Task<EndpointResult<UserDto>> Register(string email, string name)
        {
            // The Result<UserDto> is automatically converted to EndpointResult<UserDto>
            // which maps to 200 OK, 400 Bad Request, etc., with RFC 7807 support.
            var result = await service.RegisterUserAsync(email, name);
            
            // Example of modern C# ergonomics (Deconstruction)
            if (!result)
            {
                var (code, message, details) = result.Error;
                // Log or handle specifically...
            }

            return result;
        }
    }
}