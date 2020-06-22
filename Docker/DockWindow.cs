using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace Docker
{
    /// <summary>
    /// A control that can be docked at any side the user want's to. This control contains the actual
    /// child that the developer want's to be docked.
    /// </summary>
    [ContentProperty("Child")]
    [DefaultProperty("Title")]
    public class DockWindow : Control
    {
        #region Routed Commands
        public static readonly RoutedCommand CloseCommand = new RoutedCommand("Close", typeof(DockWindow));
        #endregion


        #region Events
        public event EventHandler Closed;
        public event CancelEventHandler Closing;
        #endregion


        #region Dependency Properties
        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register(
                "Child",
                typeof(UIElement),
                typeof(DockWindow),
                new FrameworkPropertyMetadata(
                    null,
                    new PropertyChangedCallback(DockWindow.OnChildChanged)));
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(DockWindow),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    new PropertyChangedCallback(DockWindow.OnTitleChanged)));
        public static readonly DependencyProperty TabTextProperty =
            DependencyProperty.Register(
                "TabText",
                typeof(string),
                typeof(DockWindow),
                new FrameworkPropertyMetadata(string.Empty));
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                "IsSelected",
                typeof(bool),
                typeof(DockWindow));
        public static readonly DependencyProperty ContentSizeProperty =
            DependencyProperty.Register(
                "ContentSize",
                typeof(double),
                typeof(DockWindow),
                new FrameworkPropertyMetadata(225.0),
                new ValidateValueCallback((o) => (double)o > 0.0));
        public static readonly DependencyProperty AllowCloseProperty = 
            DependencyProperty.Register(
                "AllowClose", 
                typeof(bool),
                typeof(DockWindow), 
                new FrameworkPropertyMetadata(true));
        #endregion


        #region Construction
        static DockWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockWindow), new FrameworkPropertyMetadata(typeof(DockWindow)));

            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(DockWindow), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(DockWindow), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));

            CommandManager.RegisterClassCommandBinding(
                typeof(DockWindow), 
                new CommandBinding(DockWindow.CloseCommand, new ExecutedRoutedEventHandler(DockWindow.OnCommand),
                new CanExecuteRoutedEventHandler(DockWindow.OnCanExecute)));
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// Tries to close the DockWindow, making it disappear.
        /// </summary>
        /// <returns>True on success, false if the operation was cancelled.</returns>
        public bool Close()
        {
            // Setup event args and raise the Closing event
            CancelEventArgs e = new CancelEventArgs();
            OnClosing(e);

            // If the developer doesn't want to close the window, stop here
            if (e.Cancel)
            {
                return false;
            }

            // Remove the DockWindow
            if (this.Parent is WindowGroup parent)
            {
                parent.Windows.Remove(this);
                if (parent.Windows.Count == 0)
                {
                    parent.Remove();
                }
            }

            // Raise the closed event in case the developer want's to react to it.
            OnClosed(EventArgs.Empty);
            
            return true;
        }
        #endregion


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
        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DockWindow window = (DockWindow)d;

            string oldValue = (string)e.OldValue;
            string newValue = (string)e.NewValue;

            if (StringComparer.CurrentCulture.Compare(window.TabText, oldValue) == 0)
            {
                window.TabText = newValue;
            }
        }
        #endregion


        #region Internal Methods
        private static void OnCommand(object sender, ExecutedRoutedEventArgs e)
        {
            DockWindow window = (DockWindow)sender;
            if (e.Command == DockWindow.CloseCommand)
            {
                window.Close();
            }
        }
        private static void OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            DockWindow window = (DockWindow)sender;
            if (e.Command == DockWindow.CloseCommand)
            {
                e.CanExecute = window.AllowClose;
            }
        }
        protected internal void OnClosed(EventArgs e)
        {
            this.Closed?.Invoke(this, e);
        }
        protected internal void OnClosing(CancelEventArgs e)
        {
            this.Closing?.Invoke(this, e);
        }
        internal void SelectAndPopup(bool activate = true)
        {
            if (this.Parent is WindowGroup parent)
            {
                if (parent.SelectedWindow != this)
                {
                    parent.SelectedWindow = this;
                    parent.UpdateLayout();
                }
            }

            if (activate)
            {
                this.Activate();
            }
        }
        internal bool Activate()
        {
            bool succeeded = false;

            // Only do anything if the control isn't currently focused
            if (!this.IsKeyboardFocusWithin)
            {
                // First find the focused element in this control
                UIElement focusedElement = null;
                if (FocusManager.GetIsFocusScope(this))
                {
                    focusedElement = FocusManager.GetFocusedElement(this) as UIElement;
                }

                // If we found an element, try to focus it
                if (focusedElement != null)
                {
                    succeeded = focusedElement.Focus();
                }

                // If this didn't succeed so far, try to activate the child element
                if (!succeeded && (this.Child != null))
                {
                    succeeded = this.Child.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                }

                // If this still didn't succeed, try to focus this control
                if (!succeeded && base.Focusable)
                {
                    succeeded = base.Focus();
                }
            }

            return succeeded;
        }
        private void ActivateIfNoFocus(object sender, EventArgs e)
        {
            // If this control is not currently focused...
            if (!this.IsKeyboardFocusWithin)
            {
                // Activate
                Activate();
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

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (e.ClickCount == 1)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new EventHandler(ActivateIfNoFocus), null, null);
            }
        }
        #endregion


        #region Properties
        public UIElement Child
        {
            get => (UIElement)GetValue(DockWindow.ChildProperty);
            set => SetValue(DockWindow.ChildProperty, value);
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected
        {
            get => (bool)GetValue(DockWindow.IsSelectedProperty);
            internal set => SetValue(DockWindow.IsSelectedProperty, value);
        }
        [Category("Text")]
        public string TabText
        {
            get => (string)GetValue(DockWindow.TabTextProperty);
            set => SetValue(DockWindow.TabTextProperty, value);
        }
        [Category("Common Properties")]
        public string Title
        {
            get => (string)GetValue(DockWindow.TitleProperty);
            set => SetValue(DockWindow.TitleProperty, value);
        }
        [Category("Docking")]
        public double ContentSize
        {
            get => (double)GetValue(DockWindow.ContentSizeProperty);
            set => SetValue(DockWindow.ContentSizeProperty, value);
        }
        [Category("Docking")]
        public bool AllowClose
        {
            get => (bool)GetValue(DockWindow.AllowCloseProperty);
            set => SetValue(DockWindow.AllowCloseProperty, value);
        }
        #endregion
    }
}
