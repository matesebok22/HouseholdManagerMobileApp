using HouseholdManagerMobileApp.Core.Models;
using HouseholdManagerMobileApp.Core.Services;
using HouseholdManagerMobileApp.Core.ViewModels;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace HouseholdManagerMobileApp.Tests;

[TestFixture]
public class DatabaseTests
{
    private Mock<IAuthService> _mockAuthService;
    private Mock<IDatabaseService> _mockDatabaseService;
    private Mock<IUserSessionService> _mockUserSessionService;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockDatabaseService = new Mock<IDatabaseService>();
        _mockUserSessionService = new Mock<IUserSessionService>();
    }

    // Háztartás készítésének tesztelése:
    [Test]
    public void CreateHouseholdAsync_NotLoggedIn()
    {
        // Arrange
        var mockUser = new User { Nickname = "TestUser" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);

        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(false);

        // Act
        viewModel.CreateHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
    }

    [Test]
    public void CreateHouseholdAsync_EmptyName()
    {
        // Arrange
        var mockUser = new User { Nickname = "TestUser" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);

        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
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
        var mockUser = new User { Nickname = "TestUser" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);

        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        _mockDatabaseService.Setup(x => x.CreateHouseholdAsync(It.IsAny<string>())).ReturnsAsync(false);
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
        var mockUser = new User { Nickname = "TestUser", Username = "testuser" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);

        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        _mockDatabaseService.Setup(x => x.CreateHouseholdAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockDatabaseService.Setup(x => x.LoadHouseholdsAsync()).ReturnsAsync(true);
        _mockDatabaseService.Setup(x => x.ChangeSelectedHouseholdAsync(It.IsAny<string>())).ReturnsAsync(true);
        var mockHousehold = new Household { HouseholdName = "testhh", HouseholdAccess = "ASD123" };
        _mockUserSessionService.Setup(x => x.GetCurrentHousehold()).Returns(mockHousehold);
        var householdList = new List<Household> { mockHousehold };
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdList()).Returns(householdList);

        _mockUserSessionService.Setup(x => x.GetCurrentBudgets()).Returns([]);
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
        var mockUser = new User { Nickname = "TestUser", Username = "testuser" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);

        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(false);

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
        var mockUser = new User { Nickname = "TestUser", Username = "testuser" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);

        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        viewModel.HouseholdAccessCode = householdAccess;

        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);

        // Act
        viewModel.JoinHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Háztartás azonosító megadása kötelező!"));
    }

    [Test]
    public void JoinHouseholdAsync_TooManyHouseholds()
    {
        // Arrange
        var mockUser = new User { Nickname = "TestUser", Username = "testuser" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);

        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockDatabaseService.Setup(x => x.JoinHouseholdAsync(It.IsAny<string>())).ReturnsAsync(false);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
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
        var mockUser = new User { Nickname = "TestUser", Username = "testuser" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);
        var mockHousehold = new Household { HouseholdName = "testhh", HouseholdAccess = "ASD123" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);
        var householdList = new List<Household> { mockHousehold };
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdList()).Returns(householdList);

        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        _mockDatabaseService.Setup(x => x.JoinHouseholdAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);
        _mockDatabaseService.Setup(x => x.LoadHouseholdsAsync()).ReturnsAsync(true);
        _mockDatabaseService.Setup(x => x.ChangeSelectedHouseholdAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockUserSessionService.Setup(x => x.GetCurrentHousehold()).Returns(mockHousehold);
        _mockUserSessionService.Setup(x => x.GetCurrentBudgets()).Returns([]);
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
        var mockUser = new User { Nickname = "TestUser", Username = "testuser" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);

        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
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
        var mockUser = new User { Nickname = "TestUser", Username = "testuser" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);
        var mockHousehold = new Household { HouseholdName = "testhh", HouseholdAccess = "ASD123" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);
        var householdList = new List<Household> { mockHousehold };
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdList()).Returns(householdList);

        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockDatabaseService.Setup(x => x.EditHouseholdAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>())).ReturnsAsync(true);
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);
        _mockDatabaseService.Setup(x => x.LoadHouseholdsAsync()).ReturnsAsync(true);
        _mockDatabaseService.Setup(x => x.ChangeSelectedHouseholdAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockUserSessionService.Setup(x => x.GetCurrentHousehold()).Returns(mockHousehold);
        _mockUserSessionService.Setup(x => x.GetCurrentBudgets()).Returns([]);
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
        var mockUser = new User { Nickname = "TestUser", Username = "testuser" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);

        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockDatabaseService.Setup(x => x.LeaveHouseholdAsync()).ReturnsAsync(false);

        // Act
        viewModel.LeaveHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Hiba a háztartásból kilépés közben."));
    }

    [Test]
    public void LeaveHouseholdAsync_Successful()
    {
        // Arrange
        var mockUser = new User { Nickname = "TestUser", Username = "testuser" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);
        var mockHousehold = new Household { HouseholdName = "testhh", HouseholdAccess = "ASD123" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);
        var householdList = new List<Household> { mockHousehold };
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdList()).Returns(householdList);

        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockDatabaseService.Setup(x => x.LeaveHouseholdAsync()).ReturnsAsync(true);
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);
        _mockDatabaseService.Setup(x => x.LoadHouseholdsAsync()).ReturnsAsync(true);
        _mockDatabaseService.Setup(x => x.ChangeSelectedHouseholdAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockUserSessionService.Setup(x => x.GetCurrentHousehold()).Returns(mockHousehold);
        _mockUserSessionService.Setup(x => x.GetCurrentBudgets()).Returns([]);
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
        var mockUser = new User { Nickname = "TestUser", Username = "testuser" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);

        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockDatabaseService.Setup(x => x.LeaveHouseholdAsync()).ReturnsAsync(false);

        // Act
        viewModel.DisbandHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Hiba a háztartás felbontása közben."));
    }

    [Test]
    public void DisbandHouseholdAsync_Successful()
    {
        // Arrange
        var mockUser = new User { Nickname = "TestUser", Username = "testuser" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);
        var mockHousehold = new Household { HouseholdName = "testhh", HouseholdAccess = "ASD123" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);
        var householdList = new List<Household> { mockHousehold };
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdList()).Returns(householdList);

        var viewModel = new SettingsViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockDatabaseService.Setup(x => x.LeaveHouseholdAsync()).ReturnsAsync(true);
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(mockUser);
        _mockDatabaseService.Setup(x => x.LoadHouseholdsAsync()).ReturnsAsync(true);
        _mockDatabaseService.Setup(x => x.ChangeSelectedHouseholdAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockUserSessionService.Setup(x => x.GetCurrentHousehold()).Returns(mockHousehold);
        _mockUserSessionService.Setup(x => x.GetCurrentBudgets()).Returns([]);
        viewModel.NewHouseholdName = "testhh";
        viewModel.HouseholdAccessCode = "ASD123";

        // Act
        viewModel.DisbandHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
        Assert.That(viewModel.HouseholdsList[0], Is.EqualTo($"testhh - ASD123"));
    }
}