using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Docker
{
    /// <summary>
    /// This class contains all dockable elements and wraps around them. Usually, you will
    /// have exactly one DockCanvas in an application which fills most of the window's space.
    /// </summary>
    [ContentProperty("Child")]
    public class DockCanvas : Control
    {
        private Rectangle background;
        private DockPanel layoutPanel;
        private DockHierarchyPresenter hierarchyPresenter;


        #region Dependency Properties
        public static readonly DependencyProperty ContentSizeProperty = 
            DependencyProperty.RegisterAttached(
                "ContentSize", 
                typeof(double), 
                typeof(DockCanvas), 
                new FrameworkPropertyMetadata(
                    200.0, 
                    FrameworkPropertyMetadataOptions.AffectsMeasure, 
                    new PropertyChangedCallback(DockCanvas.OnContentSizeChanged)), 
                new ValidateValueCallback((o) => (double)o > 0.0));
        public static readonly DependencyProperty DockProperty =
            DependencyProperty.RegisterAttached(
                "Dock", 
                typeof(Dock), 
                typeof(DockCanvas), 
                new FrameworkPropertyMetadata(
                    Dock.Right, 
                    FrameworkPropertyMetadataOptions.AffectsParentArrange, 
                    new PropertyChangedCallback(DockCanvas.OnDockChanged)));
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
            (d as DockCanvas).background.Fill = (Brush)e.NewValue;
        }
        private static void OnContentSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // TODO: Update dock hierarchy
        }
        private static void OnDockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Update splitter orientation for split containers
            if (d is SplitContainer element)
            {
                Dock newValue = (Dock)e.NewValue;
                switch(newValue)
                {
                    case Dock.Left:
                    case Dock.Right:
                        element.ClearValue(SplitContainer.SplitterOrientationProperty);
                        break;
                    default:
                        element.SetValue(SplitContainer.SplitterOrientationProperty, Orientation.Vertical);
                        break;
                }
            }
        }
        #endregion


        #region Construction
        static DockCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockCanvas), new FrameworkPropertyMetadata(typeof(DockCanvas)));

            // Watch for changes of the control's Background property.
            BackgroundProperty.OverrideMetadata(typeof(DockCanvas), new FrameworkPropertyMetadata(BackgroundProperty.DefaultMetadata.DefaultValue, new PropertyChangedCallback(OnBackgroundChanged)));
        }
        public DockCanvas()
        {
            // Create the dock hierarchy presenter first
            this.hierarchyPresenter = new DockHierarchyPresenter(this);

            // Setup split container collection. The visual parent will be the hierarchy presenter
            // so that the split containers will be displayed inside.
            this.SplitContainers = new SplitContainerCollection(this, this.hierarchyPresenter);

            // Create the background visual and add it
            this.background = new Rectangle();
            this.AddVisualChild(this.background);

            // Create the layout panel which will host our actual content
            this.layoutPanel = new DockPanel();
            this.AddVisualChild(this.layoutPanel);

            // Create the dock hierachy presenter and add it to the layout panel.
            this.layoutPanel.Children.Add(this.hierarchyPresenter);
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

            return (Dock)element.GetValue(DockCanvas.DockProperty);
        }
        public static void SetDock(UIElement element, Dock dock)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            element.SetValue(DockCanvas.DockProperty, dock);
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
            Rect final = new Rect(0.0, 0.0, arrangeBounds.Width, arrangeBounds.Height);

            // Arrange visual children
            this.background.Arrange(final);

            // Arrange the layout panel, but apply the Padding settings before
            final.Offset(this.Padding.Left, this.Padding.Top);
            final.Width = Math.Max(final.Width - (this.Padding.Left + this.Padding.Right), 0.0);
            final.Height = Math.Max(final.Height - (this.Padding.Top + this.Padding.Bottom), 0.0);
            layoutPanel.Arrange(final);

            return arrangeBounds;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // Measure background against constraints
            this.background.Measure(constraint);

            // Apply padding to constraint and measure layout panel
            constraint.Width -= this.Padding.Left + this.Padding.Right;
            constraint.Height -= this.Padding.Top + this.Padding.Bottom;
            this.layoutPanel.Measure(constraint);

            // Return layout panel's desired size with padding applied
            return new Size(
                this.layoutPanel.DesiredSize.Width + this.Padding.Left + this.Padding.Right,
                this.layoutPanel.DesiredSize.Height + this.Padding.Top + this.Padding.Bottom);
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
            switch(index)
            {
                case 0:
                    return this.background;
                case 1:
                    return this.layoutPanel;
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
                    this.hierarchyPresenter.Child = value;

                    if (this.child != null)
                    {
                        this.AddLogicalChild(Child);
                    }
                }
            }
        }
        /// <summary>
        /// A collection of all split containers of this canvas.
        /// </summary>
        public SplitContainerCollection SplitContainers { get; }
        #endregion
    }
}
