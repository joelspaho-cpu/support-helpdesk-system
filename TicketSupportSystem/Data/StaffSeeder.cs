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
    public async Task SeedAsync()
    {
        var roles = new[] { StaffRoles.Manager, StaffRoles.Advisor, StaffRoles.Engineer, StaffRoles.Admin };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole<int>(role));
                EnsureSucceeded(result, role);
            }
        }

        var managerEmail = _seedOptions.Value.Manager.Email;
        var managerName = _seedOptions.Value.Manager.DisplayName;
        var managerPassword = _seedOptions.Value.Manager.Password;

        var advisorL1Email = _seedOptions.Value.AdvisorL1.Email;
        var advisorL1Name = _seedOptions.Value.AdvisorL1.DisplayName;
        var advisorL1Password = _seedOptions.Value.AdvisorL1.Password;

        var advisorL2Email = _seedOptions.Value.AdvisorL2.Email;
        var advisorL2Name = _seedOptions.Value.AdvisorL2.DisplayName;
        var advisorL2Password = _seedOptions.Value.AdvisorL2.Password;

        var adminEmail = _seedOptions.Value.Admin.Email;
        var adminName = _seedOptions.Value.Admin.DisplayName;
        var adminPassword = _seedOptions.Value.Admin.Password;

        RequireSetting(adminEmail, "Seed:Admin:Email");
        RequireSetting(adminName, "Seed:Admin:DisplayName");
        RequireSetting(adminPassword, "Seed:Admin:Password");

        if (await _staffManager.FindByEmailAsync(adminEmail) == null)
        {
            var result = await CreateStaffAccountAsync(adminEmail, adminPassword, adminName, StaffRoles.Admin, null);
            EnsureSucceeded(result, adminEmail);
        }

        if (_seedOptions.Value.DemoData)
        {
            RequireSetting(managerEmail, "Seed:Manager:Email");
            RequireSetting(managerName, "Seed:Manager:DisplayName");
            RequireSetting(managerPassword, "Seed:Manager:Password");

            RequireSetting(advisorL1Email, "Seed:AdvisorL1:Email");
            RequireSetting(advisorL1Name, "Seed:AdvisorL1:DisplayName");
            RequireSetting(advisorL1Password, "Seed:AdvisorL1:Password");

            RequireSetting(advisorL2Email, "Seed:AdvisorL2:Email");
            RequireSetting(advisorL2Name, "Seed:AdvisorL2:DisplayName");
            RequireSetting(advisorL2Password, "Seed:AdvisorL2:Password");

            if (await _staffManager.FindByEmailAsync(managerEmail) == null)
            {
                var result = await CreateStaffAccountAsync(managerEmail, managerPassword, managerName, StaffRoles.Manager, null);
                EnsureSucceeded(result, managerEmail);
            }

            if (await _staffManager.FindByEmailAsync(advisorL1Email) == null)
            {
                var result = await CreateStaffAccountAsync(advisorL1Email, advisorL1Password, advisorL1Name, StaffRoles.Advisor, Level.Level1);
                EnsureSucceeded(result, advisorL1Email);
            }

            if (await _staffManager.FindByEmailAsync(advisorL2Email) == null)
            {
                var result = await CreateStaffAccountAsync(advisorL2Email, advisorL2Password, advisorL2Name, StaffRoles.Advisor, Level.Level2);
                EnsureSucceeded(result, advisorL2Email);
            }
        }
    }

//HELPERS
    private async Task<IdentityResult> CreateStaffAccountAsync(string email, string password, string displayName, string staffRole, Level? level)
    {
        var request = new CreateStaffRequest
        {
            DisplayName = displayName,
            Email = email,
            Password = password,
            Role = staffRole,
            Level = level
        };

        return await _staffService.CreateStaffAccountAsync(request);
    }
    private static void EnsureSucceeded(IdentityResult result, string type)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                        $"Seeding failed creating '{type}': " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
    private static void RequireSetting(string value, string key)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Missing configuration value '{key}'.");
    }
}