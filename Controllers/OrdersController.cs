using GroceryEcommerceApi.Data;
using GroceryEcommerceApi.DTOs;
using GroceryEcommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GroceryEcommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet("history/{userId:int}")]
    public async Task<ActionResult<IEnumerable<OrderHistoryDto>>> GetOrderHistory(int userId)
    {
        var authUserId = GetAuthUserId();
        if (authUserId != userId)
        {
            return Forbid();
        }

        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            return NotFound(new { Message = "User not found." });
        }

        var history = await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderHistoryDto
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                Items = o.Items.Select(i => new OrderHistoryItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product != null ? i.Product.Name : string.Empty,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.UnitPrice * i.Quantity
                }).ToList()
            })
            .ToListAsync();

        return Ok(history);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<OrderHistoryDto>> CreateOrder(CreateOrderRequestDto request)
    {
        var authUserId = GetAuthUserId();
        if (authUserId != request.UserId)
        {
            return Forbid();
        }

        if (request.Items.Count == 0)
        {
            return BadRequest(new { Message = "At least one item is required." });
        }

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        if (products.Count != productIds.Count)
        {
            return BadRequest(new { Message = "One or more products do not exist." });
        }

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                return BadRequest(new { Message = "Quantity must be greater than zero." });
            }

            var product = products[item.ProductId];
            if (product.StockQuantity < item.Quantity)
            {
                return BadRequest(new { Message = $"Insufficient stock for product: {product.Name}" });
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var order = new Order
        {
            UserId = request.UserId,
            Status = "Placed",
            OrderDate = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var orderItems = new List<OrderItem>();
        foreach (var item in request.Items)
        {
            var product = products[item.ProductId];
            product.StockQuantity -= item.Quantity;

            orderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        _context.OrderItems.AddRange(orderItems);
        order.TotalAmount = orderItems.Sum(i => i.UnitPrice * i.Quantity);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        var response = new OrderHistoryDto
        {
            OrderId = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Items = orderItems.Select(i => new OrderHistoryItemDto
            {
                ProductId = i.ProductId,
                ProductName = products[i.ProductId].Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.UnitPrice * i.Quantity
            }).ToList()
        };

        return CreatedAtAction(nameof(GetOrderHistory), new { userId = request.UserId }, response);
    }

    private int GetAuthUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : 0;
    }
}
