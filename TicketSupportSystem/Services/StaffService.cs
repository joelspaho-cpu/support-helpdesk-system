using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketSupportSystem.Data;
using TicketSupportSystem.Models;

namespace TicketSupportSystem.Services;

public class StaffService : IStaffService
{
    private readonly AppDbContext _db;
    private readonly UserManager<Staff> _staffManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    public StaffService(UserManager<Staff> staffmanager, RoleManager<IdentityRole<int>> rolemanager, AppDbContext db)
    {
        _db = db;
        _staffManager = staffmanager;
        _roleManager = rolemanager;
    }
    public async Task<IdentityResult> CreateStaffAccountAsync(CreateStaffRequest request)
    {
        var role = await _roleManager.FindByNameAsync(request.Role);
        if (role == null) return IdentityResult.Failed(new IdentityError { Code = "RoleDoesntExist", Description = "The specified job role does not exist"});
        if (request.Level != null && role.Name != StaffRoles.Advisor) return IdentityResult.Failed(new IdentityError { Code = "NonAdvisorCantHaveLevel", Description = $"Only the job role {StaffRoles.Advisor} may have a level"});
        if (request.Level == null && role.Name == StaffRoles.Advisor) return IdentityResult.Failed(new IdentityError { Code = "AdvisorMustHaveLevel", Description = $"The job role {StaffRoles.Advisor} must have a level"});
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var staff = new Staff
        {
            DisplayName = request.DisplayName,
            Level = request.Level,
            Signature = request.Signature,
            CreatedAt = DateTime.UtcNow,
            Email = request.Email,
            UserName = request.Email
        };
        var result = await _staffManager.CreateAsync(staff, request.Password);
        if (!result.Succeeded)
        {
            await transaction.RollbackAsync();
            return result;
        }
        var roleAddResult = await _staffManager.AddToRoleAsync(staff, request.Role);
        if (!roleAddResult.Succeeded)
        {
            await transaction.RollbackAsync();
            return roleAddResult;
        }
        await transaction.CommitAsync();
        return result;
    }
}
