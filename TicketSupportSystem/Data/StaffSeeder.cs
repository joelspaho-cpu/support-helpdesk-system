using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TicketSupportSystem.Models;
using TicketSupportSystem.Services;

namespace TicketSupportSystem.Data;

public class StaffSeeder{
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly UserManager<Staff> _staffManager;
    private readonly IStaffService _staffService;
    private readonly IOptions<SeedOptions> _seedOptions;
    public StaffSeeder(RoleManager<IdentityRole<int>> roleManager, UserManager<Staff> staffManager, IStaffService staffService, IOptions<SeedOptions> seedOptions)
    {
        _roleManager = roleManager;
        _staffManager = staffManager;   
        _staffService = staffService;
        _seedOptions = seedOptions;
    }
    public async Task<IdentityResult> SeedAsync()
    {
        var roles = new[] { StaffRoles.Manager, StaffRoles.Advisor, StaffRoles.Engineer, StaffRoles.Admin };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                return await _roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

        if (_seedOptions.Value == null)
        {
            throw new InvalidOperationException("Seed options are not configured.");
        }

        var managerEmail = _seedOptions.Value.Manager.Email;
        var managerPassword = _seedOptions.Value.Manager.Password;
        
        if (await _staffManager.GetUsersInRoleAsync(StaffRoles.Manager) == null || _staffManager.GetUsersInRoleAsync(StaffRoles.Advisor) == null)
        {
           return await CreateStaffAccountAsync(managerEmail, managerPassword);
        }
        return IdentityResult.Success;

}
    private Task<IdentityResult> CreateStaffAccountAsync(string email, string password)
    {
        var request = new CreateStaffRequest
        {
            DisplayName = "Test Manager",
            Email = email,
            Password = password,
            Role = StaffRoles.Manager,
        };

        return _staffService.CreateStaffAccountAsync(request);
    }
}