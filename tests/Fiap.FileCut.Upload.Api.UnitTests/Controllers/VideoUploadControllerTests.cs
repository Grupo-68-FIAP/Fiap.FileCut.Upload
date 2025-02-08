using Fiap.FileCut.Core.Interfaces.Applications;
using Fiap.FileCut.Upload.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.IO;

namespace Fiap.FileCut.Upload.Api.UnitTests.Controllers;

public class VideoUploadControllerTests
{
    private readonly Mock<IUploadApplication> _uploadApplicationMock;
    private readonly VideoUploadController _controller;

    public VideoUploadControllerTests()
    {
        _uploadApplicationMock = new Mock<IUploadApplication>();
        _controller = new VideoUploadController(_uploadApplicationMock.Object);
    }

    [Fact]
    public async Task UploadVideo_WhenUploadIsSuccessful_ShouldReturnOkWithSuccessTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileName = "test.mp4";
        var stream = new MemoryStream();
        var cancellationToken = CancellationToken.None;
        _controller.SetUserAuth(userId, "admin@test.com");

        // Mock do IFormFile como um Stream
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(stream.Length);

        // Mock do serviço de arquivo para salvar
        _uploadApplicationMock
            .Setup(s => s.UploadFileAsync(userId, fileName, It.IsAny<Stream>(), cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UploadVideo(fileMock.Object);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        _uploadApplicationMock.Verify(s => s.UploadFileAsync(userId, fileName, It.IsAny<Stream>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UploadVideo_WhenModelStateIsInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.mp4");
        _controller.ModelState.AddModelError("file", "Required");

        // Act
        var result = await _controller.UploadVideo(mockFile.Object);

        // Assert
        _ = Assert.IsType<BadRequestObjectResult>(result);
        _uploadApplicationMock.Verify(s => s.UploadFileAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never);
        _ = Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UploadVideo_WhenServiceThrowsException_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var fileName = "test.mp4";
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        var cancellationToken = CancellationToken.None;

        _uploadApplicationMock
            .Setup(s => s.UploadFileAsync(userId, fileName, It.IsAny<Stream>(), cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Upload failed"));
        _controller.SetUserAuth(userId, "admin@test.com");

        // Act
        var result = await _controller.UploadVideo(mockFile.Object);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult?.StatusCode);
        _uploadApplicationMock.Verify(s => s.UploadFileAsync(userId, fileName, It.IsAny<Stream>(), cancellationToken), Times.Once);
    }
}
