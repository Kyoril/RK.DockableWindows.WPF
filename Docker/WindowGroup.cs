using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
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
    [ContentProperty("Windows")]
    public class WindowGroup : Control
    {  
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
                new FrameworkPropertyMetadata(
                    new PropertyChangedCallback(
                        WindowGroup.OnSelectedWindowChanged)));
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
        protected override IEnumerator LogicalChildren => this.Windows.GetEnumerator();
        #endregion


        #region Internal methods
        internal void AddLogicalChildInternal(DockWindow window)
        {
            this.AddLogicalChild(window);
        }
        internal void RemoveLogicalChildInternal(DockWindow window)
        {
            this.RemoveLogicalChild(window);
        }
        internal void OnChildrenChanged()
        {
            if ((this.SelectedWindow == null) && (this.Windows.Count != 0))
            {
                this.SelectedWindow = this.Windows[0];
            }

            if ((this.SelectedWindow != null) && !this.Windows.Contains(this.SelectedWindow))
            {
                if (this.Windows.Count != 0)
                {
                    this.SelectedWindow = this.Windows[0];
                }
                else
                {
                    this.SelectedWindow = null;
                }
            }
        }
        #endregion


        #region Properties
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public DockWindowCollection Windows { get; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockWindow SelectedWindow
        {
            get => (DockWindow)base.GetValue(SelectedWindowProperty);
            set => base.SetValue(SelectedWindowProperty, value);
        }
        #endregion
    }
}
