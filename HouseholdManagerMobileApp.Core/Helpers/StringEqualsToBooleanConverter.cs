using System.Globalization;

namespace HouseholdManagerMobileApp.Core.Helpers;

public class StringEqualsToBooleanConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // Összehasonlítjuk az aktuális kártya EID-ját a ViewModel ExpandedEventId-jával
        if (values != null && values.Length == 2 && values[0] != null && values[1] != null)
        {
            return values[0].ToString() == values[1].ToString();
        }
        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
