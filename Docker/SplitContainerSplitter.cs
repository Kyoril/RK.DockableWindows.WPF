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
    public class SplitContainerSplitter : Thumb
    {
        #region Dependency Properties
        internal static readonly DependencyProperty SizeProperty = 
            DependencyProperty.Register(
                "Size", 
                typeof(double), 
                typeof(SplitContainerSplitter), 
                new FrameworkPropertyMetadata(
                    4.0, 
                    FrameworkPropertyMetadataOptions.AffectsMeasure), 
                new ValidateValueCallback((o) => (double)o > 0.0));
        #endregion


        #region Construction
        static SplitContainerSplitter()
        {
            Thumb.DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitContainerSplitter), new FrameworkPropertyMetadata(typeof(SplitContainerSplitter)));

            EventManager.RegisterClassHandler(typeof(SplitContainerSplitter), Thumb.DragStartedEvent, new DragStartedEventHandler(SplitContainerSplitter.OnDragStarted));
            EventManager.RegisterClassHandler(typeof(SplitContainerSplitter), Thumb.DragDeltaEvent, new DragDeltaEventHandler(SplitContainerSplitter.OnDragDelta));
            EventManager.RegisterClassHandler(typeof(SplitContainerSplitter), Thumb.DragCompletedEvent, new DragCompletedEventHandler(SplitContainerSplitter.OnDragCompleted));
        }
        internal SplitContainerSplitter(FrameworkElement beforeElement, FrameworkElement afterElement, Orientation splitterOrientation)
        {
            this.BeforeElement = beforeElement;
            this.AfterElement = afterElement;
            this.Orientation = splitterOrientation;
            base.Cursor = (splitterOrientation == Orientation.Horizontal) ? Cursors.SizeNS : Cursors.SizeWE;
        }
        #endregion


        #region Thumb Overrides
        protected override Size MeasureOverride(Size constraint)
        {
            return new Size(this.Size, this.Size);
        }
        #endregion

        #region Private Methods
        private void OnDragStarted(DragStartedEventArgs e)
        {
            // TODO
        }
        private static void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            (sender as SplitContainerSplitter).OnDragStarted(e);
        }
        private void OnDragDelta(DragDeltaEventArgs e)
        {
            // TODO
        }
        private static void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            (sender as SplitContainerSplitter).OnDragDelta(e);
        }
        private void OnDragCompleted(DragCompletedEventArgs e)
        {
            // TODO
        }
        private static void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            (sender as SplitContainerSplitter).OnDragCompleted(e);
        }
        #endregion


        #region Properties
        public FrameworkElement AfterElement { get; }
        public FrameworkElement BeforeElement { get; }
        public Orientation Orientation { get; }
        internal double Size
        {
            get => (double)GetValue(SplitContainerSplitter.SizeProperty);
            set => SetValue(SplitContainerSplitter.SizeProperty, value);
        }
        #endregion
    }
}
