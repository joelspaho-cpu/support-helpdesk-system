namespace TicketSupportSystem.Services;

public interface ICodeHashingService{
string Hash(string code);
bool Verify(string code, string codeHash);

}