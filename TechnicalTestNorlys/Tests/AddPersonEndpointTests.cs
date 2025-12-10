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

public class AddPersonEndpointTests
{
    private readonly Mock<ILogger<AddPersonEndpoint>> _mockLogger;
    private readonly Mock<ILogger<PersonHandler>> _mockHandlerLogger;
    private readonly Mock<IPersonRepository> _mockRepository;
    private readonly PersonHandler _handler;
    private readonly AddPersonEndpoint _controller;

    public AddPersonEndpointTests()
    {
        _mockLogger = new Mock<ILogger<AddPersonEndpoint>>();
        _mockHandlerLogger = new Mock<ILogger<PersonHandler>>();
        _mockRepository = new Mock<IPersonRepository>();
        _handler = new PersonHandler(_mockHandlerLogger.Object, _mockRepository.Object);
        _controller = new AddPersonEndpoint(_mockLogger.Object, _handler);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Should_ReturnOk_When_ValidPersonIsAdded()
    {
        // Arrange
        var request = new AddPersonRequest
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            Office = "Silkeborg"
        };

        var expectedPerson = new Person
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = request.DateOfBirth,
            OfficeId = 1,
            Office = new Office { Id = 1, Location = "Silkeborg", Capacity = 5 }
        };

        _mockRepository
            .Setup(r => r.AddPersonAsync(It.IsAny<AddPersonRequest>()))
            .ReturnsAsync(expectedPerson);

        // Act
        var result = await _controller.AddPerson(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPerson = Assert.IsType<Person>(okResult.Value);
        Assert.Equal(expectedPerson.Id, returnedPerson.Id);
        Assert.Equal(expectedPerson.FirstName, returnedPerson.FirstName);
        Assert.Equal(expectedPerson.LastName, returnedPerson.LastName);
        Assert.Equal(expectedPerson.Office.Location, returnedPerson.Office.Location);
    }

    [Fact]
    public async Task Should_CallRepositoryOnce_When_ValidRequestIsProvided()
    {
        // Arrange
        var request = new AddPersonRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
            Office = "Aalborg"
        };

        var expectedPerson = new Person
        {
            Id = 2,
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = request.DateOfBirth,
            OfficeId = 3,
            Office = new Office { Id = 3, Location = "Aalborg", Capacity = 50 }
        };

        _mockRepository
            .Setup(r => r.AddPersonAsync(It.IsAny<AddPersonRequest>()))
            .ReturnsAsync(expectedPerson);

        // Act
        await _controller.AddPerson(request);

