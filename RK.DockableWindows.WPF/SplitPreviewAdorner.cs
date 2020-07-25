using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RK.DockableWindows.WPF
{
    public class SplitPreviewAdorner : Adorner
    {
        private readonly Rectangle bar;
        private readonly TranslateTransform translation;


        #region Construction
        public SplitPreviewAdorner(UIElement element)
            : base(element)
        {
            SnapsToDevicePixels = true;

            SolidColorBrush brush = new SolidColorBrush(Colors.Black)
            {
                Opacity = 0.4
            };

            bar = new Rectangle { Fill = brush };
            translation = new TranslateTransform();
            bar.RenderTransform = this.translation;

            AddVisualChild(bar);
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

        protected override int VisualChildrenCount => 1;
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
