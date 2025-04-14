﻿using ECommerceNetApp.Domain.Entities;

namespace ECommerceNetApp.Persistence.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();

        Task<Category?> GetByIdAsync(int id);

        Task<Category> AddAsync(Category category);

        Task UpdateAsync(Category category);

        Task DeleteAsync(int id);
    }
}
