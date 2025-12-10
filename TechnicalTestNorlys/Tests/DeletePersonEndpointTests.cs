using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechnicalTestNorlys.API;
using TechnicalTestNorlys.Handlers;
using TechnicalTestNorlys.Models;
using TechnicalTestNorlys.Repositories;
using Xunit;

namespace TechnicalTestNorlys.Tests;

public class DeletePersonEndpointTests
{
    private readonly Mock<ILogger<DeletePersonEndpoint>> _mockLogger;
    private readonly Mock<ILogger<PersonHandler>> _mockHandlerLogger;
    private readonly Mock<IPersonRepository> _mockRepository;
    private readonly PersonHandler _handler;
    private readonly DeletePersonEndpoint _controller;

    public DeletePersonEndpointTests()
    {
        _mockLogger = new Mock<ILogger<DeletePersonEndpoint>>();
        _mockHandlerLogger = new Mock<ILogger<PersonHandler>>();
        _mockRepository = new Mock<IPersonRepository>();
        _handler = new PersonHandler(_mockHandlerLogger.Object, _mockRepository.Object);
        _controller = new DeletePersonEndpoint(_mockLogger.Object, _handler);
    }

    #region Happy Path Tests

    [Fact]
    public async Task Should_ReturnOkWithDeletedPerson_When_PersonExists()
    {
        // Arrange
        var person = new Person
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            OfficeId = 1,
            Office = new Office { Id = 1, Location = "Silkeborg", Capacity = 5 }
        };

        _mockRepository
            .Setup(r => r.DeletePersonAsync(1))
            .ReturnsAsync(person);

        // Act
        var result = await _controller.DeletePerson(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        // Verify the response contains the success message
        var messageProperty = okResult.Value.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        Assert.Equal("Person deleted successfully.", messageProperty.GetValue(okResult.Value));
    }

    [Fact]
    public async Task Should_ReturnDeletedPersonDetails_When_PersonIsDeleted()
    {
        // Arrange
        var person = new Person
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            OfficeId = 1,
            Office = new Office { Id = 1, Location = "Silkeborg", Capacity = 5 }
        };

        _mockRepository
            .Setup(r => r.DeletePersonAsync(1))
            .ReturnsAsync(person);

        // Act
        var result = await _controller.DeletePerson(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var deletedPersonProperty = okResult.Value?.GetType().GetProperty("deletedPerson");
        Assert.NotNull(deletedPersonProperty);
        
        var deletedPerson = deletedPersonProperty.GetValue(okResult.Value);
        var firstNameProperty = deletedPerson?.GetType().GetProperty("FirstName");
        var lastNameProperty = deletedPerson?.GetType().GetProperty("LastName");
        var officeProperty = deletedPerson?.GetType().GetProperty("Office");
        
        Assert.Equal("John", firstNameProperty?.GetValue(deletedPerson));
        Assert.Equal("Doe", lastNameProperty?.GetValue(deletedPerson));
        Assert.Equal("Silkeborg", officeProperty?.GetValue(deletedPerson));
    }

