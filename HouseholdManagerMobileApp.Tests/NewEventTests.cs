using NUnit.Framework;
using Moq;

using HouseholdManagerMobileApp.Core.Services;
using HouseholdManagerMobileApp.Core.ViewModels;
using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Tests;

public class NewEventTests
{
    private Mock<IAuthService> _mockAuthService;
    private Mock<IDatabaseService> _mockDatabaseService;
    private Mock<IUserSessionService> _mockUserSessionService;
    private NewEventViewModel viewModel;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockDatabaseService = new Mock<IDatabaseService>();
        _mockUserSessionService = new Mock<IUserSessionService>();
        viewModel = new(_mockAuthService.Object, _mockDatabaseService.Object, _mockUserSessionService.Object);
    }

    // Internetkapcsolat tesztelése:
    [Test]
    public void NavigateToAsync_NoInternetConnection()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(false);

        // Act
        viewModel.NavigateEventLogButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Az eseménynaplóhoz való navigálás sikertelen. Kérlek, jelentkezz be újra."));
    }

    // Új esemény létrehozásának tesztelése:
    [Test]
    public void CreateEventAsync_NotLoggedIn()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(false);

        // Act
        viewModel.CreateEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Az új esemény létrehozása sikertelen. Kérlek, jelentkezz be újra."));
    }

    [TestCase("", 0)]
    [TestCase("", 1000)]
    [TestCase("test", 0)]
    public void CreateEventAsync_EmptyNameOrCost(string eventName, int eventCost)
    {
        // Arrange
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

    // Meglévő esemény módosításának tesztelése:
    [Test]
    public void CreateEventAsync_EditEventFailed()
    {
        // Arrange
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

    // Felhasználható költségkeret tesztelése:
    [Test]
    public void CalculateUseableBudgetAsync_NotSuccessful()
    {
        // Arrange
        List<Budget> budgets = [new Budget { BudgetDate = new DateTime(2026, 10, 1), BudgetAmount = 0 }, new Budget { BudgetDate = new DateTime(2026, 9, 1), BudgetAmount = 200000 }];
        _mockUserSessionService.Setup(x => x.GetCurrentBudgets()).Returns(budgets);
        List<Event> events = [new Event { EventUserDate = new DateTime(2026, 10, 4), EventCost = 15000 }, new Event { EventUserDate = new DateTime(2026, 10, 14), EventCost = 20000 }, new Event { EventUserDate = new DateTime(2026, 8, 30), EventCost = 150000 }];
        _mockUserSessionService.Setup(x => x.GetCurrentEventLog()).Returns(events);

        // Act
        viewModel.EventDate = new DateTime(2026, 10, 20);

        // Assert
        Assert.That(viewModel.UseableBudgetLabel, Is.EqualTo("Nincs beállítva költségkeret!"));
    }

    [Test]
    public void CalculateUseableBudgetAsync_NoSelectedEventSuccessful()
    {
        // Arrange
        List<Budget> budgets = [new Budget { BudgetDate = new DateTime(2026, 10, 1), BudgetAmount = 100000 }, new Budget { BudgetDate = new DateTime(2026, 9, 1), BudgetAmount = 200000 }];
        _mockUserSessionService.Setup(x => x.GetCurrentBudgets()).Returns(budgets);
        List<Event> events = [new Event { EventUserDate = new DateTime(2026, 10, 4), EventCost = 15000 }, new Event { EventUserDate = new DateTime(2026, 10, 14), EventCost = 20000 }, new Event { EventUserDate = new DateTime(2026, 8, 30), EventCost = 150000 }];
        _mockUserSessionService.Setup(x => x.GetCurrentEventLog()).Returns(events);
        Event? selectedEvent = null;
        _mockUserSessionService.Setup(x => x.GetSelectedEvent()).Returns(selectedEvent);

        // Act
        viewModel.EventDate = new DateTime(2026, 10, 20);

        // Assert
        Assert.That(viewModel.UseableBudgetLabel, Is.EqualTo("65000 Ft"));
    }

    [Test]
    public void CalculateUseableBudgetAsync_SelectedEventSuccessful()
    {
        // Arrange
        List<Budget> budgets = [new Budget { BudgetDate = new DateTime(2026, 10, 1), BudgetAmount = 100000 }, new Budget { BudgetDate = new DateTime(2026, 9, 1), BudgetAmount = 200000 }];
        _mockUserSessionService.Setup(x => x.GetCurrentBudgets()).Returns(budgets);
        List<Event> events = [new Event { EventUserDate = new DateTime(2026, 10, 4), EventCost = 15000 }, new Event { EventUserDate = new DateTime(2026, 10, 14), EventCost = 20000 }, new Event { EventUserDate = new DateTime(2026, 8, 30), EventCost = 150000 }];
        _mockUserSessionService.Setup(x => x.GetCurrentEventLog()).Returns(events);
        Event? selectedEvent = new Event { EventUserDate = new DateTime(2026, 10, 4), EventCost = 15000 };
        _mockUserSessionService.Setup(x => x.GetSelectedEvent()).Returns(selectedEvent);

        // Act
        viewModel.EventDate = new DateTime(2026, 10, 20);

        // Assert
        Assert.That(viewModel.UseableBudgetLabel, Is.EqualTo("80000 Ft"));
    }

    [TestCase(100000, 10000, "90000 Ft")]
    [TestCase(50000, 45000, "5000 Ft")]
    [TestCase(10000, 54000, "-44000 Ft")]
    public void EventCostChanged_Successful(int useableBudget, int eventCost, string useableBudgetLabel)
    {
        // Arrange
        viewModel.UseableBudgetLabel = "Ft";
        viewModel.UseableBudget = useableBudget;

        // Act
        viewModel.EventCost = eventCost;

        // Assert
        Assert.That(viewModel.UseableBudgetLabel, Is.EqualTo(useableBudgetLabel));
    }
}