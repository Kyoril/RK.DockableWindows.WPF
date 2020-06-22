using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            WindowTab tab = (WindowTab)dp;
            DockWindow oldValue = (DockWindow)e.OldValue;
            DockWindow newValue = (DockWindow)e.NewValue;

            if (oldValue != null)
            {
                TypeDescriptor.GetProperties(oldValue)["IsSelected"].RemoveValueChanged(oldValue, new EventHandler(tab.OnIsSelectedChanged));
            }
            if (newValue != null)
            {
                TypeDescriptor.GetProperties(newValue)["IsSelected"].AddValueChanged(newValue, new EventHandler(tab.OnIsSelectedChanged));
            }
        }

        private void OnIsSelectedChanged(object sender, EventArgs e)
        {
            this.UpdateZIndex();
        }
        internal void UpdateZIndex()
        {
            if ((this.Window != null) && this.Window.IsSelected)
            {
                Panel.SetZIndex(this, 9000);
            }
            else
            {
                Panel.SetZIndex(this, 0);
            }
        }
        #endregion


        #region Control overrides
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // Mouse clicks are handled anywhere
            e.Handled = true;

            // Activate the window
            if (this.Window != null)
            {
                Window.SelectAndPopup();
            }
        }
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (this.Window != null)
            {
                this.Window.SelectAndPopup();
            }
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
