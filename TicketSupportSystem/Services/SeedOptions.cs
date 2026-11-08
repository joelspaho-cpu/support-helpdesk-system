namespace TicketSupportSystem.Services;

public class SeedOptions
{
    public bool DemoData {get; set;}
    public SeedAccountOptions Admin {get; set;} = new();
    public SeedAccountOptions Manager {get; set;} = new();
    public SeedAccountOptions AdvisorL1 {get; set;} = new();
    public SeedAccountOptions AdvisorL2 {get; set;} = new();
}

public class SeedAccountOptions
{
    public string Email {get; set;} = string.Empty;
    public string Password {get; set;} = string.Empty;
    public string DisplayName {get; set;} = string.Empty;
}
