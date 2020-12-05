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

namespace CoreCoder_Studio.Controls
{
    /// <summary>
    /// Interaction logic for HeaderText.xaml
    /// </summary>
    public partial class HeaderText : UserControl
    {
        public HeaderText()
        {
            InitializeComponent();
            var dpd = DependencyPropertyDescriptor.FromProperty(LabelContentProperty, typeof(HeaderText));
            dpd.AddValueChanged(this, (sender, args) =>
            {
                this.Label.Text = this.Text;
            });
        }

        public static readonly DependencyProperty LabelContentProperty = DependencyProperty.Register("Text", typeof(string), typeof(HeaderText));

        public string Text
        {
            get
            {
                return GetValue(LabelContentProperty) as string;
            }
            set
            {
                SetValue(LabelContentProperty, value);
            }
        }
    }
}
