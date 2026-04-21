using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using online_store_api.Services.Interfaces;

namespace online_store_api.Controllers
{
    public class MediaController(IMediaService _mediaService) : BaseController
    {
        [HttpPost("product/upload/{id}")]
        public async Task<IActionResult> Upload(int id, [FromForm] IFormFileCollection files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");

            var response = await _mediaService.UploadMediaAsync(id, files);

            return Ok(response);
        }

        [HttpGet("product/{id}")]
        public async Task<IActionResult> GetByProductId(int id)
        {
            var response = await _mediaService.GetMediaByProductIdAsync(id);

            return Ok(response);
        }

        [HttpDelete("product/{id}/{fileName}")]
        public async Task<IActionResult> Delete(int id, string fileName)
        {
            var response = await _mediaService.DeleteMediaAsync(id, fileName);

            return response ? Ok("Deleted successfully") : StatusCode(500, "Failed to delete media");
        }

        [HttpPost("category/upload/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadCategoryThumbnail(int id, IFormFile file)
        {
            if (file == null)
                return BadRequest("No file uploaded.");

            var result = await _mediaService.UploadCategoryThumbnailAsync(id, file);

            return Ok(new { thumbnailUrl = result });
        }

        [HttpDelete("category/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategoryThumbnail(int id)
        {
            var result = await _mediaService.DeleteCategoryThumbnailAsync(id);

            return result ? Ok("Deleted") : NotFound();
        }

        //[HttpPost("tenant/upload/{tenantId}")]
        //public async Task<IActionResult> UploadTenantDocuments(int tenantId, [FromForm] IFormFileCollection files)
        //{
        //    if (files == null || files.Count == 0)
        //        return BadRequest("No files uploaded.");

        //    var result = await _mediaService.UploadTenantDocumentsAsync(tenantId, files);
        //    return Ok(result);
        //}

        //[HttpGet("tenant/{tenantId}")]
        //public async Task<IActionResult> GetTenantDocuments(int tenantId)
        //{
        //    var result = await _mediaService.GetTenantDocumentsAsync(tenantId);
        //    return Ok(result);
        //}

        //[HttpDelete("tenant/{tenantId}/{fileName}")]
        //public async Task<IActionResult> DeleteTenantDocument(int tenantId, string fileName)
        //{
        //    var result = await _mediaService.DeleteTenantDocumentAsync(tenantId, fileName);
        //    return result ? Ok("Deleted successfully") : StatusCode(500, "Failed to delete document");
        //}
    }
}
