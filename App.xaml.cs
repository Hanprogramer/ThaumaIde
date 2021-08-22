using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ThaumaStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() {
            this.Properties["bp_folder"] = "behavior_packs/Among Us Pack BP";
            this.Properties["rp_folder"] = "";
            this.Properties["has_rp"] = false;
        }
    }
}
