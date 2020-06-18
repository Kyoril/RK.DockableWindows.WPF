using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Docker
{
    /// <summary>
    /// This control wraps around dockable windows. It contains a title bar, a content area and
    /// a tab strip below (although the layout is dynamically defined using a style and can be
    /// completely changed).
    /// </summary>
    [TemplatePart(Name = "PART_TitleBar", Type = typeof(FrameworkElement))]
    public class WindowGroup : Control
    {  
        #region Routed Commands
        /// <summary>
        /// A command to toggle the pinned state of a WindowGroup. A window group can be unpinned to minimize it.
        /// </summary>
        public static readonly RoutedCommand TogglePinCommand = new RoutedCommand("TogglePin", typeof(WindowGroup));
        #endregion


        #region Construction
        static WindowGroup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowGroup), new FrameworkPropertyMetadata(typeof(WindowGroup)));

            // Prevent the WindowGroup control from being focused
            FocusableProperty.OverrideMetadata(typeof(WindowGroup), new FrameworkPropertyMetadata(false));
        }
        #endregion


    }
}
