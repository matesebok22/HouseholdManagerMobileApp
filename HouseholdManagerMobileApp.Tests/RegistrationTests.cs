using NUnit.Framework;
using Moq;

using HouseholdManagerMobileApp.Core.Services;
using HouseholdManagerMobileApp.Core.ViewModels;

namespace HouseholdManagerMobileApp.Tests;

public class RegistrationTests
{
    private Mock<IAuthService> _mockAuthService;
    private RegistrationViewModel viewModel;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        viewModel = new(_mockAuthService.Object);
    }

    // Regisztráció tesztelése:
    [Test]
    public void RegisterAsync_NoInternetConnection()
    {
        // Arrange
        viewModel.IsOnline = false;

        // Act
        viewModel.RegisterButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Nincs internetkapcsolat, próbáld meg később!"));
    }

    [TestCase("", "TestNick", "password123", "password123")]
    [TestCase("testuser", "", "password123", "password123")]
    [TestCase("testuser", "TestNick", "", "password123")]
    [TestCase("testuser", "TestNick", "password123", "")]
    public void RegisterAsync_EmptyInputs(string username, string nickname, string password, string password2)
    {
        // Arrange
        viewModel.Username = username;
        viewModel.Nickname = nickname;
        viewModel.Password = password;
        viewModel.ConfirmPassword = password2;

        // Act
        viewModel.RegisterButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Kérlek tölts ki minden mezőt!"));
    }

    [TestCase("pass", "pass", "A jelszónak legalább 8 karakter hosszúnak kell lennie!")]
    [TestCase("password123", "password123", "A jelszónak tartalmaznia kell legalább egy nagybetűt!")]
    [TestCase("PASSWORD", "PASSWORD", "A jelszónak tartalmaznia kell legalább egy kisbetűt!")]
    [TestCase("Password", "Password", "A jelszónak tartalmaznia kell legalább egy számot!")]
    public void RegisterAsync_WeakPassword(string password, string password2, string errorMessage)
    {
        // Arrange
        viewModel.Username = "testuser";
        viewModel.Nickname = "TestUser";
        viewModel.Password = password;
        viewModel.ConfirmPassword = password2;

        // Act
        viewModel.RegisterButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(errorMessage));
    }

    [TestCase("Password123", "Password321")]
    [TestCase("Password321", "Password123")]
    public void RegisterAsync_NoMatchingPasswords(string password, string password2)
    {
        // Arrange
        viewModel.Username = "testuser";
        viewModel.Nickname = "TestUser";
        viewModel.Password = password;
        viewModel.ConfirmPassword = password2;

        // Act
        viewModel.RegisterButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("A jelszavak nem egyeznek!"));
    }

    [Test]
    public void RegisterAsync_UsernameAlreadyExists()
    {
        // Arrange
        _mockAuthService.Setup(x => x.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        viewModel.Username = "testuser";
        viewModel.Nickname = "TestUser";
        viewModel.Password = "Password123";
        viewModel.ConfirmPassword = "Password123";

        // Act
        viewModel.RegisterButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("A regisztráció sikertelen. Próbáld meg később!"));
    }

    [Test]
    public void RegisterAsync_SuccessfulRegistration()
    {
        // Arrange
        _mockAuthService.Setup(x => x.RegisterAsync("testuser", "TestUser", "Password123")).ReturnsAsync(true);

        viewModel.Username = "testuser";
        viewModel.Nickname = "TestUser";
        viewModel.Password = "Password123";
        viewModel.ConfirmPassword = "Password123";

        // Act
        viewModel.RegisterButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
    }
}