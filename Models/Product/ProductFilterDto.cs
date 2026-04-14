namespace online_store_api.Models.Product
{
    public class ProductFilterDto
    {
        public string? Search { get; set; }
        public string? Brand { get; set; }
        public string? Color { get; set; }
        public int? CategoryId { get; set; }
        public string? Size { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
