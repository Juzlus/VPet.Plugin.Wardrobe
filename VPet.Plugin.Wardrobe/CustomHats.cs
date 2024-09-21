using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.RightsManagement;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.CustomHats
{
    public class CustomHats : MainPlugin
    {
        public string path;
        public winApp winApp;
        private string graphPath;
        private string activeHat;
        private string activeMask;
        private string activeNecklace;
        public winSettings winSettings;

        private Thread animationThread1;
        private Thread animationThread2;
        private Thread animationThread3;

        public override string PluginName => nameof(CustomHats);

        public CustomHats(IMainWindow mainwin)
          : base(mainwin)
        {
        }

        public override void LoadPlugin()
        {
            this.path = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName;
            this.activeHat = this.GetFromFile("config", "ActiveHats", "None");
            this.activeMask = this.GetFromFile("config", "ActiveMasks", "None");
            this.activeNecklace = this.GetFromFile("config", "ActiveNecklaces", "None");
            this.CreateImage();
            this.MW.Main.GraphDisplayHandler += this.AnimationChange;
        }

        public override void LoadDIY() => this.CreateMenuItem();

        public string GetFromFile(string fileName, string key, string alt = null)
        {
            key = key.Split("_")[0].Replace(" ", "_");
            string configPath = Path.Combine(this.path, "config", $"{fileName}.cfg");
            XmlDocument xmlDoc = new XmlDocument();
            if (!File.Exists(configPath))
                return alt;

            xmlDoc.Load(configPath);
            XmlNode node = xmlDoc.SelectSingleNode($"//{key}");
            if (node != null)
                return node.InnerText;

            return alt;
        }

        public void SaveToFile(string fileName, string key, string value)
        {
            key = key.Split("_")[0].Replace(" ", "_");
            string configPath = Path.Combine(this.path, "config", $"{fileName}.cfg");
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(configPath))
                xmlDoc.Load(configPath);
            else
            {
                XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(xmlDeclaration);

                XmlElement root = xmlDoc.CreateElement($"Root");
                xmlDoc.AppendChild(root);
            }

            XmlElement node = xmlDoc.SelectSingleNode($"//{key}") as XmlElement;
            if (node is null)
            {
                node = xmlDoc.CreateElement(key);
                xmlDoc.DocumentElement?.AppendChild(node);
            }
            node.InnerText = value;
            try
            {
                xmlDoc.Save(configPath);
            }
            catch { }
        }

        private void AnimationChange(object sender)
        {
            GraphInfo graphInfo = (GraphInfo)sender;
            this.graphPath = $"{graphInfo.Name}_{graphInfo.Animat}_{graphInfo.ModeType}";  // default++Single

            this.animationThread1 = new Thread(() => this.GetAnim("hats"));
            this.animationThread1.Start();

            this.animationThread2 = new Thread(() => this.GetAnim("masks"));
            this.animationThread2.Start();

            this.animationThread3 = new Thread(() => this.GetAnim("necklaces"));
            this.animationThread3.Start();
        }

        private void CreateImage()
        {
            this.winApp = new winApp();
            this.ChangeOutfit("hats", this.activeHat);
            this.ChangeOutfit("masks", this.activeMask);
            this.ChangeOutfit("necklaces", this.activeNecklace);
            this.MW.Main.UIGrid.Children.Add(this.winApp);
        }

        public void ChangeOutfit(string imageType, string imageName)
        {
            if (this.winApp == null) return;

            string imagePath = null;
            List<string> paths = this.GetPaths(imageType);
            foreach (string pathF in paths)
            {
                string img = Path.Combine(pathF, imageName + ".png");
                if (!File.Exists(img))
                    continue;
                imagePath = img;
                break;
            }
            if (imagePath == null) return;

            if (!bool.Parse(this.GetFromFile(imageType, imageName, "false")))
            {
                string[] args = imageName.Split('_');
                string name = args[0];
                double price = double.Parse(args[1]);
                double userSalary = this.MW.Core.Save.Money;
                if (userSalary < price)
                {
                    MessageBox.Show("You don't have money!".Translate());
                    return;
                }
                this.SaveToFile(imageType, imageName, "true");
                this.MW.Core.Save.Money -= price;

                this.winSettings.UpdateItems(imageType);
            }

            this.SaveToFile("config", $"Active{imageType.Substring(0, 1).ToUpper()}{imageType.Substring(1)}", imageName);
            this.winApp.ChangeImage(imageType, imageName == "None_0" ? null : imagePath);
        }

        public List<string> GetPaths(string type)
        {
            List<string> modPaths = new List<string>();
            string parentFolder = new DirectoryInfo(this.path).Parent.FullName;
            string[] folders = Directory.GetDirectories(parentFolder);
            foreach (string folder in folders)
            {
                string path = Path.Combine(folder, "wardrobe", type);
                if (Directory.Exists(path))
                    modPaths.Add(path);
            }
            return modPaths;
        }

        private void GetAnim(string type)
        {
            string filePath = $"{this.path}\\data\\{type}\\{this.graphPath}.txt";
            if (!File.Exists(filePath))
            {
                this.winApp.ChangeVisibility(type, false);
                return;
            }

            this.winApp.ChangeVisibility(type, true);
            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length <= 0) return;

            foreach (string line in lines)
            {
                string[] args = line.Split('|');
                float x = float.Parse(args[0]);
                float y = float.Parse(args[1]);
                float deg = float.Parse(args[2]);
                int seconds = int.Parse(args[3]);
                this.winApp.ChangeCordinations(type, x / 2, y / 2, -deg);
                Thread.Sleep(seconds);
            }
        }

        public override void Setting()
        {
            if (this.winSettings == null)
            {
                this.winSettings = new winSettings(this);
                this.winSettings.Show();
            }
            else
                this.winSettings.Topmost = true;
        }

        private void CreateMenuItem()
        {
            this.MW.Main.ToolBar.AddMenuButton(VPet_Simulator.Core.ToolBar.MenuType.Feed, "Clothes".Translate(),
                (Action)(() => this.Setting()));
        }

        private void SendMsg(string msgContent)
        {
            this.MW.Main.Say(msgContent);
        }
    }
}