using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace RK.DockableWindows.WPF
{
    /// <summary>
    /// This control wraps around dockable windows. It contains a title bar, a content area and
    /// a tab strip below (although the layout is dynamically defined using a style and can be
    /// completely changed).
    /// </summary>
    [TemplatePart(Name = "PART_TitleBar", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_WindowList", Type = typeof(WindowList))]
    [ContentProperty("Windows")]
    public class WindowGroup : Control
    {
        private FrameworkElement titleBar;


        #region Routed Commands
        /// <summary>
        /// A command to toggle the pinned state of a WindowGroup. A window group can be unpinned to minimize it.
        /// </summary>
        public static readonly RoutedCommand TogglePinCommand = new RoutedCommand("TogglePin", typeof(WindowGroup));
        #endregion


        #region Dependency Properties

        public static readonly DependencyProperty SelectedWindowProperty =
            DependencyProperty.Register(
                "SelectedWindow",
                typeof(DockWindow),
                typeof(WindowGroup),
                new FrameworkPropertyMetadata(OnSelectedWindowChanged));

        private static readonly DependencyPropertyKey HasMultipleWindowsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "HasMultipleWindows",
                typeof(bool),
                typeof(WindowGroup),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasMultipleWindowsProperty = HasMultipleWindowsPropertyKey.DependencyProperty;
        #endregion


        #region Dependency Property Callbacks
        private static void OnSelectedWindowChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            DockWindow oldValue = (DockWindow)e.OldValue;
            DockWindow newValue = (DockWindow)e.NewValue;

            if (oldValue != null)
            {
                oldValue.IsSelected = false;
            }
            if (newValue != null)
            {
                newValue.IsSelected = true;
            }
        }
        #endregion


        #region Construction
        static WindowGroup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowGroup), new FrameworkPropertyMetadata(typeof(WindowGroup)));

            // Prevent the WindowGroup control from being focused
            FocusableProperty.OverrideMetadata(typeof(WindowGroup), new FrameworkPropertyMetadata(false));
        }

        public WindowGroup()
        {
            this.Windows = new DockWindowCollection(this);
        }
        #endregion


        #region Control overrides
        protected override IEnumerator LogicalChildren => Windows.GetEnumerator();

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (titleBar != null)
            {
                titleBar.PreviewMouseDown -= OnTitleBarPreviewMouseDown;
            }

            titleBar = GetTemplateChild("PART_TitleBar") as FrameworkElement;

            if (titleBar != null)
            {
                titleBar.PreviewMouseDown += OnTitleBarPreviewMouseDown;
            }
        }
        #endregion


        #region Internal methods
        internal void AddLogicalChildInternal(DockWindow window)
        {
            AddLogicalChild(window);
        }

        internal void RemoveLogicalChildInternal(DockWindow window)
        {
            RemoveLogicalChild(window);
        }

        internal void OnChildrenChanged()
        {
            // Mark the first available window the selected one if there isn't any already and
            // if there are windows available at all
            if (SelectedWindow == null && Windows.Count != 0)
            {
                SelectedWindow = Windows[0];
            }

            // If there is a selected window, but it isn't registered as a child window of this group,
            // try to find a new selected window (or reset to null).
            if (SelectedWindow != null && !Windows.Contains(SelectedWindow))
            {
                if (Windows.Count != 0)
                {
                    SelectedWindow = Windows[0];
                }
                else
                {
                    SelectedWindow = null;
                }
            }

            // Eventually update the tab bar visibility
            HasMultipleWindows = Windows.Count > 1;
        }

        internal void Remove()
        {
            if (Parent is SplitContainer parent)
            {
                parent.Children.Remove(this);

                if (parent.Children.Count == 1 && parent.Parent is SplitContainer splitContainerParent)
                {
                    FrameworkElement child = parent.Children[0];

                    int parentIndex = splitContainerParent.Children.IndexOf(parent);

                    Size workingSize = SplitContainer.GetWorkingSize(parent);
                    parent.Children.Remove(child);

                    splitContainerParent.Children.RemoveAt(parentIndex);
                    splitContainerParent.Children.Insert(parentIndex, child);

                    SplitContainer.SetWorkingSize(child, workingSize);
                    return;
                }

                // Remove the entire split container if there are no more children inside
                if (parent.Children.Count == 0)
                {
                    parent.Remove();
                }
            }

            if (Parent != null)
            {
                throw new InvalidOperationException();
            }
        }

        private void OnTitleBarPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectedWindow?.SelectAndPopup(true);
        }
        #endregion


        #region Properties
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public DockWindowCollection Windows { get; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockWindow SelectedWindow
        {
            get => (DockWindow)GetValue(SelectedWindowProperty);
            set => SetValue(SelectedWindowProperty, value);
        }

        /// <summary>
        /// This property is used to toggle the visibility of the tab bar beneath the window group.
        /// If there is only a single window available in the group, we might not want to show a tab
        /// bar at all (this mimics Visual Studio behavior).
        /// </summary>
        public bool HasMultipleWindows
        {
            get => (bool)GetValue(HasMultipleWindowsProperty);
            private set => SetValue(HasMultipleWindowsPropertyKey, value);
        }
        #endregion
    }
}
