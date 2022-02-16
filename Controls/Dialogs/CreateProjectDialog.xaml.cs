using ThaumaStudio.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Path = System.IO.Path;
using System.Reflection;

namespace ThaumaStudio.Controls.Dialogs
{
    public class ProjectJson
    {
        public int[] thauma = { 0, 0, 1 };
        public string project_name = "MyProject";
        public string main = "main.lua";
        public string description = "Made with Thauma Studio";
    }
    /// <summary>
    /// Interaction logic for CreateProjectDialog.xaml
    /// </summary>
    public partial class CreateProjectDialog : Window
    {
        MainWindow parent;
        public CreateProjectDialog(MainWindow parent)
        {
            this.parent = parent;
            InitializeComponent();
        }


        public Boolean isAllInfoNeededOkay()
        {
            if (this.projName.Text == "" || this.projDesc.Text == "")
                return false;
            return true;
        }

        public void fillInInformationNeeded()
        {
            if (this.projName.Text == "")
            {
                this.projName.Text = "My Project";
            }
            if (this.projDesc.Text == "")
            {
                this.projDesc.Text = "Made using Thauma Studio";
            }
        }

        public void CreateProject()
        {

            if (this.isAllInfoNeededOkay())
            {
                string targetPath = this.projDir.Text;
                string targetName = this.projName.Text;
                // Actually start making the project
                Directory.CreateDirectory(targetPath);

                // Create project.json
                ProjectJson proj = new ProjectJson();
                proj.project_name = targetName;
                proj.description = projDesc.Text;
                string json = JsonConvert.SerializeObject(proj, Formatting.Indented);
                File.WriteAllText(targetPath + "project.json", json);

                // Creates main.lua 
                string content = GetTemplateMainLua();// Assembly.GetExecutingAssembly().GetManifestResourceStream("").Read();
                File.WriteAllText(targetPath + "main.lua", content);


                /// Writes icon.png
                content = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAkklEQVQ4jcWTwQ2AIAxFPyzgEMQFnIVlnMBlnKULEIZgAjwoBmgRogf/sfQ9SlIU5MRGXfUKEQD2ZRFpS8S4XBA3YzBPU+PyMy4ErN7fbBIMwZJEp+IoXPcqALH15l4sEXS/7Tn/CVwI7wUJ/jRBLlCWqLCO3M72oB5NAqVztsojkVb5lgBAS3SBBce+Zy4SwvoPgSQ37k+vrRYAAAAASUVORK5CYII=";
                byte[] tempBytes = Convert.FromBase64String(content);
                File.WriteAllBytes(targetPath + "icon.png", tempBytes);

                /// Creates the folder structure
                string[] folders = { "Objects", "Rooms", "Textures", "Sounds", "Scripts" };
                foreach (string f in folders)
                {
                    var path = targetPath + f;
                    Directory.CreateDirectory(path);
                }

                // Closes the dialog
                this.DialogResult = true;
            }
            else
            {
                this.fillInInformationNeeded();
            }
        }


        private void OnNameChanged(object sender, TextChangedEventArgs e)
        {
            this.projDir.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar
                + "ThaumaStudio" + Path.DirectorySeparatorChar +
                Regex.Replace(this.projName.Text, "[\\\\|/\"':$#*%&<>]", "_") + Path.DirectorySeparatorChar; // Folder name
        }

        private void OnCreateClick(object sender, RoutedEventArgs e)
        {
            CreateProject();
        }

        private string GetTemplateMainLua()
        {
            string strResourceName = "main.lua";

            Assembly asm = Assembly.GetExecutingAssembly();
            string strTxt;
            using (Stream rsrcStream = asm.GetManifestResourceStream("ThaumaStudio.Resources." + strResourceName))
            {
                using (StreamReader sRdr = new StreamReader(rsrcStream))
                {
                    //For instance, gets it as text
                    strTxt = sRdr.ReadToEnd();
                }
            }
            return strTxt;
        }
    }
}