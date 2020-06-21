using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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


        #region Construction
        public DockHierarchyPresenter(DockCanvas parent)
        {
            this.parent = parent;
        }
        #endregion


        #region FrameworkElement overrides
        protected override Size ArrangeOverride(Size finalSize)
        {
            // Calculate the final rectangle
            Rect final = new Rect(0.0, 0.0, finalSize.Width, finalSize.Height);

            // Iterate through the containers
            foreach(SplitContainer container in this.parent.SplitContainers)
            {
                // Determine the dock side of each split container
                Dock dockSide = DockCanvas.GetDock(container);

                // Depending on the dock side, we need to arrange the container differently
                switch(dockSide)
                {
                    case Dock.Left:
                        container.Arrange(new Rect(final.X, final.Y, container.DesiredSize.Width, final.Height));
                        final.X += container.DesiredSize.Width;
                        final.Width -= container.DesiredSize.Width;
                        break;
                    case Dock.Right:
                        container.Arrange(new Rect(final.Right - container.DesiredSize.Width, final.Y, container.DesiredSize.Width, final.Height));
                        final.Width -= container.DesiredSize.Width;
                        break;
                    case Dock.Top:
                        container.Arrange(new Rect(final.X, final.Y, final.Width, container.DesiredSize.Height));
                        final.Y += container.DesiredSize.Height;
                        final.Height -= container.DesiredSize.Height;
                        break;
                    case Dock.Bottom:
                        container.Arrange(new Rect(final.X, final.Bottom - container.DesiredSize.Height, final.Width, container.DesiredSize.Height));
                        final.Height -= container.DesiredSize.Height;
                        break;
                }
            }

            if (Child != null)
            {
                Child.Arrange(final);
            }

            // The size isn't changed
            return finalSize;
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            // This is the size that is taken by the split containers
            Size size = new Size(0.0, 0.0);

            // Iterate through all split containers
            foreach(SplitContainer container in this.parent.SplitContainers)
            {
                // Get the dock side
                Dock dockSide = DockCanvas.GetDock(container);

                // Calculate the available size first and measure the container
                Size available = new Size(
                    Math.Max(availableSize.Width - size.Width, 0.0),
                    Math.Max(availableSize.Height - size.Height, 0.0)
                    );
                container.Measure(available);

                // Depending on the dock side, we change the size accordingly
                switch(dockSide)
                {
                    case Dock.Top:
                    case Dock.Bottom:
                        size.Height += container.DesiredSize.Height;
                        break;
                    case Dock.Left:
                    case Dock.Right:
                        size.Width += container.DesiredSize.Width;
                        break;
                }
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
            // The first elements are the split containers of the DockCanvas
            if (index < this.parent.SplitContainers.Count)
            {
                return this.parent.SplitContainers[index];
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }
        protected override int VisualChildrenCount => this.parent.SplitContainers.Count;
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
                        this.RemoveVisualChild(child);
                    }

                    this.child = value;
                    if (this.child != null)
                    {
                        this.AddVisualChild(child);
                    }
                }
            }
        }
        #endregion
    }
}
