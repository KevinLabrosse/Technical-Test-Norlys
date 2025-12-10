using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TechnicalTestNorlys.API;
using TechnicalTestNorlys.Handlers;
using TechnicalTestNorlys.Models;
using TechnicalTestNorlys.Repositories;
using Xunit;

namespace TechnicalTestNorlys.Tests;

public class GetPersonsEndpointTests
{
    private readonly Mock<ILogger<GetPersonsEndpoint>> _mockLogger;
    private readonly Mock<ILogger<PersonHandler>> _mockHandlerLogger;
    private readonly Mock<IPersonRepository> _mockRepository;
    private readonly PersonHandler _handler;
    private readonly GetPersonsEndpoint _controller;

    public GetPersonsEndpointTests()
    {
        _mockLogger = new Mock<ILogger<GetPersonsEndpoint>>();
        _mockHandlerLogger = new Mock<ILogger<PersonHandler>>();
        _mockRepository = new Mock<IPersonRepository>();
        _handler = new PersonHandler(_mockHandlerLogger.Object, _mockRepository.Object);
        _controller = new GetPersonsEndpoint(_mockLogger.Object, _handler);
    }

    #region GetAllPersons Tests

    [Fact]
    public async Task Should_ReturnOkWithPersons_When_PersonsExist()
    {
        // Arrange
        var persons = new List<Person>
        {
            new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                OfficeId = 1,
                Office = new Office { Id = 1, Location = "Silkeborg", Capacity = 5 }
            },
            new Person
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
                OfficeId = 2,
                Office = new Office { Id = 2, Location = "Esbjerg", Capacity = 10 }
            }
        };

        _mockRepository
            .Setup(r => r.GetAllPersonsAsync())
            .ReturnsAsync(persons);

        // Act
        var result = await _controller.GetAllPersons();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPersons = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.Equal(2, returnedPersons.Count());
    }

    [Fact]
    public async Task Should_ReturnOkWithEmptyList_When_NoPersonsExist()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAllPersonsAsync())
            .ReturnsAsync(new List<Person>());

        // Act
        var result = await _controller.GetAllPersons();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPersons = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.Empty(returnedPersons);
    }

    [Fact]
    public async Task Should_ReturnPersonsWithCorrectOfficeLocation_When_PersonsExist()
    {
        // Arrange
        var persons = new List<Person>
        {
            new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
                OfficeId = 1,
                Office = new Office { Id = 1, Location = "Silkeborg", Capacity = 5 }
            }
        };

        _mockRepository
            .Setup(r => r.GetAllPersonsAsync())
            .ReturnsAsync(persons);

        // Act
        var result = await _controller.GetAllPersons();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPersons = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value).ToList();
        
        // Verify the response contains Office as a string location, not the full Office object
        var firstPerson = returnedPersons.First();
        var officeProperty = firstPerson.GetType().GetProperty("Office");
        Assert.NotNull(officeProperty);
        Assert.Equal("Silkeborg", officeProperty.GetValue(firstPerson));
    }

    [Fact]
    public async Task Should_CallRepositoryOnce_When_GetAllPersonsIsCalled()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAllPersonsAsync())
            .ReturnsAsync(new List<Person>());

        // Act
        await _controller.GetAllPersons();

        // Assert
        _mockRepository.Verify(r => r.GetAllPersonsAsync(), Times.Once);
    }

    [Fact]
    public async Task Should_Return500_When_GetAllPersonsThrowsException()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAllPersonsAsync())
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetAllPersons();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    #endregion

    #region GetPersonById Tests

    [Fact]
    public async Task Should_ReturnOkWithPerson_When_PersonExists()
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
            .Setup(r => r.GetPersonByIdAsync(1))
            .ReturnsAsync(person);

        // Act
        var result = await _controller.GetPersonById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        // Verify the correct data is returned
        var idProperty = okResult.Value.GetType().GetProperty("Id");
        var firstNameProperty = okResult.Value.GetType().GetProperty("FirstName");
        Assert.Equal(1, idProperty?.GetValue(okResult.Value));
        Assert.Equal("John", firstNameProperty?.GetValue(okResult.Value));
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_PersonDoesNotExist()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetPersonByIdAsync(999))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _controller.GetPersonById(999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("999", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task Should_CallRepositoryWithCorrectId_When_GetPersonByIdIsCalled()
    {
        // Arrange
        var personId = 42;
        _mockRepository
            .Setup(r => r.GetPersonByIdAsync(personId))
            .ReturnsAsync((Person?)null);

        // Act
        await _controller.GetPersonById(personId);

        // Assert
        _mockRepository.Verify(r => r.GetPersonByIdAsync(42), Times.Once);
    }

    [Fact]
    public async Task Should_Return500_When_GetPersonByIdThrowsException()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetPersonByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetPersonById(1);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnPersonWithOfficeLocationString_When_PersonExists()
    {
        // Arrange
        var person = new Person
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-25)),
            OfficeId = 3,
            Office = new Office { Id = 3, Location = "Aalborg", Capacity = 50 }
        };

        _mockRepository
            .Setup(r => r.GetPersonByIdAsync(1))
            .ReturnsAsync(person);

        // Act
        var result = await _controller.GetPersonById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var officeProperty = okResult.Value?.GetType().GetProperty("Office");
        Assert.NotNull(officeProperty);
        Assert.Equal("Aalborg", officeProperty.GetValue(okResult.Value));
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Should_ReturnNotFound_When_IdIsZero()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetPersonByIdAsync(0))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _controller.GetPersonById(0);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_IdIsNegative()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetPersonByIdAsync(-1))
            .ReturnsAsync((Person?)null);

        // Act
        var result = await _controller.GetPersonById(-1);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion
}

