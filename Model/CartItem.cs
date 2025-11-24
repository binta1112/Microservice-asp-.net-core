namespace MicroServicePanier.Model
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; } = 0;
        public string ProductImage { get; set; }
    }
}
