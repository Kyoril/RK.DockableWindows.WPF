using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Docker
{
    /// <summary>
    /// A splitter control that is used by the SplitContainer control. These splitters allow
    /// the user to resize WindowGroup controls inside a SplitContainer.
    /// </summary>
    public class SplitContainerSplitter : Thumb
    {
        private sealed class ResizeData
        {
            public double maxOffset;
            public double minOffset;
            public double afterElementSize;
            public SplitPreviewAdorner previewAdorner;
            public SplitContainer parent;
            public double beforeElementSize;
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
                new ValidateValueCallback((o) => (double)o > 0.0));
        #endregion


        #region Construction
        /// <summary>
        /// Initializes static WPF settings.
        /// </summary>
        static SplitContainerSplitter()
        {
            // Allow custom styling
            Thumb.DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitContainerSplitter), new FrameworkPropertyMetadata(typeof(SplitContainerSplitter)));

            // Subscribe event handlers
            EventManager.RegisterClassHandler(typeof(SplitContainerSplitter), Thumb.DragStartedEvent, new DragStartedEventHandler(SplitContainerSplitter.OnDragStarted));
            EventManager.RegisterClassHandler(typeof(SplitContainerSplitter), Thumb.DragDeltaEvent, new DragDeltaEventHandler(SplitContainerSplitter.OnDragDelta));
            EventManager.RegisterClassHandler(typeof(SplitContainerSplitter), Thumb.DragCompletedEvent, new DragCompletedEventHandler(SplitContainerSplitter.OnDragCompleted));
        }
        /// <summary>
        /// Initializes a new instance of the SplitContainerSplitter class.
        /// </summary>
        /// <param name="beforeElement">The element before the splitter.</param>
        /// <param name="afterElement">The element after the splitter.</param>
        /// <param name="splitterOrientation">The splitter orientation.</param>
        internal SplitContainerSplitter(FrameworkElement beforeElement, FrameworkElement afterElement, Orientation splitterOrientation)
        {
            this.BeforeElement = beforeElement;
            this.AfterElement = afterElement;
            this.Orientation = splitterOrientation;
            base.Cursor = (splitterOrientation == Orientation.Horizontal) ? Cursors.SizeNS : Cursors.SizeWE;
        }
        #endregion


        #region Thumb Overrides
        protected override Size MeasureOverride(Size constraint)
        {
            return new Size(this.Size, this.Size);
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
                    this.resizeData = new ResizeData();
                    this.resizeData.parent = splitContainer;
                    this.resizeData.beforeElementSize = (Orientation == Orientation.Horizontal) ? LayoutInformation.GetLayoutSlot(BeforeElement).Height : LayoutInformation.GetLayoutSlot(BeforeElement).Width;
                    this.resizeData.afterElementSize = (Orientation == Orientation.Horizontal) ? LayoutInformation.GetLayoutSlot(AfterElement).Height : LayoutInformation.GetLayoutSlot(AfterElement).Width;
                    this.resizeData.minOffset = resizeData.beforeElementSize - 22.0;
                    this.resizeData.maxOffset = resizeData.afterElementSize - 22.0;
                    this.resizeData.previewAdorner = new SplitPreviewAdorner(this, null);
                    adornerLayer.Add(this.resizeData.previewAdorner);
                }
            }
        }
        private static void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            (sender as SplitContainerSplitter).OnDragStarted(e);
        }
        private void OnDragDelta(DragDeltaEventArgs e)
        {
            if (this.resizeData != null)
            {
                if (Orientation == Orientation.Vertical)
                {
                    this.resizeData.previewAdorner.OffsetX = Math.Min(Math.Max(e.HorizontalChange, -this.resizeData.minOffset), this.resizeData.maxOffset);
                }
                else
                {
                    this.resizeData.previewAdorner.OffsetY = Math.Min(Math.Max(e.VerticalChange, -this.resizeData.minOffset), this.resizeData.maxOffset);
                }
            }
        }
        private static void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            (sender as SplitContainerSplitter).OnDragDelta(e);
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
                    double oritinalElementsTotalSize = this.resizeData.beforeElementSize + resizeData.afterElementSize;

                    if (Orientation == Orientation.Horizontal)
                    {
                        double delta = resizeData.previewAdorner.OffsetY / oritinalElementsTotalSize * totalElementSize;
                        SplitContainer.SetWorkingSize(BeforeElement, new Size(beforeElementSize.Width, beforeElementSize.Height + delta));
                        SplitContainer.SetWorkingSize(AfterElement, new Size(afterElementSize.Width, afterElementSize.Height - delta));
                    }
                    else
                    {
                        double delta = this.resizeData.previewAdorner.OffsetX / oritinalElementsTotalSize * totalElementSize;
                        SplitContainer.SetWorkingSize(BeforeElement, new Size(beforeElementSize.Width + delta, beforeElementSize.Height));
                        SplitContainer.SetWorkingSize(AfterElement, new Size(afterElementSize.Width - delta, afterElementSize.Height));
                    }
                }


                if (VisualTreeHelper.GetParent(this.resizeData.previewAdorner) is AdornerLayer adornerLayer)
                {
                    adornerLayer.Remove(this.resizeData.previewAdorner);
                }

                resizeData = null;
            }
        }
        private static void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            (sender as SplitContainerSplitter).OnDragCompleted(e);
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
            get => (double)GetValue(SplitContainerSplitter.SizeProperty);
            set => SetValue(SplitContainerSplitter.SizeProperty, value);
        }
        #endregion
    }
}
