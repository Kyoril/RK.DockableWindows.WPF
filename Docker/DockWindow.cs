using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Docker
{
    [ContentProperty("Child")]
    public class DockWindow : Control
    {
        #region Dependency Properties
        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register(
                "Child",
                typeof(UIElement),
                typeof(DockWindow),
                new FrameworkPropertyMetadata(
                    null,
                    new PropertyChangedCallback(DockWindow.OnChildChanged)));
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                "IsSelected",
                typeof(bool),
                typeof(DockWindow));
        #endregion


        static DockWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockWindow), new FrameworkPropertyMetadata(typeof(DockWindow)));

            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(DockWindow), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(DockWindow), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
        }


        #region Dependency Property Callbacks
        private static void OnChildChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            DockWindow window = (DockWindow)dp;
            UIElement oldValue = (UIElement)e.OldValue;
            UIElement newValue = (UIElement)e.NewValue;

            if (oldValue != null)
            {
                window.RemoveLogicalChild(oldValue);
            }

            if (newValue != null)
            {
                window.AddLogicalChild(newValue);
            }
        }
        #endregion


        #region Control overrides
        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (this.Child != null)
                {
                    return new UIElement[] { Child }.GetEnumerator();
                }
                return null;
            }
        }
        #endregion


        #region Properties
        public UIElement Child
        {
            get => (UIElement)GetValue(ChildProperty);
            set => SetValue(ChildProperty, value);
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            internal set => SetValue(IsSelectedProperty, value);
        }
        #endregion
    }
}
