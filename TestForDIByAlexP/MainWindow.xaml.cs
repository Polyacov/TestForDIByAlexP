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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using HelixToolkit;
using HelixToolkit.Wpf;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Assimp;
using Microsoft.Win32;

namespace TestForDIByAlexP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isLoading;

        public MainWindow()
        {
            InitializeComponent();

            TaskPanelView.Content = ((ViewModel)DataContext).TasksNames[0].PanelView;
        }

        private void XButton_Click(object sender, RoutedEventArgs e)
        {
            TaskPanelView.Content = ((XButton)sender).ViewToShow;
        }
    }

    public class XButton : Button
    {
        public UserControl ViewToShow
        {
            get { return (UserControl)GetValue(ViewToShowProperty); }
            set { SetValue(ViewToShowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewToShow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewToShowProperty =
            DependencyProperty.Register("ViewToShow", typeof(UserControl), typeof(XButton));
    }
}
