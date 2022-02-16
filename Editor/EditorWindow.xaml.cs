using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using ThaumaStudio.Controls;
using ChromeTabs;
using AvalonDock.Themes;
using System.IO;
using Path = System.IO.Path;
using AvalonDock.Layout;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Xml;
using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Diagnostics;
using ThaumaStudio.Util;
using static ThaumaStudio.Util.IconChanger;

namespace ThaumaStudio.Editor
{
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        public string projectPath = ""; // will be set from MainWindow
        public static Uri icFolder = new Uri($"pack://application:,,,/Resources/icons/folder.png", UriKind.Absolute);
        public static Uri icFile = new Uri($"pack://application:,,,/Resources/icons/file.png", UriKind.Absolute);
        public static Uri icArchive = new Uri($"pack://application:,,,/Resources/icons/archive.png", UriKind.Absolute);
        public static Uri icSettings = new Uri($"pack://application:,,,/Resources/icons/settings-filled.png", UriKind.Absolute);
        public static Uri icSave = new Uri($"pack://application:,,,/Resources/icons/save.png", UriKind.Absolute);
        public static Uri icSaveAs = new Uri($"pack://application:,,,/Resources/icons/saveas.png", UriKind.Absolute);

        public BitmapSource bmpFolder = new BitmapImage(icFolder);
        public BitmapSource bmpFile = new BitmapImage(icFile);
        public BitmapSource bmpArchive = new BitmapImage(icArchive);
        public BitmapSource bmpSettings = new BitmapImage(icSettings);
        public BitmapSource bmpSave = new BitmapImage(icSave);
        public BitmapSource bmpSaveAs = new BitmapImage(icSaveAs);
        public ObservableCollection<CCTabClass> ItemCollection { get; set; }
        public CCTabClass SelectedTab { get; set; }
        public static EditorWindow Instance;
        public ContextMenu FileBrowserContextMenu = new ContextMenu();

        public Dictionary<string, LayoutDocument> openedFiles = new Dictionary<string, LayoutDocument>(); // key: filepath, value: tab 

        public FontFamily FFSourceCodePro;
        IHighlightingDefinition LuaHighlightDef;

        //public Dictionary<string, FileBrowserData>

        public EditorWindow()
        {
            InitializeComponent();
            Instance = this;
            this.TitleBar.w = this;
            this.TitleBar.RefreshMaximizeRestoreButton();
            FFSourceCodePro = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/font/#Source Code Pro Medium");

            // Initialize the File Browser Context Menu
            MenuItem createFolder = new MenuItem();
            createFolder.Header = "Create folder";
            createFolder.Click += FileBrowser_CreateFolderClick;
            FileBrowserContextMenu.Items.Add(createFolder);
        }

        private void FileBrowser_CreateFolderClick(object sender, RoutedEventArgs e)
        {
            
        }

