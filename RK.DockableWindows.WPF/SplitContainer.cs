using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace RK.DockableWindows.WPF
{
    /// <summary>
    /// This class represents a SplitContainer which is used to split dockable controls
    /// in a DockCanvas element.
    /// </summary>
    [ContentProperty("Children")]
    public class SplitContainer : FrameworkElement
    {
        private List<SplitContainerSplitter> splitters;
        private bool splittersInvalid;


        #region Events
        public event EventHandler ChildrenChanged;
        #endregion


        #region Dependeny Properties
        public static readonly DependencyProperty SplitterOrientationProperty =
            DependencyProperty.Register(
                "SplitterOrientation",
                typeof(Orientation),
                typeof(SplitContainer),
                new FrameworkPropertyMetadata(
                    Orientation.Horizontal,
                    FrameworkPropertyMetadataOptions.None,
                    OnSplitterOrientationChanged));

        public static readonly DependencyProperty WorkingSizeProperty =
            DependencyProperty.RegisterAttached(
                "WorkingSize",
                typeof(Size),
                typeof(SplitContainer),
                new FrameworkPropertyMetadata(
                    new Size(240.0, 180.0),
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        #endregion


        #region Dependency Property Callbacks
        private static void OnSplitterOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SplitContainer)?.OnSplitterOrientationChanged();
        }
        #endregion


        #region Construction
        static SplitContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitContainer), new FrameworkPropertyMetadata(typeof(SplitContainer)));
        }

        public SplitContainer()
        {
            // Setup splitters
            splitters = new List<SplitContainerSplitter>();

            // Setup the content collection
            Children = new SplitContainerContentCollection(this);
        }
        #endregion


        #region Internal Methods
        private void OnSplitterOrientationChanged()
        {
            InvalidateMeasure();
        }

        private double GetTotalDesiredExtent()
        {
            double num = 0.0;

            foreach (UIElement element in Children)
            {
                Size workingSize = GetWorkingSize(element);
                num += (SplitterOrientation == Orientation.Horizontal) ? workingSize.Height : workingSize.Width;
            }

            return num;
        }

        private double GetTotalSplitterExtent()
        {
            double extent = 0.0;

            foreach (SplitContainerSplitter splitter in splitters)
            {
                extent += (SplitterOrientation == Orientation.Horizontal) ? splitter.DesiredSize.Height : splitter.DesiredSize.Width;
            }

            return extent;
        }

        private void RecreateSplitters()
        {
            // Avoid recreating of splitters when accessing any other method
            splittersInvalid = false;

            // Remove old splitters
            foreach (SplitContainerSplitter splitter in splitters)
            {
                RemoveVisualChild(splitter);
            }

            var containerSplitters = new List<SplitContainerSplitter>();

            FrameworkElement beforeElement = null;

            // Iterate through each child
            foreach (FrameworkElement child in Children)
            {
                if (beforeElement != null)
                {
                    SplitContainerSplitter splitter = new SplitContainerSplitter(beforeElement, child, SplitterOrientation);
                    AddVisualChild(splitter);
                    containerSplitters.Add(splitter);
                }

                beforeElement = child;
            }

            // We have new splitters
            splitters = containerSplitters;
        }
        #endregion


        #region Internal Properties
        internal double ContentSize
        {
            get => (double)GetValue(DockCanvas.ContentSizeProperty);
            set => SetValue(DockCanvas.ContentSizeProperty, value);
        }
        #endregion


        #region Public Methods
        [AttachedPropertyBrowsableForChildren]
        public static Size GetWorkingSize(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return (Size)element.GetValue(WorkingSizeProperty);
        }

        public static void SetWorkingSize(UIElement element, Size size)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            element.SetValue(WorkingSizeProperty, size);
        }

        internal void Remove()
        {
            // Check if this split container is in another split container
            if (Parent is SplitContainer parent)
            {
                // It is, so remove it
                parent.Children.Remove(this);

                // And again, check if the parent split container now is empty and if so,
                // remove it as well.
                if (parent.Children.Count == 0)
                {
                    parent.Remove();
                }
            }
            else
            {
                // Otherwise, check if this split container is part of a DockCanvas and remove it there
                if (Parent is DockCanvas canvas && canvas.SplitContainers.Contains(this))
                {
                    canvas.SplitContainers.Remove(this);
                }
            }
        }
        #endregion


        #region FrameworkElement overrides
        protected override Size ArrangeOverride(Size finalSize)
        {
            double leadingSize = (SplitterOrientation == Orientation.Horizontal) ? finalSize.Height : finalSize.Width;
            leadingSize = Math.Max(leadingSize - GetTotalSplitterExtent(), 0.0);

            double totalDesiredExtent = GetTotalDesiredExtent();
            double y = 0.0;

            int index = 0;
            bool updateSplitters = false;

            foreach (UIElement element in Children)
            {
                // There is a splitter between two elements, so for the first element, we skip
                // updating the respective splitter
                if (updateSplitters)
                {
                    SplitContainerSplitter splitter = splitters[index++];

                    Rect rect = (SplitterOrientation == Orientation.Horizontal) ? new Rect(0.0, y, finalSize.Width, splitter.DesiredSize.Height) : new Rect(y, 0.0, splitter.DesiredSize.Width, finalSize.Height);
                    splitter.Arrange(rect);

                    y += (SplitterOrientation == Orientation.Horizontal) ? splitter.DesiredSize.Height : splitter.DesiredSize.Width;
                }

                Size workingSize = GetWorkingSize(element);

                double leadWorkingSize = (SplitterOrientation == Orientation.Horizontal) ? workingSize.Height : workingSize.Width;
                leadWorkingSize = leadWorkingSize / totalDesiredExtent * leadingSize;
                leadWorkingSize = Math.Max(leadWorkingSize, 18.0);

                Rect finalRect = (SplitterOrientation == Orientation.Horizontal) ? new Rect(0.0, y, finalSize.Width, leadWorkingSize) : new Rect(y, 0.0, leadWorkingSize, finalSize.Height);
                element.Arrange(finalRect);

                y += leadWorkingSize;

                // From now on, update splitters
                updateSplitters = true;
            }

            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size size;

            if (splittersInvalid)
            {
                RecreateSplitters();
            }

            // Depending on the dock side, use an alternative size value
            Dock dockSide = DockCanvas.GetDock(this);
            switch (dockSide)
            {
                case Dock.Top:
                case Dock.Bottom:
                    size = new Size(availableSize.Width, ContentSize);
                    break;
                default:
                    size = new Size(ContentSize, availableSize.Height);
                    break;
            }

            // Measure all splitters
            foreach (SplitContainerSplitter splitter in splitters)
            {
                splitter.Measure(size);
            }

            double elementSize = Math.Max(SplitterOrientation == Orientation.Horizontal ? size.Height - GetTotalSplitterExtent() : size.Width - GetTotalSplitterExtent(), 0.0);
            double totalDesiredExtent = GetTotalDesiredExtent();

            foreach (FrameworkElement element in Children)
            {
                if (this.SplitterOrientation == Orientation.Horizontal)
                {
                    double height = GetWorkingSize(element).Height / totalDesiredExtent * elementSize;
                    element.Measure(new Size(size.Width, height));
                }
                else
                {
                    double width = GetWorkingSize(element).Width / totalDesiredExtent * elementSize;
                    element.Measure(new Size(width, size.Height));
                }
            }

            return size;
        }

        protected override IEnumerator LogicalChildren => Children.GetEnumerator();

        protected override int VisualChildrenCount
        {
            get
            {
                if (splittersInvalid)
                {
                    RecreateSplitters();
                }

                return Children.Count + splitters.Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (splittersInvalid)
            {
                RecreateSplitters();
            }

            if (index < Children.Count)
            {
                return Children[index];
            }

            index -= Children.Count;
            return splitters[index];
        }
        #endregion


        #region Internals
        internal void AddLogicalChildInternal(FrameworkElement element)
        {
            AddLogicalChild(element);
        }
        internal void RemoveLogicalChildInternal(FrameworkElement element)
        {
            RemoveLogicalChild(element);
        }
        internal void AddVisualChildInternal(Visual child)
        {
            AddVisualChild(child);
        }
        internal void RemoveVisualChildInternal(Visual child)
        {
            RemoveVisualChild(child);
        }
        internal void OnChildrenChanging()
        {
            splittersInvalid = true;
            InvalidateMeasure();
        }
        internal void OnChildrenChanged()
        {
            splittersInvalid = true;
            InvalidateMeasure();
            ChildrenChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion


        #region Properties
        /// <summary>
        /// The content property of the SplitContainer.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public SplitContainerContentCollection Children { get; }

        [Category("Common Properties")]
        public Orientation SplitterOrientation
        {
            get => (Orientation)GetValue(SplitterOrientationProperty);
            set => SetValue(SplitterOrientationProperty, value);
        }
        #endregion
    }
}
