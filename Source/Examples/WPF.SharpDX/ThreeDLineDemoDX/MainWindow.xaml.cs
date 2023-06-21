using System.Windows;
using HelixToolkit.Wpf.SharpDX.Utilities;

namespace ThreeDLineDemo
{
    public partial class MainWindow : Window
    {
        static NVOptimusEnabler nvOptimusEnabler = new NVOptimusEnabler();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}