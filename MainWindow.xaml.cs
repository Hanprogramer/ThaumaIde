using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using CoreCoder_Studio.Util;
using CoreCoder_Studio.Controls.Dialogs;

namespace CoreCoder_Studio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            /// Initialize and read all projets from com.mojang
            InitializeComponent();
            RefreshPacksFolders();
        }
        protected override void OnStateChanged(EventArgs e)
        {
            /// Refresh the maximize/restore button state
            base.OnStateChanged(e);
            this.TitleBar.RefreshMaximizeRestoreButton();
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
                    b.ToolTip = info.ToReadableString();

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
                            Uri uri = new Uri(folder + "\\pack_icon.png", UriKind.RelativeOrAbsolute);
                            BitmapImage Source = new BitmapImage(uri);

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
                        }catch(Exception e)
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


        public void OpenCreateDialog(object sender, RoutedEventArgs e) {
            /// <summary>
            /// Open the create project dialog
            /// </summary>
            CreateProjectDialog dlg = new CreateProjectDialog(this);
            if (dlg.ShowDialog() == true) { 
                /// if accepted
            }
        }


    }
}
