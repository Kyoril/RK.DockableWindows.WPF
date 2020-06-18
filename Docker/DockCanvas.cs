﻿using System;
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


        #region DependencyProperties
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
            // Create the background visual and add it
            this.background = new Rectangle();
            this.AddVisualChild(this.background);

            // Setup split container collection
            this.SplitContainers = new SplitContainerCollection(this, this);
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

            return arrangeBounds;
        }

        /// <summary>
        /// We have a fixed amount of visual children.
        /// </summary>
        protected override int VisualChildrenCount => 1;

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
