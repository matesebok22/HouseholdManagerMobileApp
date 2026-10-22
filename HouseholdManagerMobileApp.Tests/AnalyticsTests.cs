using NUnit.Framework;
using Moq;

using HouseholdManagerMobileApp.Core.Services;
using HouseholdManagerMobileApp.Core.ViewModels;

namespace HouseholdManagerMobileApp.Tests;

public class AnalyticsTests
{
    private Mock<IAuthService> _mockAuthService;
    private Mock<IUserSessionService> _mockUserSessionService;
    private AnalyticsViewModel viewModel;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockUserSessionService = new Mock<IUserSessionService>();
        viewModel = new(_mockUserSessionService.Object, _mockAuthService.Object);
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

    // Listák közötti váltás tesztelése:
    [TestCase(true, true, "2026. október", "2026.", "Hónap nézet")] // Dátum, alapvető lista.
    [TestCase(false, true, "2026. október", "2026. október", "Év nézet")] // Dátum, teljes lista.
    [TestCase(true, false, "Pihenés", "Pihenés - Összes", "Jelen év nézet")] // Kategória, alapvető lista.
    [TestCase(false, false, "Pihenés", "Pihenés - Idén", "Összes év nézet")] // Kategória, teljes lista.
    public void ChangeListAsync_Successful(bool listVisible, bool isDate, string queryName, string categoryName, string buttonName)
    {
        // Arrange
        viewModel.DefaultListVisible = listVisible;
        viewModel.FullListVisible = !listVisible;
        viewModel.QueryIsDate = isDate;
        viewModel.QueryCategory = queryName;

        // Act
        viewModel.ChangeListButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.CategoryName, Is.EqualTo(categoryName));
        Assert.That(viewModel.ChangeListButtonName, Is.EqualTo(buttonName));
    }
}