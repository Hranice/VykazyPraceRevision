using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VykazyPrace.Core.Database.Models;
using WorkLogWpf.Views.Dialogs;

namespace WorkLogWpf.Views.Controls.WeekCalendarVertical
{
    public partial class CalendarBlock : UserControl
    {
        public event EventHandler ResizeStarted;
        public event EventHandler<Point> ResizeDelta;
        public event EventHandler ResizeCompleted;
        public bool CanBeEdited { get; set; } = true;
        private TimeEntry _entry;

        public TimeEntry Entry
        {
            get => _entry;
            set
            {
                _entry = value;
                UpdateUi();
            }
        }

        public void Highlight()
        {
            if (CanBeEdited)
            {
                BlockBorder.Background = new SolidColorBrush(Color.FromArgb(255, 155, 205, 255));
            }

            else
            {
                BlockBorder.Background = new SolidColorBrush(Color.FromArgb(255, 205, 205, 205));
            }
        }

        public void ClearHighlight()
        {
            if (CanBeEdited)
            {
                BlockBorder.Background = new SolidColorBrush(Color.FromArgb(255, 105, 181, 255));
            }

            else
            {
                BlockBorder.Background = new SolidColorBrush(Color.FromArgb(255, 181, 181, 181));
            }
        }

        private void UpdateUi()
        {
            BlockImageInfo.Visibility = Visibility.Collapsed;

            // Pokud se jedná o svačinu
            if (_entry.ProjectId == 132 && _entry.EntryTypeId == 24)
            {
                BlockImageInfo.Source = new BitmapImage(new Uri("pack://application:,,,/Views/Controls/Assets/sandwich.png"));
                BlockImageInfo.Visibility = Visibility.Visible;

                BlockBorder.Background = new SolidColorBrush(Color.FromArgb(255, 181, 181, 181));
                BlockBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 96, 96, 96));

                BottomThumb.Visibility = Visibility.Collapsed;
                TopThumb.Visibility = Visibility.Collapsed;
                CanBeEdited = false;
            }

            // Pokud se jedná o after-care
            else if (_entry.AfterCare == 1)
            {
                BlockImageInfo.Source = new BitmapImage(new Uri("pack://application:,,,/Views/Controls/Assets/tools.png"));
                BlockImageInfo.Visibility = Visibility.Visible;
            }
            else
            {
                BlockImageInfo.Source = null;
            }

            if (_entry.Project is not null)
            {
                if (!string.IsNullOrEmpty(_entry.Project.ProjectDescription) && _entry.Project.ProjectType != 0)
                {
                    BlockTitle.Text = $"{_entry.Project.ProjectDescription} - {_entry.Project.ProjectTitle}";
                }

                else
                {
                    BlockTitle.Text = $"{_entry.Project.ProjectTitle}";
                }
            }

            if (string.IsNullOrWhiteSpace(_entry.Description))
            {
                BlockSubtitle.Text = "";
                BlockSubtitle.Visibility = Visibility.Collapsed;

                BlockTitle.TextAlignment = TextAlignment.Center;
                BlockTitle.VerticalAlignment = VerticalAlignment.Center;
                TitleGrid.VerticalAlignment = VerticalAlignment.Center;
            }
            else
            {
                BlockSubtitle.Text = _entry.Description;
                BlockSubtitle.Visibility = Visibility.Visible;

                BlockTitle.TextAlignment = TextAlignment.Left;
                BlockTitle.VerticalAlignment = VerticalAlignment.Top;
                TitleGrid.VerticalAlignment = VerticalAlignment.Top;
            }

        }


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
            ResizeDelta?.Invoke(sender, new Point(e.HorizontalChange, 0));
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            ResizeCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
