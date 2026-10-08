using TicketSupportSystem.Models;

namespace TicketSupportSystem.Services; 

public enum LoginResult { 
    InvalidCredentials, 
    Locked, 
    Success }
