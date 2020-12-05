using CoreCoder_Studio.Util;
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

namespace CoreCoder_Studio.Controls.Dialogs
{
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


        public Boolean isAllInfoNeededOkay() {
            if (this.projName.Text == "" || this.projDesc.Text == "" || this.projNamespace.Text == "")
                return false;
            return true;
        }

        public void fillInInformationNeeded() {
            if (this.projName.Text == "") {
                this.projName.Text = "My Project";
            }
            if (this.projDesc.Text == "") {
                this.projDesc.Text = "Made using CoreCoder:Studio";
            }
            if (this.projNamespace.Text == "") {
                this.projNamespace.Text = "cc";
            }
        }

        public void CreateProject() {
            if (this.isAllInfoNeededOkay())
            {
                // Check for directory name availability
                if (Directory.Exists(Util.Util.com_mojang_path + this.projDir.Text + " BP") && // check for bp folder existance
                    (this.projType.SelectedIndex == 0 || this.projType.SelectedIndex == 1) ) // is BP or addon
                {
                    MessageBox.Show("Project with that folder name already exists!");
                    return;
                }
                if (Directory.Exists(Util.Util.com_mojang_path + this.projDir.Text + " RP") && // check for bp folder existance
                    (this.projType.SelectedIndex == 0 || this.projType.SelectedIndex == 2)) // is BP or addon
                {
                    MessageBox.Show("Project with that folder name already exists!");
                    return;
                }
                String dependencies = ""; // RP UUID if made
                // If all passes, then a project is ready to be made
                // Create BP folder
                if (this.projType.SelectedIndex == 0 || this.projType.SelectedIndex == 1)
                {
                    String path = Util.Util.com_mojang_path + "/development_behavior_packs/" + this.projDir.Text + " BP";
                    // Make the folder
                    Directory.CreateDirectory(path);
                    // Write manifest
                    StreamWriter w = File.CreateText(path + "/manifest.json");

                    ProjectInfo info = new ProjectInfo();
                    info.name = this.projName.Text + " BP";
                    info.description = this.projDesc.Text;
                    info.dependencies = new List<String>();
                    info.uuid = Guid.NewGuid().ToString();
                    if (this.projType.SelectedIndex == 0) {
                        // if addon
                        info.dependencies.Add(dependencies = Guid.NewGuid().ToString());
                    }
                    info.type = "data";
                    info.version = "1";

                    // Write manifest and pack image
                    Util.Util.SavePackImage(path);
                    w.Write(Util.Util.ProjectInfoToJSON(info));
                    w.Close();
                }
                // Create RP folder
                if (this.projType.SelectedIndex == 0 || this.projType.SelectedIndex == 2)
                {
                    String path = Util.Util.com_mojang_path + "/development_resource_packs/" + this.projDir.Text + " RP";
                    // Make the folder
                    Directory.CreateDirectory(path);
                    // Write manifest
                    StreamWriter w = File.CreateText(path + "/manifest.json");

                    ProjectInfo info = new ProjectInfo();
                    info.name = this.projName.Text + " RP";
                    info.description = this.projDesc.Text;
                    info.dependencies = new List<String>();
                    info.type = "resources";
                    info.version = "1";
                    info.uuid = dependencies == "" ? Guid.NewGuid().ToString() : dependencies;

                    // Write manifest and pack image
                    Util.Util.SavePackImage(path);
                    w.Write(Util.Util.ProjectInfoToJSON(info));
                    w.Close();
                }
                this.parent.RefreshPacksFolders();
                this.Close();
            }
            else {
                this.fillInInformationNeeded();
            }
        }

        /// Events

        public void OnCreateClick(object sender, RoutedEventArgs e) {
            CreateProject();
        }


        private void OnNameChanged(object sender, TextChangedEventArgs e)
        {
            this.projDir.Text = "/" + Regex.Replace(this.projName.Text, "[\\\\|/\"':$#*%&<>]", " ");
        }
    }
}
