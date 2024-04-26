using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Collections.ObjectModel;

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
            _strokeType = new double[] { 1, 0 };

            string canvasName = "myCanvas";
            Canvas canvasToAdd = new Canvas();
            canvasToAdd.Name = canvasName;
            canvasToAdd.Background = Brushes.White;
            Canvas.SetZIndex(canvasToAdd, 0);
            Layers.Add(canvasName);
            DrawArea.Children.Add(canvasToAdd);
            selectedLayer = FindCanvasByName("myCanvas");
            Canvas.SetZIndex(tempCanvas, 1);

            _paintersLayer.Add(new Stack<IShape>());
            selectedPainter = _paintersLayer[0];

            DataContext = this;
        }

        #region Khai báo biến

        public ObservableCollection<String> Layers { get; set; } = new ObservableCollection<String>();
        public ObservableCollection<Stack<IShape>> _paintersLayer { get; set; } = new ObservableCollection<Stack<IShape>>();

        Canvas selectedLayer;
        Stack<IShape> selectedPainter = new Stack<IShape>();
        bool _isDrawing = false;
        Point _start;
        Point _end;
        int _strokeThickness;
        double[] _strokeType;

        List<UIElement> _list = new List<UIElement>();
        Stack<IShape> _undoStack = new Stack<IShape>();
        Stack<IShape> _redoStack = new Stack<IShape>();
        UIElement _lastElement;
        List<IShape> _prototypes = new List<IShape>();
        IShape _painter = null;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        public static readonly DependencyProperty ChosenColorProperty =
        DependencyProperty.Register("ChosenColor", typeof(Brush), typeof(MainWindow), new PropertyMetadata(null));

        public SolidColorBrush ChosenColor
        {
            get { return (SolidColorBrush)GetValue(ChosenColorProperty); }
            set { SetValue(ChosenColorProperty, value); }
        }

        #endregion

        #region Bắt sự kiện, Khởi tạo
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Single configuration
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            var fis = new DirectoryInfo(folder).GetFiles("*.dll");

            foreach (var fi in fis)
            {
                // Lấy tất cả kiểu dữ liệu trong dll
                var assembly = System.Reflection.Assembly.LoadFrom(fi.FullName);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if ((type.IsClass)
                        && (typeof(IShape).IsAssignableFrom(type)))
                    {
                        _prototypes.Add((IShape)Activator.CreateInstance(type)!);
                    }
                }
            }
            // ---------------------------------------------------

            // Tự tạo ra giao diện
            foreach (var item in _prototypes)
            {
                var control = new Button();

                switch (item.Name)
                {
                    case "Line":
                        control.Style = FindResource("ShapeBtnLine") as Style;
                        ShapeButtons1.Children.Add(control);
                        break;

                    case "Rectangle":
                        control.Style = FindResource("ShapeBtnRectangle") as Style;
                        ShapeButtons1.Children.Add(control);
                        break;

                    case "Ellipse":
                        control.Style = FindResource("ShapeBtnEllipse") as Style;
                        ShapeButtons1.Children.Add(control);

                        break;
                    case "Star":
                        control.Style = FindResource("ShapeBtnStar") as Style;
                        ShapeButtons1.Children.Add(control);
                        break;
                    case "ArrowUp":
                        control.Style = FindResource("ShapeBtnArrowUp") as Style;
                        ShapeButtons2.Children.Add(control);
                        break;
                    case "ArrowDown":
                        control.Style = FindResource("ShapeBtnArrowDown") as Style;
                        ShapeButtons2.Children.Add(control);
                        break;
                    case "ArrowLeft":
                        control.Style = FindResource("ShapeBtnArrowLeft") as Style;
                        ShapeButtons2.Children.Add(control);
                        break;
                    case "ArrowRight":
                        control.Style = FindResource("ShapeBtnArrowRight") as Style;
                        ShapeButtons2.Children.Add(control);
                        break;
                }

                control.Tag = item;
                control.Click += Control_Click;
            }

            _painter = _prototypes[0];
        }

        private void SetWindowSizeToScreenSize()
        {
            // Set Window size according to actual Screen size
            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;
        }

        private void Control_Click(object sender, RoutedEventArgs e)
        {
            IShape item = (IShape)(sender as Button)!.Tag;
            _painter = item;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;
            _start = e.GetPosition(selectedLayer);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _end = e.GetPosition(selectedLayer);
                selectedLayer.Children.Clear();
                foreach (var item in selectedPainter)
                {
                    selectedLayer.Children.Add(item.Convert());
                }
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    _end = new Point(_start.X + (_end.Y - _start.Y), _start.Y + (_end.Y - _start.Y));
                }
                _painter.AddFirst(_start);
                _painter.AddSecond(_end);
                _painter.AddColor(ChosenColor);
                _painter.AddStrokeThickness(_strokeThickness);
                _painter.AddStrokeDashArray(_strokeType);
                selectedLayer.Children.Add(_painter.Convert());
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            selectedPainter.Push((IShape)_painter.Clone());
            _undoStack.Push((IShape)_painter.Clone());
            _redoStack.Clear();
        }

        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

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

        private void LayerBtn_Click(object sender, RoutedEventArgs e)
        {
            LayersPopup.IsOpen = !LayersPopup.IsOpen;
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

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Files (*.png)|*.png";
            if (saveFileDialog.ShowDialog() == true)
            {
                SaveCanvasToPng(saveFileDialog.FileName);
            }
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_undoStack.Count > 0)
            {
                IShape lastAction = _undoStack.Pop();
                _redoStack.Push(lastAction);
                selectedPainter.Pop();
                RedrawCanvas();
            }
        }

        private void RedoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_redoStack.Count > 0)
            {
                var redoAction = _redoStack.Pop();
                _undoStack.Push(redoAction);
                selectedPainter.Push(redoAction);
                RedrawCanvas();
            }
        }

        private void AddLayerBtn_Click(object sender, RoutedEventArgs e)
        {
            string canvasName = "myLayer" + Layers.Count().ToString();

            Canvas canvasToAdd = new Canvas();
            canvasToAdd.Name = canvasName;
            Canvas.SetZIndex(canvasToAdd, Layers.Count());
            canvasToAdd.Background = Brushes.Transparent;

            Layers.Insert(0, canvasName);
            _paintersLayer.Insert(0, new Stack<IShape>());
            DrawArea.Children.Add(canvasToAdd);
            Canvas.SetZIndex(tempCanvas, Layers.Count() + 1);
        }

        private void Layer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                int selectedIndex = (sender as ListBox).SelectedIndex;
                if (selectedIndex >= 0 && selectedIndex < Layers.Count)
                {
                    selectedLayer = FindCanvasByName(Layers[selectedIndex]);
                    selectedPainter = _paintersLayer[selectedIndex];
                }
            }
        }

        private void LayerVisibleBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender as Button;
            if (btn != null && btn.Tag.ToString().Equals("Eye"))
            {
                btn.Tag = "EyeOff";
                selectedLayer.Visibility = Visibility.Hidden;
            }
            else
            {
                btn.Tag = "Eye";
                selectedLayer.Visibility = Visibility.Visible;

            }
        }

        #endregion

        #region Hàm chức năng

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

        private void StrokeTypeCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            string selectedStrokeType = (string)selectedItem.Content;

            switch (selectedStrokeType)
            {
                case "Solid":
                    _strokeType = new double[] { 1, 0 };
                    break;
                case "Dash":
                    _strokeType = new double[] { 5, 3 };
                    break;
                case "Dot":
                    _strokeType = new double[] { 1, 1 };
                    break;
                case "DashDotDot":
                    _strokeType = new double[] { 5, 3, 1, 1, 1, 1 };
                    break;
                default:
                    break;
            }
        }

        private void SaveCanvasToPng(string filename)
        {

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)selectedLayer.ActualWidth, (int)selectedLayer.ActualHeight,
                96d, 96d, PixelFormats.Pbgra32);


            renderBitmap.Render(selectedLayer);


            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));


            using (FileStream fileStream = new FileStream(filename, FileMode.Create))
            {
                pngEncoder.Save(fileStream);
            }
        }

        private void RedrawCanvas()
        {
            selectedLayer.Children.Clear();
            foreach (var item in selectedPainter)
            {
                selectedLayer.Children.Add(item.Convert());
            }
        }

        private Canvas FindCanvasByName(string canvasName)
        {
            foreach (var child in DrawArea.Children)
            {
                if (child is Canvas canvas && canvas.Name == canvasName)
                {
                    return canvas;
                }
            }
            return null;
        }

        #endregion

    }
}
