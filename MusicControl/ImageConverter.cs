using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Data;

namespace System.Windows.Media
{
    [ValueConversion(typeof(System.Drawing.Image), typeof(System.Windows.Media.ImageSource))]
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) { return null; }

            var imageBrush = (ImageBrush)value;
            Imaging.BitmapSource src = (Imaging.BitmapSource)imageBrush.ImageSource;
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(
            src.PixelWidth,
            src.PixelHeight,
            System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            src.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}