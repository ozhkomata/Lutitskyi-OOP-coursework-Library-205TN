using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Library
{
    public partial class ErrorWindow : Window
    {
        private readonly MediaPlayer errorSoundPlayer = new MediaPlayer();

        public ErrorWindow(string errorMessage)
        {
            InitializeComponent();
            ErrorMessageTextBlock.Text = errorMessage;

            errorSoundPlayer.Open(new Uri("pack://siteoforigin:,,,/sound2.mp3"));
            errorSoundPlayer.Play();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TitleBarThumb_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                return;

            Left += e.HorizontalChange;
            Top += e.VerticalChange;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
