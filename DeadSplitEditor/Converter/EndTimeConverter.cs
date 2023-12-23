using NCalc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace DeadSplitEditor.Converter
{
    class EndTimeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is decimal value && values[1] is bool roundToNearestValue)
            {
                if (roundToNearestValue)
                {
                    return RoundToNearestValue(value).ToString();
                }
                else
                {
                    return value.ToString();
                }
            }

            return null;
        }

        /// <summary>
        /// Rounds to the nearest 50 fps value
        /// </summary>
        /// <param name="value">The timestamp that should be rounded</param>
        /// <returns></returns>
        public static decimal RoundToNearestValue(decimal value)
        {
            var tmp = Math.Floor(value * 100);
            return (tmp + (tmp % 2)) / 100;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            decimal result = 0;

            if (decimal.TryParse(value as string, out decimal decimalValue))
            {
                result = decimalValue;
            }
            else
            {
                var expr = new Expression(value as string);
                if (!expr.HasErrors())
                {
                    var evaluated = expr.Evaluate();
                    if (evaluated is double doubleValue)
                    {
                        result = (decimal)doubleValue;
                    }
                    else if (evaluated is int intValue)
                    {
                        result = (decimal)intValue;
                    }
                }
            }

            return new object[]
            {
                result,
                false
            };
        }
    }
}
