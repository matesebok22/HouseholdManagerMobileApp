using System.ComponentModel;
using System.Security.Cryptography;
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
        _mockDatabaseService.Setup(x => x.LoadHouseholdsAsync(It.IsAny<int>())).ReturnsAsync(true);
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
        _mockDatabaseService.Setup(x => x.LoadHouseholdsAsync(It.IsAny<int>())).ReturnsAsync(true);
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
        _mockDatabaseService.Setup(x => x.LoadHouseholdsAsync(It.IsAny<int>())).ReturnsAsync(true);
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
        _mockDatabaseService.Setup(x => x.LoadHouseholdsAsync(It.IsAny<int>())).ReturnsAsync(true);
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
        _mockDatabaseService.Setup(x => x.LoadHouseholdsAsync(It.IsAny<int>())).ReturnsAsync(true);
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

    // Új esemény létrehozásának tesztelése:
    [Test]
    public void CreateEventAsync_NotLoggedIn()
    {
        // Arrange
        var viewModel = new NewEventViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(false);

        // Act
        viewModel.CreateEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
    }

    [TestCase("", 0)]
    [TestCase("", 1000)]
    [TestCase("test", 0)]
    public void CreateEventAsync_EmptyNameOrCost(string eventName, int eventCost)
    {
        // Arrange
        var viewModel = new NewEventViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        viewModel.EventName = eventName;
        viewModel.EventCost = eventCost;

        // Act
        viewModel.CreateEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Kötelező megadni az esemény nevét és költségét!"));
    }

    [TestCase("testtesttesttesttesttesttesttestname", "testtype", "testdesc", "Az esemény neve maximum 30 karakter hosszú lehet!")]
    [TestCase("testname", "testtesttesttesttesttesttesttesttype", "testdesc", "Az esemény típusa maximum 20 karakter hosszú lehet!")]
    [TestCase("testname", "testtype", "testtesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttesttestdesc", "Az esemény leírása maximum 500 karakter hosszú lehet!")]
    public void CreateEventAsync_InputTooLong(string eventName, string eventType, string eventDesc, string errorMessage)
    {
        // Arrange
        var viewModel = new NewEventViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        viewModel.EventName = eventName;
        viewModel.EventCost = 1000;
        viewModel.EventType = eventType;
        viewModel.EventDesc = eventDesc;

        // Act
        viewModel.CreateEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Test]
    public void CreateEventAsync_CreateEventFailed()
    {
        // Arrange
        var viewModel = new NewEventViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        Event? _event = null;
        _mockUserSessionService.Setup(x => x.GetSelectedEvent()).Returns(_event);
        _mockDatabaseService.Setup(x => x.CreateEventAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(false);
        viewModel.EventName = "testname";
        viewModel.EventCost = 1000;

        // Act
        viewModel.CreateEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Új esemény létrehozása sikertelen!"));
    }

    [Test]
    public void CreateEventAsync_CreateEventSuccessful()
    {
        // Arrange
        var viewModel = new NewEventViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        Event? _event = null;
        _mockUserSessionService.Setup(x => x.GetSelectedEvent()).Returns(_event);
        _mockDatabaseService.Setup(x => x.CreateEventAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(true);
        viewModel.EventName = "testname";
        viewModel.EventCost = 1000;

        // Act
        viewModel.CreateEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
    }

    [Test]
    public void CreateEventAsync_EditEventFailed()
    {
        // Arrange
        var viewModel = new NewEventViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        Event? _event = new();
        _mockUserSessionService.Setup(x => x.GetSelectedEvent()).Returns(_event);
        _mockDatabaseService.Setup(x => x.EditEventAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(false);
        viewModel.EventName = "testname";
        viewModel.EventCost = 1000;

        // Act
        viewModel.CreateEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Új esemény létrehozása sikertelen!"));
    }

    [Test]
    public void CreateEventAsync_EditEventSuccessful()
    {
        // Arrange
        var viewModel = new NewEventViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        Event? _event = new();
        _mockUserSessionService.Setup(x => x.GetSelectedEvent()).Returns(_event);
        _mockDatabaseService.Setup(x => x.EditEventAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(true);
        viewModel.EventName = "testname";
        viewModel.EventCost = 1000;

        // Act
        viewModel.CreateEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
    }

    // Eseménynapló tesztelése:
    [TestCase("testId", "")]
    [TestCase("", "testId")]
    public void ToggleEventExpandAsync_Successful(string eID, string expandedEventId)
    {
        // Arrange
        var viewModel = new EventLogViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        Event parameter = new() { EID = eID };
        viewModel.ExpandedEventId = expandedEventId;

        // Act
        viewModel.ToggleEventExpandCommand.Execute(parameter);

        // Assert
        Assert.That(viewModel.ExpandedEventId, Is.EqualTo(eID));
    }

    [Test]
    public void EditEventAsync_NotLoggedIn()
    {
        // Arrange
        var viewModel = new EventLogViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(false);

        // Act
        viewModel.EditEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ExpandedEventId, Is.EqualTo(string.Empty));
    }

    [Test]
    public void EditEventAsync_Successful()
    {
        // Arrange
        var viewModel = new EventLogViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);

        // Act
        viewModel.EditEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ExpandedEventId, Is.EqualTo(string.Empty));
    }

    [Test]
    public void DeleteEventAsync_NotLoggedIn()
    {
        // Arrange
        var viewModel = new EventLogViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(false);

        // Act
        viewModel.DeleteEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ExpandedEventId, Is.EqualTo(string.Empty));
    }

    [Test]
    public void DeleteEventAsync_Successful()
    {
        // Arrange
        var viewModel = new EventLogViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);

        // Act
        viewModel.DeleteEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ExpandedEventId, Is.EqualTo(string.Empty));
    }

    [TestCase("Kategória nézet", "Dátum nézet")]
    [TestCase("Dátum nézet", "Kategória nézet")]
    public void ChangeSortTypeAsync_Successful(string categoryOne, string categoryTwo)
    {
        // Arrange
        var viewModel = new EventLogViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        viewModel.ChangeSortTypeName = categoryOne;

        // Act
        viewModel.ChangeSortTypeButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ChangeSortTypeName, Is.EqualTo(categoryTwo));
    }

    [TestCase(-1, "test1", "test3")]
    [TestCase(1, "test3", "test1")]
    [TestCase(-1, "test2", "test1")]
    [TestCase(1, "test2", "test3")]
    public void SelectTypeAsync_Successful(int direction, string selectedName, string answer)
    {
        // Arrange
        var viewModel = new EventLogViewModel(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
        viewModel.ChangeSortTypeName = "Kategória nézet";
        viewModel.EventTypes = new(["test1", "test2", "test3"]);
        viewModel.SelectedTypeName = selectedName;

        // Act
        if (direction == -1)
            viewModel.PreviousMonthButtonCommand.Execute(string.Empty);
        else
            viewModel.NextMonthButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.SelectedCategory, Is.EqualTo(answer));
    }
}