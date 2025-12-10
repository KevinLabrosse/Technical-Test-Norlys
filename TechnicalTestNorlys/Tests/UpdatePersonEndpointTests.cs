using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechnicalTestNorlys.API;
using TechnicalTestNorlys.Entities;
using TechnicalTestNorlys.Handlers;
using TechnicalTestNorlys.Models;
using TechnicalTestNorlys.Repositories;
using Xunit;

namespace TechnicalTestNorlys.Tests;

public class UpdatePersonEndpointTests
{
    private readonly Mock<ILogger<UpdatePersonEndpoint>> _mockLogger;
    private readonly Mock<ILogger<PersonHandler>> _mockHandlerLogger;
    private readonly Mock<IPersonRepository> _mockRepository;
    private readonly PersonHandler _handler;
    private readonly UpdatePersonEndpoint _controller;

    public UpdatePersonEndpointTests()
    {
        _mockLogger = new Mock<ILogger<UpdatePersonEndpoint>>();
        _mockHandlerLogger = new Mock<ILogger<PersonHandler>>();
        _mockRepository = new Mock<IPersonRepository>();
        _handler = new PersonHandler(_mockHandlerLogger.Object, _mockRepository.Object);
        _controller = new UpdatePersonEndpoint(_mockLogger.Object, _handler);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Should_ReturnOk_When_PersonIsUpdatedSuccessfully()
    {
        // Arrange
        var request = new UpdatePersonRequest
        {
            FirstName = "UpdatedJohn"
        };

        var updatedPerson = new Person
        {
            Id = 1,
            FirstName = "UpdatedJohn",
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            OfficeId = 1,
            Office = new Office { Id = 1, Location = "Silkeborg", Capacity = 5 }
        };

        _mockRepository
            .Setup(r => r.UpdatePersonAsync(1, It.IsAny<UpdatePersonRequest>()))
            .ReturnsAsync(updatedPerson);

        // Act
        var result = await _controller.UpdatePerson(1, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var firstNameProperty = okResult.Value?.GetType().GetProperty("FirstName");
        Assert.Equal("UpdatedJohn", firstNameProperty?.GetValue(okResult.Value));
    }

    [Fact]
    public async Task Should_ReturnOk_When_OnlyLastNameIsUpdated()
    {
        // Arrange
        var request = new UpdatePersonRequest
        {
            LastName = "NewLastName"
        };

        var updatedPerson = new Person
        {
            Id = 1,
            FirstName = "John",
            LastName = "NewLastName",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            OfficeId = 1,
            Office = new Office { Id = 1, Location = "Silkeborg", Capacity = 5 }
        };

        _mockRepository
            .Setup(r => r.UpdatePersonAsync(1, It.IsAny<UpdatePersonRequest>()))
            .ReturnsAsync(updatedPerson);

        // Act
        var result = await _controller.UpdatePerson(1, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var lastNameProperty = okResult.Value?.GetType().GetProperty("LastName");
        Assert.Equal("NewLastName", lastNameProperty?.GetValue(okResult.Value));
    }

    [Fact]
    public async Task Should_ReturnOk_When_OfficeIsUpdated()
    {
        // Arrange
        var request = new UpdatePersonRequest
        {
            Office = "Aalborg"
        };

        var updatedPerson = new Person
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            OfficeId = 3,
            Office = new Office { Id = 3, Location = "Aalborg", Capacity = 50 }
        };

        _mockRepository
            .Setup(r => r.UpdatePersonAsync(1, It.IsAny<UpdatePersonRequest>()))
            .ReturnsAsync(updatedPerson);

        // Act
        var result = await _controller.UpdatePerson(1, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var officeProperty = okResult.Value?.GetType().GetProperty("Office");
        Assert.Equal("Aalborg", officeProperty?.GetValue(okResult.Value));
    }

    [Fact]
    public async Task Should_CallRepositoryWithCorrectIdAndRequest_When_UpdateIsCalled()
    {
        // Arrange
        var request = new UpdatePersonRequest
        {
            FirstName = "UpdatedName"
        };

        var updatedPerson = new Person
        {
            Id = 5,
            FirstName = "UpdatedName",
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            OfficeId = 1,
            Office = new Office { Id = 1, Location = "Silkeborg", Capacity = 5 }
        };

        _mockRepository
            .Setup(r => r.UpdatePersonAsync(5, It.IsAny<UpdatePersonRequest>()))
            .ReturnsAsync(updatedPerson);

        // Act
        await _controller.UpdatePerson(5, request);

        // Assert
        _mockRepository.Verify(r => r.UpdatePersonAsync(5, It.Is<UpdatePersonRequest>(
            req => req.FirstName == "UpdatedName")), Times.Once);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task Should_ReturnNotFound_When_PersonDoesNotExist()
    {
        // Arrange
        var request = new UpdatePersonRequest
        {
            FirstName = "UpdatedName"
        };

        _mockRepository
            .Setup(r => r.UpdatePersonAsync(999, It.IsAny<UpdatePersonRequest>()))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _controller.UpdatePerson(999, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("999", notFoundResult.Value?.ToString());
    }

    #endregion

    #region Validation Failure Tests

    [Fact]
    public async Task Should_ReturnBadRequest_When_LastNameContainsWhitespace()
    {
        // Arrange
        var request = new UpdatePersonRequest
        {
            LastName = "Doe Smith" // Contains whitespace - invalid
        };

        // Act
        var result = await _controller.UpdatePerson(1, request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockRepository.Verify(r => r.UpdatePersonAsync(It.IsAny<int>(), It.IsAny<UpdatePersonRequest>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_FirstNameExceeds50Characters()
    {
        // Arrange
        var request = new UpdatePersonRequest
        {
            FirstName = new string('A', 51) // 51 characters - exceeds limit
        };

        // Act
        var result = await _controller.UpdatePerson(1, request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockRepository.Verify(r => r.UpdatePersonAsync(It.IsAny<int>(), It.IsAny<UpdatePersonRequest>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_DateOfBirthMakesPersonTooYoung()
    {
        // Arrange - Person would be only 10 years old
        var request = new UpdatePersonRequest
        {
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-10))
        };

        // Act
        var result = await _controller.UpdatePerson(1, request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockRepository.Verify(r => r.UpdatePersonAsync(It.IsAny<int>(), It.IsAny<UpdatePersonRequest>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_DateOfBirthMakesPersonTooOld()
    {
        // Arrange - Person would be 150 years old
        var request = new UpdatePersonRequest
        {
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-150))
        };

        // Act
        var result = await _controller.UpdatePerson(1, request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockRepository.Verify(r => r.UpdatePersonAsync(It.IsAny<int>(), It.IsAny<UpdatePersonRequest>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_OfficeExceeds100Characters()
    {
        // Arrange
        var request = new UpdatePersonRequest
        {
            Office = new string('A', 101) // 101 characters - exceeds limit
        };

        // Act
        var result = await _controller.UpdatePerson(1, request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockRepository.Verify(r => r.UpdatePersonAsync(It.IsAny<int>(), It.IsAny<UpdatePersonRequest>()), Times.Never);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task Should_ReturnBadRequest_When_OfficeDoesNotExist()
    {
        // Arrange
        var request = new UpdatePersonRequest
        {
            Office = "NonExistentOffice"
        };

        _mockRepository
            .Setup(r => r.UpdatePersonAsync(1, It.IsAny<UpdatePersonRequest>()))
            .ThrowsAsync(new InvalidOperationException("Office with location 'NonExistentOffice' does not exist."));

        // Act
        var result = await _controller.UpdatePerson(1, request);

        // Assert - Controller catches InvalidOperationException and returns BadRequest
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("NonExistentOffice", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task Should_Return500_When_UnexpectedExceptionOccurs()
    {
        // Arrange
        var request = new UpdatePersonRequest
        {
            FirstName = "UpdatedName"
        };

        _mockRepository
            .Setup(r => r.UpdatePersonAsync(1, It.IsAny<UpdatePersonRequest>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.UpdatePerson(1, request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    #endregion

    #region Partial Update (PATCH) Behavior Tests

    [Fact]
    public async Task Should_AllowEmptyRequest_When_NoFieldsNeedUpdate()
    {
        // Arrange - Empty request (all fields null) should be valid for PATCH
        var request = new UpdatePersonRequest();

        var existingPerson = new Person
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            OfficeId = 1,
            Office = new Office { Id = 1, Location = "Silkeborg", Capacity = 5 }
        };

        _mockRepository
            .Setup(r => r.UpdatePersonAsync(1, It.IsAny<UpdatePersonRequest>()))
            .ReturnsAsync(existingPerson);

        // Act
        var result = await _controller.UpdatePerson(1, request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Should_ReturnOk_When_MultipleFieldsAreUpdated()
    {
        // Arrange
        var request = new UpdatePersonRequest
        {
            FirstName = "NewFirstName",
            LastName = "NewLastName",
            Office = "Esbjerg"
        };

        var updatedPerson = new Person
        {
            Id = 1,
            FirstName = "NewFirstName",
            LastName = "NewLastName",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            OfficeId = 2,
            Office = new Office { Id = 2, Location = "Esbjerg", Capacity = 10 }
        };

        _mockRepository
            .Setup(r => r.UpdatePersonAsync(1, It.IsAny<UpdatePersonRequest>()))
            .ReturnsAsync(updatedPerson);

        // Act
        var result = await _controller.UpdatePerson(1, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var firstNameProperty = okResult.Value?.GetType().GetProperty("FirstName");
        var lastNameProperty = okResult.Value?.GetType().GetProperty("LastName");
        var officeProperty = okResult.Value?.GetType().GetProperty("Office");
        
        Assert.Equal("NewFirstName", firstNameProperty?.GetValue(okResult.Value));
        Assert.Equal("NewLastName", lastNameProperty?.GetValue(okResult.Value));
        Assert.Equal("Esbjerg", officeProperty?.GetValue(okResult.Value));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Should_ReturnOk_When_FirstNameIsExactly50Characters()
    {
        // Arrange
        var request = new UpdatePersonRequest
        {
            FirstName = new string('A', 50) // Exactly 50 characters - valid
        };

        var updatedPerson = new Person
        {
            Id = 1,
            FirstName = new string('A', 50),
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            OfficeId = 1,
            Office = new Office { Id = 1, Location = "Silkeborg", Capacity = 5 }
        };

        _mockRepository
            .Setup(r => r.UpdatePersonAsync(1, It.IsAny<UpdatePersonRequest>()))
            .ReturnsAsync(updatedPerson);

        // Act
        var result = await _controller.UpdatePerson(1, request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    #endregion
}

