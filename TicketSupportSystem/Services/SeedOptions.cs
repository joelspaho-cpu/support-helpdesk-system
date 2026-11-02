namespace TicketSupportSystem.Services;

public class SeedOptions
{
    public bool DemoData {get; set;}
    public ManagerSeedOptions Manager {get; set;} = new();
}

public class ManagerSeedOptions
{
    public string Email {get; set;} = string.Empty;
    public string Password {get; set;} = string.Empty;
    public string DisplayName {get; set;} = string.Empty;
}
