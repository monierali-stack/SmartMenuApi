using Microsoft.EntityFrameworkCore;
using SmartMenuApi.Data;
using SmartMenuApi.Models;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=smartmenu.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

// Test endpoint
app.MapGet("/", () => "Smart Menu API is running! 🍕");

// POST /api/orders - Create Order
app.MapPost("/api/orders", async (AppDbContext db, OrderRequest req) =>
{
    try
    {
        var errors = new Dictionary<string, string[]>();

        // Validation
        if (string.IsNullOrWhiteSpace(req.customerName) || req.customerName.Trim().Length < 3)
            errors["customerName"] = new[] { "الاسم لازم يكون 3 حروف على الأقل" };

        if (string.IsNullOrWhiteSpace(req.phoneNumber) || !Regex.IsMatch(req.phoneNumber.Trim(), @"^01\d{9}$"))
            errors["phoneNumber"] = new[] { "رقم الموبايل لازم يبدأ بـ 01 ويتكون من 11 رقم" };

        if (string.IsNullOrWhiteSpace(req.address) || req.address.Trim().Length < 6)
            errors["address"] = new[] { "اكتب عنوان واضح" };

        if (req.items == null || req.items.Count == 0)
            errors["items"] = new[] { "السلة فارغة" };

        if (errors.Count > 0)
            return Results.ValidationProblem(errors, statusCode: 422);

        var order = new Order
        {
            CustomerName = req.customerName.Trim(),
            CustomerPhone = req.phoneNumber.Trim(),
            CustomerAddress = req.address.Trim(),
            Currency = "SAR",
            Subtotal = req.totalAmount / 1.15,
            Tax = req.totalAmount - (req.totalAmount / 1.15),
            Total = req.totalAmount,
            CreatedAt = DateTime.UtcNow,
            Items = req.items.Select(i => new OrderItem
            {
                ItemId = i.itemName,
                ItemName = i.itemName,
                Price = i.price,
                Qty = i.quantity
            }).ToList()
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return Results.Ok(new { ok = true, orderId = order.Id, message = "تم استلام طلبك بنجاح!" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return Results.Problem("حدث خطأ أثناء حفظ الطلب");
    }
});

// GET /api/orders - Get all orders
app.MapGet("/api/orders", async (AppDbContext db) =>
{
    try
    {
        await db.Database.EnsureCreatedAsync();

        var orders = await db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new
            {
                orderId = o.Id,
                customerName = o.CustomerName,
                phoneNumber = o.CustomerPhone,
                address = o.CustomerAddress,
                totalAmount = o.Total,
                orderDate = o.CreatedAt,
                items = o.Items.Select(item => new
                {
                    itemName = item.ItemName,
                    price = item.Price,
                    quantity = item.Qty
                }).ToList()
            })
            .ToListAsync();

        return Results.Ok(orders);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return Results.Ok(new List<object>());
    }
});

// GET /api/orders/{id}
app.MapGet("/api/orders/{id}", async (int id, AppDbContext db) =>
{
    try
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .Where(o => o.Id == id)
            .Select(o => new
            {
                orderId = o.Id,
                customerName = o.CustomerName,
                phoneNumber = o.CustomerPhone,
                address = o.CustomerAddress,
                totalAmount = o.Total,
                orderDate = o.CreatedAt,
                items = o.Items.Select(item => new
                {
                    itemName = item.ItemName,
                    price = item.Price,
                    quantity = item.Qty
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (order == null)
            return Results.NotFound(new { ok = false, message = "الطلب غير موجود" });

        return Results.Ok(order);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return Results.NotFound(new { ok = false, message = "حدث خطأ" });
    }
});

Console.WriteLine("Starting application...");
app.Run();
Console.WriteLine("Application stopped.");

// DTO
public class OrderRequest
{
    public string customerName { get; set; } = "";
    public string phoneNumber { get; set; } = "";
    public string address { get; set; } = "";
    public double totalAmount { get; set; }
    public List<OrderItemRequest> items { get; set; } = new();
}

public class OrderItemRequest
{
    public string itemName { get; set; } = "";
    public double price { get; set; }
    public int quantity { get; set; }
}
