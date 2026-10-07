namespace TicketSupportSystem.Services;


public record UserSettings(bool Has2Fa, string DisplayName, string Region, string Language, string Email);