        // Assert
        _mockRepository.Verify(r => r.AddPersonAsync(It.Is<AddPersonRequest>(
            req => req.FirstName == "Jane" && req.LastName == "Smith")), Times.Once);
    }

    #endregion

    #region Validation Failure Tests

    [Fact]
    public async Task Should_ReturnBadRequest_When_FirstNameIsEmpty()
    {
        // Arrange
        var request = new AddPersonRequest
        {
            FirstName = "",
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            Office = "Silkeborg"
        };

        // Act
        var result = await _controller.AddPerson(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        
        // Verify repository was never called due to validation failure
        _mockRepository.Verify(r => r.AddPersonAsync(It.IsAny<AddPersonRequest>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_LastNameIsEmpty()
    {
        // Arrange
        var request = new AddPersonRequest
        {
            FirstName = "John",
            LastName = "",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            Office = "Silkeborg"
        };

        // Act
        var result = await _controller.AddPerson(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        _mockRepository.Verify(r => r.AddPersonAsync(It.IsAny<AddPersonRequest>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_LastNameContainsWhitespace()
    {
        // Arrange
        var request = new AddPersonRequest
        {
            FirstName = "John",
            LastName = "Doe Smith", // Contains whitespace - should fail
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            Office = "Silkeborg"
        };

        // Act
        var result = await _controller.AddPerson(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        _mockRepository.Verify(r => r.AddPersonAsync(It.IsAny<AddPersonRequest>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_PersonIsTooYoung()
    {
        // Arrange - Person is only 10 years old (must be at least 13)
        var request = new AddPersonRequest
        {
            FirstName = "Young",
            LastName = "Person",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-10)),
            Office = "Silkeborg"
        };

        // Act
        var result = await _controller.AddPerson(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        _mockRepository.Verify(r => r.AddPersonAsync(It.IsAny<AddPersonRequest>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_PersonIsTooOld()
    {
        // Arrange - Person is 150 years old (max is 120)
        var request = new AddPersonRequest
        {
            FirstName = "Ancient",
            LastName = "Person",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-150)),
            Office = "Silkeborg"
        };

        // Act
        var result = await _controller.AddPerson(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        _mockRepository.Verify(r => r.AddPersonAsync(It.IsAny<AddPersonRequest>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_OfficeIsEmpty()
    {
        // Arrange
        var request = new AddPersonRequest
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            Office = ""
        };

        // Act
        var result = await _controller.AddPerson(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        _mockRepository.Verify(r => r.AddPersonAsync(It.IsAny<AddPersonRequest>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_FirstNameExceeds50Characters()
    {
        // Arrange
        var request = new AddPersonRequest
        {
            FirstName = new string('A', 51), // 51 characters - exceeds limit
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            Office = "Silkeborg"
        };

        // Act
        var result = await _controller.AddPerson(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        _mockRepository.Verify(r => r.AddPersonAsync(It.IsAny<AddPersonRequest>()), Times.Never);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task Should_Return500_When_RepositoryThrowsException()
    {
        // Arrange
        var request = new AddPersonRequest
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            Office = "Silkeborg"
        };

        _mockRepository
            .Setup(r => r.AddPersonAsync(It.IsAny<AddPersonRequest>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.AddPerson(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task Should_Return500_When_OfficeDoesNotExist()
    {
        // Arrange
        var request = new AddPersonRequest
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            Office = "NonExistentOffice"
        };

        _mockRepository
            .Setup(r => r.AddPersonAsync(It.IsAny<AddPersonRequest>()))
            .ThrowsAsync(new InvalidOperationException("Office with location 'NonExistentOffice' does not exist."));

        // Act
        var result = await _controller.AddPerson(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task Should_ReturnOk_When_PersonIsExactly13YearsOld()
    {
        // Arrange - Person is exactly 13 years and 1 day old (just valid)
        var request = new AddPersonRequest
        {
            FirstName = "Teen",
            LastName = "Worker",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-13).AddDays(-1)),
            Office = "Silkeborg"
        };

        var expectedPerson = new Person
        {
            Id = 1,
            FirstName = "Teen",
            LastName = "Worker",
            DateOfBirth = request.DateOfBirth,
            OfficeId = 1,
            Office = new Office { Id = 1, Location = "Silkeborg", Capacity = 5 }
        };

        _mockRepository
            .Setup(r => r.AddPersonAsync(It.IsAny<AddPersonRequest>()))
            .ReturnsAsync(expectedPerson);

        // Act
        var result = await _controller.AddPerson(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Should_ReturnOk_When_FirstNameIsExactly50Characters()
    {
        // Arrange
        var request = new AddPersonRequest
        {
            FirstName = new string('A', 50), // Exactly 50 characters - valid
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            Office = "Silkeborg"
        };

        var expectedPerson = new Person
        {
            Id = 1,
            FirstName = request.FirstName,
            LastName = "Doe",
            DateOfBirth = request.DateOfBirth,
            OfficeId = 1,
            Office = new Office { Id = 1, Location = "Silkeborg", Capacity = 5 }
        };

        _mockRepository
            .Setup(r => r.AddPersonAsync(It.IsAny<AddPersonRequest>()))
            .ReturnsAsync(expectedPerson);

        // Act
        var result = await _controller.AddPerson(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    #endregion
}

