using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace TicketSupportSystem.Services;

public class CodeHashingService : ICodeHashingService{
private readonly CodeHashingServiceOptions _options;
public CodeHashingService(IOptions<CodeHashingServiceOptions> options)
    {
        _options = options.Value;
    }
public string Hash(string code)
    {
        var hash = HMACSHA256.HashData(Convert.FromBase64String(_options.CodeSecret), Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(hash);
    }
public bool Verify(string code, string codeHash)
    {
        var hash = HMACSHA256.HashData(Convert.FromBase64String(_options.CodeSecret), Encoding.UTF8.GetBytes(code));
        return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(codeHash), hash);
    }

}