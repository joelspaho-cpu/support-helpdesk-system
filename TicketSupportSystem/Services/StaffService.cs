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
        var advisor = await _roleManager.FindByNameAsync(StaffRoles.Advisor);
        if (request.Level != null && role != advisor) return IdentityResult.Failed(new IdentityError { Code = "NonAdvisorCantHaveLevel", Description = $"Only the job role {advisor.NormalizedName} may have a level"});
        
        var staff = new Staff
        {
            DisplayName = request.DisplayName,
            Level = request.Level,
            Signature = request.Signature,
            CreatedAt = DateTime.UtcNow,
            Email = request.Email,
            UserName = request.Email
        };
        await _staffManager.CreateAsync(staff, request.Password);
        await _db.SaveChangesAsync();
    }
}
