using System;
using System.Globalization;
using System.Windows.Data;

namespace FeedbackFactory
{
    
    public class BarHeightConverter : IValueConverter
    {
        
        public double MaxValue { get; set; } = 5.0;

      
        public double MaxHeight { get; set; } = 200.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double numericValue)
            {
  
                if (numericValue < 0) numericValue = 0;
                if (numericValue > MaxValue) numericValue = MaxValue;

                double ratio = numericValue / MaxValue;  // 0..1
                return ratio * MaxHeight;                // 0..MaxHeight
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            throw new NotImplementedException();
        }
    }
}
