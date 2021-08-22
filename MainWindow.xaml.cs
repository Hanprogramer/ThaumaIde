using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ThaumaStudio.Util;
using ThaumaStudio.Controls.Dialogs;
using Ionic.Zip;
using System.Threading.Tasks;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;
using ThaumaStudio.Editor;
using System.Collections;
using System.Collections.Generic;

namespace ThaumaStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Points to the appdata folder of CoreCoder
        public static string APPDATA_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CoreCoderStudio/";

        // Stores all packs path with the UUID as key 
        public Dictionary<string, string> packs = new Dictionary<string, string>();
        public MainWindow()
        {
            /// Initialize and read all projets from com.mojang
            InitializeComponent();
            RefreshPacksFolders();
            if (!Directory.Exists(APPDATA_FOLDER))
                Directory.CreateDirectory(APPDATA_FOLDER);
            this.TitleBar.w = this;
            this.TitleBar.RefreshMaximizeRestoreButton();
        }
        protected override void OnStateChanged(EventArgs e)
        {
            /// Refresh the maximize/restore button state
            base.OnStateChanged(e);
            this.TitleBar.RefreshMaximizeRestoreButton();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            RefreshPacksFolders();
        }
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            RefreshPacksFolders();
        }

        private void Discord_Join_Click(object sender, RoutedEventArgs e)
        {
            /// Open a link in browser
            System.Diagnostics.Process.Start("https://discord.com/invite/UyyBkEMvmx");
        }

        
        public void RefreshPacksFolders() {
            /// <summary>
            /// Reads all packs installed in minecraft
            /// </summary>
            /// 

            // first of, clear the packs
            this.bpCont.Children.Clear();
            this.bpDevCont.Children.Clear();
            this.rpCont.Children.Clear();
            this.rpDevCont.Children.Clear();


            String[] paths = { "behavior_packs","development_behavior_packs", "resource_packs", "development_resource_packs" };
            foreach (String p in paths) {
                // The pack category path
                String path = Util.Util.com_mojang_path + p + "\\";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                foreach (String folder in Directory.GetDirectories(path)) {
                    // The folder name only
                    String foldername = folder.Split('\\').Last();

                    // Now reads every folders with manifest.json inside
                    if (!File.Exists(folder + "/manifest.json"))
                    {
                        // If not exists, then continue the loop
                        continue;
                    }

                    // Creates a button using the style "ProjectButton"
                    Button b = new Button { 
                        Content = foldername, ToolTip = folder, 
                        Style = this.FindResource("ProjectButton") as Style 
                    };

                    // Read project info and set tooltip
                    ProjectInfo info = Util.Util.readProjectInfo(folder);
                    packs[info.uuid] = folder.Replace(Util.Util.com_mojang_path, "");
                    b.ToolTip = info.ToReadableString();

                    b.ContextMenu = new ContextMenu();
                    MenuItem mi1 = new MenuItem();
                    mi1.Header = "Edit manifest";
                    MenuItem mi2 = new MenuItem();
                    mi2.Header = "Delete Pack";
                    mi2.Click += async (Object sender, RoutedEventArgs e) => {
                        // Delete the current pack
                        if (askYesNo("Are you sure you want to delete " + info.name + "? (This cannot be undone!)", "Confirm pack delete")) {
                            // Proceed deleting the pack
                            await Task.Run(()=>Directory.Delete(folder, true));
                            RefreshPacksFolders();
                        }
                    };
                    
                    //b.ContextMenu.Items.Add(mi1);
                    b.ContextMenu.Items.Add(mi2);

                    // Setup the button click event
                    b.Click += (Object sender, RoutedEventArgs e) => {
                        // Open the project
                        Application.Current.Properties["has_rp"] = false;
                        Application.Current.Properties["rp_folder"] = "";
                        Application.Current.Properties["bp_folder"] = folder.Replace(Util.Util.com_mojang_path, "");
                        if (info.dependencies.Count > 0) {
                            Application.Current.Properties["has_rp"] = true;
                            try
                            {
                                Application.Current.Properties["rp_folder"] = packs[info.dependencies[0]];
                            }
                            catch (KeyNotFoundException err) {
                                MessageBox.Show("Warning! One or more of your dependencies are failed to load.");
                                Application.Current.Properties["has_rp"] = false;
                            }
                        }
                        EditorWindow w = new EditorWindow();
                        w.Owner = this;
                        this.Hide();
                        w.ShowDialog();
                        this.Show();
                    };


                    // Create a label for it, and a container StackPanel for the button's content
                    Label l = new Label { Content = info.name };
                    StackPanel s = new StackPanel();

                    // If exists, try reading the pack icon and set it
                    if (File.Exists(folder + "\\pack_icon.png"))
                    {
                        try
                        {
                            // Create the image control
                            System.Windows.Controls.Image i = new System.Windows.Controls.Image();

                            // Read the image from path
                            //Uri uri = new Uri(folder + "\\pack_icon.png", UriKind.RelativeOrAbsolute);
                            var Source = new BitmapImage();
                            var stream = File.OpenRead(folder + "\\pack_icon.png");

                            Source.BeginInit();
                            Source.CacheOption = BitmapCacheOption.OnLoad;
                            Source.StreamSource = stream;
                            Source.EndInit();
                            stream.Close();
                            stream.Dispose();
                            // Set it to the control
                            i.Source = Source;

                            // Filtering
                            if (i.Source.Width < 64 || i.Source.Height < 64)
                            {
                                /// If image is smaller than 128x128, disable filtering
                                RenderOptions.SetBitmapScalingMode(i, BitmapScalingMode.NearestNeighbor);
                            }

                            // Common properties
                            i.Stretch = Stretch.Uniform;
                            i.StretchDirection = StretchDirection.Both;
                            i.Width = 40;
                            i.Height = 40;

                            // Add it to the container
                            s.Children.Add(i);
                        }catch
                        {
                            // Do nothing if failed to load the image
                        }
                    }

                    // Add the label to container and add container to the button
                    s.Children.Add(l);
                    b.Content = s;

                    // Decide which category it goes
                    if (p == "behavior_packs") {
                        this.bpCont.Children.Add(b);
                    }
                    else if (p == "development_behavior_packs")
                    {
                        this.bpDevCont.Children.Add(b);
                    }
                    else if (p == "resource_packs")
                    {
                        this.rpCont.Children.Add(b);
                    }
                    else if (p == "development_resource_packs")
                    {
                        this.rpDevCont.Children.Add(b);
                    }
                }
            }
        }

        public void DeletePack(object sender, RoutedEventArgs e) {

        }

        public void OpenCreateDialog(object sender, RoutedEventArgs e) {
            /// <summary>
            /// Open the create project dialog
            /// </summary>
            CreateProjectDialog dlg = new CreateProjectDialog(this);
            if (dlg.ShowDialog() == true) { 
                /// if accepted
            }
        }

        private async void Import_Click(object sender, RoutedEventArgs e)
        {
            // Open a .mcaddon file then import it to minecraft
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Minecraft Addon (*.mcaddon)|*.mcaddon|Minecraft Pack (*.mcpack)|*.mcpack|ZIP files (*.zip)|*.zip|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // If selected a file
                if (openFileDialog.FileName.EndsWith(".mcaddon") || openFileDialog.FileName.EndsWith(".zip") || openFileDialog.FileName.EndsWith(".mcpack"))
                {
                    try {
                        using (var zip = ZipFile.Read(openFileDialog.FileName))
                        {
                            int totalEntries = zip.Entries.Count;
                            string names = "";
                            foreach (ZipEntry entry in zip.Entries)
                            {
                                if(entry.FileName.EndsWith(".mcpack"))
                                    names += entry.FileName + "\n";
                            }

                            if (names == "") {
                                // Continue without asking
                                await DeleteAllFilesInFolder(APPDATA_FOLDER + "/temp/");
                                await ExtractAddon(openFileDialog.FileName, APPDATA_FOLDER + "/temp/");
                            }
                            else if (askYesNo("Do you want to import the following packs? \n" + names, "Import pack")) {
                                // Continue to extract those
                                await DeleteAllFilesInFolder(APPDATA_FOLDER + "/temp/");
                                await ExtractAddon(openFileDialog.FileName,APPDATA_FOLDER + "/temp/");
                            }
                        }
                    }
                    catch(Exception err)
                    {
                        MessageBox.Show("Can't import selected file: " + openFileDialog.FileName + " StackTrace: " + err.StackTrace);
                    }
                }
                else {
                    // file is unknown
                    System.Windows.MessageBox.Show("The file selected is of an unknown format(" + openFileDialog.FileName + ").","Error!");
                }
            }
        }

        internal async Task DeleteAllFilesInFolder(string path) {
            System.IO.DirectoryInfo di = new DirectoryInfo(path);
            //await Task.Run(() => {
            //    foreach (FileInfo file in di.GetFiles())
            //    {
            //        file.Delete();
            //    }
            //});
            await Task.Run(() =>
            {
                //foreach (DirectoryInfo dir in di.GetDirectories())
                //{
                //    if(dir.Exists)
                //        dir.Delete(true);
                //}
                di.Delete(true);
            });
        }

        internal async Task ExtractAddon(string srcPath, string dstPath) {
            try
            {
                if (!Directory.Exists(dstPath))
                    Directory.CreateDirectory(dstPath);
                using (var zip = ZipFile.Read(srcPath))
                {
                    int totalEntries = zip.Entries.Count;
                    foreach (ZipEntry entry in zip.Entries)
                    {
                        await Task.Run(()=> {
                            entry.Extract(dstPath);
                        });
                        if (entry.FileName.EndsWith(".zip") || entry.FileName.EndsWith(".mcpack"))
                        {
                            // If it is a pack inside a .mcaddon
                            // then unpack it
                            string name = entry.FileName;
                            if (Directory.Exists(dstPath + "/" + name))
                                name += " [1]";

                            await ExtractAddon(dstPath + "/" + name, dstPath + "/" + name.Replace(".mcpack", ""));
                            await InstallAddonFromFolder(dstPath + "/" + name.Replace(".mcpack", ""), name.Replace(".mcpack", ""));
                        }
                    }

                    if (File.Exists(dstPath + "/manifest.json"))
                    {
                        // If we're extracting a json, then install a pack from "temp" with zipfile's name
                        await InstallAddonFromFolder(dstPath, Path.GetFileNameWithoutExtension(srcPath));
                    }
                }

            }
            catch (Exception err)
            {
                MessageBox.Show("Can't extract selected file: " + srcPath + " StackTrace: " + err.StackTrace);
            }
            RefreshPacksFolders();
        }

        internal async Task InstallAddonFromFolder(string path, string name) {

            // Condition 1, the pack has manifest.json immediately
            if (!File.Exists(path + "/manifest.json")) {
                // Condition 2, the pack has a single folder inside a folder.
                string[] dirs = Directory.GetDirectories(path);
                if (dirs.Length == 1)
                {
                    if (!File.Exists(dirs[0] + "/manifest.json"))
                    {
                        MessageBox.Show("Error, can't install " + name + "\nUnknown addon type, manifest json unknown.");
                        return;
                    }
                    else {
                        // if found manifest inside a folder inside the pack folder
                        await InstallAddonFromFolder(dirs[0], name);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Error, can't install " + name + "\nUnknown addon type, manifest json unknown.");
                    return;
                }
            }

            // Reads the manifest json
            ProjectInfo info = Util.Util.readProjectInfo(path);
            try
            {
                if (info.type == "data") {
                    // Behavior pack
                    await CopyFolderAndContent(path, Util.Util.com_mojang_path + "/behavior_packs/" + name);
                }
                if (info.type == "resources")
                {
                    // Resource pack
                    await CopyFolderAndContent(path, Util.Util.com_mojang_path + "/resource_packs/" + name);
                }
            }
            catch (Exception e) {
                MessageBox.Show("Error installing " + name + ". Can't copy folders to com.mojang. Make sure to run CoreCoder as administrator or sudo."+e.Message+" Stacktrace:\n" + e.StackTrace);
            }
        }

        internal async Task CopyFolderAndContent(string srcPath, string dstPath) {
            if(!(dstPath.EndsWith("/") || dstPath.EndsWith("\\"))){
                dstPath += "/";
            }
            // Deletes the folder if exists
            if (Directory.Exists(dstPath))
                await DeleteAllFilesInFolder(dstPath);
            //Now Create all of the directories
            await Task.Run(() =>
            {
                foreach (string dirPath in Directory.GetDirectories(srcPath, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(srcPath, dstPath));
            });
            //Copy all the files & Replaces any files with the same name
            await Task.Run(() => {
                foreach (string newPath in Directory.GetFiles(srcPath, "*.*", SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(srcPath, dstPath), true);
            });
        }

        public bool askYesNo(string message, string title) {
            MessageBoxResult rsltMessageBox = MessageBox.Show(message, title, MessageBoxButton.YesNo);
            return (rsltMessageBox == MessageBoxResult.Yes);
        }

        
    }
}
