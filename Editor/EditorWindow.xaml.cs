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

namespace ThaumaStudio.Editor
{
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
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

        //public Dictionary<string, FileBrowserData>

        public EditorWindow()
        {
            InitializeComponent();
            this.TitleBar.w = this;
            this.TitleBar.RefreshMaximizeRestoreButton();
            this.RefreshFileBrowser();
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

        public void RefreshFileBrowser() {
            /// Refresh the content of the file browser
            FileBrowser.Items.Clear();
            //FileBrowser.Items.Add(new FileBrowserData { Filename = "zzz", Icon = null });

            string bp_path = Util.Util.com_mojang_path + "/" + Application.Current.Properties["bp_folder"];
            FileBrowserData bp_item = new FileBrowserData { Filename = Path.GetFileName(bp_path), Path = bp_path, isFolder = true };
            readFolderToBrowser(bp_item, FileBrowser.Items, bp_path);
            FileBrowser.Items.Add(bp_item);

            if ((bool)Application.Current.Properties["has_rp"] == true)
            {
                string rp_path = Util.Util.com_mojang_path + "/" + Application.Current.Properties["rp_folder"];
                FileBrowserData rp_item = new FileBrowserData { Filename = Path.GetFileName(rp_path), Path = rp_path, isFolder = true };
                readFolderToBrowser(rp_item, FileBrowser.Items, rp_path);
                FileBrowser.Items.Add(rp_item);
            }
        }

        private ItemCollection readFolderToBrowser(FileBrowserData i, ItemCollection c, string path)
        {
            foreach (string dir in Directory.GetDirectories(path))
            {
                FileBrowserData item = new FileBrowserData { Filename = Path.GetFileName(dir), Icon = bmpFolder, isFolder = true, Path=dir };
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
                else {
                    c.Add(data);
                }
            }
            return c;
        }

        private void FileBrowser_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            FileBrowserData data = (FileBrowserData)e.NewValue;
        }

        private void FileBrowserItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileBrowserData data = (FileBrowserData)((TreeViewItem)e.Source).DataContext;
            if (!data.isFolder)
            {
                // Docking panel that can be floating
                LayoutDocument content = new LayoutDocument();

                // All the controls
                DockPanel dp = new DockPanel();
                StackPanel tb = new StackPanel() { Orientation = Orientation.Horizontal, Height = 32 };
                TextEditor editor = new ICSharpCode.AvalonEdit.TextEditor() { ShowLineNumbers=true };

                // Toolbar buttons
                var btnSave = new Button() {
                    Width = 32,
                    ToolTip = "Save (Ctrl+S)",
                    Style = this.FindResource("ToolBarButton") as Style,
                    Content = new Image() { 
                        Margin = new Thickness(4),
                        Source = bmpSave, Stretch=Stretch.UniformToFill
                    }
                };
                btnSave.Click += ((object _sender, RoutedEventArgs _e) => {
                    Editor_SaveFile(editor, data, content);
                });

                var btnSaveAs = new Button() {
                    Width = 32,
                    ToolTip = "Save (Ctrl+Shift+S)",
                    Style = this.FindResource("ToolBarButton") as Style,
                    Content = new Image() {
                        Margin = new Thickness(4),
                        Source = bmpSaveAs, Stretch = Stretch.UniformToFill
                    } 
                };
                btnSaveAs.Click += ((object _sender, RoutedEventArgs _e) => {
                    Editor_SaveFileAs(editor, data, content);
                });

                // Add all to the layouts
                tb.Children.Add(btnSave); tb.Children.Add(btnSaveAs);
                dp.Children.Add(tb);
                DockPanel.SetDock(tb, Dock.Top);
                dp.Children.Add(editor);

                // Open the file and set syntax highlighting
                editor.Text = File.ReadAllText(data.Path);
                if (data.Filename.EndsWith(".json") || data.Filename.EndsWith(".js")) {
                    InitializeEditor(editor, data.Path);
                }

                // Set the docking panel's content and title
                content.Content = dp;
                content.Title = data.Filename;
                Documents.Children.Add(content);
                content.IsSelected = true;

                // Handle editor events
                editor.TextChanged += ((object o, EventArgs ev) => {
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
                content.Closing += ((object o, CancelEventArgs ev) => {
                    if (content.Title.EndsWith("*")) {
                        MessageBoxResult res = askYesNoCancel("File is not saved, would you like to save?", "Warning");
                        if (res == MessageBoxResult.Yes)
                        {
                            // Proceed with saving
                            Editor_SaveFile(editor, data, content);
                        }
                        else if (res == MessageBoxResult.No) { 
                            // Proceed without saving
                        }
                        else
                        {
                            // Cancel
                            ev.Cancel = true;
                        }
                    }
                });
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
        public void SaveFile(string content, string path) {
            File.WriteAllText(path, content);
        }

        /** When an editor's text content changed */
        public void Editor_OnChange(TextEditor editor, LayoutDocument doc) {
            if (!doc.Title.EndsWith("*")) {
                doc.Title += "*";
            }
        }

        /** Initialize AvalonEdit editor */
        public void InitializeEditor(TextEditor editor, string path) {
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
        }

        /** Set the editor theme */
        private void SetEditorTheme(TextEditor editor, string path) {
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
        private void FileBrowserItem_MouseClick(object sender, MouseButtonEventArgs e)
        {
            FileBrowserData data;
            if(e.Source is ContentPresenter)
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

    }

    public class EditorData {
        public ItemCollection fileBrowserItems;
    }

    public class FileBrowserData {
        public FileBrowserData() {
            Contents = new List<FileBrowserData>();
        }
        public String Filename { get; set; }
        public String Path { get; set; }
        public BitmapSource Icon { get; set; }
        public List<FileBrowserData> Contents { get; private set; }

        public bool isFolder { get; set; } = false;
    }

}
