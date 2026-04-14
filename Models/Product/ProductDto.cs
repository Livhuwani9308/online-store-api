namespace online_store_api.Models.Product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string Color { get; set; } = string.Empty;
        public List<ProductSizeDto> Sizes { get; set; } = [];
    }
}
