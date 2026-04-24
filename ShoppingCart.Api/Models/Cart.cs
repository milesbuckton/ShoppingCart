namespace ShoppingCart.Api.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public List<CartItem> Items { get; set; } = [];
    }
}
