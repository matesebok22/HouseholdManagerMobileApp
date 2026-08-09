using NUnit.Framework;
using Moq;

using HouseholdManagerMobileApp.Core.Services;
using HouseholdManagerMobileApp.Core.ViewModels;

namespace HouseholdManagerMobileApp.Tests;

[TestFixture]
public class AuthTests
{
    private Mock<IAuthService> _mockAuthService;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
    }

    // Regisztráció tesztelése:
    [Test]
    public void RegisterAsync_EmptyInputs()
    {
        // Arrange
        var viewModel = new RegistrationViewModel(_mockAuthService.Object);
        viewModel.Username = "testuser";
        viewModel.Nickname = string.Empty;
        viewModel.Password = "password123";
        viewModel.ConfirmPassword = "password123";

        // Act
        viewModel.RegisterButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Kérlek tölts ki minden mezőt!"));
        _mockAuthService.Verify(x => x.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void RegisterAsync_NoMatchingPasswords()
    {
        // Arrange
        var viewModel = new RegistrationViewModel(_mockAuthService.Object);
        viewModel.Username = "testuser";
        viewModel.Nickname = "TestUser";
        viewModel.Password = "password123";
        viewModel.ConfirmPassword = "password";

        // Act
        viewModel.RegisterButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("A jelszavak nem egyeznek!"));
        _mockAuthService.Verify(x => x.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void RegisterAsync_UsernameAlreadyExists()
    {
        // Arrange
        var viewModel = new RegistrationViewModel(_mockAuthService.Object);
        _mockAuthService.Setup(x => x.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        viewModel.Username = "testuser";
        viewModel.Nickname = "TestUser";
        viewModel.Password = "password123";
        viewModel.ConfirmPassword = "password123";

        // Act
        viewModel.RegisterButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("A regisztráció sikertelen. Próbáld meg később!"));
    }

    [Test]
    public void RegisterAsync_SuccessfulRegistration()
    {
        // Arrange
        var viewModel = new RegistrationViewModel(_mockAuthService.Object);
        _mockAuthService.Setup(x => x.RegisterAsync("testuser", "TestUser", "password123")).ReturnsAsync(true);

        viewModel.Username = "testuser";
        viewModel.Nickname = "TestUser";
        viewModel.Password = "password123";
        viewModel.ConfirmPassword = "password123";

        // Act
        viewModel.RegisterButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
        _mockAuthService.Verify(x => x.RegisterAsync("testuser", "TestUser", "password123"), Times.Once);
    }

    // Bejelentkezés tesztelése:
    [Test]
    public void LoginAsync_EmptyInputs()
    {
        // Arrange
        var viewModel = new LoginViewModel(_mockAuthService.Object);

        viewModel.Username = string.Empty;
        viewModel.Password = "password123";

        // Act
        viewModel.LoginButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Kérlek tölts ki minden mezőt!"));
        _mockAuthService.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void LoginAsync_InvalidInputs()
    {
        // Arrange
        var viewModel = new LoginViewModel(_mockAuthService.Object);
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        viewModel.Username = "testuser";
        viewModel.Password = "password123";

        // Act
        viewModel.LoginButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Hibás felhasználónév vagy jelszó!"));
        _mockAuthService.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void LoginAsync_SuccessfulLogin()
    {
        // Arrange
        var viewModel = new LoginViewModel(_mockAuthService.Object);
        _mockAuthService.Setup(x => x.LoginAsync("testuser", "password123")).ReturnsAsync(true);

        viewModel.Username = "testuser";
        viewModel.Password = "password123";

        // Act
        viewModel.LoginButtonCommand.Execute(string.Empty);
        //await Task.Delay(100);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
        _mockAuthService.Verify(x => x.LoginAsync("testuser", "password123"), Times.Once);
    }
}