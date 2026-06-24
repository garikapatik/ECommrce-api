using GroceryEcommerceApi.Models;

namespace GroceryEcommerceApi.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Users.Any())
        {
            return;
        }

        var users = new List<User>
        {
            new() { FullName = "Anita Sharma", Email = "anita@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123"), Phone = "9876543210", Address = "12 Green Park, Delhi" },
            new() { FullName = "Rahul Verma", Email = "rahul@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123"), Phone = "9876501234", Address = "45 Lake View, Pune" }
        };

        var products = new List<Product>
        {
            new() { Name = "Fresh Milk 1L", Category = "Dairy", Description = "Pasteurized full cream milk", Price = 2.49m, StockQuantity = 80 },
            new() { Name = "Brown Bread", Category = "Bakery", Description = "Whole wheat bread loaf", Price = 1.99m, StockQuantity = 60 },
            new() { Name = "Banana 1kg", Category = "Fruits", Description = "Farm fresh bananas", Price = 1.50m, StockQuantity = 120 },
            new() { Name = "Tomato 1kg", Category = "Vegetables", Description = "Fresh red tomatoes", Price = 1.20m, StockQuantity = 90 },
            new() { Name = "Basmati Rice 5kg", Category = "Grains", Description = "Premium aged basmati rice", Price = 8.99m, StockQuantity = 40 }
        };

        context.Users.AddRange(users);
        context.Products.AddRange(products);
        context.SaveChanges();

        var orders = new List<Order>
        {
            new() { UserId = users[0].Id, OrderDate = DateTime.UtcNow.AddDays(-4), Status = "Delivered" },
            new() { UserId = users[1].Id, OrderDate = DateTime.UtcNow.AddDays(-1), Status = "Shipped" }
        };

        context.Orders.AddRange(orders);
        context.SaveChanges();

        var orderItems = new List<OrderItem>
        {
            new() { OrderId = orders[0].Id, ProductId = products[0].Id, Quantity = 2, UnitPrice = products[0].Price },
            new() { OrderId = orders[0].Id, ProductId = products[1].Id, Quantity = 1, UnitPrice = products[1].Price },
            new() { OrderId = orders[0].Id, ProductId = products[2].Id, Quantity = 3, UnitPrice = products[2].Price },
            new() { OrderId = orders[1].Id, ProductId = products[4].Id, Quantity = 1, UnitPrice = products[4].Price },
            new() { OrderId = orders[1].Id, ProductId = products[3].Id, Quantity = 2, UnitPrice = products[3].Price }
        };

        context.OrderItems.AddRange(orderItems);
        context.SaveChanges();

        foreach (var order in orders)
        {
            order.TotalAmount = context.OrderItems
                .Where(i => i.OrderId == order.Id)
                .Sum(i => i.UnitPrice * i.Quantity);
        }

        context.SaveChanges();
    }
}
