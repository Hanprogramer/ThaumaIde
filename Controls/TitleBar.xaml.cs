using System;
using System.Collections.Generic;
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

using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace ThaumaStudio.Controls
{
    /// <summary>
    /// Interaction logic for TitleBar.xaml
    /// </summary>
    public partial class TitleBar : UserControl
    {
        public Window w;
        public TitleBar()
        {
            InitializeComponent();
            // Detect when the variable changes
            var dpd = DependencyPropertyDescriptor.FromProperty(TitleProperty, typeof(TitleBar));
            dpd.AddValueChanged(this, (sender, args) =>
            {
                this.TitleText.Content = this.Title;
            });
        }
        
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            w = Window.GetWindow(this);
        }

        /* Custom Property */
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(TitleBar));
        public string Title
        {
            get { return GetValue(TitleProperty) as string; }
            set { SetValue(TitleProperty, value); }
        }

        public void RefetchParent() {
            if (w == null)
                w = Window.GetWindow(this);
        }

        /* Methods */
        public void RefreshMaximizeRestoreButton()
        {
            if (w.WindowState == WindowState.Maximized)
            {
                this.maximizeButton.Visibility = Visibility.Collapsed;
                this.restoreButton.Visibility = Visibility.Visible;
                this.maximizeRestoreButton.ToolTip = "Restore";
            }
            else
            {
                this.maximizeButton.Visibility = Visibility.Visible;
                this.restoreButton.Visibility = Visibility.Collapsed;
                this.maximizeRestoreButton.ToolTip = "Maximize";
            }
        }
        private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            w.WindowState = WindowState.Minimized;
        }

        private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e)
        {
            if (w.WindowState == WindowState.Maximized)
            {
                w.WindowState = WindowState.Normal;
            }
            else
            {
                w.WindowState = WindowState.Maximized;
            }
            RefreshMaximizeRestoreButton();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            w.Close();
        }

    }
}
