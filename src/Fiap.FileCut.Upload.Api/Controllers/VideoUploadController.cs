using Fiap.FileCut.Upload.Api.Infra.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.FileCut.Upload.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class VideoUploadController : ControllerBase
	{
		private readonly IFileStorageService _fileStorageService;

		public VideoUploadController(IFileStorageService fileStorageService)
		{
			_fileStorageService = fileStorageService;
		}

		[HttpPost("upload")]
		[Produces("application/json")]
		public async Task<IActionResult> UploadVideo([FromForm] IFormFile file, [FromForm] Guid userId)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				var uploadedFileUrl = await _fileStorageService.UploadFileAsync(file, userId);
				return Ok(new { FileUrl = uploadedFileUrl });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { Error = ex.Message });
			}
		}
	}
}
