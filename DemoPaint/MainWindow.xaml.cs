using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shapes;

namespace DemoPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetWindowSizeToScreenSize();
            ChosenColor = Brushes.Black;
            _strokeThickness = 1;

            DataContext = this;
        }

        bool _isDrawing = false;
        Point _start;
        Point _end;
        int _strokeThickness;

        List<UIElement> _list = new List<UIElement>();
        List<IShape> _painters = new List<IShape>();
        UIElement _lastElement;
        List<IShape> _prototypes = new List<IShape>();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Single configuration
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            var fis = new DirectoryInfo(folder).GetFiles("*.dll");

            foreach(var fi in fis)
            {
                // Lấy tất cả kiểu dữ liệu trong dll
                var assembly = Assembly.LoadFrom(fi.FullName);
                var types = assembly.GetTypes();

                foreach(var type in types)
                {
                    if ((type.IsClass) 
                        && (typeof(IShape).IsAssignableFrom(type))) {
                        _prototypes.Add((IShape) Activator.CreateInstance(type)!);
                    }
                }
            }
            // ---------------------------------------------------

            // Tự tạo ra giao diện
            int count = 0;
            foreach (var item in _prototypes)
            {
                var control = new Button();

                switch (item.Name)
                {
                    case "Line":
                        control.Style = FindResource("ShapeBtnLine") as Style;
                        break;

                    case "Rectangle":
                        control.Style = FindResource("ShapeBtnRectangle") as Style;
                        break;

                    case "Ellipse":
                        control.Style = FindResource("ShapeBtnEllipse") as Style;
                        break;
                }

                control.Tag = item;
                control.Click += Control_Click;

                if (count < 4)
                {
                    ShapeButtons1.Children.Add(control);
                }
                else
                {
                    ShapeButtons2.Children.Add(control);
                }

                count++;
            }

            _painter = _prototypes[0];
        }
        private void Control_Click(object sender, RoutedEventArgs e)  {
            IShape item = (IShape)(sender as Button)!.Tag;
            _painter = item; 
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;
            _start = e.GetPosition(myCanvas);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _end = e.GetPosition(myCanvas);
                myCanvas.Children.Clear(); 
                foreach(var item in _painters)
                {
                    myCanvas.Children.Add(item.Convert());
                }
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    _end = new Point(_start.X + (_end.Y - _start.Y), _start.Y + (_end.Y - _start.Y));
                }
                _painter.AddFirst(_start);
                _painter.AddSecond(_end);
                _painter.AddColor(ChosenColor);
                _painter.AddStrokeThickness(_strokeThickness);
                myCanvas.Children.Add(_painter.Convert());
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            _painters.Add((IShape)_painter.Clone());
        }

        IShape _painter = null;


        private void SetWindowSizeToScreenSize()
        {
            // Set Window size according to actual Screen size
            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;
        }

        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void pnlControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else this.WindowState = WindowState.Normal;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null && menuItem.ContextMenu != null)
            {
                menuItem.ContextMenu.IsOpen = true;
            }
        }

        private void Text_Formatting_Click(object sender, RoutedEventArgs e)
        {
            textFormattingPopup.IsOpen = !textFormattingPopup.IsOpen;
        }

        public static readonly DependencyProperty ChosenColorProperty =
        DependencyProperty.Register("ChosenColor", typeof(Brush), typeof(MainWindow), new PropertyMetadata(null));

        public SolidColorBrush ChosenColor
        {
            get { return (SolidColorBrush)GetValue(ChosenColorProperty); }
            set { SetValue(ChosenColorProperty, value); }
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                SolidColorBrush selectedColor = button.Tag as SolidColorBrush;
                if (selectedColor != null)
                {
                    ChosenColor = selectedColor;
                }
            }
        }

        private void StrokeThicknessCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            if (selectedItem != null)
            {
                string content = selectedItem.Content.ToString();
                if (int.TryParse(content.Replace("px", ""), out int thickness))
                {
                    _strokeThickness = thickness;
                }
            }
        }
    }
}