using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace CoreCoder_Studio.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for DialogTitleBar.xaml
    /// </summary>
    public partial class DialogTitleBar : UserControl
    {
        public DialogTitleBar()
        {
            InitializeComponent();

            // Detect when the variable changes
            var dpd = DependencyPropertyDescriptor.FromProperty(TitleProperty, typeof(DialogTitleBar));
            dpd.AddValueChanged(this, (sender, args) =>
            {
                this.TitleText.Content = this.Title;
            });
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        /* Custom Property */
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(DialogTitleBar));
        public string Title
        {
            get{return GetValue(TitleProperty) as string;}
            set{SetValue(TitleProperty, value);}
        }
    }
}
