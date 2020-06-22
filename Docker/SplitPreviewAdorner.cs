using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Docker
{
    public class SplitPreviewAdorner : Adorner
    {
        private Rectangle bar;
        private TranslateTransform translation;


        #region Construction
        public SplitPreviewAdorner(UIElement element, Style style)
            : base(element)
        {
            this.SnapsToDevicePixels = true;

            SolidColorBrush brush = new SolidColorBrush(Colors.Black)
            {
                Opacity = 0.4
            };

            this.bar = new Rectangle();
            this.bar.Fill = brush;
            this.translation = new TranslateTransform();
            this.bar.RenderTransform = this.translation;
            this.AddVisualChild(bar);
        }
        #endregion

        #region Adorner Overrides
        protected override Size ArrangeOverride(Size finalSize)
        {
            bar.Arrange(new Rect(new Point(), finalSize));
            return finalSize;
        }
        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return bar;
        }
        protected override int VisualChildrenCount { get => 1; }
        #endregion


        #region Properties

        public double OffsetX
        {
            get => translation.X;
            set => translation.X = value;
        }
        public double OffsetY
        {
            get => translation.Y;
            set => translation.Y = value;
        }
        #endregion
    }
}
