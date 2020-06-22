using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Docker
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
                    new PropertyChangedCallback(SplitContainer.OnSplitterOrientationChanged)));
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
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitContainer), new FrameworkPropertyMetadata(typeof(SplitContainer)));
        }

        public SplitContainer()
        {
            // Setup splitters
            this.splitters = new List<SplitContainerSplitter>();

            // Setup the content collection
            this.Children = new SplitContainerContentCollection(this);
        }
        #endregion


        #region Internal Methods
        private void OnSplitterOrientationChanged()
        {
            this.InvalidateMeasure();
        }
        private double GetTotalDesiredExtent()
        {
            double num = 0.0;

            foreach (UIElement element in this.Children)
            {
                Size workingSize = GetWorkingSize(element);
                num += (this.SplitterOrientation == Orientation.Horizontal) ? workingSize.Height : workingSize.Width;
            }

            return num;
        }
        private double GetTotalSplitterExtent()
        {
            double extent = 0.0;

            foreach (SplitContainerSplitter splitter in this.splitters)
            {
                extent += (this.SplitterOrientation == Orientation.Horizontal) ? splitter.DesiredSize.Height : splitter.DesiredSize.Width;
            }

            return extent;
        }
        private void RecreateSplitters()
        {
            // Avoid recreating of splitters when accessing any other method
            this.splittersInvalid = false;

            // Remove old splitters
            foreach (SplitContainerSplitter splitter in this.splitters)
            {
                this.RemoveVisualChild(splitter);
            }

            List<SplitContainerSplitter> splitters = new List<SplitContainerSplitter>();
            List<FrameworkElement> elements = new List<FrameworkElement>();

            FrameworkElement beforeElement = null;

            // Iterate through each child
            foreach (FrameworkElement child in Children)
            {
                if (beforeElement != null)
                {
                    SplitContainerSplitter splitter = new SplitContainerSplitter(beforeElement, child, this.SplitterOrientation);
                    base.AddVisualChild(splitter);
                    splitters.Add(splitter);
                }

                beforeElement = child;
                elements.Add(child);
            }
            
            // We have new splitters
            this.splitters = splitters;
        }
        #endregion


        #region Internal Properties
        internal double ContentSize
        {
            get => (double)base.GetValue(DockCanvas.ContentSizeProperty);
            set => base.SetValue(DockCanvas.ContentSizeProperty, value);
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
            return (Size)element.GetValue(SplitContainer.WorkingSizeProperty);
        }
        public static void SetWorkingSize(UIElement element, Size size)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
            element.SetValue(SplitContainer.WorkingSizeProperty, size);
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
        }

        internal void Remove()
        {
            // Check if this split container is in another split container
            if (this.Parent is SplitContainer parent)
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
                DockCanvas canvas = this.Parent as DockCanvas;
                if ((canvas != null) && canvas.SplitContainers.Contains(this))
                {
                    canvas.SplitContainers.Remove(this);
                }
            }
        }
        #endregion


        #region FrameworkElement overrides
        protected override Size ArrangeOverride(Size finalSize)
        {
            double leadingSize = (this.SplitterOrientation == Orientation.Horizontal) ? finalSize.Height : finalSize.Width;
            leadingSize = Math.Max(leadingSize - this.GetTotalSplitterExtent(), 0.0);

            double totalDesiredExtent = this.GetTotalDesiredExtent();
            double y = 0.0;

            int index = 0;
            bool updateSplitters = false;

            foreach (UIElement element in this.Children)
            {
                // There is a splitter between two elements, so for the first element, we skip
                // updating the respective splitter
                if (updateSplitters)
                {
                    SplitContainerSplitter splitter = this.splitters[index++];

                    Rect rect = (this.SplitterOrientation == Orientation.Horizontal) ? new Rect(0.0, y, finalSize.Width, splitter.DesiredSize.Height) : new Rect(y, 0.0, splitter.DesiredSize.Width, finalSize.Height);
                    splitter.Arrange(rect);

                    y += (this.SplitterOrientation == Orientation.Horizontal) ? splitter.DesiredSize.Height : splitter.DesiredSize.Width;
                }

                Size workingSize = GetWorkingSize(element);

                double leadWorkingSize = (this.SplitterOrientation == Orientation.Horizontal) ? workingSize.Height : workingSize.Width;
                leadWorkingSize = leadWorkingSize / totalDesiredExtent * leadingSize;
                leadWorkingSize = Math.Max(leadWorkingSize, 18.0);

                Rect finalRect = (this.SplitterOrientation == Orientation.Horizontal) ? new Rect(0.0, y, finalSize.Width, leadWorkingSize) : new Rect(y, 0.0, leadWorkingSize, finalSize.Height);
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
            
            if (this.splittersInvalid)
            {
                this.RecreateSplitters();
            }

            // Depending on the dock side, use an alternative size value
            Dock dockSide = DockCanvas.GetDock(this);
            switch(dockSide)
            {
                case Dock.Top:
                case Dock.Bottom:
                    size = new Size(availableSize.Width, this.ContentSize);
                    break;
                default:
                    size = new Size(this.ContentSize, availableSize.Height);
                    break;
            }

            // Measure all splitters
            foreach (SplitContainerSplitter splitter in this.splitters)
            {
                splitter.Measure(size);
            }
            
            double elementSize = Math.Max((this.SplitterOrientation == Orientation.Horizontal) ? size.Height - this.GetTotalSplitterExtent() : size.Width - this.GetTotalSplitterExtent(), 0.0);
            double totalDesiredExtent = this.GetTotalDesiredExtent();

            foreach (FrameworkElement element in this.Children)
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
        protected override IEnumerator LogicalChildren => this.Children.GetEnumerator();
        protected override int VisualChildrenCount
        {
            get
            {
                if (this.splittersInvalid)
                {
                    this.RecreateSplitters();
                }

                return this.Children.Count + this.splitters.Count;
            }
        }
        protected override Visual GetVisualChild(int index)
        {
            if (this.splittersInvalid)
            {
                this.RecreateSplitters();
            }

            if (index < this.Children.Count)
            {
                return this.Children[index];
            }

            index -= this.Children.Count;
            return this.splitters[index];
        }
        #endregion


        #region Internals
        internal void AddLogicalChildInternal(FrameworkElement element)
        {
            this.AddLogicalChild(element);
        }
        internal void RemoveLogicalChildInternal(FrameworkElement element)
        {
            this.RemoveLogicalChild(element);
        }
        internal void AddVisualChildInternal(Visual child)
        {
            this.AddVisualChild(child);
        }
        internal void RemoveVisualChildInternal(Visual child)
        {
            this.RemoveVisualChild(child);
        }
        internal void OnChildrenChanging()
        {
            this.splittersInvalid = true;
            this.InvalidateMeasure();
        }
        internal void OnChildrenChanged()
        {
            this.splittersInvalid = true;
            this.InvalidateMeasure();
            this.ChildrenChanged?.Invoke(this, EventArgs.Empty);
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
            get => (Orientation)base.GetValue(SplitterOrientationProperty);
            set => base.SetValue(SplitterOrientationProperty, value);
        }
        #endregion
    }
}
