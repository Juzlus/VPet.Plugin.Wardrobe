using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

public class Items
{
    public ImageSource Source { get; set; }
    public string Name { get; set; }
    public string PriceText { get; set; }
    public string OutfitID { get; set; }
    public Items(ImageSource imageSource, string name, string priceText, string type)
    {
        Source = imageSource;
        Name = name.Split("_")[0].Translate();
        PriceText = priceText;
        OutfitID = $"{type}-{name}";
    }

}

namespace VPet.Plugin.CustomHats
{
    public partial class winSettings : WindowX
    {
        CustomHats main;

        public winSettings(CustomHats main)
        {
            this.main = main;
            this.InitializeComponent();
            this.UpdateItems("hats");
            this.UpdateItems("masks");
            this.UpdateItems("necklaces");
        }

        public void UpdateItems(string type)
        {
            switch (type)
            {
                case "hats":
                    this.InitializeItems("hats", this.HatsPurchasedItemsList, this.HatsItemsToBuyList);
                    break;
                case "masks":
                    this.InitializeItems("masks", this.MasksPurchasedItemsList, this.MasksItemsToBuyList);
                    break;
                case "necklaces":
                    this.InitializeItems("necklaces", this.NecklacesPurchasedItemsList, this.NecklacesItemsToBuyList);
                    break;
            }
        }

        private void InitializeItems(string type, ItemsControl purchasedItemsControl, ItemsControl toBuyItemsControl)
        {
            purchasedItemsControl.ItemsSource = null;
            toBuyItemsControl.ItemsSource = null;

            List<Items> purchasedItems = new List<Items>();
            List<Items> toBuyItems = new List<Items>();

            List<string> paths = this.main.GetPaths(type);
            foreach (string path in paths)
            {
                (List<Items> items1, List<Items> items2) = this.GetItems(path, type);
                purchasedItems.AddRange(items1);
                toBuyItems.AddRange(items2);
            }
            purchasedItemsControl.ItemsSource = purchasedItems;
            toBuyItemsControl.ItemsSource = toBuyItems;
        }

        private (List<Items>, List<Items>) GetItems(string path, string type)
        {
            List<Items> purchasedItems = new List<Items>();
            List<Items> toBuyItems = new List<Items>();
            string[] imagePaths = Directory.GetFiles(path, "*.png");
            foreach (string imagePath in imagePaths)
            {
                string pathName = System.IO.Path.GetFileName(imagePath);
                string name = pathName.Substring(0, pathName.Length - 4);
                string price = pathName.Split('_')[1].Replace(".png", string.Empty);
                string priceText = null;
                if (!bool.Parse(this.main.GetFromFile(type, name, "false")) && price != "0")
                    priceText = $"$ {price}";

                BitmapImage imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.UriSource = new Uri(imagePath, UriKind.Absolute);
                imageSource.EndInit();
                if (priceText != null)
                    toBuyItems.Add(new Items(imageSource, name, priceText, type));
                else
                    purchasedItems.Add(new Items(imageSource, name, priceText, type));
            }
            return (purchasedItems, toBuyItems);
        }

        private void ChangeOutfit(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            string tag = border.Tag.ToString();
            string[] args = tag.Split('-');
            string type = args[0];
            string name = args[1];
            this.main.ChangeOutfit(type, name);
        }

        private void WClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.main.winSettings = null;
        }
    }
}
