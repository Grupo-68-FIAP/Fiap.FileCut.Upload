using Fiap.FileCut.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.FileCut.Upload.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class VideoUploadController : ControllerBase
	{
		private readonly IFileService _fileStorageService;

		public VideoUploadController(IFileService fileStorageService)
		{
			_fileStorageService = fileStorageService;
		}

		[HttpPost("upload")]
		[Produces("application/json")]
		public async Task<IActionResult> UploadVideo([FromForm] Stream fileStream, [FromForm] string fileName, [FromForm] Guid userId)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				bool uploaded = await _fileStorageService.SaveFileAsync(userId, fileName, fileStream, CancellationToken.None);
				return Ok(new { Success = uploaded });
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { Error = ex.Message });
			}
		}
	}
}