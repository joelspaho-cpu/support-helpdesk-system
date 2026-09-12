using TicketSupportSystem.Services;

namespace TicketSupportSystem.Tests;

public class HashingServiceTests
{
    private readonly HashingService _sut = new();
    [Fact]
    public void Hash_produces_different_output_for_the_same_password()
    {
         var hash1 = _sut.Hash("Test123%.*");
         var hash2 = _sut.Hash("Test123%.*");

        Assert.NotEqual(hash1, hash2);
    }
    [Fact]
    public void Verify_returns_Success_for_the_correct_password()
    {        
        string password = "Test123%.*";
        string passwordHash = _sut.Hash(password);

        var result = _sut.Verify(password, passwordHash);
        
        Assert.Equal(HashCheckResult.Success, result);
    }
    [Fact]
    public void Verify_returns_Failed_for_a_wrong_password()
     {        
        string password = "Test123%.*";
        string passwordHash = _sut.Hash(password);

        var result = _sut.Verify("ThisIsAWrongPassword!@#12", passwordHash);
        
        Assert.Equal(HashCheckResult.Failed, result);
    }
    [Fact]
    public void Verify_returns_Failed_for_an_empty_hash()
    {
        string passwordHash = "";

        var result = _sut.Verify("Test123%.*", passwordHash);

        Assert.Equal(HashCheckResult.Failed, result);
    }
    [Fact]
    public void Verify_returns_Failed_for_a_non_base64_hash()
    {
        string passwordHash = "asdfokpg@$FA223";

        var result = _sut.Verify("Test123%.*", passwordHash);

        Assert.Equal(HashCheckResult.Failed, result);
    }
    [Fact]
    public void DummyHashVerify_always_returns_Failed()
    {
        var result = _sut.DummyHashVerify("asdfokpg@$FA223");

        Assert.Equal(HashCheckResult.Failed, result);
    }
    [Fact]
    public void Verify_returns_Failed_for_garbage_base64_input()
    {
        string passwordHash = Convert.ToBase64String(new byte[] { 1, 2, 3 });

        var result = _sut.Verify("Test123%.*", passwordHash);

        Assert.Equal(HashCheckResult.Failed, result);
    }
}