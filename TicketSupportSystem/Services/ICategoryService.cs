using TicketSupportSystem.Models;

namespace TicketSupportSystem.Services;

public interface ICategoryService
{
    Task<Category?> GetCategoryAsync(int category);
    Task<List<Category>> ListCategoriesAsync();
}
