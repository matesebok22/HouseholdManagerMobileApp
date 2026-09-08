using NUnit.Framework;
using Moq;

using HouseholdManagerMobileApp.Core.Services;
using HouseholdManagerMobileApp.Core.ViewModels;
using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Tests;

public class NotificationsTests
{
    private Mock<IAuthService> _mockAuthService;
    private Mock<IUserSessionService> _mockUserSessionService;
    private Mock<INotificationService> _mockNotificationService;
    private NotificationsViewModel viewModel;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockUserSessionService = new Mock<IUserSessionService>();
        _mockNotificationService = new Mock<INotificationService>();
        viewModel = new(_mockAuthService.Object, _mockUserSessionService.Object, _mockNotificationService.Object);
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
        viewModel.NavigateEventLogButtonCommand.Execute(string.Empty);
        // Assert 3
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Az eseménynaplóhoz való navigálás sikertelen. Kérlek, jelentkezz be újra."));
    }

    // Események betöltésének tesztelése:
    [Test]
    public async Task LoadNotificationsPageAsync_Successful()
    {
        // Arrange
        Notification notification = new() { NID = "nid123" };
        List<Notification> notifications = [notification];
        _mockNotificationService.Setup(x => x.GetAllNotificationsAsync()).ReturnsAsync(notifications);

        // Act
        await viewModel.LoadNotificationsPageAsync();

        // Assert
        Assert.That(viewModel.Notifications, Has.Count.EqualTo(1));
    }

    // Események törlésének tesztelése:
    [Test]
    public void DeleteAllNotificationsAsync_NoInternetConnection()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsOfflineMode()).Returns(true);

        // Act
        viewModel.DeleteAllNotificationsButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Offline módban nem elérhető az értesítések törlése."));
    }

    [Test]
    public void DeleteAllNotificationsAsync_Successful()
    {
        // Arrange
        Notification notification = new() { NID = "nid123" };
        _mockUserSessionService.Setup(x => x.IsOfflineMode()).Returns(false);

        viewModel.Notifications = [notification];

        // Act
        viewModel.DeleteAllNotificationsButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
        Assert.That(viewModel.Notifications, Has.Count.EqualTo(0));
    }
}
