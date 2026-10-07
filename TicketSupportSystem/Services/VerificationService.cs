using TicketSupportSystem.Models;
using System.Security.Cryptography;
using TicketSupportSystem.Data;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace TicketSupportSystem.Services;

public class VerificationService : IVerificationService
{
    private readonly AppDbContext _db;
    private readonly IHashingService _hash;
    private readonly IEmailService _email;
    private readonly ICodeHashingService _codehash;
    private readonly IUserService _user;
    public int MaxAttempts { get; set; } = 7;
    public int MaxResends { get; set; } = 7;
    public int CodeLifetimeMinutes { get; set; } = 10;
    public int ResendWindowMinutes { get; set; } = 60;

    public VerificationService(AppDbContext db, IHashingService hash, IEmailService email, ICodeHashingService codehash, IUserService user)
    {
        _db = db;
        _hash = hash;
        _email = email;
        _codehash = codehash;
        _user = user;
    }
// REGISTRATION
    public async Task<Guid?> StartAsync(string displayName, string email, string password, string region, string language, bool has2fa)
    {
        var normalisedEmail = email.Trim().ToLowerInvariant(); // normalise email

        bool existsInUsers = await _db.Users.AnyAsync(u => u.Email == normalisedEmail); // check if currently exists return if it does
        if (existsInUsers) return null; 

        if (!await TryRecordSendAsync(normalisedEmail)) return null; // go through check to ensure user isnt ratelimited, if they arent add the email to emailsend if email doesnt exist add to emailsend, increment count and return true, set counter to 1 return true if it exists, false if they are either ratelimited by count or date

        await _db.PendingRegistrations.Where(p => p.Email == normalisedEmail).ExecuteDeleteAsync(); // if we pass the above and the email exists, we delete the previous pendingregistration since we are attempting new registration

        PendingRegistration prospect = new PendingRegistration{
            Id = Guid.NewGuid(),
            DisplayName = displayName,
            Email = normalisedEmail,
            PasswordHash = _hash.Hash(password),
            Has2fa = has2fa,
            Region = region,
            Language = language,
            CodeHash = string.Empty, // required by the model; IssueCode sets the real hash below
            CreatedAt = DateTime.UtcNow
        };
        var code = IssueCode(prospect);

        _db.PendingRegistrations.Add(prospect);
        await _db.SaveChangesAsync();
        await _email.SendAsync(
            prospect.Email,
            $"Your Verification Code - {code}",  
            BuildCodeEmail(code));
        
        return prospect.Id;
    }
    public async Task<VerificationResult> RegisterVerifyAsync(Guid id, int enteredCode)
    {   
        var prospect = await _db.PendingRegistrations.FirstOrDefaultAsync(u => u.Id == id); 
        if (prospect == null) return VerificationResult.NotFound; // this check is to prevent bad state, if we somehow make it to verifyasync but email doesnt exist we return instead of continuing
        
        var result = await CheckCodeAsync(prospect, enteredCode);
        
        if (result != VerificationResult.Success) return result;

        var user = await _user.CreateVerifiedUserAsync(prospect);
        if (user == null) return VerificationResult.NotFound;
        await _db.PendingRegistrations.Where(u => u.Id == id).ExecuteDeleteAsync(); // create and save user, delete prospect and return success
        return VerificationResult.Success;
    }
    public async Task<ResendStatus> GetResendStatusAsync(Guid id)
    {
        var prospect = await _db.PendingRegistrations.FirstOrDefaultAsync(p => p.Id == id);
        if (prospect == null) return ResendStatus.NotFound;

        return await IsSendAllowedAsync(prospect.Email) ? ResendStatus.Allowed : ResendStatus.Limited;
    }   
    public async Task<ResendResult> ResendAsync(Guid id)
    {
        var prospect = await _db.PendingRegistrations.FirstOrDefaultAsync(u => u.Id == id);
        if (prospect == null) return ResendResult.NotFound; // prevent bad state

        if (!await TryRecordSendAsync(prospect.Email)) return ResendResult.TooManyResends; // ratelimit on email sends if count is higher than what we allow

        var code = IssueCode(prospect);
        await _db.SaveChangesAsync();
        
        await _email.SendAsync(
            prospect.Email,
            "Your Verification Code",  
            BuildCodeEmail(code));

        return ResendResult.Success;
    }
// LOGIN
    public async Task<Guid?> StartLoginAsync(int userId, bool isPersistent)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        if (user == null) return null;
        await _db.PendingLogins.Where(p => p.UserID == userId).ExecuteDeleteAsync();
        
        var normalisedEmail = user.Email.Trim().ToLowerInvariant();
        if (!await TryRecordSendAsync(normalisedEmail)) return null;
        
        PendingLogin prospectUser = new PendingLogin{
            ID = Guid.NewGuid(),
            UserID = user.UserID,
            CodeHash = string.Empty, // required by the model; IssueCode sets the real hash below
            CreatedAt = DateTime.UtcNow,
            IsPersistent = isPersistent
        };
        var code = IssueCode(prospectUser);
        _db.PendingLogins.Add(prospectUser);
        await _db.SaveChangesAsync();
        await _email.SendAsync(
            user.Email,
            $"Your Verification Code - {code}",  
            BuildCodeEmail(code));
        
