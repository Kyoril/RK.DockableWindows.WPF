using System.Windows;
using System.Windows.Controls;

namespace Docker
{
    /// <summary>
    /// This control represents a single tab at the bottom of a WindowGroup. This tab can be clicked
    /// and dragged to activate / reorder the DockWindow controls in a WindowGroup.
    /// </summary>
    public class WindowTab : Control
    {
        #region Dependency Properties
        public static readonly DependencyProperty WindowProperty = 
            DependencyProperty.Register(
                "Window", 
                typeof(DockWindow),
                typeof(WindowTab), 
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(WindowTab.OnWindowChanged)));
        #endregion


        #region Construction
        static WindowTab()
        {
            WindowTab.DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowTab), new FrameworkPropertyMetadata(typeof(WindowTab)));
            WindowTab.FocusableProperty.OverrideMetadata(typeof(WindowTab), new FrameworkPropertyMetadata(false));
        }
        #endregion


        #region Dependency Property Callbacks
        private static void OnWindowChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            // TODO
        }
        #endregion


        #region Properties
        public DockWindow Window
        {
            get => (DockWindow)GetValue(WindowTab.WindowProperty);
            set => SetValue(WindowTab.WindowProperty, value);
        }
        #endregion
    }
}
