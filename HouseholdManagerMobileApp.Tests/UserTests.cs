using NUnit.Framework;
using Moq;

using HouseholdManagerMobileApp.Core.Services;
using HouseholdManagerMobileApp.Core.ViewModels;

namespace HouseholdManagerMobileApp.Tests;

public class UserTests
{
    private Mock<IAuthService> _mockAuthService;
    private Mock<IUserSessionService> _mockUserSessionService;
    private Mock<IDatabaseService> _mockDatabaseService;
    private Mock<INotificationService> _mockNotificationService;
    private UserViewModel viewModel;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockDatabaseService = new Mock<IDatabaseService>();
        _mockUserSessionService = new Mock<IUserSessionService>();
        viewModel = new(_mockAuthService.Object, _mockUserSessionService.Object, _mockDatabaseService.Object, _mockNotificationService.Object);
    }

    [Test]
    public void NavigateToAsync_NotLoggedIn()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(false);
        _mockNotificationService.Setup(x => x.MarkAllNotificationsAsReadAsync()).ReturnsAsync(true);

        // Act 1
        viewModel.NavigateSettingsButtonCommand.Execute(string.Empty);
        // Assert 1
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("A beállításokhoz való navigálás sikertelen. Kérlek, jelentkezz be újra."));

        // Act 2
        viewModel.NavigateEventLogButtonCommand.Execute(string.Empty);
        // Assert 2
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Az eseménynaplóhoz való navigálás sikertelen. Kérlek, jelentkezz be újra."));

        // Act 3
        viewModel.NavigateNotificationsButtonCommand.Execute(string.Empty);
        // Assert 3
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Az értesítésekhez való navigálás sikertelen. Kérlek, jelentkezz be újra."));
    }
}