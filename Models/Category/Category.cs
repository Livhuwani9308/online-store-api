namespace online_store_api.Models.Category
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