    [Fact]
    public async Task Should_CallRepositoryWithCorrectId_When_DeleteIsCalled()
    {
        // Arrange
        var person = new Person
        {
            Id = 42,
            FirstName = "Test",
            LastName = "Person",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
            OfficeId = 1,
            Office = new Office { Id = 1, Location = "Silkeborg", Capacity = 5 }
        };

        _mockRepository
            .Setup(r => r.DeletePersonAsync(42))
            .ReturnsAsync(person);

        // Act
        await _controller.DeletePerson(42);

        // Assert
        _mockRepository.Verify(r => r.DeletePersonAsync(42), Times.Once);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task Should_ReturnNotFound_When_PersonDoesNotExist()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeletePersonAsync(999))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _controller.DeletePerson(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("999", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task Should_NotCallDeleteTwice_When_PersonDoesNotExist()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeletePersonAsync(999))
            .ReturnsAsync((Person?)null);

        // Act
        await _controller.DeletePerson(999);

        // Assert
        _mockRepository.Verify(r => r.DeletePersonAsync(999), Times.Once);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task Should_Return500_When_RepositoryThrowsException()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeletePersonAsync(1))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.DeletePerson(1);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task Should_Return500_When_RepositoryThrowsInvalidOperationException()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeletePersonAsync(1))
            .ThrowsAsync(new InvalidOperationException("Person with ID 1 does not exist."));

        // Act
        var result = await _controller.DeletePerson(1);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Should_ReturnNotFound_When_IdIsZero()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeletePersonAsync(0))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _controller.DeletePerson(0);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_IdIsNegative()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.DeletePersonAsync(-1))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _controller.DeletePerson(-1);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Should_ReturnOkWithCorrectOfficeLocation_When_PersonHasDifferentOffice()
    {
        // Arrange
        var person = new Person
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-28)),
            OfficeId = 3,
            Office = new Office { Id = 3, Location = "Aalborg", Capacity = 50 }
        };

        _mockRepository
            .Setup(r => r.DeletePersonAsync(1))
            .ReturnsAsync(person);

        // Act
        var result = await _controller.DeletePerson(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var deletedPersonProperty = okResult.Value?.GetType().GetProperty("deletedPerson");
        var deletedPerson = deletedPersonProperty?.GetValue(okResult.Value);
        var officeProperty = deletedPerson?.GetType().GetProperty("Office");
        
        Assert.Equal("Aalborg", officeProperty?.GetValue(deletedPerson));
    }

    #endregion

    #region Verify Response Structure Tests

    [Fact]
    public async Task Should_ReturnResponseWithMessageAndDeletedPerson_When_Successful()
    {
        // Arrange
        var person = new Person
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            OfficeId = 1,
            Office = new Office { Id = 1, Location = "Silkeborg", Capacity = 5 }
        };

        _mockRepository
            .Setup(r => r.DeletePersonAsync(1))
            .ReturnsAsync(person);

        // Act
        var result = await _controller.DeletePerson(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        
        // Verify response has both "message" and "deletedPerson" properties
        var messageProperty = okResult.Value?.GetType().GetProperty("message");
        var deletedPersonProperty = okResult.Value?.GetType().GetProperty("deletedPerson");
        
        Assert.NotNull(messageProperty);
        Assert.NotNull(deletedPersonProperty);
    }

    [Fact]
    public async Task Should_ReturnDeletedPersonWithAllExpectedFields_When_Successful()
    {
        // Arrange
        var expectedDateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25));
        var person = new Person
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = expectedDateOfBirth,
            OfficeId = 1,
            Office = new Office { Id = 1, Location = "Silkeborg", Capacity = 5 }
        };

        _mockRepository
            .Setup(r => r.DeletePersonAsync(1))
            .ReturnsAsync(person);

        // Act
        var result = await _controller.DeletePerson(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var deletedPersonProperty = okResult.Value?.GetType().GetProperty("deletedPerson");
        var deletedPerson = deletedPersonProperty?.GetValue(okResult.Value);
        
        // Verify all expected fields exist in deletedPerson
        Assert.NotNull(deletedPerson?.GetType().GetProperty("Id"));
        Assert.NotNull(deletedPerson?.GetType().GetProperty("FirstName"));
        Assert.NotNull(deletedPerson?.GetType().GetProperty("LastName"));
        Assert.NotNull(deletedPerson?.GetType().GetProperty("DateOfBirth"));
        Assert.NotNull(deletedPerson?.GetType().GetProperty("Office"));
        
        // Verify values
        Assert.Equal(1, deletedPerson?.GetType().GetProperty("Id")?.GetValue(deletedPerson));
        Assert.Equal("John", deletedPerson?.GetType().GetProperty("FirstName")?.GetValue(deletedPerson));
        Assert.Equal("Doe", deletedPerson?.GetType().GetProperty("LastName")?.GetValue(deletedPerson));
        Assert.Equal(expectedDateOfBirth, deletedPerson?.GetType().GetProperty("DateOfBirth")?.GetValue(deletedPerson));
        Assert.Equal("Silkeborg", deletedPerson?.GetType().GetProperty("Office")?.GetValue(deletedPerson));
    }

    #endregion
}

