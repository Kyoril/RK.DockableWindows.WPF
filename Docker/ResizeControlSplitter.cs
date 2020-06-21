using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Docker
{
    public class ResizeControlSplitter : Thumb
    {
        private DockCanvas canvas;


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
            // TODO
        }
        private static void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            (sender as ResizeControlSplitter).OnDragCompleted(e);
        }
        private void OnDragDelta(DragDeltaEventArgs e)
        {
            // TODO
        }
        private static void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            (sender as ResizeControlSplitter).OnDragDelta(e);
        }
        private void OnDragStarted(DragStartedEventArgs e)
        {
            // TODO
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
