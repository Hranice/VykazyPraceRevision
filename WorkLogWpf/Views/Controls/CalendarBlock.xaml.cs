using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using VykazyPrace.Core.Database.Models;

namespace WorkLogWpf.Views.Controls
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

            LeftThumb.DragStarted += Thumb_DragStarted;
            LeftThumb.DragDelta += Thumb_DragDelta;
            LeftThumb.DragCompleted += Thumb_DragCompleted;

            RightThumb.DragStarted += Thumb_DragStarted;
            RightThumb.DragDelta += Thumb_DragDelta;
            RightThumb.DragCompleted += Thumb_DragCompleted;
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
