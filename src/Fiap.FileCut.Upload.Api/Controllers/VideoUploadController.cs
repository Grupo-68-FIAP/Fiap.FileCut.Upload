using Fiap.FileCut.Upload.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.FileCut.Upload.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class VideoUploadController : ControllerBase
	{
		private readonly IFileRepository _fileRepository;

		public VideoUploadController(IFileRepository fileRepository)
		{
			_fileRepository = fileRepository;
		}

		[HttpPost("upload")]
		[Produces("application/json")]
		public async Task<IActionResult> UploadVideo([FromForm] IFormFile file, [FromForm] Guid userId)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			try
			{
				if (file.Length <= 0)
					throw new InvalidOperationException("File is empty.");

				var result = await _fileRepository.UpdateAsync(userId, file, CancellationToken.None);
				if (!result)
					throw new InvalidOperationException("Failed to upload the file.");

				return Ok(new { FileUrl = $"https://example.com/files/{userId}/{file.FileName}" }); 
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { Error = ex.Message });
			}
		}

		[HttpGet("download")]
		public async Task<IActionResult> DownloadFile(Guid userId, string fileName)
		{
			try
			{
				var file = await _fileRepository.GetAsync(userId, fileName, CancellationToken.None);
				if (file == null)
					return NotFound(new { Error = "File not found." });

				var filePath = Path.Combine(Directory.GetCurrentDirectory(), "LocalStorage", userId.ToString(), fileName);
				var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

				return File(fileBytes, "application/octet-stream", fileName);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { Error = ex.Message });
			}
		}
	}
}
