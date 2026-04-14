namespace online_store_api.Models.Product
{
    public class ProductSize
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string SizeValue { get; set; } = string.Empty;
        // Examples:
        // T-Shirt -> S, M, L
        // Shoes -> 6, 7, 8
        // Pants -> 28, 30, 32
        public int StockQuantity { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
