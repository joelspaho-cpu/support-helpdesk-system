using TicketSupportSystem.Services;
using System.Security.Cryptography;
using TicketSupportSystem.Tests.Fakes;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TicketSupportSystem.Data;
using TicketSupportSystem.Models;

namespace TicketSupportSystem.Tests;

public class VerificationServiceTests : IDisposable
{
    private readonly TestDatabase _testdb = new();
    private readonly FakeEmailService _testemail = new();
    private sealed record InsertUserResult(int? UserId, Guid? PendingId);
    private const string TestEmail = "test@test.com";
    private readonly VerificationService _sut;
    private readonly AppDbContext _db;
    public VerificationServiceTests()
    {
        _db = _testdb.CreateContext();
        var hasher = new HashingService();
         var codeHasher = new CodeHashingService(Options.Create(new CodeHashingServiceOptions
        {
            CodeSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        }));
        var userService = new UserService(_db, hasher, _testemail);

        _sut = new VerificationService(_db, hasher, _testemail, codeHasher, userService);
    }
    [Fact]
    public async Task Verify_User_And_Create_Account_with_correct_code()
    {
        var start = await StartRegistrationAsync();
        var final = await _sut.RegisterVerifyAsync(start, LatestCode());
        
        var exists = await _db.Users.AnyAsync(u => u.Email == TestEmail);
        Assert.True(exists);     
        Assert.Equal(VerificationResult.Success, final);
    }
    [Fact]
    public async Task Verify_User_And_Ensure_No_Account_Is_Created_with_Incorrect_Code()
    {
        var start = await StartRegistrationAsync();
        var wrongCode = LatestCode() + 1;
        var final = await _sut.RegisterVerifyAsync(start, wrongCode);

        var exists = await _db.Users.AnyAsync(u => u.Email == TestEmail);
        Assert.Equal(VerificationResult.InvalidCode, final);
        Assert.False(exists);     
    }
    [Fact]
    public async Task Max_Code_Insertions_Locks_Out_Even_If_Correct_Code_Is_Entered()
    {
        var start = await StartRegistrationAsync();
        var wrongCode = LatestCode() + 1;
        for (int i = 0; i < _sut.MaxAttempts; i++)
          {  await _sut.RegisterVerifyAsync(start, wrongCode);  }

        var final = await _sut.RegisterVerifyAsync(start, LatestCode());

        var exists = await _db.Users.AnyAsync(u => u.Email == TestEmail);
        Assert.Equal(VerificationResult.TooManyAttempts, final);
        Assert.False(exists);
    }
    [Fact]
    public async Task Expired_code_is_rejected_even_when_correct()
    {
        var start = await StartRegistrationAsync();
        var result = await _db.PendingRegistrations.FirstOrDefaultAsync(u => u.Id == start);
        Assert.NotNull(result);
        result.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await _db.SaveChangesAsync();
        var final = await _sut.RegisterVerifyAsync(result.Id, LatestCode());
        Assert.Equal(VerificationResult.Expired, final);
        Assert.Single(_testemail.Sent);
    }
    [Fact]
    public async Task Resend_replaces_code_and_old_code_is_rejected()
    {
        var start = await StartRegistrationAsync();
        var oldCode = LatestCode();
        var resend = await _sut.ResendAsync(start);
        Assert.Equal(ResendResult.Success, resend);
        var newCode = LatestCode();
        var verification = await _sut.RegisterVerifyAsync(start, oldCode);
        Assert.Equal(VerificationResult.InvalidCode, verification);
        var verification2 = await _sut.RegisterVerifyAsync(start, newCode);
        Assert.Equal(VerificationResult.Success, verification2);
        Assert.Equal(2, _testemail.Sent.Count);
    }
    [Fact]
    public async Task Correct_code_signs_you_in()
    {
        var result = await InsertUserAndStartLogin();
        Assert.NotNull(result.PendingId);
        var code = LatestCode();
        var codeResult = await _sut.TwoFactorVerifyAsync(result.PendingId.Value, code);
        Assert.Equal(new TwoFactorOutcome(VerificationResult.Success, result.UserId, true), codeResult);
    }
    [Fact]
    public async Task Wrong_code_doesnt_sign_you_in()
    {
        var result = await InsertUserAndStartLogin();
        Assert.NotNull(result.PendingId);
        var code = LatestCode() + 1;
        var codeResult = await _sut.TwoFactorVerifyAsync(result.PendingId.Value, code);
        Assert.Equal(new TwoFactorOutcome(VerificationResult.InvalidCode), codeResult);
    }
    [Fact]
    public async Task No_email_is_sent_when_limit_is_reached()
    {
        var result = await InsertUserAndStartLogin();
        Assert.NotNull(result.PendingId);
        for (int i = 0; i < _sut.MaxResends - 1; i++)
        {
            await _sut.ResendLoginAsync(result.PendingId.Value);
        }
        var final = await _sut.ResendLoginAsync(result.PendingId.Value);
        Assert.Equal(_sut.MaxResends, _testemail.Sent.Count);
        Assert.Equal(ResendResult.TooManyResends, final);
    }
// HELPERS
    private async Task<InsertUserResult> InsertUserAndStartLogin()
    {
        User user = new User
        {
            DisplayName = "TEST",
            Email = TestEmail,
            PasswordHash = "not-a-real-hash",
            Has2fa = true,
            Region = "GB",
            Language = "EN",
            CreatedAt = DateTime.UtcNow,
            Status = UserStatus.Active,
            LoginAttempts = 0,
        };
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
        var guid = await _sut.StartLoginAsync(user.UserID, true);
        return new InsertUserResult(user.UserID, guid);
    }
    private async Task<Guid> StartRegistrationAsync()
    {
        var result = await _sut.StartAsync("test", TestEmail, "asd123$%234a", "GB", "EN", true);
        Assert.NotNull(result);
        return result.Value;
    }
    
    private int LatestCode() =>
    int.Parse(Regex.Match(_testemail.Sent.Last().Body, @">\s*(\d{6})\s*<").Groups[1].Value);

    public void Dispose() => _testdb.Dispose();
}