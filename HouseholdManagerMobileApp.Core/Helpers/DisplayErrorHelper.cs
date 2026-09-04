namespace HouseholdManagerMobileApp.Core.Helpers;

public class DisplayErrorHelper
{
    public static async Task<string> DisplayErrorAsync(string message)
    {
        if (Shell.Current != null)
            await Shell.Current.DisplayAlert("Hiba", message, "OK");

        return message;
    }
}