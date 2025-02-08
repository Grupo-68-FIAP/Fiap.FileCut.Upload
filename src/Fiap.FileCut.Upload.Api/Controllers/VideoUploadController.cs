using Fiap.FileCut.Core.Interfaces.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.FileCut.Upload.Api.Controllers
{
    [Authorize]
    [Route("api/v1")]
    [ApiController]
    public class VideoUploadController : ControllerBase
    {
        private readonly IUploadApplication _uploadApplication;

        public VideoUploadController(IUploadApplication uploadApplication)
        {
            this._uploadApplication = uploadApplication;
        }

        [HttpPost("upload")]
        [Produces("application/json")]
        public async Task<IActionResult> UploadVideo(IFormFile file)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            ArgumentNullException.ThrowIfNull(userId);

            try
            {
                bool uploaded = await _uploadApplication.UploadFileAsync(new Guid(userId), file.FileName, file.OpenReadStream(), CancellationToken.None);
                return Ok(new { Success = uploaded });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}