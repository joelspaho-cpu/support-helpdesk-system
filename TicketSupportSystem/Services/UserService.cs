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
    public string AccountCreateSuccessEmail =
    """
    <p style="font-family: Arial, sans-serif; color: #333333; font-size: 16px;">
      Welcome! Your account has been created.
    </p>
    """;
    public int MaxLoginAttempts = 7;
    public UserService(AppDbContext db, IHashingService hash, IEmailService email)
    {
        _db = db;
        _hasher = hash;
        _email = email;
    }
    public async Task<LoginOutcome> AuthenticateAsync(string email, string password)
    {
        string formattedEmail = email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == formattedEmail);
        if (user == null) { _hasher.DummyHashVerify(password);  return new LoginOutcome(LoginResult.InvalidCredentials); } // prevention against timing
        if (user.Status != UserStatus.Active) return new LoginOutcome(LoginResult.Locked);
        HashCheckResult verifyPassword = _hasher.Verify(password, user.PasswordHash);
        if (verifyPassword == HashCheckResult.Failed) {
            user.LoginAttempts++;
            if (user.LoginAttempts >= MaxLoginAttempts) {
                user.Status = UserStatus.Locked;
                await _db.SaveChangesAsync();
                return new LoginOutcome(LoginResult.Locked);}
            await _db.SaveChangesAsync();    
            return new LoginOutcome(LoginResult.InvalidCredentials);
        }
        if (verifyPassword == HashCheckResult.SuccessRehashNeeded) user.PasswordHash = _hasher.Hash(password);

        user.LoginAttempts = 0;
        await _db.SaveChangesAsync();
        return new LoginOutcome(LoginResult.Success, user);
    }
    public async Task<int?> RegisterAsync(string displayName, string email, string password, string region, string language, bool has2fa)
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
            CreatedAt = DateTime.UtcNow,
            Has2fa = has2fa
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        await _email.SendAsync(
    user.Email,
    "Account created successfully",
    AccountCreateSuccessEmail);
        return user.UserID;
    }
    public ClaimsPrincipal ConstructPrincipal(int UserID)
    {
         var claims = new List<Claim>
            {
               new Claim(ClaimTypes.NameIdentifier, UserID.ToString())
            };
         var identity = new ClaimsIdentity(claims, "UserScheme");
         return new ClaimsPrincipal(identity);
    }
    public async Task<int?> CreateVerifiedUserAsync(PendingRegistration pending)
    {
        bool isTaken = await _db.Users.AnyAsync(u => u.Email == pending.Email);
        if (isTaken) return null;
        User user = new User
        {
            DisplayName = pending.DisplayName,
            Email = pending.Email,
            PasswordHash = pending.PasswordHash,
            Region = pending.Region,
            Language = pending.Language,
            CreatedAt = DateTime.UtcNow,
            Has2fa = pending.Has2fa
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user.UserID;
    }
    public async Task<SettingsUpdateResult> ChangeSettingsAsync(int id, string? currentPassword, bool Has2Fa, string? DisplayName = null, string? Region = null, string? Language = null, string? newPassword = null)
    {

        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserID == id);
        if (user == null) return SettingsUpdateResult.NotFound;

        if (currentPassword == null) return SettingsUpdateResult.WrongPassword;
        if (_hasher.Verify(currentPassword, user.PasswordHash) == HashCheckResult.Failed) return SettingsUpdateResult.WrongPassword;

        user.DisplayName = DisplayName ?? user.DisplayName;
        user.Has2fa = Has2Fa;
        user.Region = Region ?? user.Region;
        user.Language = Language ?? user.Language;
        if (newPassword != null) user.PasswordHash = _hasher.Hash(newPassword);

        if (_db.Entry(user).State == EntityState.Unchanged) return SettingsUpdateResult.NoChanges;

        await _db.SaveChangesAsync();
        return SettingsUpdateResult.Success;
    }
    public async Task<UserSettings?> GetSettingsAsync(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserID == id);
        if (user == null) return null;

        return new UserSettings(user.Has2fa, user.DisplayName, user.Region, user.Language, user.Email);
    }

}