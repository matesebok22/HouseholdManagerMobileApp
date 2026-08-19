using NUnit.Framework;
using Moq;

using HouseholdManagerMobileApp.Core.Services;
using HouseholdManagerMobileApp.Core.ViewModels;

namespace HouseholdManagerMobileApp.Tests;

[TestFixture]
public class DatabaseTests
{
    private Mock<IAuthService> _mockAuthService;
    private Mock<IDatabaseService> _mockDatabaseService;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockDatabaseService = new Mock<IDatabaseService>();
    }

    // Háztartás készítésének tesztelése:
    [Test]
    public void CreateHouseholdAsync_NotLoggedIn()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object);
        _mockAuthService.Setup(x => x.CurrentUsername).Returns(string.Empty);

        // Act
        viewModel.CreateHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
    }

    [Test]
    public void CreateHouseholdAsync_EmptyName()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object);
        viewModel.NewHouseholdName = string.Empty;

        // Act
        viewModel.CreateHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Háztartásnév megadása kötelező!"));
    }

    [Test]
    public void CreateHouseholdAsync_TooManyHouseholds()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object);
        _mockDatabaseService.Setup(x => x.CreateHouseholdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        viewModel.NewHouseholdName = "testhh";

        // Act
        viewModel.CreateHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Hiba a háztartás elkészítése közben."));
    }

    [Test]
    public void CreateHouseholdAsync_Successful()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object);
        _mockAuthService.Setup(x => x.CurrentUsername).Returns("testuser");
        _mockDatabaseService.Setup(x => x.CreateHouseholdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        _mockDatabaseService.Setup(x => x.LoadHouseholdsAsync(It.IsAny<string>())).ReturnsAsync([("testhh", "ASD123")]);
        _mockDatabaseService.Setup(x => x.ChangeSelectedHouseholdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        _mockDatabaseService.Setup(x => x.CurrentHouseholdName).Returns("testhh");
        _mockDatabaseService.Setup(x => x.CurrentHouseholdBudgets).Returns([]);
        viewModel.NewHouseholdName = "testhh";

        // Act
        viewModel.CreateHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
        Assert.That(viewModel.HouseholdsList[0], Is.EqualTo($"testhh - ASD123"));
    }

    // Háztartáshoz csatlakozás tesztelése:
    [Test]
    public void JoinHouseholdAsync_NotLoggedIn()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object);
        _mockAuthService.Setup(x => x.CurrentUsername).Returns(string.Empty);

        // Act
        viewModel.JoinHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
    }

    [TestCase("")]
    [TestCase("    ")]
    public void JoinHouseholdAsync_EmptyHouseholdAccess(string householdAccess)
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object);
        viewModel.HouseholdAccessCode = householdAccess;

        // Act
        viewModel.JoinHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Háztartás azonosító megadása kötelező!"));
    }

    [Test]
    public void JoinHouseholdAsync_TooManyHouseholds()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object);
        _mockDatabaseService.Setup(x => x.JoinHouseholdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        viewModel.HouseholdAccessCode = "ASD123";

        // Act
        viewModel.JoinHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Hiba a háztartáshoz csatlakozás közben."));
    }

    [Test]
    public void JoinHouseholdAsync_Successful()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object);
        _mockDatabaseService.Setup(x => x.JoinHouseholdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        _mockAuthService.Setup(x => x.CurrentUsername).Returns("testuser");
        _mockDatabaseService.Setup(x => x.LoadHouseholdsAsync(It.IsAny<string>())).ReturnsAsync([("testhh", "ASD123")]);
        _mockDatabaseService.Setup(x => x.ChangeSelectedHouseholdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        _mockDatabaseService.Setup(x => x.CurrentHouseholdName).Returns("testhh");
        _mockDatabaseService.Setup(x => x.CurrentHouseholdBudgets).Returns([]);
        viewModel.NewHouseholdName = "testhh";
        viewModel.HouseholdAccessCode = "ASD123";

        // Act
        viewModel.JoinHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
        Assert.That(viewModel.HouseholdsList[0], Is.EqualTo($"testhh - ASD123"));
    }

    // Háztartás módosítáának tesztelése:
    [Test]
    public void EditHouseholdAsync_CurrentHouseholdNotExist()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object);
        _mockDatabaseService.Setup(x => x.EditHouseholdAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>())).ReturnsAsync(false);

        // Act
        viewModel.EditHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Hiba a háztartás módosítása közben."));
    }

    [Test]
    public void EditHouseholdAsync_Successful()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object);
        _mockDatabaseService.Setup(x => x.EditHouseholdAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>())).ReturnsAsync(true);
        _mockAuthService.Setup(x => x.CurrentUsername).Returns("testuser");
        _mockDatabaseService.Setup(x => x.LoadHouseholdsAsync(It.IsAny<string>())).ReturnsAsync([("testhh", "ASD123")]);
        _mockDatabaseService.Setup(x => x.ChangeSelectedHouseholdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        _mockDatabaseService.Setup(x => x.CurrentHouseholdName).Returns("testhh");
        _mockDatabaseService.Setup(x => x.CurrentHouseholdBudgets).Returns([]);
        viewModel.NewHouseholdName = "testhh";
        viewModel.HouseholdAccessCode = "ASD123";

        // Act
        viewModel.EditHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
        Assert.That(viewModel.HouseholdsList[0], Is.EqualTo($"testhh - ASD123"));
    }

    // Háztartásból kilépés tesztelése:
    [Test]
    public void LeaveHouseholdAsync_CurrentHouseholdNotExist()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object);
        _mockDatabaseService.Setup(x => x.LeaveHouseholdAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        viewModel.LeaveHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Hiba a háztartásból kilépés közben."));
    }

    [Test]
    public void LeaveHouseholdAsync_Successful()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object);
        _mockDatabaseService.Setup(x => x.LeaveHouseholdAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockAuthService.Setup(x => x.CurrentUsername).Returns("testuser");
        _mockDatabaseService.Setup(x => x.LoadHouseholdsAsync(It.IsAny<string>())).ReturnsAsync([("testhh", "ASD123")]);
        _mockDatabaseService.Setup(x => x.ChangeSelectedHouseholdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        _mockDatabaseService.Setup(x => x.CurrentHouseholdName).Returns("testhh");
        _mockDatabaseService.Setup(x => x.CurrentHouseholdBudgets).Returns([]);
        viewModel.NewHouseholdName = "testhh";
        viewModel.HouseholdAccessCode = "ASD123";

        // Act
        viewModel.LeaveHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
        Assert.That(viewModel.HouseholdsList[0], Is.EqualTo($"testhh - ASD123"));
    }

    // Háztartás felbontásának tesztelése:
    [Test]
    public void DisbandHouseholdAsync_CurrentHouseholdNotExist()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object);
        _mockDatabaseService.Setup(x => x.LeaveHouseholdAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        viewModel.DisbandHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Hiba a háztartás felbontása közben."));
    }

    [Test]
    public void DisbandHouseholdAsync_Successful()
    {
        // Arrange
        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object);
        _mockDatabaseService.Setup(x => x.LeaveHouseholdAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockAuthService.Setup(x => x.CurrentUsername).Returns("testuser");
        _mockDatabaseService.Setup(x => x.LoadHouseholdsAsync(It.IsAny<string>())).ReturnsAsync([("testhh", "ASD123")]);
        _mockDatabaseService.Setup(x => x.ChangeSelectedHouseholdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        _mockDatabaseService.Setup(x => x.CurrentHouseholdName).Returns("testhh");
        _mockDatabaseService.Setup(x => x.CurrentHouseholdBudgets).Returns([]);
        viewModel.NewHouseholdName = "testhh";
        viewModel.HouseholdAccessCode = "ASD123";

        // Act
        viewModel.DisbandHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
        Assert.That(viewModel.HouseholdsList[0], Is.EqualTo($"testhh - ASD123"));
    }
}