using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RK.DockableWindows.WPF
{
    /// <summary>
    /// This class contains all dockable elements and wraps around them. Usually, you will
    /// have exactly one DockCanvas in an application which fills most of the window's space.
    /// </summary>
    [ContentProperty("Child")]
    public class DockCanvas : Control
    {
        private readonly Rectangle background;
        private readonly DockPanel layoutPanel;
        private readonly DockHierarchyPresenter hierarchyPresenter;


        #region Dependency Properties

        public static readonly DependencyProperty ContentSizeProperty =
            DependencyProperty.RegisterAttached(
                "ContentSize",
                typeof(double),
                typeof(DockCanvas),
                new FrameworkPropertyMetadata(
                    200.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnContentSizeChanged),
                o => (double)o > 0.0);

        public static readonly DependencyProperty DockProperty =
            DependencyProperty.RegisterAttached(
                "Dock",
                typeof(Dock),
                typeof(DockCanvas),
                new FrameworkPropertyMetadata(
                    Dock.Right,
                    FrameworkPropertyMetadataOptions.AffectsParentArrange,
                    OnDockChanged));

        #endregion


        #region DependencyProperty Callbacks

        /// <summary>
        /// Callback for a change of the Control's background property. If the property was changed, we want to update
        /// our background visual element.
        /// </summary>
        /// <param name="d">The changed dependency property.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DockCanvas)d).background.Fill = (Brush)e.NewValue;
        }

        private static void OnContentSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // TODO: Update dock hierarchy
        }

        private static void OnDockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Update splitter orientation for split containers
            if (!(d is SplitContainer element)) return;

            var newValue = (Dock)e.NewValue;

            if (newValue == Dock.Left || newValue == Dock.Right)
                element.ClearValue(SplitContainer.SplitterOrientationProperty);
            else
                element.SetValue(SplitContainer.SplitterOrientationProperty, Orientation.Vertical);
        }

        #endregion


        #region Construction

        static DockCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockCanvas),
                new FrameworkPropertyMetadata(typeof(DockCanvas)));

            // Watch for changes of the control's Background property.
            BackgroundProperty.OverrideMetadata(typeof(DockCanvas),
                new FrameworkPropertyMetadata(BackgroundProperty.DefaultMetadata.DefaultValue, OnBackgroundChanged));
        }

        public DockCanvas()
        {
            // Create the dock hierarchy presenter first
            hierarchyPresenter = new DockHierarchyPresenter(this);

            // Setup split container collection. The visual parent will be the hierarchy presenter
            // so that the split containers will be displayed inside.
            SplitContainers = new SplitContainerCollection(this, hierarchyPresenter);

            // Create the background visual and add it
            background = new Rectangle();
            AddVisualChild(background);

            // Create the layout panel which will host our actual content
            layoutPanel = new DockPanel();
            AddVisualChild(layoutPanel);

            // Create the dock hierarchy presenter and add it to the layout panel.
            layoutPanel.Children.Add(hierarchyPresenter);
        }

        #endregion


        #region Methods

        [AttachedPropertyBrowsableForChildren]
        public static Dock GetDock(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return (Dock)element.GetValue(DockProperty);
        }

        public static void SetDock(UIElement element, Dock dock)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            element.SetValue(DockProperty, dock);
        }

        #endregion


        #region Internal methods

        internal void OnSplitContainersChanged()
        {
            hierarchyPresenter.InvalidateSplitters();
        }

        internal void InternalAddLogicalChild(object newChild)
        {
            AddLogicalChild(newChild);
        }

        internal void InternalRemoveLogicalChild(object oldChild)
        {
            RemoveLogicalChild(oldChild);
            hierarchyPresenter.InvalidateMeasure();
        }

        #endregion


        #region Control overrides

        /// <summary>
        /// Updates the size of all visual children when required.
        /// </summary>
        /// <param name="arrangeBounds"></param>
        /// <returns></returns>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            // Calculate the final rectangle
            var final = new Rect(0.0, 0.0, arrangeBounds.Width, arrangeBounds.Height);

            // Arrange visual children
            background.Arrange(final);

            // Arrange the layout panel, but apply the Padding settings before
            final.Offset(Padding.Left, Padding.Top);
            final.Width = Math.Max(final.Width - (Padding.Left + Padding.Right), 0.0);
            final.Height = Math.Max(final.Height - (Padding.Top + Padding.Bottom), 0.0);
            layoutPanel.Arrange(final);

            return arrangeBounds;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // Measure background against constraints
            background.Measure(constraint);

            // Apply padding to constraint and measure layout panel
            constraint.Width -= Padding.Left + Padding.Right;
            constraint.Height -= Padding.Top + Padding.Bottom;
            layoutPanel.Measure(constraint);

            // Return layout panel's desired size with padding applied
            return new Size(
                layoutPanel.DesiredSize.Width + Padding.Left + Padding.Right,
                layoutPanel.DesiredSize.Height + Padding.Top + Padding.Bottom);
        }

        /// <summary>
        /// We have a fixed amount of visual children.
        /// </summary>
        protected override int VisualChildrenCount => 2;

        /// <summary>
        /// Assigns an index to each available visual child.
        /// </summary>
        /// <param name="index">Index of the requested visual child.</param>
        /// <returns>The visual child.</returns>
        protected override Visual GetVisualChild(int index)
        {
            switch (index)
            {
                case 0:
                    return background;
                case 1:
                    return layoutPanel;
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        #endregion


        #region Properties

        private UIElement child;

        /// <summary>
        /// The content property of this control.
        /// </summary>
        public UIElement Child
        {
            get => child;
            set
            {
                if (value != child)
                {
                    if (child != null)
                    {
                        RemoveLogicalChild(Child);
                    }

                    child = value;
                    hierarchyPresenter.Child = value;

                    if (child != null)
                    {
                        AddLogicalChild(Child);
                    }
                }
            }
        }

        /// <summary>
        /// A collection of all split containers of this canvas.
        /// </summary>
        public SplitContainerCollection SplitContainers { get; }

        public Rect ClientBounds =>
            new Rect(hierarchyPresenter.TransformToAncestor(this).Transform(hierarchyPresenter.ClientBounds.Location),
                hierarchyPresenter.ClientBounds.Size);

        #endregion
    }
}
