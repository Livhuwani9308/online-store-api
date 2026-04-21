namespace online_store_api.Models
{
    public class TenantDocument
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty; // .pdf, .jpg, .png, .docx
        public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
    }
}
