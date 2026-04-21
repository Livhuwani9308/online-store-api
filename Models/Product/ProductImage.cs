namespace online_store_api.Models.Product
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
