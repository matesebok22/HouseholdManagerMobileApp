using NUnit.Framework;
using Moq;

using HouseholdManagerMobileApp.Core.Services;
using HouseholdManagerMobileApp.Core.ViewModels;
using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Tests;

public class EventLogTests
{
    private Mock<IAuthService> _mockAuthService;
    private Mock<IDatabaseService> _mockDatabaseService;
    private Mock<IUserSessionService> _mockUserSessionService;
    private EventLogViewModel viewModel;

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

        // Act 1
        viewModel.NavigateHomeButtonCommand.Execute(string.Empty);
        // Assert 1
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("A kezdőlaphoz való navigálás sikertelen. Kérlek, jelentkezz be újra."));

        // Act 2
        viewModel.NavigateSettingsButtonCommand.Execute(string.Empty);
        // Assert 2
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("A beállításokhoz való navigálás sikertelen. Kérlek, jelentkezz be újra."));

        // Act 3
        viewModel.NavigateNotificationsButtonCommand.Execute(string.Empty);
        // Assert 3
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Az értesítésekhez való navigálás sikertelen. Kérlek, jelentkezz be újra."));

        // Act 4
        viewModel.NavigateNewEventButtonCommand.Execute(string.Empty);
        // Assert 4
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Az új esemény létrehozásához való navigálás sikertelen. Kérlek, jelentkezz be újra."));
    }

    // Esemény kinyitásának tesztelése:
    [TestCase("testId", "")]
    [TestCase("", "testId")]
    public void ToggleEventExpandAsync_Successful(string eID, string expandedEventId)
    {
        // Arrange
        Event parameter = new() { EID = eID };

        viewModel.ExpandedEventId = expandedEventId;

        // Act
        viewModel.ToggleEventExpandCommand.Execute(parameter);

        // Assert
        Assert.That(viewModel.ExpandedEventId, Is.EqualTo(eID));
    }

    // Esemény módosítás megnyitásának tesztelése:
    [Test]
    public void EditEventAsync_NotLoggedIn()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(false);

        // Act
        viewModel.EditEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Az esemény módosításához való navigálás sikertelen. Kérlek, jelentkezz be újra."));
    }

    [Test]
    public void EditEventAsync_Successful()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);

        // Act
        viewModel.EditEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
    }

    // Esemény törlésének tesztelése:
    [Test]
    public void DeleteEventAsync_NotLoggedIn()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(false);

        // Act
        viewModel.DeleteEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Az esemény törlése sikertelen. Kérlek, jelentkezz be újra."));
    }

    [Test]
    public void DeleteEventAsync_NoInternetConnection()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        _mockUserSessionService.Setup(x => x.IsOfflineMode()).Returns(true);

        // Act
        viewModel.DeleteEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Offline módban nem lehetséges az esemény törlése."));
    }

    [Test]
    public void DeleteEventAsync_Successful()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);

        // Act
        viewModel.DeleteEventButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
    }

    // Rendszerezési típus tesztelése:
    [TestCase("Kategória nézet", "Dátum nézet")]
    [TestCase("Dátum nézet", "Kategória nézet")]
    public void ChangeSortTypeAsync_Successful(string type, string type2)
    {
        // Arrange
        viewModel.ChangeSortTypeName = type;

        // Act
        viewModel.ChangeSortTypeButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ChangeSortTypeName, Is.EqualTo(type2));
    }

    // Adott típus értékei közötti váltás tesztelése:
    [TestCase(-1, "test1", "test3")]
    [TestCase(1, "test3", "test1")]
    [TestCase(-1, "test2", "test1")]
    [TestCase(1, "test2", "test3")]
    public void SelectTypeAsync_Successful(int direction, string selectedName, string answer)
    {
        // Arrange
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
