using Fiap.FileCut.Core.Interfaces.Services;
using Fiap.FileCut.Upload.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class VideoUploadControllerTests
{
	private readonly Mock<IFileService> _fileServiceMock;
	private readonly VideoUploadController _controller;

	public VideoUploadControllerTests()
	{
		_fileServiceMock = new Mock<IFileService>();
		_controller = new VideoUploadController(_fileServiceMock.Object);
	}

	[Fact]
	public async Task UploadVideo_WhenUploadIsSuccessful_ShouldReturnOkWithSuccessTrue()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var fileMock = new Mock<IFormFile>();
		var fileName = "test.mp4";
		var stream = new MemoryStream();
		var cancellationToken = CancellationToken.None;

		fileMock.Setup(f => f.FileName).Returns(fileName);
		fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
		fileMock.Setup(f => f.Length).Returns(stream.Length);

		_fileServiceMock
			.Setup(s => s.SaveFileAsync(userId, fileMock.Object, cancellationToken))
			.ReturnsAsync(true);

		// Act
		var result = await _controller.UploadVideo(fileMock.Object, userId);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result);
		Assert.Equal(okResult.StatusCode, StatusCodes.Status200OK);
		_fileServiceMock.Verify(s => s.SaveFileAsync(userId, fileMock.Object, cancellationToken), Times.Once);
	}

	[Fact]
	public async Task UploadVideo_WhenModelStateIsInvalid_ShouldReturnBadRequest()
	{
		// Arrange
		_controller.ModelState.AddModelError("file", "Required");
		var userId = Guid.NewGuid();
		var fileMock = new Mock<IFormFile>();
		var cancellationToken = CancellationToken.None;

		// Act
		var result = await _controller.UploadVideo(fileMock.Object, userId);

		// Assert
		var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
		_fileServiceMock.Verify(s => s.SaveFileAsync(It.IsAny<Guid>(), It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task UploadVideo_WhenServiceThrowsException_ShouldReturnBadRequest()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var fileMock = new Mock<IFormFile>();
		var cancellationToken = CancellationToken.None;

		_fileServiceMock
			.Setup(s => s.SaveFileAsync(userId, fileMock.Object, cancellationToken))
			.ThrowsAsync(new InvalidOperationException("Upload failed"));

		// Act
		var result = await _controller.UploadVideo(fileMock.Object, userId);

		// Assert
		var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
		dynamic response = badRequestResult.Value;
		Assert.Equal(badRequestResult.StatusCode, StatusCodes.Status400BadRequest);
		_fileServiceMock.Verify(s => s.SaveFileAsync(userId, fileMock.Object, cancellationToken), Times.Once);
	}
}
