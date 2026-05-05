using Microsoft.EntityFrameworkCore;
using online_store_api.Data;
using online_store_api.Models.Product;
using online_store_api.Services.Interfaces;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace online_store_api.Services
{
    public class MediaService(IConfiguration config, SirvAuthService authService, AppDbContext _db) : IMediaService
    {
        private readonly IConfiguration _config = config;
        private readonly HttpClient _httpClient = new();
        private readonly SirvAuthService _authService = authService;


        // ----------------------------- Property Images ------------------------------
        public async Task<List<ProductImage>> UploadMediaAsync(int id, IFormFileCollection files)
        {
            var sirvFolder = $"online-store/products/{id}";
            var uploadedMedia = new List<ProductImage>();
            var token = await _authService.GetAccessTokenAsync();

            foreach (var file in files)
            {
                // Check for existing media with same PropertyId and FileName
                bool exists = await _db.ProductImages
                    .AnyAsync(m => m.ProductId == id && m.FileName.ToLower() == file.FileName.ToLower());

                if (exists)
                {
                    // Skip duplicates (optional: you can also choose to throw instead)
                    continue;
                }

                // Upload to Sirv
                var sirvUploadUrl = $"{_config["SirvSettings:BaseUrl"]}/files/upload?filename=/{sirvFolder}/{file.FileName}";
                using var stream = file.OpenReadStream();
                using var content = new StreamContent(stream);
                content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                var request = new HttpRequestMessage(HttpMethod.Post, sirvUploadUrl)
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
                    Content = content
                };

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Upload failed for {file.FileName}: {response.StatusCode}");
                }

                // Create new record
                var media = new ProductImage
                {
                    ProductId = id,
                    FileName = file.FileName,
                    FileType = file.ContentType,
                    FileUrl = $"{_config["SirvSettings:CdnUrl"]}/{sirvFolder}/{file.FileName}"
                };

                _db.ProductImages.Add(media);
                uploadedMedia.Add(media);
            }

            // Save only new records
            if (uploadedMedia.Any())
                await _db.SaveChangesAsync();

            return uploadedMedia;
        }

        public async Task<List<ProductImage>> GetMediaByProductIdAsync(int id)
        {
            // Retrieve directly from DB (faster)
            var mediaFromDb = await _db.ProductImages
                .Where(m => m.ProductId == id)
                .OrderByDescending(m => m.CreatedOn)
                .ToListAsync();

            // If DB empty, fallback to Sirv listing
            if (mediaFromDb.Count > 0)
                return mediaFromDb;

            var sirvFolder = $"online-store/products/{id}";
            var token = await _authService.GetAccessTokenAsync();
            var sirvListUrl = $"{_config["SirvSettings:BaseUrl"]}/files/readdir?dirname=/{sirvFolder}";

            var request = new HttpRequestMessage(HttpMethod.Get, sirvListUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return [];

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("contents", out var contents))
                return new List<ProductImage>();

            var mediaList = new List<ProductImage>();
            foreach (var item in contents.EnumerateArray())
            {
                if (item.GetProperty("type").GetString() == "file")
                {
                    var fileName = item.GetProperty("name").GetString();
                    mediaList.Add(new ProductImage
                    {
                        ProductId = id,
                        FileName = fileName,
                        FileUrl = $"{_config["SirvSettings:CdnUrl"]}/{sirvFolder}/{fileName}",
                        CreatedOn = DateTime.UtcNow
                    });
                }
            }

            // Cache Sirv results in DB
            _db.ProductImages.AddRange(mediaList);
            await _db.SaveChangesAsync();

            return mediaList;
        }

        public async Task<bool> DeleteMediaAsync(int id, string fileName)
        {
            var sirvFolder = $"online-store/products/{id}";
            var token = await _authService.GetAccessTokenAsync();
            var sirvDeleteUrl = $"{_config["SirvSettings:BaseUrl"]}/files/delete?filename=/{sirvFolder}/{fileName}";

            var request = new HttpRequestMessage(HttpMethod.Delete, sirvDeleteUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.NotFound)
                response.EnsureSuccessStatusCode();

            // Remove from DB
            var media = await _db.ProductImages
                .FirstOrDefaultAsync(m => m.ProductId == id && m.FileName == fileName);
            if (media != null)
            {
                _db.ProductImages.Remove(media);
                await _db.SaveChangesAsync();
            }

            return true;
        }

        // -------------------- Category Thumbnail --------------------
        public async Task<string> UploadCategoryThumbnailAsync(int categoryId, IFormFile file)
        {
            var sirvFolder = $"online-store/categories/{categoryId}";
            var token = await _authService.GetAccessTokenAsync();

            var sirvUploadUrl =
                $"{_config["SirvSettings:BaseUrl"]}/files/upload?filename=/{sirvFolder}/{file.FileName}";

            using var stream = file.OpenReadStream();
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            var request = new HttpRequestMessage(HttpMethod.Post, sirvUploadUrl)
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
                Content = content
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var url =
                $"{_config["SirvSettings:CdnUrl"]}/{sirvFolder}/{file.FileName}";

            // Save to Category table
            var category = await _db.Categories.FindAsync(categoryId);
            category.ThumbnailUrl = url;

            await _db.SaveChangesAsync();

            return url;
        }

        public async Task<bool> DeleteCategoryThumbnailAsync(int categoryId)
        {
            var category = await _db.Categories.FindAsync(categoryId);
            if (category == null || string.IsNullOrEmpty(category.ThumbnailUrl))
                return false;

            var fileName = Path.GetFileName(category.ThumbnailUrl);
            var sirvFolder = $"online-store/categories/{categoryId}";
            var token = await _authService.GetAccessTokenAsync();

            var deleteUrl = $"{_config["SirvSettings:BaseUrl"]}/files/delete?filename=/{sirvFolder}/{fileName}";

            var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await _httpClient.SendAsync(request);

            category.ThumbnailUrl = null;
            await _db.SaveChangesAsync();

            return true;
        }

        // -------------------------- Tenant Documents ---------------------------
        //public async Task<List<TenantDocument>> UploadTenantDocumentsAsync(int tenantId, IFormFileCollection files)
        //{
        //    var sirvFolder = $"TMS/documents/tenants/{tenantId}";
        //    var uploadedDocs = new List<TenantDocument>();
        //    var token = await _authService.GetAccessTokenAsync();

        //    foreach (var file in files)
        //    {
        //        // ✅ Allowed file types
        //        var allowedExtensions = new[] { ".pdf", ".docx", ".jpg", ".jpeg", ".png" };
        //        var extension = Path.GetExtension(file.FileName).ToLower();

        //        if (!allowedExtensions.Contains(extension))
        //            continue; // Skip invalid file types

        //        // 🔍 Skip duplicates
        //        bool exists = await _db.TenantDocuments
        //            .AnyAsync(d => d.TenantId == tenantId && d.FileName.ToLower() == file.FileName.ToLower());
        //        if (exists) continue;

        //        // 📤 Upload to Sirv
        //        var sirvUploadUrl = $"{_config["SirvSettings:BaseUrl"]}/files/upload?filename=/{sirvFolder}/{file.FileName}";
        //        using var stream = file.OpenReadStream();
        //        using var content = new StreamContent(stream);
        //        content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        //        var request = new HttpRequestMessage(HttpMethod.Post, sirvUploadUrl)
        //        {
        //            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
        //            Content = content
        //        };

        //        var response = await _httpClient.SendAsync(request);
        //        if (!response.IsSuccessStatusCode)
        //            throw new Exception($"Upload failed for {file.FileName}: {response.StatusCode}");

        //        // 🧾 Save in DB
        //        var document = new TenantDocument
        //        {
        //            TenantId = tenantId,
        //            FileName = file.FileName,
        //            FileType = file.ContentType,
        //            FileUrl = $"{_config["SirvSettings:CdnUrl"]}/{sirvFolder}/{file.FileName}",
        //            UploadedOn = DateTime.UtcNow
        //        };

        //        _db.TenantDocuments.Add(document);
        //        uploadedDocs.Add(document);
        //    }

        //    if (uploadedDocs.Any())
        //        await _db.SaveChangesAsync();

        //    return uploadedDocs;
        //}

        //public async Task<List<TenantDocument>> GetTenantDocumentsAsync(int tenantId)
        //{
        //    var docsFromDb = await _db.TenantDocuments
        //        .Where(d => d.TenantId == tenantId)
        //        .OrderByDescending(d => d.UploadedOn)
        //        .ToListAsync();

        //    if (docsFromDb.Count > 0)
        //        return docsFromDb;

        //    var sirvFolder = $"TMS/documents/tenants/{tenantId}";
        //    var token = await _authService.GetAccessTokenAsync();
        //    var sirvListUrl = $"{_config["SirvSettings:BaseUrl"]}/files/readdir?dirname=/{sirvFolder}";

        //    var request = new HttpRequestMessage(HttpMethod.Get, sirvListUrl);
        //    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //    var response = await _httpClient.SendAsync(request);
        //    if (response.StatusCode == HttpStatusCode.NotFound)
        //        return [];

        //    response.EnsureSuccessStatusCode();
        //    var json = await response.Content.ReadAsStringAsync();
        //    using var doc = JsonDocument.Parse(json);

        //    if (!doc.RootElement.TryGetProperty("contents", out var contents))
        //        return new List<TenantDocument>();

        //    var documentList = new List<TenantDocument>();
        //    foreach (var item in contents.EnumerateArray())
        //    {
        //        if (item.GetProperty("type").GetString() == "file")
        //        {
        //            var fileName = item.GetProperty("name").GetString();
        //            documentList.Add(new TenantDocument
        //            {
        //                TenantId = tenantId,
        //                FileName = fileName,
        //                FileUrl = $"{_config["SirvSettings:CdnUrl"]}/{sirvFolder}/{fileName}",
        //                UploadedOn = DateTime.UtcNow
        //            });
        //        }
        //    }

        //    _db.TenantDocuments.AddRange(documentList);
        //    await _db.SaveChangesAsync();

        //    return documentList;
        //}

        //public async Task<bool> DeleteTenantDocumentAsync(int tenantId, string fileName)
        //{
        //    var sirvFolder = $"TMS/documents/tenants/{tenantId}";
        //    var token = await _authService.GetAccessTokenAsync();
        //    var sirvDeleteUrl = $"{_config["SirvSettings:BaseUrl"]}/files/delete?filename=/{sirvFolder}/{fileName}";

        //    var request = new HttpRequestMessage(HttpMethod.Delete, sirvDeleteUrl);
        //    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //    var response = await _httpClient.SendAsync(request);
        //    if (response.StatusCode != HttpStatusCode.NotFound)
        //        response.EnsureSuccessStatusCode();

        //    var doc = await _db.TenantDocuments
        //        .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.FileName == fileName);
        //    if (doc != null)
        //    {
        //        _db.TenantDocuments.Remove(doc);
        //        await _db.SaveChangesAsync();
        //    }

        //    return true;
        //}








    }
    //public async Task<List<Media>> UploadMediaAsync(int id, IFormFileCollection files)
    //{
    //    var sirvFolder = $"TMS/properties/{id}";
    //    var uploadedMedia = new List<Media>();
    //    var token = await _authService.GetAccessTokenAsync();

    //    foreach (var file in files)
    //    {
    //        //var sirvUploadUrl = $"{_config["SirvSettings:BaseUrl"]}/files/upload?filename=/{sirvFolder}/{file.FileName}";
    //        var sirvUploadUrl = $"{_config["SirvSettings:BaseUrl"]}/files/upload?filename=/{sirvFolder}/{file.FileName}";
    //        using var stream = file.OpenReadStream();
    //        using var content = new StreamContent(stream);
    //        content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

    //        var request = new HttpRequestMessage(HttpMethod.Post, sirvUploadUrl);
    //        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    //        request.Content = content;

    //        var response = await _httpClient.SendAsync(request);
    //        if (!response.IsSuccessStatusCode)
    //            throw new Exception($"Upload failed for {file.FileName}: {response.StatusCode}");

    //        var media = new Media
    //        {
    //            PropertyId = id,
    //            Name = file.FileName,
    //            Type = file.ContentType,
    //            WebUrl = $"{_config["SirvSettings:CdnUrl"]}/{sirvFolder}/{file.FileName}"
    //        };

    //        _db.Media.Add(media);
    //        uploadedMedia.Add(media);
    //    }

    //    await _db.SaveChangesAsync();
    //    return uploadedMedia;
    //}
}
