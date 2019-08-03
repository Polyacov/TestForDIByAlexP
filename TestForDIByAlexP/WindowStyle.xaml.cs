using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace TestForDIByAlexP
{
    public partial class WindowStyle : ResourceDictionary
    {
        bool _isResizing = false;

        public WindowStyle()
        {
            InitializeComponent();
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.Close();
        }

        private void MaximizeRestoreClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            if (window.WindowState == System.Windows.WindowState.Normal)
            {
                window.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                window.WindowState = System.Windows.WindowState.Normal;
            }
        }

        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = System.Windows.WindowState.Minimized;
        }

        private void GripResize_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!Mouse.Capture((ResizeGrip)sender))
                return;

            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            _isResizing = true;
        }

        private void GripResize_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!_isResizing)
                return;

            _isResizing = false;
            Mouse.Capture(null);
        }

        private void GripResize_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isResizing)
                return;

            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            Point currentPosition = Mouse.GetPosition(window);
            var currH  = currentPosition.Y;
            var currW = currentPosition.X;

            if (window.MinHeight <= currH && currH <= window.MaxHeight)
            {
                window.Height = currH;
            }

            if (window.MinWidth <= currW && currW <= window.MaxWidth)
            {
                window.Width = currW;
            }
        }
    }
}
