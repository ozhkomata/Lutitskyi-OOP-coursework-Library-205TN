using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Controls;
using System.Windows;

namespace Library
{
    public class WatermarkedTextBox : TextBox
    {
        #region Fields

        private const string DefaultWatermark = "None";

        public static readonly DependencyProperty WatermarkTextProperty =
            DependencyProperty.Register("WatermarkText", typeof(string), typeof(WatermarkedTextBox), new UIPropertyMetadata(string.Empty, OnWatermarkTextChanged));

        #endregion

        #region Constructor

        public WatermarkedTextBox()
            : this(DefaultWatermark)
        {
        }

        public WatermarkedTextBox(string watermark)
        {
            WatermarkText = watermark;
            UpdateWatermarkVisibility();
        }

        #endregion

        #region Properties

        public string WatermarkText
        {
            get { return (string)GetValue(WatermarkTextProperty); }
            set { SetValue(WatermarkTextProperty, value); }
        }

        #endregion

        #region Methods

        private static void OnWatermarkTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (WatermarkedTextBox)obj;
            textBox.UpdateWatermarkVisibility();
        }

        private void UpdateWatermarkVisibility()
        {
            if (string.IsNullOrEmpty(Text))
            {
                ShowWatermark();
            }
            else
            {
                HideWatermark();
            }
        }

        private void ShowWatermark()
        {
            var watermarkTextBlock = GetTemplateChild("WatermarkText") as TextBlock;
            if (watermarkTextBlock != null)
            {
                watermarkTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void HideWatermark()
        {
            var watermarkTextBlock = GetTemplateChild("WatermarkText") as TextBlock;
            if (watermarkTextBlock != null)
            {
                watermarkTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        #endregion
    }
}
