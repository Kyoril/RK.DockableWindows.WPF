using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace RK.DockableWindows.WPF
{
    /// <summary>
    /// A splitter control that is used by the SplitContainer control. These splitters allow
    /// the user to resize WindowGroup controls inside a SplitContainer.
    /// </summary>
    public class SplitContainerSplitter : Thumb
    {
        private sealed class ResizeData
        {
            public double MaxOffset;
            public double MinOffset;
            public double AfterElementSize;
            public SplitPreviewAdorner PreviewAdorner;
            public SplitContainer Parent;
            public double BeforeElementSize;
        }


        private ResizeData resizeData;


        #region Dependency Properties
        internal static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(
                "Size",
                typeof(double),
                typeof(SplitContainerSplitter),
                new FrameworkPropertyMetadata(
                    4.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure),
                o => (double)o > 0.0);
        #endregion


        #region Construction
        /// <summary>
        /// Initializes static WPF settings.
        /// </summary>
        static SplitContainerSplitter()
        {
            // Allow custom styling
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitContainerSplitter), new FrameworkPropertyMetadata(typeof(SplitContainerSplitter)));

            // Subscribe event handlers
            EventManager.RegisterClassHandler(typeof(SplitContainerSplitter), DragStartedEvent, new DragStartedEventHandler(OnDragStarted));
            EventManager.RegisterClassHandler(typeof(SplitContainerSplitter), DragDeltaEvent, new DragDeltaEventHandler(OnDragDelta));
            EventManager.RegisterClassHandler(typeof(SplitContainerSplitter), DragCompletedEvent, new DragCompletedEventHandler(OnDragCompleted));
        }

        /// <summary>
        /// Initializes a new instance of the SplitContainerSplitter class.
        /// </summary>
        /// <param name="beforeElement">The element before the splitter.</param>
        /// <param name="afterElement">The element after the splitter.</param>
        /// <param name="splitterOrientation">The splitter orientation.</param>
        internal SplitContainerSplitter(FrameworkElement beforeElement, FrameworkElement afterElement, Orientation splitterOrientation)
        {
            BeforeElement = beforeElement;
            AfterElement = afterElement;
            Orientation = splitterOrientation;
            Cursor = (splitterOrientation == Orientation.Horizontal) ? Cursors.SizeNS : Cursors.SizeWE;
        }
        #endregion


        #region Thumb Overrides
        protected override Size MeasureOverride(Size constraint)
        {
            return new Size(Size, Size);
        }
        #endregion

        #region Private Methods
        private void OnDragStarted(DragStartedEventArgs e)
        {
            if (this.VisualParent is SplitContainer splitContainer)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(splitContainer);
                if (adornerLayer != null)
                {
                    resizeData = new ResizeData
                    {
                        Parent = splitContainer,

                        BeforeElementSize = Orientation == Orientation.Horizontal
                            ? LayoutInformation.GetLayoutSlot(BeforeElement).Height
                            : LayoutInformation.GetLayoutSlot(BeforeElement).Width,
                        AfterElementSize = Orientation == Orientation.Horizontal
                            ? LayoutInformation.GetLayoutSlot(AfterElement).Height
                            : LayoutInformation.GetLayoutSlot(AfterElement).Width,

                        MinOffset = resizeData.BeforeElementSize - 22.0,
                        MaxOffset = resizeData.AfterElementSize - 22.0,
                        PreviewAdorner = new SplitPreviewAdorner(this, null)
                    };

                    adornerLayer.Add(this.resizeData.PreviewAdorner);
                }
            }
        }

        private static void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            (sender as SplitContainerSplitter)?.OnDragStarted(e);
        }

        private void OnDragDelta(DragDeltaEventArgs e)
        {
            if (resizeData != null)
            {
                if (Orientation == Orientation.Vertical)
                {
                    resizeData.PreviewAdorner.OffsetX = Math.Min(Math.Max(e.HorizontalChange, -resizeData.MinOffset), resizeData.MaxOffset);
                }
                else
                {
                    resizeData.PreviewAdorner.OffsetY = Math.Min(Math.Max(e.VerticalChange, -resizeData.MinOffset), resizeData.MaxOffset);
                }
            }
        }

        private static void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            (sender as SplitContainerSplitter)?.OnDragDelta(e);
        }

        private void OnDragCompleted(DragCompletedEventArgs e)
        {
            if (resizeData != null)
            {
                if (!e.Canceled)
                {
                    Size beforeElementSize = SplitContainer.GetWorkingSize(BeforeElement);
                    Size afterElementSize = SplitContainer.GetWorkingSize(AfterElement);

                    double totalElementSize = beforeElementSize.Height + afterElementSize.Height;
                    double originalElementsTotalSize = this.resizeData.BeforeElementSize + resizeData.AfterElementSize;

                    if (Orientation == Orientation.Horizontal)
                    {
                        double delta = resizeData.PreviewAdorner.OffsetY / originalElementsTotalSize * totalElementSize;
                        SplitContainer.SetWorkingSize(BeforeElement, new Size(beforeElementSize.Width, beforeElementSize.Height + delta));
                        SplitContainer.SetWorkingSize(AfterElement, new Size(afterElementSize.Width, afterElementSize.Height - delta));
                    }
                    else
                    {
                        double delta = this.resizeData.PreviewAdorner.OffsetX / originalElementsTotalSize * totalElementSize;
                        SplitContainer.SetWorkingSize(BeforeElement, new Size(beforeElementSize.Width + delta, beforeElementSize.Height));
                        SplitContainer.SetWorkingSize(AfterElement, new Size(afterElementSize.Width - delta, afterElementSize.Height));
                    }
                }


                if (VisualTreeHelper.GetParent(resizeData.PreviewAdorner) is AdornerLayer adornerLayer)
                {
                    adornerLayer.Remove(resizeData.PreviewAdorner);
                }

                resizeData = null;
            }
        }

        private static void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            (sender as SplitContainerSplitter)?.OnDragCompleted(e);
        }
        #endregion


        #region Properties
        /// <summary>
        /// Gets the element after the splitter (right / bottom side).
        /// </summary>
        public FrameworkElement AfterElement { get; }

        /// <summary>
        /// Gets the element before the splitter (left / top side).
        /// </summary>
        public FrameworkElement BeforeElement { get; }

        /// <summary>
        /// Gets the splitter orientation.
        /// </summary>
        public Orientation Orientation { get; }

        /// <summary>
        /// Gets or sets the size of the splitter.
        /// </summary>
        internal double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }
        #endregion
    }
}
