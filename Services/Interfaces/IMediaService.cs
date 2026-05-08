using online_store_api.Models.Product;

namespace online_store_api.Services.Interfaces
{
    public interface IMediaService
    {
        // -------------------- Property Images --------------------
        Task<List<ProductImage>> UploadMediaAsync(int id, IFormFileCollection files);
        Task<List<ProductImage>> GetMediaByProductIdAsync(int id);
        Task<bool> DeleteMediaAsync(int id, string fileName);

        // -------------------- Category Thumbnail --------------------
        Task<string> UploadCategoryThumbnailAsync(int categoryId, IFormFile file);
        Task<bool> DeleteCategoryThumbnailAsync(int id);

        // -------------------- User Thumbnail --------------------
        Task<string> UploadUserThumbnailAsync(int userId, IFormFile file);
        Task<bool> DeleteUserThumbnailAsync(int userId);

        // -------------------- Tenant Documents --------------------
        //Task<List<TenantDocument>> UploadTenantDocumentsAsync(int tenantId, IFormFileCollection files);
        //Task<List<TenantDocument>> GetTenantDocumentsAsync(int tenantId);
        //Task<bool> DeleteTenantDocumentAsync(int tenantId, string fileName);
    }
}
