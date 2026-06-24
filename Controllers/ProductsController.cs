using GroceryEcommerceApi.Data;
using GroceryEcommerceApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroceryEcommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResultDto<ProductDto>>> Search(
        [FromQuery] string? query,
        [FromQuery] string? category,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortDirection = "asc")
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

        var productsQuery = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalized = query.Trim().ToLower();
            productsQuery = productsQuery.Where(p =>
                p.Name.ToLower().Contains(normalized) ||
                p.Description.ToLower().Contains(normalized));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = category.Trim().ToLower();
            productsQuery = productsQuery.Where(p => p.Category.ToLower() == normalizedCategory);
        }

        if (minPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
        }

        productsQuery = (sortBy.ToLower(), sortDirection.ToLower()) switch
        {
            ("price", "desc") => productsQuery.OrderByDescending(p => p.Price),
            ("price", _) => productsQuery.OrderBy(p => p.Price),
            ("category", "desc") => productsQuery.OrderByDescending(p => p.Category),
            ("category", _) => productsQuery.OrderBy(p => p.Category),
            ("name", "desc") => productsQuery.OrderByDescending(p => p.Name),
            _ => productsQuery.OrderBy(p => p.Name)
        };

        var totalCount = await productsQuery.CountAsync();

        var results = await productsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity
            })
            .ToListAsync();

        return Ok(new PagedResultDto<ProductDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = results
        });
    }
}
