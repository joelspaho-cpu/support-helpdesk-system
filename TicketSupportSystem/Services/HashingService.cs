using Microsoft.AspNetCore.Identity;

namespace TicketSupportSystem.Services;

public class HashingService : IHashingService
{
    private readonly PasswordHasher<object> _hasher = new();
    private readonly string DummyHash = "AQAAAAIAAYagAAAAEFX7Tn8vXDyUIW+AEdCj3PQbmgPZDJhyAOxmevE3N6ad/jlBNzK16zf4tsMRXZPaug==";
    
    public HashCheckResult DummyHashVerify(string password){
         var result = _hasher.VerifyHashedPassword(new object(), DummyHash, password);
         return result switch
        {
            PasswordVerificationResult.Success => HashCheckResult.Success,
            PasswordVerificationResult.SuccessRehashNeeded => HashCheckResult.SuccessRehashNeeded,
            _ => HashCheckResult.Failed
        };
    }
    public string Hash(string password){
        return _hasher.HashPassword(new object(), password);
    }
    
    public HashCheckResult Verify(string password, string hash){
        try
        {
        var result = _hasher.VerifyHashedPassword(new object(), hash, password);
        return result switch
        {
            PasswordVerificationResult.Success => HashCheckResult.Success,
            PasswordVerificationResult.SuccessRehashNeeded => HashCheckResult.SuccessRehashNeeded,
            _ => HashCheckResult.Failed
        };
        } catch (FormatException) { return HashCheckResult.Failed; }
    }
}