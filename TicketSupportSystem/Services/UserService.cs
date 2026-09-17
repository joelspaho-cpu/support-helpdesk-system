using TicketSupportSystem.Models;
using TicketSupportSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace TicketSupportSystem.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IHashingService _hasher;
    private readonly IEmailService _email;
    public UserService(AppDbContext db, IHashingService hash, IEmailService email)
    {
        _db = db;
        _hasher = hash;
        _email = email;
    }
    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        string formattedEmail = email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == formattedEmail);
        if (user == null) { _hasher.DummyHashVerify(password); return null; } // prevention against timing
        HashCheckResult verifyPassword = _hasher.Verify(password, user.PasswordHash);
        if (verifyPassword == HashCheckResult.Failed) return null;

        if (verifyPassword == HashCheckResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _hasher.Hash(password);
                await _db.SaveChangesAsync();
            }

        return user;
    }
    public async Task<int?> RegisterAsync(string displayName, string email, string password, string region, string language)
    {
        string formattedEmail = email.Trim().ToLowerInvariant();
        bool isTaken = await _db.Users.AnyAsync(u => u.Email == formattedEmail);
        if (isTaken) return null;
        var passwordHash = _hasher.Hash(password);
        User user = new User
        {
            DisplayName = displayName,
            Email = formattedEmail,
            PasswordHash = passwordHash,
            Region = region,
            Language = language,
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        await _email.SendAsync(
    user.Email,
    "Account created successfully",
    """
    <p style="font-family: Arial, sans-serif; color: #333333; font-size: 16px;">
      Welcome! Your account has been created.
    </p>
    """);
        return user.UserID;
    }
    public ClaimsPrincipal ConstructPrincipal(User user)
    {
         var claims = new List<Claim>
            {
               new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString())
            };
         var identity = new ClaimsIdentity(claims, "UserScheme");
         return new ClaimsPrincipal(identity);
    }

}