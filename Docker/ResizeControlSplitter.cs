using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Docker
{
    /// <summary>
    /// A splitter control that is used by DockHierarchyPresenter. These splitters allow the
    /// user to resize the docked windows at the edges.
    /// </summary>
    public class ResizeControlSplitter : Thumb
    {
        /// <summary>
        /// Stores data of a drag&drop session.
        /// </summary>
        private sealed class ResizeData
        {
            /// <summary>
            /// The dock side of the associated split container element.
            /// </summary>
            public Dock splitterDockSide;
            /// <summary>
            /// The adorner that is rendered to preview the new splitter position.
            /// </summary>
            public SplitPreviewAdorner previewAdorner;
            /// <summary>
            /// The associated split container.
            /// </summary>
            public SplitContainer parent;
            /// <summary>
            /// The minimum offset of the splitter.
            /// </summary>
            public double minOffset;
            /// <summary>
            /// The maximum offset of the splitter.
            /// </summary>
            public double maxOffset;
        }


        private DockCanvas canvas;
        private ResizeData resizeData;


        #region Dependency Properties
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(
                "Size",
                typeof(double),
                typeof(ResizeControlSplitter),
                new FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.AffectsMeasure),
                new ValidateValueCallback((o) => (double)o >= 0.0));
        #endregion


        #region Construction
        static ResizeControlSplitter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeControlSplitter), new FrameworkPropertyMetadata(typeof(ResizeControlSplitter)));

            EventManager.RegisterClassHandler(typeof(ResizeControlSplitter), ResizeControlSplitter.DragStartedEvent, new DragStartedEventHandler(ResizeControlSplitter.OnDragStarted));
            EventManager.RegisterClassHandler(typeof(ResizeControlSplitter), ResizeControlSplitter.DragDeltaEvent, new DragDeltaEventHandler(ResizeControlSplitter.OnDragDelta));
            EventManager.RegisterClassHandler(typeof(ResizeControlSplitter), ResizeControlSplitter.DragCompletedEvent, new DragCompletedEventHandler(ResizeControlSplitter.OnDragCompleted));
        }
        internal ResizeControlSplitter(DockCanvas canvas, SplitContainer resizeControl)
        {
            this.canvas = canvas;
            this.ResizeControl = resizeControl;

            if (this.ResizeControl != null)
            {
                UpdateCursor();
            }
        }
        #endregion


        #region Internal Methods
        private void OnDragCompleted(DragCompletedEventArgs e)
        {
            if (resizeData != null)
            {
                if (!e.Canceled)
                {
                    switch(this.resizeData.splitterDockSide)
                    {
                        case Dock.Right:
                            this.resizeData.parent.ContentSize = Math.Max(this.resizeData.parent.ContentSize - this.resizeData.previewAdorner.OffsetX, 15.0);
                            break;
                        case Dock.Left:
                            this.resizeData.parent.ContentSize = Math.Max(this.resizeData.parent.ContentSize + this.resizeData.previewAdorner.OffsetX, 15.0);
                            break;
                        case Dock.Top:
                            this.resizeData.parent.ContentSize = Math.Max(this.resizeData.parent.ContentSize + this.resizeData.previewAdorner.OffsetY, 15.0);
                            break;
                        default:
                            this.resizeData.parent.ContentSize = Math.Max(this.resizeData.parent.ContentSize - this.resizeData.previewAdorner.OffsetY, 15.0);
                            break;
                    }
                }

                if (VisualTreeHelper.GetParent(this.resizeData.previewAdorner) is AdornerLayer adornerLayer)
                {
                    adornerLayer.Remove(this.resizeData.previewAdorner);
                }
                
                this.resizeData = null;
            }
        }
        private static void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            (sender as ResizeControlSplitter).OnDragCompleted(e);
        }
        private void OnDragDelta(DragDeltaEventArgs e)
        {
            if (resizeData != null)
            {
                switch(resizeData.splitterDockSide)
                {
                    case Dock.Top:
                    case Dock.Bottom:
                        this.resizeData.previewAdorner.OffsetY = 
                            Math.Max(
                                Math.Min(e.VerticalChange, resizeData.maxOffset), 
                                this.resizeData.minOffset);
                        break;
                    default:
                        this.resizeData.previewAdorner.OffsetX =
                            Math.Max(
                                Math.Min(e.HorizontalChange, this.resizeData.maxOffset),
                                this.resizeData.minOffset);
                        break;
                }
            }
        }
        private static void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            (sender as ResizeControlSplitter).OnDragDelta(e);
        }
        private void OnDragStarted(DragStartedEventArgs e)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this.ResizeControl);
            if (adornerLayer != null)
            {
                this.resizeData = new ResizeData();
                this.resizeData.splitterDockSide = DockCanvas.GetDock(this.ResizeControl);
                this.resizeData.parent = this.ResizeControl;

                switch (resizeData.splitterDockSide)
                {
                    case Dock.Left:
                        resizeData.minOffset = 15.0 - this.ResizeControl.ContentSize;
                        resizeData.maxOffset = this.canvas.ClientBounds.Width - 32.0;
                        break;

                    case Dock.Top:
                        resizeData.minOffset = 15.0 - this.ResizeControl.ContentSize;
                        resizeData.maxOffset = this.canvas.ClientBounds.Height - 32.0;
                        break;

                    case Dock.Right:
                        resizeData.maxOffset = this.ResizeControl.ContentSize - 15.0;
                        resizeData.minOffset = 32.0 - this.canvas.ClientBounds.Width;
                        break;

                    case Dock.Bottom:
                        resizeData.maxOffset = this.ResizeControl.ContentSize - 15.0;
                        resizeData.minOffset = 32.0 - this.canvas.ClientBounds.Height;
                        break;
                }

                resizeData.previewAdorner = new SplitPreviewAdorner(this, null);
                adornerLayer.Add(this.resizeData.previewAdorner);
            }
        }
        private static void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            (sender as ResizeControlSplitter).OnDragStarted(e);
        }
        internal void UpdateCursor()
        {
            switch (DockCanvas.GetDock(this.ResizeControl))
            {
                case Dock.Left:
                case Dock.Right:
                    Cursor = Cursors.SizeWE;
                    break;
                default:
                    Cursor = Cursors.SizeNS;
                    break;
            }
        }
        #endregion


        #region Thumb overrides
        protected override Size MeasureOverride(Size constraint)
        {
            return new Size(this.Size, this.Size);
        }
        #endregion


        #region Properties
        public SplitContainer ResizeControl { get; }
        public double Size
        {
            get => (double)GetValue(ResizeControlSplitter.SizeProperty);
            set => SetValue(ResizeControlSplitter.SizeProperty, value);
        }
        #endregion
    }
}
