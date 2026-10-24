using TicketSupportSystem.Models;
using TicketSupportSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace TicketSupportSystem.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;
    public CategoryService(AppDbContext db)
    {
        _db = db;
    }
    public async Task<Category?> GetCategoryAsync(int categoryId)
    {
        var result = await _db.Categories.FirstOrDefaultAsync(i => i.CategoryID == categoryId);
        if (result == null || !result.IsEnabled) return null;
        return result;
    }
    public async Task<List<Category>> ListCategoriesAsync()
    {
        return await _db.Categories
                                    .Where(c => c.IsEnabled)
                                    .OrderBy(c => c.SortOrder)
                                    .ToListAsync();
    } 
}