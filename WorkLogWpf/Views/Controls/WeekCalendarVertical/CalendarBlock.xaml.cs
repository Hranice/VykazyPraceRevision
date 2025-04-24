using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using VykazyPrace.Core.Database.Models;

namespace WorkLogWpf.Views.Controls.WeekCalendarVertical
{
    public partial class CalendarBlock : UserControl
    {
        public event EventHandler ResizeStarted;
        public event EventHandler<Point> ResizeDelta;
        public event EventHandler ResizeCompleted;
        public TimeEntry Entry { get; set; }


        public CalendarBlock()
        {
            InitializeComponent();

            TopThumb.DragStarted += Thumb_DragStarted;
            TopThumb.DragDelta += Thumb_DragDelta;
            TopThumb.DragCompleted += Thumb_DragCompleted;
                
            BottomThumb.DragStarted += Thumb_DragStarted;
            BottomThumb.DragDelta += Thumb_DragDelta;
            BottomThumb.DragCompleted += Thumb_DragCompleted;
        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            ResizeStarted?.Invoke(this, EventArgs.Empty);
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // Posíláme informaci o směru (a který thumb)
            ResizeDelta?.Invoke(sender, new Point(e.HorizontalChange, 0));
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            ResizeCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
