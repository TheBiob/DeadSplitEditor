using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace DeadSplitEditor.UserControls
{
    public class EventKeystone : Control
    {
        public double EventOffset
        {
            get { return (double)GetValue(EventOffsetProperty); }
            set { SetValue(EventOffsetProperty, value); }
        }

        public static readonly DependencyProperty EventOffsetProperty =
            DependencyProperty.Register("EventOffset", typeof(double), typeof(EventKeystone), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsParentArrange, OnPropertyChanged, OnCoerceValue));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EventKeystone keystone)
            {
                keystone.InvalidateArrange();
            }
        }

        private static object OnCoerceValue(DependencyObject d, object baseValue)
        {
            if (baseValue is double value)
            {
                if (value < 0)
                {
                    return 0;
                }

                return value;
            }

            throw new ArgumentException();
        }
    }
}
