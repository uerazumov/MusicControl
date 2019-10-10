using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MusicControl
{
    public partial class ControlButton : INotifyPropertyChanged
    {
        public ControlButton()
        {
            InitializeComponent();
            DataContext = this;
            IsButtonEnabled = true;
        }

        //public ImageBrush DisableBackgroundImage { get; set; }

        private ImageBrush _enableBackgroundImageActive;
        private ImageBrush _disableBackgroundImage;
        private ImageBrush _enableBackgroundImage;
        public ImageBrush EnableBackgroundImage
        {
            get
            {
                return _enableBackgroundImage;
            }
            set
            {
                _enableBackgroundImage = value;
                GetGrayScaleImage();
                GetbrighterImage(1.3f);
                DoPropertyChanged("BackgroundImageActive");
                DoPropertyChanged("BackgroundImage");
            }
        }
        

        public ICommand ControlCommand
        {
            get { return (ICommand)GetValue(ControlCommandProperty); }
            set
            {
                SetValue(ControlCommandProperty, value);
                DoPropertyChanged("ControlCommand");
            }
        }

        public static DependencyProperty ControlCommandProperty =
            DependencyProperty.Register("ControlCommand", typeof(ICommand), typeof(ControlButton));

        public static DependencyProperty IsButtonEnabledProperty =
            DependencyProperty.Register("IsButtonEnabled", typeof(bool), typeof(ControlButton), new UIPropertyMetadata(false, Refresh));

        private bool _isButtonEnabled;

        public bool IsButtonEnabled
        {
            get { return _isButtonEnabled; }
            set
            {
                SetValue(IsButtonEnabledProperty, value);
                _isButtonEnabled = value;
                IsEnabled = value;
            }
        }

        public static void Refresh(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ControlButton controlButton = (ControlButton)property;
            controlButton.DoPropertyChanged("IsButtonEnabled");
            controlButton.DoPropertyChanged("BackgroundImageActive");
            controlButton.DoPropertyChanged("BackgroundImage");
            controlButton.DoPropertyChanged("IsEnabled");
        }

        private void GetGrayScaleImage()
        {
            BitmapSource src = (BitmapSource)_enableBackgroundImage.ImageSource;
            Bitmap bmp = new Bitmap(
            src.PixelWidth,
            src.PixelHeight,
            System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            src.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            Bitmap newBitmap = new Bitmap(bmp.Width, bmp.Height);
            Graphics g = Graphics.FromImage(newBitmap);
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
               });
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);
            g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height),
               0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, attributes);
            g.Dispose();

            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(newBitmap.GetHbitmap(),
                              IntPtr.Zero,
                              Int32Rect.Empty,
                              BitmapSizeOptions.FromEmptyOptions());
            var grayImage = new ImageBrush(bitmapSource);
            _disableBackgroundImage = grayImage;
        }

        private void GetbrighterImage(float brightness)
        {
            BitmapSource src = (BitmapSource)_enableBackgroundImage.ImageSource;
            Bitmap bmp = new Bitmap(
            src.PixelWidth,
            src.PixelHeight,
            System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            src.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            Image img = (Image) bmp;
            float b = brightness;
            ColorMatrix cm = new ColorMatrix(new float[][]
                {
                    new float[] {b, 0, 0, 0, 0},
                    new float[] {0, b, 0, 0, 0},
                    new float[] {0, 0, b, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1},
                });
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(cm);
            System.Drawing.Point[] points =
            {
                new System.Drawing.Point(0, 0),
                new System.Drawing.Point(img.Width, 0),
                new System.Drawing.Point(0, img.Height),
            };
            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);
            Bitmap bm = new Bitmap(img.Width, img.Height);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.DrawImage(img, points, rect,
                    GraphicsUnit.Pixel, attributes);
            }
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bm.GetHbitmap(),
                                  IntPtr.Zero,
                                  Int32Rect.Empty,
                                  BitmapSizeOptions.FromEmptyOptions());
            _enableBackgroundImageActive = new ImageBrush(bitmapSource); ;
        }

        public ImageBrush BackgroundImageActive => !(bool)GetValue(IsButtonEnabledProperty) ? _disableBackgroundImage : _enableBackgroundImageActive;

        public ImageBrush BackgroundImage => !(bool)GetValue(IsButtonEnabledProperty) ? _disableBackgroundImage : _enableBackgroundImage;

        public event PropertyChangedEventHandler PropertyChanged;

        public void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
