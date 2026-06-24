namespace GroceryEcommerceApi.DTOs;

public class CreateOrderRequestDto
{
    public int UserId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
