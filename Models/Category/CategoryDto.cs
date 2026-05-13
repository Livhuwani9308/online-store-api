namespace online_store_api.Models.Category
{
    public class CategoryDto

    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
    }
}
