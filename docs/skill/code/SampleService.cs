using LightningArc.Results;
using LightningArc.Abstractions.ValueObjects;

namespace Docs.Skill.Code;

/// <summary>
/// Reference implementation showing best practices using LightningArc.Utils.
/// </summary>
public class UserService(IUserRepository repo, IMapper mapper, ILogger logger)
{
    public async TaskResult<UserResponse> RegisterUser(RegisterRequest request)
    {
        // 1. Validate Input using Value Objects and Ensure
        return await Result.Success(request)
            .Ensure(req => !string.IsNullOrEmpty(req.Name), Error.Validation.MissingField("Name is required"))
            .Bind(req => CreateEmail(req.Email)) // Try create Value Object
            .BindAsync(email => CheckAvailability(email))
            
            // 2. Map to Domain Entity
            .Map(email => new User { 
                Name = request.Name, 
                Email = email,
                Status = UserStatus.Pending 
            })
            
            // 3. Persist using Repository
            .BindAsync(user => repo.Save(user))
            
            // 4. Side effects
            .Tap(user => logger.LogInformation("User {Id} registered", user.Id))
            
            // 5. Final Mapping to Response DTO
            .Map(user => mapper.Map<UserResponse>(user));
    }

    private Result<Email> CreateEmail(string raw)
    {
        try {
            return Email.Create(raw);
        } catch {
            return Error.Validation.InvalidFormat("Email address is invalid");
        }
    }

    private async TaskResult<Email> CheckAvailability(Email email)
    {
        var exists = await repo.ExistsByEmail(email);
        return exists 
            ? Error.Resource.AlreadyExists("Email already in use") 
            : email;
    }
}

