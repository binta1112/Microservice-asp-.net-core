namespace MicroServicePanier.Model
{
    public class Cart
    {
        public int UserId { get; set; } 
        public List<CartItem> Items { get; set; } = new();
    }
}
