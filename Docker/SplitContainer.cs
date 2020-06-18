using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
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
        #region Events
        public event EventHandler ChildrenChanged;
        #endregion


        #region Construction
        public SplitContainer()
        {
            // Setup the content collection
            this.Children = new SplitContainerContentCollection(this);
        }
        #endregion
        

        #region FrameworkElement overrides
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement element in this.Children)
            {
                element.Arrange(new Rect(0.0, 0.0, finalSize.Width, finalSize.Height));
            }

            return finalSize;
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            //TODO
            
            return availableSize;
        }
        protected override IEnumerator LogicalChildren => this.Children.GetEnumerator();
        protected override int VisualChildrenCount => this.Children.Count;
        protected override Visual GetVisualChild(int index)
        {
            return this.Children[index];
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
            InvalidateMeasure();
        }
        internal void OnChildrenChanged()
        {
            InvalidateMeasure();
            this.ChildrenChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion


        #region Properties
        /// <summary>
        /// The content property of the SplitContainer.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public SplitContainerContentCollection Children { get; }
        #endregion
    }
}
