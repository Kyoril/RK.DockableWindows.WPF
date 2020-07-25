using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace RK.DockableWindows.WPF
{
    /// <summary>
    /// A splitter control that is used by DockHierarchyPresenter. These splitters allow the
    /// user to resize the docked windows at the edges.
    /// </summary>
    public class ResizeControlSplitter : Thumb
    {
        /// <summary>
        /// Stores data of a drag & drop session.
        /// </summary>
        private sealed class ResizeData
        {
            /// <summary>
            /// The dock side of the associated split container element.
            /// </summary>
            public Dock SplitterDockSide;
            /// <summary>
            /// The adorner that is rendered to preview the new splitter position.
            /// </summary>
            public SplitPreviewAdorner PreviewAdorner;
            /// <summary>
            /// The associated split container.
            /// </summary>
            public SplitContainer Parent;
            /// <summary>
            /// The minimum offset of the splitter.
            /// </summary>
            public double MinOffset;
            /// <summary>
            /// The maximum offset of the splitter.
            /// </summary>
            public double MaxOffset;
        }


        private readonly DockCanvas canvas;
        private ResizeData resizeData;


        #region Dependency Properties
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(
                "Size",
                typeof(double),
                typeof(ResizeControlSplitter),
                new FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.AffectsMeasure),
                o => (double)o >= 0.0);
        #endregion


        #region Construction
        static ResizeControlSplitter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeControlSplitter), new FrameworkPropertyMetadata(typeof(ResizeControlSplitter)));

            EventManager.RegisterClassHandler(typeof(ResizeControlSplitter), DragStartedEvent, new DragStartedEventHandler(OnDragStarted));
            EventManager.RegisterClassHandler(typeof(ResizeControlSplitter), DragDeltaEvent, new DragDeltaEventHandler(OnDragDelta));
            EventManager.RegisterClassHandler(typeof(ResizeControlSplitter), DragCompletedEvent, new DragCompletedEventHandler(OnDragCompleted));
        }

        internal ResizeControlSplitter(DockCanvas canvas, SplitContainer resizeControl)
        {
            this.canvas = canvas;
            ResizeControl = resizeControl;

            if (ResizeControl != null)
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
                    switch (resizeData.SplitterDockSide)
                    {
                        case Dock.Right:
                            resizeData.Parent.ContentSize = Math.Max(this.resizeData.Parent.ContentSize - this.resizeData.PreviewAdorner.OffsetX, 15.0);
                            break;
                        case Dock.Left:
                            resizeData.Parent.ContentSize = Math.Max(this.resizeData.Parent.ContentSize + this.resizeData.PreviewAdorner.OffsetX, 15.0);
                            break;
                        case Dock.Top:
                            resizeData.Parent.ContentSize = Math.Max(this.resizeData.Parent.ContentSize + this.resizeData.PreviewAdorner.OffsetY, 15.0);
                            break;
                        default:
                            resizeData.Parent.ContentSize = Math.Max(this.resizeData.Parent.ContentSize - this.resizeData.PreviewAdorner.OffsetY, 15.0);
                            break;
                    }
                }

                if (VisualTreeHelper.GetParent(resizeData.PreviewAdorner) is AdornerLayer adornerLayer)
                {
                    adornerLayer.Remove(resizeData.PreviewAdorner);
                }

                resizeData = null;
            }
        }

        private static void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            (sender as ResizeControlSplitter)?.OnDragCompleted(e);
        }

        private void OnDragDelta(DragDeltaEventArgs e)
        {
            if (resizeData != null)
            {
                switch (resizeData.SplitterDockSide)
                {
                    case Dock.Top:
                    case Dock.Bottom:
                        resizeData.PreviewAdorner.OffsetY =
                            Math.Max(
                                Math.Min(e.VerticalChange, resizeData.MaxOffset),
                                resizeData.MinOffset);
                        break;
                    default:
                        resizeData.PreviewAdorner.OffsetX =
                            Math.Max(
                                Math.Min(e.HorizontalChange, resizeData.MaxOffset),
                                resizeData.MinOffset);
                        break;
                }
            }
        }

        private static void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            (sender as ResizeControlSplitter)?.OnDragDelta(e);
        }

        private void OnDragStarted()
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(ResizeControl);
            if (adornerLayer != null)
            {
                resizeData = new ResizeData
                {
                    SplitterDockSide = DockCanvas.GetDock(ResizeControl),
                    Parent = ResizeControl
                };

                switch (resizeData.SplitterDockSide)
                {
                    case Dock.Left:
                        resizeData.MinOffset = 15.0 - ResizeControl.ContentSize;
                        resizeData.MaxOffset = canvas.ClientBounds.Width - 32.0;
                        break;

                    case Dock.Top:
                        resizeData.MinOffset = 15.0 - ResizeControl.ContentSize;
                        resizeData.MaxOffset = canvas.ClientBounds.Height - 32.0;
                        break;

                    case Dock.Right:
                        resizeData.MaxOffset = ResizeControl.ContentSize - 15.0;
                        resizeData.MinOffset = 32.0 - canvas.ClientBounds.Width;
                        break;

                    case Dock.Bottom:
                        resizeData.MaxOffset = ResizeControl.ContentSize - 15.0;
                        resizeData.MinOffset = 32.0 - canvas.ClientBounds.Height;
                        break;
                }

                resizeData.PreviewAdorner = new SplitPreviewAdorner(this);
                adornerLayer.Add(resizeData.PreviewAdorner);
            }
        }

        private static void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            (sender as ResizeControlSplitter)?.OnDragStarted();
        }

        internal void UpdateCursor()
        {
            switch (DockCanvas.GetDock(ResizeControl))
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
            return new Size(Size, Size);
        }
        #endregion


        #region Properties
        public SplitContainer ResizeControl { get; }
        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }
        #endregion
    }
}
