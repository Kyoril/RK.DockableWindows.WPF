using System;
using System.Windows;
using System.Windows.Controls;

namespace Docker
{
    /// <summary>
    /// A layout panel which contains all WindowTab items in it and arranges them properly.
    /// </summary>
    public class WindowTabPanel : Panel
    {
        #region Dependency Properties
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                "Orientation",
                typeof(Orientation),
                typeof(WindowTabPanel),
                new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));
        #endregion


        #region Panel overrides
        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = 0.0;

            foreach (UIElement element in InternalChildren)
            {
                double width;

                if (Orientation == Orientation.Horizontal)
                {
                    width = element.DesiredSize.Width;
                    element.Arrange(new Rect(x, 0.0, width, finalSize.Height));
                }
                else
                {
                    width = element.DesiredSize.Height;
                    element.Arrange(new Rect(0.0, x, finalSize.Width, width));
                }

                x += width;
            }

            return new Size(finalSize.Width, finalSize.Height);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // Gather tab text length
            int tabTextLength = 0;
            foreach (WindowTab tab in Children)
            {
                tabTextLength += Math.Max(GetWindowTabText(tab).Length, 1);
            }

            double width = 0.0;
            double height = 0.0;

            // Measure all tab controls and sum up the width and height
            foreach (WindowTab tab in this.InternalChildren)
            {
                if (Orientation == Orientation.Horizontal)
                {
                    tab.Measure(new Size(((double)Math.Max(GetWindowTabText(tab).Length, 1)) / tabTextLength * availableSize.Width, availableSize.Height));
                    width += tab.DesiredSize.Width;
                    height = Math.Max(height, tab.DesiredSize.Height);
                }
                else
                {
                    tab.Measure(new Size(availableSize.Width, Math.Max(GetWindowTabText(tab).Length, 1) / ((double)tabTextLength) * availableSize.Height));
                    width += tab.DesiredSize.Height;
                    height = Math.Max(height, tab.DesiredSize.Width);
                }
            }

            if (Orientation == Orientation.Horizontal)
            {
                return new Size(width, height);
            }

            return new Size(height, width);
        }
        #endregion


        #region Internal Methods
        private string GetWindowTabText(WindowTab tab)
        {
            if (tab.Window != null)
            {
                return tab.Window.TabText;
            }

            return string.Empty;
        }
        #endregion


        #region Properties
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
        #endregion
    }
}
