using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
            return base.ArrangeOverride(finalSize);
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            return base.MeasureOverride(availableSize);
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
    }
}
