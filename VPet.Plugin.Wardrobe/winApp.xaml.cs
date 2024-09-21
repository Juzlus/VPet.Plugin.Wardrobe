using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.CustomHats
{
    public partial class winApp : UserControl
    {
        public winApp()
        {
            this.InitializeComponent();
        }

        public void ChangeCordinations(string type, float x, float y, float deg)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => ChangeCordinations(type, x, y, deg));
                return;
            }
            this.GetCordinates(type).Margin = new Thickness(x, y, 0, 0);
            this.GetAngle(type).Angle = deg;
        }

        public void ChangeImage(string type, string imagePath)
        {
            if (imagePath == null)
            {
                this.GetCordinates(type).Source = null;
                return;
            }
            if (!File.Exists(imagePath)) return;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            this.GetCordinates(type).Source = bitmapImage;
        }

        public void ChangeVisibility(string type, bool isVisible)
        {
            if (this.Dispatcher.CheckAccess())
            {
                if (isVisible && this.GetCordinates(type).Visibility == Visibility.Collapsed)
                    this.GetCordinates(type).Visibility = Visibility.Visible;
                else if (!isVisible)
                    this.GetCordinates(type).Visibility = Visibility.Collapsed;
            }
            else
                this.Dispatcher.BeginInvoke(new Action(() => ChangeVisibility(type, isVisible)));
        }

        private Image GetCordinates(string type)
        {
            switch (type)
            {
                case "hats":
                    return this.HatsCordinates;
                case "masks":
                    return this.MasksCordinates;
                case "necklaces":
                    return this.NecklacesCordinates;
                default:
                    return null;
            }
        }

        private RotateTransform GetAngle(string type)
        {
            switch (type)
            {
                case "hats":
                    return this.HatsAngle;
                case "masks":
                    return this.MasksAngle;
                case "necklaces":
                    return this.NecklacesAngle;
                default:
                    return null;
            }
        }
    }
}
