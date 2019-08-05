using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using TestForDIByAlexP.ViewModel;

namespace TestForDIByAlexP.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        UserControl _exercize3View = new Task3PanelView();
        UserControl _exercize4View = new Task4PanelView();
        UserControl _exercize5View = new Task5PanelView();

        public string CurrentExercise
        {
            get { return (string)GetValue(CurrentExerciseProperty); }
            set { SetValue(CurrentExerciseProperty, value); }
        }


        // Using a DependencyProperty as the backing store for CurrentExercize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentExerciseProperty =
            DependencyProperty.Register(nameof(CurrentExercise), typeof(string), typeof(MainWindow),
                new PropertyMetadata("", new PropertyChangedCallback(CurrentExerciseChanged)));

        private static void CurrentExerciseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MainWindow)d).SetExercizeView(e.NewValue.ToString());
        }

        public MainWindow()
        {
            InitializeComponent();

            var vm = new MainViewModel();
            DataContext = vm;

            Binding binding = new Binding();
            binding.Path = new PropertyPath(nameof(vm.CurrentExercise));
            var bq = SetBinding(CurrentExerciseProperty, binding);
        }

        private void VM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var vm = (MainViewModel)sender;
            if (e.PropertyName != nameof(vm.CurrentExercise))
                return;
            
            SetExercizeView(vm.CurrentExercise);
        }

        public void SetExercizeView(string value = "")
        {
            switch (value)
            {
                case "Task 4":
                    TaskPanelView.Content = _exercize4View;
                    return;
                case "Task 5":
                    TaskPanelView.Content = _exercize5View;
                    return;
                case "Task 3":
                default:
                    TaskPanelView.Content = _exercize3View;
                    return;
            }
        }
    }

    class ActiveKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Brushes.Beige;
            else
                return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }

    class BoolToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (Visibility)value;

            switch (val)
            {
                case Visibility.Visible:
                    return true;
                case Visibility.Hidden:
                case Visibility.Collapsed:
                default:
                    return false;
            }
        }
    }
}
