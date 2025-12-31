namespace SmartMenuApi.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }

    public string ItemId { get; set; } = "";
    public string ItemName { get; set; } = "";
    public double Price { get; set; }
    public int Qty { get; set; }

    public Order? Order { get; set; }
}
