using Fiap.FileCut.Core.Interfaces.Services;
using Fiap.FileCut.Upload.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Fiap.FileCut.Upload.Api.UnitTests.Controllers;

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
        var fileName = "test.mp4";
        var stream = new MemoryStream();
        var cancellationToken = CancellationToken.None;

        // Mock do IFormFile como um Stream
        var fileStreamMock = new Mock<Stream>();
        fileStreamMock.Setup(f => f.Length).Returns(stream.Length);

        // Mock do serviço de arquivo para salvar
        _fileServiceMock
            .Setup(s => s.SaveFileAsync(userId, fileName, It.IsAny<Stream>(), cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UploadVideo(fileStreamMock.Object, fileName, userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        _fileServiceMock.Verify(s => s.SaveFileAsync(userId, fileName, It.IsAny<Stream>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UploadVideo_WhenModelStateIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("file", "Required");
        var userId = Guid.NewGuid();
        var fileName = "test.mp4";
        var fileStream = new Mock<Stream>();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _controller.UploadVideo(fileStream.Object, fileName, userId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        _fileServiceMock.Verify(s => s.SaveFileAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UploadVideo_WhenServiceThrowsException_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileName = "test.mp4";
        var fileStream = new Mock<Stream>();
        var cancellationToken = CancellationToken.None;

        _fileServiceMock
            .Setup(s => s.SaveFileAsync(userId, fileName, It.IsAny<Stream>(), cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Upload failed"));

        // Act
        var result = await _controller.UploadVideo(fileStream.Object, fileName, userId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        dynamic? response = badRequestResult?.Value;
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult?.StatusCode);
        _fileServiceMock.Verify(s => s.SaveFileAsync(userId, fileName, It.IsAny<Stream>(), cancellationToken), Times.Once);
    }
}