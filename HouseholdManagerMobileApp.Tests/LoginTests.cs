using NUnit.Framework;
using Moq;

using HouseholdManagerMobileApp.Core.Services;
using HouseholdManagerMobileApp.Core.ViewModels;

namespace HouseholdManagerMobileApp.Tests;

public class LoginTests
{
    private Mock<IAuthService> _mockAuthService;
    private LoginViewModel viewModel;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        viewModel = new(_mockAuthService.Object);
    }

    // Bejelentkezés tesztelése:
    [TestCase("", "password123")]
    [TestCase("usertest", "")]
    public void LoginAsync_EmptyInputs(string username, string password)
    {
        // Arrange
        viewModel.Username = username;
        viewModel.Password = password;

        // Act
        viewModel.LoginButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Kérlek tölts ki minden mezőt!"));
    }

    [Test]
    public void LoginAsync_InvalidInputs()
    {
        // Arrange
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        viewModel.Username = "testuser";
        viewModel.Password = "password123";

        // Act
        viewModel.LoginButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Hibás felhasználónév vagy jelszó!"));
    }

    [Test]
    public void LoginAsync_OfflineLoginUnsuccessful()
    {
        // Arrange
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        viewModel.Username = "testuser";
        viewModel.Password = "password123";
        viewModel.IsOnline = false;

        // Act
        viewModel.LoginButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Offline bejelentkezéshez kérlek jelentkezz legalább egy fiókba internetkapcsolattal!"));
    }

    [Test]
    public void LoginAsync_SuccessfulLogin()
    {
        // Arrange
        _mockAuthService.Setup(x => x.LoginAsync("testuser", "password123")).ReturnsAsync(true);

        viewModel.Username = "testuser";
        viewModel.Password = "password123";

        // Act
        viewModel.LoginButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
    }
}