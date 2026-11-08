using Microsoft.AspNetCore.Identity;

namespace TicketSupportSystem.Services;

public interface IStaffService
{
    Task<IdentityResult> CreateStaffAccountAsync(CreateStaffRequest request);
}