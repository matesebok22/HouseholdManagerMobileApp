using System.Globalization;

namespace HouseholdManagerMobileApp.Core.Helpers;

public class BudgetToColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
            return Colors.Green;

        if (values[0] == null || values[1] == null)
            return Colors.Green;

        string useableBudgetString = values[0].ToString()?.Replace(" Ft", "").Trim() ?? "0";
        string totalBudgetString = values[1].ToString()?.Replace(" Ft", "").Trim() ?? "0";

        if (!int.TryParse(useableBudgetString, out int useableBudget))
            useableBudget = 0;
        if (!int.TryParse(totalBudgetString, out int totalBudget))
            totalBudget = 0;

        if (useableBudget < 0)
            return Colors.Red;

        if (totalBudget <= 0)
            return Colors.Green;

        if (useableBudget < (int)(totalBudget * 0.25))
            return Colors.Yellow;

        return Colors.Green;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}