        return prospectUser.ID;
    }   
    public async Task<ResendStatus> GetResendStatusLoginAsync(Guid id)
    {
        var prospect = await _db.PendingLogins.Include(u => u.User).FirstOrDefaultAsync(p => p.ID == id);
        if (prospect == null || prospect.User == null) return ResendStatus.NotFound;

        return await IsSendAllowedAsync(prospect.User.Email) ? ResendStatus.Allowed : ResendStatus.Limited;
    }
    public async Task<ResendResult> ResendLoginAsync(Guid id)
    {
        var prospectLogin = await _db.PendingLogins.Include(p => p.User)
                                              .FirstOrDefaultAsync(p => p.ID == id);
        if (prospectLogin == null || prospectLogin.User == null) return ResendResult.NotFound; // prevent bad state

        if (!await TryRecordSendAsync(prospectLogin.User.Email)) return ResendResult.TooManyResends; // ratelimit on email sends if count is higher than what we allow

        var code = IssueCode(prospectLogin);
        await _db.SaveChangesAsync();
        
        await _email.SendAsync(
            prospectLogin.User.Email,
            $"Your Verification Code - {code}",  
            BuildCodeEmail(code));

        return ResendResult.Success;
    }
    public async Task<TwoFactorOutcome> TwoFactorVerifyAsync(Guid id, int enteredCode) //login
    {
        var prospectUser = await _db.PendingLogins
                                                    .Include(p => p.User)
                                                    .FirstOrDefaultAsync(p => p.ID == id);
                                                    

        if (prospectUser == null || prospectUser.User == null) return new TwoFactorOutcome(VerificationResult.NotFound);
        
        var result = await CheckCodeAsync(prospectUser, enteredCode);
        if (result != VerificationResult.Success) return new TwoFactorOutcome(result);

        await _db.PendingLogins.Where(u => u.ID == id).ExecuteDeleteAsync();
        return new TwoFactorOutcome(VerificationResult.Success, prospectUser.UserID, prospectUser.IsPersistent);
    }



// Universal
    private string IssueCode(ICodeChallenge pending)
    {
        string code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var codehash = _codehash.Hash(code);
        pending.CodeHash = codehash;
        pending.ExpiresAt = DateTime.UtcNow.AddMinutes(CodeLifetimeMinutes);
        pending.AttemptCount = 0;
        return code;
    }
    private async Task<bool> IsSendAllowedAsync(string email)
    {
        var record = await _db.EmailSends.FirstOrDefaultAsync(e => e.Email == email);
        if (record == null) return true;

        return record.Expiry <= DateTime.UtcNow || record.ResendCount < MaxResends;
    }
    private async Task<VerificationResult> CheckCodeAsync(ICodeChallenge pending, int enteredCode)
    {
        if (pending.AttemptCount >= MaxAttempts) return VerificationResult.TooManyAttempts;
        if (pending.ExpiresAt <= DateTime.UtcNow) return VerificationResult.Expired; // expired if code has expired, ratelimit if user has tried entering code too many times
        
        bool verify = _codehash.Verify(enteredCode.ToString(), pending.CodeHash);
        if (!verify) { pending.AttemptCount++; await _db.SaveChangesAsync(); return VerificationResult.InvalidCode; }

        return VerificationResult.Success;
    }
    private async Task<bool> TryRecordSendAsync(string email)
    {
        var record = await _db.EmailSends.FirstOrDefaultAsync(e => e.Email == email);

            if (record == null) // if email doesnt exist, add to emailsend, return true
            {
                    _db.EmailSends.Add(new EmailSend
                    {
                        Email = email,
                        ResendCount = 1,
                        Expiry = DateTime.UtcNow.AddMinutes(ResendWindowMinutes)
                    });
            return true;
            }

        bool windowActive = record.Expiry > DateTime.UtcNow; // if email exists, check against this condition

        if (!windowActive)
        {
            record.ResendCount = 1; // if email exists but window isnot active, we set count to one return true and give a fresh 60 minutes to send new codes
            record.Expiry = DateTime.UtcNow.AddMinutes(ResendWindowMinutes);
            return true;
        }

        if (record.ResendCount >= MaxResends) return false; // last check, ensure that after checking validity and whether window active, if we are at max resends we cannot do anything

        record.ResendCount++;
        return true;
    }
    private string BuildCodeEmail(string code)
    {
           return $@"
        <div style='font-family: Arial, sans-serif; max-width: 450px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
            <h2 style='color: #333333; text-align: center;'>Your Verification Code</h2>
            <p style='color: #666666; font-size: 15px; text-align: center;'>Use this code to verify your identity. Valid for {CodeLifetimeMinutes} minutes.</p>
            <div style='background-color: #f5f5f5; border: 1px dashed #cccccc; padding: 15px; margin: 25px 0; text-align: center; font-size: 28px; font-weight: bold; letter-spacing: 5px; color: #4f46e5; border-radius: 4px;'>
                {code}
            </div>
            <p style='color: #999999; font-size: 12px; text-align: center;'>If you didn't request this, you can safely ignore this email.</p>
        </div>"; // 2fa email template
    }
}
