using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FeedbackFactory
{
    public class RoleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int role)
            {
                return role == 0 ? "Lehrer" : role == 1 ? "Admin" : "Unbekannt";
            }
            return "Ungültig";
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string role)
            {
                return role == "Lehrer" ? 0 : role == "Admin" ? 1 : -1;
            }
            return -1;
        }
    }
}