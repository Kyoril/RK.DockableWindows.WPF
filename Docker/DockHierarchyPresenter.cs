using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Docker
{
    /// <summary>
    /// This class presents the actual dock hierarchy, which means it presents all SplitContainers
    /// of it's parent DockCanvas.
    /// </summary>
    public class DockHierarchyPresenter : FrameworkElement
    {
        private DockCanvas parent;
        private List<ResizeControlSplitter> splitters;
        private bool splittersInvalid;


        #region Construction
        static DockHierarchyPresenter()
        {
            DockHierarchyPresenter.ClipToBoundsProperty.OverrideMetadata(typeof(DockHierarchyPresenter), new FrameworkPropertyMetadata(true));
        }
        public DockHierarchyPresenter(DockCanvas parent)
        {
            this.splitters = new List<ResizeControlSplitter>();
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }
        #endregion


        #region Internal Methods
        private void RecreateSplitters()
        {
            this.splittersInvalid = false;

            // Remove all old splitter controls
            foreach (ResizeControlSplitter splitter in this.splitters)
            {
                this.RemoveVisualChild(splitter);
            }

            // Empty splitter list
            this.splitters.Clear();

            // Create splitters
            foreach(SplitContainer container in this.parent.SplitContainers)
            {
                var splitter = new ResizeControlSplitter(this.parent, container);

                Binding binding = new Binding
                {
                    Source = container,
                    Path = new PropertyPath(DockCanvas.DockProperty),
                    Mode = BindingMode.OneWay
                };

                splitter.SetBinding(DockPanel.DockProperty, binding);

                this.splitters.Add(splitter);
                this.AddVisualChild(splitter);
            }
        }
        internal void InvalidateSplitters()
        {
            this.splittersInvalid = true;
        }
        #endregion


        #region FrameworkElement overrides
        protected override Size ArrangeOverride(Size finalSize)
        {
            // Calculate the final rectangle
            Rect final = new Rect(
                0.0, 
                0.0, 
                finalSize.Width, 
                finalSize.Height);

            int index = 0;

            // Iterate through the containers
            foreach(SplitContainer container in this.parent.SplitContainers)
            {
                // Determine the dock side of each split container
                Dock dockSide = DockCanvas.GetDock(container);

                // Depending on the dock side, we need to arrange the container differently
                switch(dockSide)
                {
                    case Dock.Left:
                        container.Arrange(
                            new Rect(
                                final.X, 
                                final.Y, 
                                container.DesiredSize.Width, 
                                final.Height));

                        this.splitters[index].Arrange(
                            new Rect(
                                final.X + container.DesiredSize.Width, 
                                final.Y, 
                                this.splitters[index].DesiredSize.Width, 
                                final.Height));

                        final.X += container.DesiredSize.Width + this.splitters[index].DesiredSize.Width;
                        final.Width -= container.DesiredSize.Width + this.splitters[index].DesiredSize.Width;
                        break;

                    case Dock.Top:
                        container.Arrange(
                            new Rect(
                                final.X, 
                                final.Y, 
                                final.Width, 
                                container.DesiredSize.Height));

                        this.splitters[index].Arrange(
                            new Rect(
                                final.X, 
                                final.Y + container.DesiredSize.Height, 
                                final.Width, 
                                this.splitters[index].DesiredSize.Height));

                        final.Y += container.DesiredSize.Height + this.splitters[index].DesiredSize.Height;
                        final.Height -= container.DesiredSize.Height + this.splitters[index].DesiredSize.Height;
                        break;

                    case Dock.Right:
                        container.Arrange(
                            new Rect(
                                final.Right - container.DesiredSize.Width,
                                final.Y,
                                container.DesiredSize.Width,
                                final.Height));

                        this.splitters[index].Arrange(
                            new Rect(
                                final.Right - container.DesiredSize.Width - this.splitters[index].DesiredSize.Width,
                                final.Y,
                                this.splitters[index].DesiredSize.Width,
                                final.Height));

                        final.Width -= container.DesiredSize.Width + this.splitters[index].DesiredSize.Width;
                        break;

                    case Dock.Bottom:
                        container.Arrange(
                            new Rect(
                                final.X,
                                final.Bottom - container.DesiredSize.Height,
                                final.Width,
                                container.DesiredSize.Height));

                        this.splitters[index].Arrange(
                            new Rect(
                                final.X,
                                final.Bottom - container.DesiredSize.Height - this.splitters[index].DesiredSize.Height,
                                final.Width,
                                this.splitters[index].DesiredSize.Height));

                        final.Height -= container.DesiredSize.Height + this.splitters[index].DesiredSize.Height;
                        break;
                }

                // Handle next splitter
                index++;
            }

            if (Child != null)
            {
                Child.Arrange(final);
            }

            // Update client bounds (needed for splitter preview)
            this.ClientBounds = final;

            // The size isn't changed
            return finalSize;
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            // This is the size that is taken by the split containers
            Size size = new Size(0.0, 0.0);

            if (this.splittersInvalid)
            {
                this.RecreateSplitters();
            }

            int index = 0;

            // Iterate through all split containers
            foreach (SplitContainer container in this.parent.SplitContainers)
            {
                // Get the dock side
                Dock dockSide = DockCanvas.GetDock(container);

                // Calculate the available size first and measure the container
                Size available = new Size(
                    Math.Max(availableSize.Width - size.Width, 0.0),
                    Math.Max(availableSize.Height - size.Height, 0.0)
                    );
                container.Measure(available);

                double splitterSize = 0.0f;

                // Depending on the dock side, we change the size accordingly
                switch(dockSide)
                {
                    case Dock.Top:
                    case Dock.Bottom:
                        size.Height += splitterSize = container.DesiredSize.Height;
                        break;
                    case Dock.Left:
                    case Dock.Right:
                        size.Width += splitterSize = container.DesiredSize.Width;
                        break;
                }

                available = new Size(
                    Math.Max(availableSize.Width - size.Width, 0.0), 
                    Math.Max(availableSize.Height - size.Height, 0.0));
                splitters[index].Measure(available);

                if (splitterSize > 0.0)
                {
                    switch (dockSide)
                    {
                        case Dock.Top:
                        case Dock.Bottom:
                            size.Height += this.splitters[index].DesiredSize.Height;
                            break;
                        case Dock.Left:
                        case Dock.Right:
                            size.Width += this.splitters[index].DesiredSize.Width;
                            break;
                    }
                }

                index++;
            }

            if (Child != null)
            {
                Child.Measure(
                    new Size(
                        Math.Max(availableSize.Width - size.Width, 0.0),
                        Math.Max(availableSize.Height - size.Height, 0.0)));
            }

            return size;
        }
        protected override Visual GetVisualChild(int index)
        {
            if (this.splittersInvalid)
            {
                this.RecreateSplitters();
            }

            // The first elements are the split containers of the DockCanvas
            if (index < this.parent.SplitContainers.Count)
            {
                return this.parent.SplitContainers[index];
            }

            // Reduce the index to check if the child is requested
            index -= this.parent.SplitContainers.Count;

            // Map index to splitters next
            if (index < this.splitters.Count)
            {
                return this.splitters[index];
            }

            // Map index to child now
            index -= this.splitters.Count;
            if (this.Child != null)
            {
                if (index == 0)
                {
                    return this.Child;
                }

                index--;
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }
        protected override int VisualChildrenCount
        {
            get
            {
                if (this.splittersInvalid)
                {
                    this.RecreateSplitters();
                }

                int count = this.parent.SplitContainers.Count + this.splitters.Count;

                // Add child window
                if (this.Child != null)
                {
                    count++;
                }

                return count;
            }
        }
        #endregion


        #region Properties
        private UIElement child;
        internal UIElement Child
        {
            get => this.child;
            set
            {
                if (value != this.child)
                {
                    if (this.child != null)
                    {
                        this.RemoveVisualChild(this.child);
                    }

                    this.child = value;

                    if (this.child != null)
                    {
                        this.AddVisualChild(this.child);
                    }
                }
            }
        }
        public Rect ClientBounds { get; private set; }
        #endregion
    }
}
