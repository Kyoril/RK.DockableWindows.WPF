using System.Windows;
using System.Windows.Controls;

namespace Docker
{
    /// <summary>
    /// This class manages the DockWindows in a WindowGroup and presents them as tabs in the WindowGroup.
    /// </summary>
    public class WindowList : ItemsControl
    {
        #region ItemsControl overrides
        protected override DependencyObject GetContainerForItemOverride()
        {
            // Child elements will be WindowTab elements instead of DockWindow
            return new WindowTab();
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            WindowTab tab = (WindowTab)element;
            tab.Window = null;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is WindowTab;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            WindowTab tab = (WindowTab)element;
            DockWindow window = (DockWindow)item;

            tab.Window = window;
        }
        #endregion
    }
}