        public bool askYesNo(string message, string title)
        {
            MessageBoxResult rsltMessageBox = MessageBox.Show(message, title, MessageBoxButton.YesNo);
            return (rsltMessageBox == MessageBoxResult.Yes);
        }
        public MessageBoxResult askYesNoCancel(string message, string title)
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNoCancel);
        }
        protected override void OnStateChanged(EventArgs e)
        {
            /// Refresh the maximize/restore button state
            base.OnStateChanged(e);
            this.TitleBar.RefreshMaximizeRestoreButton();
        }

        public void RefreshFileBrowser()
        {
            /// Refresh the content of the file browser
            FileBrowser.Items.Clear();

            FileBrowserData root_folder = new FileBrowserData { Filename = Path.GetFileName(projectPath), Path = projectPath, isFolder = true };
            readFolderToBrowser(root_folder, FileBrowser.Items, projectPath);
            FileBrowser.Items.Add(root_folder);

        }

        public ContextMenu GetFileContextMenu(FileBrowserData data)
        {
            //TODO: this would cause a lot of memory use for large projects(> 100 items)
            ContextMenu contextMenu = new ContextMenu();

            MenuItem mCreateFolder = new MenuItem();
            mCreateFolder.Header = "Create Folder";
            contextMenu.Items.Add(mCreateFolder);
            mCreateFolder.Click += (object s, RoutedEventArgs e) => { 
                // Create a new folder

            };

            MenuItem mCreateFile = new MenuItem();
            mCreateFile.Header = "Create File";
            contextMenu.Items.Add(mCreateFile);

            MenuItem mDelete = new MenuItem();
            mDelete.Header = "Delete";
            contextMenu.Items.Add(mDelete);

            MenuItem mProperties = new MenuItem();
            mProperties.Header = "Properties";
            contextMenu.Items.Add(mProperties);

            return contextMenu;
        }

        private ItemCollection readFolderToBrowser(FileBrowserData i, ItemCollection c, string path)
        {
            foreach (string dir in Directory.GetDirectories(path))
            {
                FileBrowserData item = new FileBrowserData { Filename = Path.GetFileName(dir), Icon = bmpFolder, isFolder = true, Path = dir };
                item.ContextMenu = GetFileContextMenu(item);
                if (i != null)
                {
                    i.Contents.Add(item);
                    readFolderToBrowser(item, c, path + "/" + item.Filename);
                }
                else
                {
                    c.Add(item);
                    readFolderToBrowser(item, c, path + "/" + item.Filename);
                }

            }
            foreach (string dir in Directory.GetFiles(path))
            {
                BitmapSource source = null;

                if (dir.EndsWith(".png") || dir.EndsWith(".jpeg") || dir.EndsWith(".jpg") || dir.EndsWith(".bmp"))
                {
                    try
                    {
                        // Read the image from path
                        //Uri uri = new Uri(folder + "\\pack_icon.png", UriKind.RelativeOrAbsolute);
                        var Source = new BitmapImage();


                        var stream = File.OpenRead(dir);

                        Source.BeginInit();
                        Source.CacheOption = BitmapCacheOption.OnLoad;
                        Source.StreamSource = stream;
                        Source.EndInit();
                        stream.Close();
                        stream.Dispose();

                        source = Source;
                    }
                    catch
                    {
                        // Do nothing if failed to load the image
                    }
                }
                else if (dir.EndsWith(".mcaddon") || dir.EndsWith(".mcpack") || dir.EndsWith(".zip") || dir.EndsWith(".rar") || dir.EndsWith(".zip"))
                    source = bmpArchive;
                else if (dir.EndsWith("manifest.json"))
                    source = bmpSettings;
                else
                {
                    source = bmpFile;
                }
                FileBrowserData data = new FileBrowserData { Filename = Path.GetFileName(dir), Icon = source, Path = dir };
                if (i != null)
                {
                    i.Contents.Add(data);
                }
                else
                {
                    c.Add(data);
                }
            }
            return c;
        }

        private void FileBrowser_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            FileBrowserData data = (FileBrowserData)e.NewValue;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        private void FileBrowserItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileBrowserData data = (FileBrowserData)((TreeViewItem)e.Source).DataContext;
            if (!data.isFolder)
            {
                if (openedFiles.ContainsKey(data.Path))
                {
                    openedFiles[data.Path].IsActive = true;
                    return;
                }
                // Docking panel that can be floating
                LayoutDocument content = new LayoutDocument();

                // All the controls
                DockPanel dp = new DockPanel();
                StackPanel tb = new StackPanel() { Orientation = Orientation.Horizontal, Height = 32 };
                tb.SetResourceReference(BackgroundProperty, "var_backgroundColorDarker");
                TextEditorOptions options = new TextEditorOptions();
                options.ShowTabs = false;

                TextEditor editor = new TextEditor() { ShowLineNumbers = true, Options = options };

                // Toolbar buttons
                var btnSave = new Button()
                {
                    Width = 32,
                    ToolTip = "Save (Ctrl+S)",
                    Style = this.FindResource("ToolBarButton") as Style,
                    Content = new Image()
                    {
                        Margin = new Thickness(4),
                        Source = bmpSave,
                        Stretch = Stretch.UniformToFill
                    }
                };
                btnSave.Click += ((object _sender, RoutedEventArgs _e) =>
                {
                    Editor_SaveFile(editor, data, content);
                });

                var btnSaveAs = new Button()
                {
                    Width = 32,
                    ToolTip = "Save (Ctrl+Shift+S)",
                    Style = this.FindResource("ToolBarButton") as Style,
                    Content = new Image()
                    {
                        Margin = new Thickness(4),
                        Source = bmpSaveAs,
                        Stretch = Stretch.UniformToFill
                    }
                };
                btnSaveAs.Click += ((object _sender, RoutedEventArgs _e) =>
                {
                    Editor_SaveFileAs(editor, data, content);
                });

                // Add all to the layouts
                tb.Children.Add(btnSave); 
                tb.Children.Add(btnSaveAs);

                dp.Children.Add(tb);
                DockPanel.SetDock(tb, Dock.Top);
                dp.Children.Add(editor);

                // Open the file and set syntax highlighting
                editor.Text = File.ReadAllText(data.Path).Replace("    ", "\t");
                if (data.Filename.EndsWith(".json") || data.Filename.EndsWith(".js") || data.Filename.EndsWith(".lua"))
                {
                    InitializeEditor(editor, data.Path);
                }

                // Set the docking panel's content and title
                content.Content = dp;
                content.Title = data.Filename;
                Documents.Children.Add(content);
                content.IsSelected = true;

                // Handle editor events
                editor.TextChanged += ((object o, EventArgs ev) =>
                {
                    if (content.Title != null)
                        Editor_OnChange(editor, content);
                });

                editor.KeyDown += ((object _o, KeyEventArgs _e) =>
                {
                    if (_e.Key == Key.S && Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        Editor_SaveFileAs(editor, data, content);
                    }
                    if (_e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        Editor_SaveFile(editor, data, content);
                    }
                });

                // Handle tab closing event
                content.Closing += ((object o, CancelEventArgs ev) =>
                {
                    if (content.Title.EndsWith("*"))
                    {
                        MessageBoxResult res = askYesNoCancel("File is not saved, would you like to save?", "Warning");
                        if (res == MessageBoxResult.Yes)
                        {
                            // Proceed with saving
                            Editor_SaveFile(editor, data, content);
                        }
                        else if (res == MessageBoxResult.No)
                        {
                            // Proceed without saving
                        }
                        else
                        {
                            // Cancel
                            ev.Cancel = true;
                        }
                    }
                });

                // Add to opened files
                openedFiles.Add(data.Path, content);
            }
        }
        /** Called when the editor requesting to save a file */
        public void Editor_SaveFile(TextEditor editor, FileBrowserData data, LayoutDocument content)
        {
            SaveFile(editor.Text, data.Path);
            if (content.Title.EndsWith("*"))
                content.Title = content.Title.Substring(0, content.Title.Length - 1);
        }

        /** Called when the editor requesting to save a file as another file */
        public void Editor_SaveFileAs(TextEditor editor, FileBrowserData data, LayoutDocument content)
        {
            System.Windows.Forms.SaveFileDialog s = new System.Windows.Forms.SaveFileDialog();
            s.InitialDirectory = Path.GetDirectoryName(data.Path);
            if (s.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveFile(editor.Text, s.FileName);
                if (content.Title.EndsWith("*"))
                    content.Title = content.Title.Substring(0, content.Title.Length - 1);
                data.Path = s.FileName;
                content.Title = Path.GetFileName(s.FileName);
            }
            RefreshFileBrowser();
        }
        /** Save the currently opened tab file */
        public void SaveFile(string content, string path)
        {
            File.WriteAllText(path, content);
        }

        /** When an editor's text content changed */
        public void Editor_OnChange(TextEditor editor, LayoutDocument doc)
        {
            if (!doc.Title.EndsWith("*"))
            {
                doc.Title += "*";
            }
        }
        public void LoadLuaHighlighter()
        {
            /// Enable custom highlighter
            XshdSyntaxDefinition xshd;
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("ThaumaStudio.Resources.highlighting.LuaHighlighting.xml"))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    xshd = HighlightingLoader.LoadXshd(reader);
                    LuaHighlightDef = HighlightingLoader.Load(xshd, null);
                }
            }
        }
        /** Initialize AvalonEdit editor */
        public void InitializeEditor(TextEditor editor, string path)
        {

            if (LuaHighlightDef == null)
            {
                LoadLuaHighlighter();
            }
            // Set the highlighting
            SetEditorTheme(editor, path);

            editor.ContextMenu = new ContextMenu();

            var item = new MenuItem();
            item.Header = "Cut";
            item.Command = ApplicationCommands.Cut;
            editor.ContextMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "Copy";
            item.Command = ApplicationCommands.Copy;
            editor.ContextMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "Paste";
            item.Command = ApplicationCommands.Paste;
            editor.ContextMenu.Items.Add(item);

            item = new MenuItem();
            item.Header = "Delete";
            item.Command = ApplicationCommands.Delete;
            editor.ContextMenu.Items.Add(item);
            // Set font
            //editor.FontFamily = FFSourceCodePro;
        }

        /** Set the editor theme */
        private void SetEditorTheme(TextEditor editor, string path)
        {

            // Set highlighted language
            if (path.EndsWith(".js") || path.EndsWith(".json"))
            {
                editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("JavaScript");
                var highlighting = editor.SyntaxHighlighting;

                // Digits
                var digitHighlighting = highlighting.NamedHighlightingColors.First(c => c.Name == "Digits");
                digitHighlighting.Foreground = new SimpleHighlightingBrush((Color)FindResource("ch_numerical"));

                // String
                var stringHighlighting = highlighting.NamedHighlightingColors.First(c => c.Name == "String");
                stringHighlighting.Foreground = new SimpleHighlightingBrush((Color)FindResource("ch_string"));

                // Comment
                var commentHighlighting = highlighting.NamedHighlightingColors.First(c => c.Name == "Comment");
                commentHighlighting.Foreground = new SimpleHighlightingBrush((Color)FindResource("ch_comment"));

                // JavaScriptKeyWords" foreground="Blue" exampleText="return myVariable;" />
                var JavaScriptKeyWords = highlighting.NamedHighlightingColors.First(c => c.Name == "JavaScriptKeyWords");
                JavaScriptKeyWords.Foreground = new SimpleHighlightingBrush((Color)FindResource("ch_special1"));

                // JavaScriptIntrinsics" foreground="Blue" exampleText="Math.random()" />
                var JavaScriptIntrinsics = highlighting.NamedHighlightingColors.First(c => c.Name == "JavaScriptIntrinsics");
                JavaScriptIntrinsics.Foreground = new SimpleHighlightingBrush((Color)FindResource("ch_special2"));

                // JavaScriptLiterals foreground="Blue" exampleText="return false;" />
                var JavaScriptLiterals = highlighting.NamedHighlightingColors.First(c => c.Name == "JavaScriptLiterals");
                JavaScriptLiterals.Foreground = new SimpleHighlightingBrush((Color)FindResource("ch_special3"));

                // JavaScriptGlobalFunctions foreground="Blue" exampleText="escape(myString);" />
                var JavaScriptGlobalFunctions = highlighting.NamedHighlightingColors.First(c => c.Name == "JavaScriptGlobalFunctions");
                JavaScriptGlobalFunctions.Foreground = new SimpleHighlightingBrush((Color)FindResource("ch_special1"));

                // Set the syntaxHighlighting
                editor.SyntaxHighlighting = highlighting;
            }
            else if (path.EndsWith(".lua"))
            {
                var highlighting = LuaHighlightDef;

                // Digits
                var digitHighlighting = highlighting.NamedHighlightingColors.First(c => c.Name == "Digits");
                digitHighlighting.Foreground = new SimpleHighlightingBrush((Color)FindResource("ch_numerical"));

                // String
                var stringHighlighting = highlighting.NamedHighlightingColors.First(c => c.Name == "String");
                stringHighlighting.Foreground = new SimpleHighlightingBrush((Color)FindResource("ch_string"));

                // Comment
                var commentHighlighting = highlighting.NamedHighlightingColors.First(c => c.Name == "Comment");
                commentHighlighting.Foreground = new SimpleHighlightingBrush((Color)FindResource("ch_comment"));

                // JavaScriptKeyWords" foreground="Blue" exampleText="return myVariable;" />
                var JavaScriptKeyWords = highlighting.NamedHighlightingColors.First(c => c.Name == "LuaKeyWords");
                JavaScriptKeyWords.Foreground = new SimpleHighlightingBrush((Color)FindResource("ch_special1"));

                // JavaScriptIntrinsics" foreground="Blue" exampleText="Math.random()" />
                //var JavaScriptIntrinsics = highlighting.NamedHighlightingColors.First(c => c.Name == "LuaIntrinsics");
                //JavaScriptIntrinsics.Foreground = new SimpleHighlightingBrush((Color)FindResource("ch_special2"));

                // JavaScriptLiterals foreground="Blue" exampleText="return false;" />
                var JavaScriptLiterals = highlighting.NamedHighlightingColors.First(c => c.Name == "LuaLiterals");
                JavaScriptLiterals.Foreground = new SimpleHighlightingBrush((Color)FindResource("ch_special3"));

                // JavaScriptGlobalFunctions foreground="Blue" exampleText="escape(myString);" />
                var JavaScriptGlobalFunctions = highlighting.NamedHighlightingColors.First(c => c.Name == "LuaGlobalFunctions");
                JavaScriptGlobalFunctions.Foreground = new SimpleHighlightingBrush((Color)FindResource("ch_special1"));

                // LuaObjects foreground="Blue" exampleText="Object.function(myString);" />
                var LuaObjects = highlighting.NamedHighlightingColors.First(c => c.Name == "Object");
                LuaObjects.Foreground = new SimpleHighlightingBrush((Color)FindResource("ch_special4"));
                editor.SyntaxHighlighting = highlighting;
            }
        }
        private void FileBrowserItem_MouseClick(object sender, MouseButtonEventArgs e)
        {
            FileBrowserData data;
            if (e.Source is ContentPresenter)
                data = (FileBrowserData)((ContentPresenter)e.Source).DataContext;
            if (e.Source is ItemsPresenter)
                data = (FileBrowserData)((ItemsPresenter)e.Source).DataContext;
        }

        private void MenuItem_View_FileBrowser_Click(object sender, RoutedEventArgs e)
        {
            FileBrowserPanel.Show();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            RefreshFileBrowser();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            RefreshFileBrowser();
        }

        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            /// <summary>
            /// Export the game to a ready executable
            /// </summary>
            

            // Copy the Runtime
            string exportPath = projectPath + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar;
            TextBoxOutput.Text+= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Util.Util.CopyFilesRecursively(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Runtime", exportPath);
            string executablePath = exportPath + Path.GetFileName(projectPath) + ".exe";
            if (File.Exists(executablePath))
                File.Delete(executablePath);

            // Rename it to game's name
            File.Move(exportPath + "ThaumaRuntime.exe", executablePath);

            // Change the file icon
            IconChanger iconChanger = new IconChanger();
            ICResult result = iconChanger.ChangeIcon(executablePath, "D:\\Programming\\C++Projects\\old\\ArmoEngine\\res\\armo-icon.ico");
            if (result == ICResult.Success)
            {
                TextBoxOutput.Text += "Release Build SUCCESS";
            }
            else
            {
                TextBoxOutput.Text += "Release Build FAILED";
            }
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            TextBoxOutput.Text = "";
            // Run the gameStringBuilder outputBuilder;
            ProcessStartInfo processStartInfo;
            Process process;

            var outputBuilder = new StringBuilder();

            processStartInfo = new ProcessStartInfo();
            processStartInfo.CreateNoWindow = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.Arguments = "\"" + projectPath + "\"";
            processStartInfo.FileName = "./Runtime/ThaumaRuntime.exe";

            process = new Process();
            process.StartInfo = processStartInfo;
            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            //process.WaitForExit();
            //process.CancelOutputRead();
            //process.CancelErrorRead();
        }

        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            this.Dispatcher.Invoke(() =>
            {
                // your code here.
                Console.WriteLine(outLine.Data);
                TextBoxOutput.Text += outLine.Data + "\n";
            });
        }
    }

    public class EditorData
    {
        public ItemCollection fileBrowserItems;
    }

    public class FileBrowserData
    {
        public FileBrowserData()
        {
            Contents = new List<FileBrowserData>();
        }
        public String Filename { get; set; }
        public String Path { get; set; }
        public BitmapSource Icon { get; set; }
        public List<FileBrowserData> Contents { get; private set; }
        public ContextMenu ContextMenu { get; set; }

        public bool isFolder { get; set; } = false;
    }

}
