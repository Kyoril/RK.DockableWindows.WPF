using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Docker
{
    /// <summary>
    /// This class contains all dockable elements and wraps around them. Usually, you will
    /// have exactly one DockCanvas in an application which fills most of the window's space.
    /// </summary>
    [ContentProperty("Child")]
    public class DockCanvas : Control
    {
        #region Construction
        static DockCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockCanvas), new FrameworkPropertyMetadata(typeof(DockCanvas)));
        }
        #endregion


        #region Properties
        private UIElement child;
        /// <summary>
        /// The content property of this control.
        /// </summary>
        public UIElement Child
        {
            get => this.child;
            set
            {
                if (value != this.child)
                {
                    if (this.child != null)
                    {
                        this.RemoveLogicalChild(Child);
                    }

                    this.child = value;

                    if (this.child != null)
                    {
                        this.AddLogicalChild(Child);
                    }
                }
            }
        }
        #endregion
    }
}
