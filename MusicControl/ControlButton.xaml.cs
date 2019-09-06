using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public ImageBrush DisableBackgroundImage { get; set; }
        public ImageBrush DisableBackgroundImageActive { get; set; }
        public ImageBrush EnableBackgroundImage { get; set; }
        public ImageBrush EnableBackgroundImageActive { get; set; }

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

        public bool IsButtonEnabled
        {
            get { return (bool)GetValue(IsButtonEnabledProperty); }
            set
            {
                SetValue(IsButtonEnabledProperty, value);
                IsEnabled = value;
                DoPropertyChanged("DisableButton");
                DoPropertyChanged("BackgroundImageActive");
                DoPropertyChanged("BackgroundImage");
            }
        }

        public static void Refresh(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ControlButton circularButton = (ControlButton)property;
            circularButton.IsButtonEnabled = (bool)args.NewValue;
            circularButton.DoPropertyChanged("IsButtonEnabled");
            circularButton.DoPropertyChanged("BackgroundImageActive");
            circularButton.DoPropertyChanged("BackgroundImage");
            circularButton.DoPropertyChanged("IsEnabled");
        }

        public ImageBrush BackgroundImageActive => !IsButtonEnabled ? DisableBackgroundImageActive : EnableBackgroundImageActive;

        public ImageBrush BackgroundImage => !IsButtonEnabled ? DisableBackgroundImage : EnableBackgroundImage;

        public event PropertyChangedEventHandler PropertyChanged;

        public void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
