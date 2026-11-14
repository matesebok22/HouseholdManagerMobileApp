using NUnit.Framework;
using Moq;

using HouseholdManagerMobileApp.Core.Services;
using HouseholdManagerMobileApp.Core.ViewModels;
using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Tests;

public class SettingsTests
{
    private Mock<IAuthService> _mockAuthService;
    private Mock<IDatabaseService> _mockDatabaseService;
    private Mock<IUserSessionService> _mockUserSessionService;
    private SettingsViewModel viewModel;

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
        viewModel.NavigateEventLogButtonCommand.Execute(string.Empty);
        // Assert 2
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Az eseménynaplóhoz való navigálás sikertelen. Kérlek, jelentkezz be újra."));

        // Act 3
        viewModel.NavigateNotificationsButtonCommand.Execute(string.Empty);
        // Assert 3
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Az értesítésekhez való navigálás sikertelen. Kérlek, jelentkezz be újra."));
    }

    // Háztartásváltás tesztelése:
    [Test]
    public void LoadHouseholdsAsync_Unsuccessful()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsOfflineMode()).Returns(false);
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(new User() { Nickname = "TesztElek" });
        _mockDatabaseService.Setup(x => x.LoadSettingsPageAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        viewModel.SelectedHousehold = "testHH - ASD123";

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Háztartás váltás sikertelen!"));
    }

    [Test]
    public void LoadHouseholdsAsync_Successful()
    {
        // Arrange
        _mockDatabaseService.Setup(x => x.LoadSettingsPageAsync(It.IsAny<string>())).ReturnsAsync(true);
        User user = new() { UID = "uid123" };
        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(user);
        Household household = new() { HHID = "hhid123", HouseholdName = "HH123" };
        _mockUserSessionService.Setup(x => x.GetCurrentHousehold()).Returns(household);
        _mockUserSessionService.Setup(x => x.GetCurrentBudgets()).Returns([]);
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdUsers()).Returns([]);

        // Act
        viewModel.SelectedHousehold = "testHH - ASD123";

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
    }

    // Háztartáshoz csatlakozás tesztelése:
    [Test]
    public void JoinHouseholdAsync_NotLoggedIn()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(false);

        // Act
        viewModel.JoinHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Háztartáshoz csatlakozás sikertelen. Kérlek, jelentkezz be újra."));
    }

    [Test]
    public void JoinHouseholdAsync_NoInternetConnection()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        _mockUserSessionService.Setup(x => x.IsOfflineMode()).Returns(true);

        // Act
        viewModel.JoinHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Offline módban nem lehetséges a háztartáshoz csatlakozás."));
    }

    [TestCase("", "Háztartás azonosító megadása kötelező!")]
    [TestCase("    ", "Háztartás azonosító megadása kötelező!")]
    [TestCase("ASDF1234", "A háztartás azonosítójának 6 karakter hosszúnak kell lennie!")]
    [TestCase("AS12", "A háztartás azonosítójának 6 karakter hosszúnak kell lennie!")]
    public void JoinHouseholdAsync_HouseholdAccessInvalid(string householdAccess, string errorMessage)
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);

        viewModel.HouseholdAccessCode = householdAccess;

        // Act
        viewModel.JoinHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Test]
    public void JoinHouseholdAsync_TooManyHouseholds()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        _mockDatabaseService.Setup(x => x.JoinHouseholdAsync(It.IsAny<string>())).ReturnsAsync(false);

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
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        _mockUserSessionService.Setup(x => x.IsOfflineMode()).Returns(false);
        _mockDatabaseService.Setup(x => x.JoinHouseholdAsync(It.IsAny<string>())).ReturnsAsync(true);

        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(new User { Nickname = "TesztElek" });
        _mockDatabaseService.Setup(x => x.LoadSettingsPageAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdList()).Returns([new Household { HouseholdName = "HH123", HouseholdAccess = "ABC123" }]);
        _mockUserSessionService.Setup(x => x.GetCurrentHousehold()).Returns(new Household { HouseholdName = "HH123", HouseholdAccess = "ABC123" });
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdUsers()).Returns([]);

        viewModel.HouseholdAccessCode = "ABC123";

        // Act
        viewModel.JoinHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
        Assert.That(viewModel.HouseholdsList[0], Is.EqualTo($"HH123 - ABC123"));
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
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Új háztartás készítése sikertelen. Kérlek, jelentkezz be újra."));
    }

    [Test]
    public void CreateHouseholdAsync_NoInternetConnection()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        _mockUserSessionService.Setup(x => x.IsOfflineMode()).Returns(true);

        // Act
        viewModel.CreateHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Offline módban nem lehetséges a háztartás készítése."));
    }

    [TestCase("", "Háztartásnév megadása kötelező!")]
    [TestCase("    ", "Háztartásnév megadása kötelező!")]
    [TestCase("Te", "A háztartásnév túl rövid!")]
    [TestCase("TestHouseholdTestHousehold", "A háztartásnév túl hosszú!")]
    public void CreateHouseholdAsync_HouseholdNameInvalid(string householdName, string errorMessage)
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);

        viewModel.NewHouseholdName = householdName;

        // Act
        viewModel.CreateHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Test]
    public void CreateHouseholdAsync_TooManyHouseholds()
    {
        // Arrange
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
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        _mockUserSessionService.Setup(x => x.IsOfflineMode()).Returns(false);
        _mockDatabaseService.Setup(x => x.CreateHouseholdAsync(It.IsAny<string>())).ReturnsAsync(true);

        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(new User { Nickname = "TesztElek" });
        _mockDatabaseService.Setup(x => x.LoadSettingsPageAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdList()).Returns([new Household { HouseholdName = "testhh", HouseholdAccess = "ASD123" }]);
        _mockUserSessionService.Setup(x => x.GetCurrentHousehold()).Returns(new Household { HouseholdName = "testhh", HouseholdAccess = "ASD123" });
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdUsers()).Returns([]);

        viewModel.NewHouseholdName = "testhh";

        // Act
        viewModel.CreateHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
        Assert.That(viewModel.HouseholdsList[0], Is.EqualTo($"testhh - ASD123"));
    }

    // Háztartás módosításának tesztelése:
    [Test]
    public void EditHouseholdAsync_NotHouseholdOwner()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsCurrentHouseholdOwner()).Returns(false);

        // Act
        viewModel.EditHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Csak a háztartás tulajdonosa módosíthatja a háztartást."));
    }

    [TestCase("", "Háztartásnév megadása kötelező!")]
    [TestCase("     ", "Háztartásnév megadása kötelező!")]
    [TestCase("Te", "A háztartásnév túl rövid!")]
    [TestCase("TestHouseholdTestHousehold", "A háztartásnév túl hosszú!")]
    public void EditHouseholdAsync_NewHouseholdNameInvalid(string newHouseholdName, string errorMessage)
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsCurrentHouseholdOwner()).Returns(true);

        viewModel.EditHouseholdName = newHouseholdName;

        // Act
        viewModel.EditHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Test]
    public void EditHouseholdAsync_CurrentHouseholdNotExist()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsCurrentHouseholdOwner()).Returns(true);
        _mockDatabaseService.Setup(x => x.EditHouseholdAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>())).ReturnsAsync(false);

        viewModel.EditHouseholdName = "TestHousehold";

        // Act
        viewModel.EditHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Hiba a háztartás módosítása közben."));
    }

    [Test]
    public void EditHouseholdAsync_Successful()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsCurrentHouseholdOwner()).Returns(true);
        _mockDatabaseService.Setup(x => x.EditHouseholdAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<int>())).ReturnsAsync(true);
        _mockUserSessionService.Setup(x => x.GetCurrentHousehold()).Returns(new Household { HouseholdName = "testhh", HouseholdAccess = "ASD123" });

        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(new User { Nickname = "TesztElek" });
        _mockDatabaseService.Setup(x => x.LoadSettingsPageAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdList()).Returns([new Household { HouseholdName = "testhh", HouseholdAccess = "ASD123" }]);
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdUsers()).Returns([]);

        viewModel.EditHouseholdName = "testhh";
        viewModel.HouseholdAccessCode = "ASD123";

        // Act
        viewModel.EditHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
        Assert.That(viewModel.HouseholdsList[0], Is.EqualTo($"testhh - ASD123"));
    }

    // Háztartásból kilépés tesztelése:
    [Test]
    public void LeaveHouseholdAsync_NoInternetConnection()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsOfflineMode()).Returns(true);

        // Act
        viewModel.LeaveHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Offline módban nem lehetséges a háztartásból való kilépés."));
    }

    [Test]
    public void LeaveHouseholdAsync_CurrentHouseholdNotExist()
    {
        // Arrange
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
        _mockUserSessionService.Setup(x => x.IsOfflineMode()).Returns(false);
        _mockDatabaseService.Setup(x => x.LeaveHouseholdAsync()).ReturnsAsync(true);

        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(new User { Nickname = "TesztElek" });
        _mockDatabaseService.Setup(x => x.LoadSettingsPageAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdList()).Returns([new Household { HouseholdName = "testhh", HouseholdAccess = "ASD123" }]);
        _mockUserSessionService.Setup(x => x.GetCurrentHousehold()).Returns(new Household { HouseholdName = "testhh", HouseholdAccess = "ASD123" });
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdUsers()).Returns([]);

        // Act
        viewModel.LeaveHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
        Assert.That(viewModel.HouseholdsList[0], Is.EqualTo($"testhh - ASD123"));
    }

    // Háztartás felbontásának tesztelése:
    [Test]
    public void DisbandHouseholdAsync_NoInternetConnection()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsOfflineMode()).Returns(true);

        // Act
        viewModel.DisbandHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Offline módban nem lehetséges a háztartás felbontása."));
    }

    [Test]
    public void DisbandHouseholdAsync_CurrentHouseholdNotExist()
    {
        // Arrange
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
        _mockUserSessionService.Setup(x => x.IsOfflineMode()).Returns(false);
        _mockDatabaseService.Setup(x => x.LeaveHouseholdAsync()).ReturnsAsync(true);

        _mockUserSessionService.Setup(x => x.GetCurrentUser()).Returns(new User { Nickname = "TesztElek" });
        _mockDatabaseService.Setup(x => x.LoadSettingsPageAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdList()).Returns([new Household { HouseholdName = "testhh", HouseholdAccess = "ASD123" }]);
        _mockUserSessionService.Setup(x => x.GetCurrentHousehold()).Returns(new Household { HouseholdName = "testhh", HouseholdAccess = "ASD123" });
        _mockUserSessionService.Setup(x => x.GetCurrentHouseholdUsers()).Returns([]);

        // Act
        viewModel.DisbandHouseholdButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(string.Empty));
        Assert.That(viewModel.HouseholdsList[0], Is.EqualTo($"testhh - ASD123"));
    }

    // Jelszóváltoztatás tesztelése:
    [Test]
    public void ChangePasswordAsync_NotLoggedIn()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(false);

        // Act
        viewModel.ChangePasswordButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("A jelszó megváltoztatása sikertelen. Kérlek, jelentkezz be újra."));
    }

    [Test]
    public void ChangePasswordAsync_NoInternetConnection()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        _mockUserSessionService.Setup(x => x.IsOfflineMode()).Returns(true);

        // Act
        viewModel.ChangePasswordButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Offline módban nem lehetséges a jelszó megváltoztatása."));
    }

    [TestCase("oldpassword", "", "newpassword")]
    [TestCase("", "newpassword", "newpassword")]
    [TestCase("oldpassword", "newpassword", "")]
    public void ChangePasswordAsync_EmptyInputs(string oldpass, string newpass1, string newpass2)
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);

        viewModel.OldPassword = oldpass;
        viewModel.NewPassword = newpass1;
        viewModel.ConfirmNewPassword = newpass2;

        // Acts
        viewModel.ChangePasswordButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Minden jelszó mezőt ki kell tölteni."));
    }

    [TestCase("Oldpassword123", "Oldpassword123", "Newpassword123")]
    [TestCase("Oldpassword123", "Newpassword123", "Oldpassword123")]
    public void ChangePasswordAsync_OldNewPasswordSame(string oldpass, string newpass1, string newpass2)
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);

        viewModel.OldPassword = oldpass;
        viewModel.NewPassword = newpass1;
        viewModel.ConfirmNewPassword = newpass2;

        // Acts
        viewModel.ChangePasswordButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("A régi jelszó nem egyezhet az új jelszóval."));
    }

    [TestCase("New123", "Az új jelszónak legalább 8 karakter hosszúnak kell lennie!")]
    [TestCase("newpassword123", "Az új jelszónak tartalmaznia kell legalább egy nagybetűt!")]
    [TestCase("NEWPASSWORD123", "Az új jelszónak tartalmaznia kell legalább egy kisbetűt!")]
    [TestCase("Newpassword", "Az új jelszónak tartalmaznia kell legalább egy számot!")]
    public void ChangePasswordAsync_WeakNewPassword(string newpassword, string errorMessage)
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        viewModel.OldPassword = "Oldpassword123";
        viewModel.NewPassword = newpassword;
        viewModel.ConfirmNewPassword = newpassword;

        // Acts
        viewModel.ChangePasswordButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Test]
    public void ChangePasswordAsync_NewPasswordsNotSame()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);

        viewModel.OldPassword = "Oldpassword123";
        viewModel.NewPassword = "Newpassword123";
        viewModel.ConfirmNewPassword = "newpass";

        // Acts
        viewModel.ChangePasswordButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("Az új jelszavak nem egyeznek."));
    }

    [Test]
    public void ChangePasswordAsync_OldCurrentPasswordNotSame()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        _mockAuthService.Setup(x => x.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        viewModel.OldPassword = "Oldpassword123";
        viewModel.NewPassword = "Newpassword123";
        viewModel.ConfirmNewPassword = "Newpassword123";

        // Acts
        viewModel.ChangePasswordButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.ErrorMessage, Is.EqualTo("A régi és a jelenlegi jelszó nem egyezik!"));
    }

    [Test]
    public void ChangePasswordAsync_Successful()
    {
        // Arrange
        _mockUserSessionService.Setup(x => x.IsUserLoggedIn()).Returns(true);
        _mockAuthService.Setup(x => x.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        viewModel.OldPassword = "Oldpassword123";
        viewModel.NewPassword = "Newpassword123";
        viewModel.ConfirmNewPassword = "Newpassword123";

        // Acts
        viewModel.ChangePasswordButtonCommand.Execute(string.Empty);

        // Assert
        Assert.That(viewModel.OldPassword, Is.EqualTo(string.Empty));
        Assert.That(viewModel.NewPassword, Is.EqualTo(string.Empty));
        Assert.That(viewModel.ConfirmNewPassword, Is.EqualTo(string.Empty));
    }
}