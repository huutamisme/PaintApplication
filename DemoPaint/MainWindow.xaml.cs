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
using Newtonsoft.Json;
using System.Text;
using System.Windows.Shapes;

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
            BackgroundColor = Brushes.Black;
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

            _paintersLayer.Add(new Stack<object>());
            selectedPainter = _paintersLayer[0];

            DataContext = this;
        }

        #region Khai báo biến

        public ObservableCollection<String> Layers { get; set; } = new ObservableCollection<String>();
        public ObservableCollection<Stack<object>> _paintersLayer { get; set; } = new ObservableCollection<Stack<object>>();

        Canvas selectedLayer;
        Stack<object> selectedPainter = new Stack<object>(); // mảng chứa các hình đã vẽ trong 1 Layer
        bool _isDrawing = false;
        bool _isTextFormatChosen = false;
        bool _isTexting = false;
        bool _isChosenColorClicked = true;
        bool _isBackgroundColorClicked = false;
        bool _isBackgrounFillCheckBox = false;
        Point _start;
        Point _end;
        int _strokeThickness;
        double[] _strokeType;
        String textFontStyle;
        int textFontSize;

        List<UIElement> _list = new List<UIElement>();
        Stack<object> _undoStack = new Stack<object>();
        Stack<object> _redoStack = new Stack<object>();
        UIElement _lastElement;
        List<IShape> _prototypes = new List<IShape>();  //chứa tất cả các shape load từ file dll
        List<IShape> _allPainter = new List<IShape>(); // chứa tất cả những hình đã vẽ
        IShape _painter = null;
        private string json;


        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        public static readonly DependencyProperty ChosenColorProperty =
        DependencyProperty.Register("ChosenColor", typeof(Brush), typeof(MainWindow), new PropertyMetadata(null));

        public SolidColorBrush ChosenColor
        {
            get { return (SolidColorBrush)GetValue(ChosenColorProperty); }
            set { SetValue(ChosenColorProperty, value); }
        }

        public static readonly DependencyProperty BackgroundColorProperty =
        DependencyProperty.Register("BackgroundColor", typeof(Brush), typeof(MainWindow), new PropertyMetadata(null));

        public SolidColorBrush BackgroundColor
        {
            get { return (SolidColorBrush)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
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
            if(_isTextFormatChosen)
            {
                _isTexting = false;
                _isTextFormatChosen = false;
                textFormattingPopup.IsOpen = !textFormattingPopup.IsOpen;
            }    
            IShape item = (IShape)(sender as Button)!.Tag;
            _painter = item;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isTextFormatChosen)
            {
                _isTexting = true;
                _isDrawing = false;
                _painter = _prototypes[6];
                
            }
            else
            {
                _isTexting = false;
                _isDrawing = true;
            }
            _start = e.GetPosition(selectedLayer);
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _end = e.GetPosition(selectedLayer);
                selectedLayer.Children.Clear();
                RedrawCanvas();
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
                
                UndoBtn.IsEnabled = true;
            }
            if (_isTexting)
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.IBeam;
                _end = e.GetPosition(selectedLayer);
                selectedLayer.Children.Clear();
                RedrawCanvas();
                _painter.AddFirst(_start);
                _painter.AddSecond(_end);
                _painter.AddColor(Brushes.Black);
                _painter.AddStrokeThickness(1);
                _painter.AddStrokeDashArray(new double[] {4,4});
                selectedLayer.Children.Add(_painter.Convert());
                
            }
            else
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(_isDrawing)
            {
                _isDrawing = false;
                selectedPainter.Push((IShape)_painter.Clone());
                _allPainter.Add((IShape)_painter.Clone());
                _undoStack.Push((IShape)_painter.Clone());
                _redoStack.Clear();
            }    
            if (_isTexting)
            {
                _isTexting = false;
                double width = Math.Abs(_end.X - _start.X);
                double height = Math.Abs(_end.Y - _start.Y);

                double padding1 = height * 0.05;
                double padding2 = width * 0.05;

                double padding = Math.Min(padding1, padding2);

                SolidColorBrush bgColor = Brushes.Transparent;
                if(_isBackgrounFillCheckBox)
                {
                    bgColor = BackgroundColor;
                }    

                TextBox textBox = new TextBox
                {
                    Width = width - padding * 2,
                    Height = height - padding * 2,
                    Background = bgColor,
                    BorderThickness = new Thickness(0),
                    Foreground = ChosenColor,
                    FontSize = textFontSize,
                    FontFamily = new FontFamily(textFontStyle),
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(Math.Min(_start.X, _end.X) + padding, Math.Min(_start.Y, _end.Y) + padding, 0, 0),
                };

                textBox.LostFocus += TextBox_LostFocus;

                selectedLayer.Children.Add(textBox);

                textBox.Focus();
            }
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
            _isTextFormatChosen = !_isTextFormatChosen;
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
                    if(_isBackgroundColorClicked)
                    {
                        BackgroundColor = selectedColor;
                    }    
                    else
                    {
                        ChosenColor = selectedColor;
                    }
                }
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            Load();
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_undoStack.Count > 0)
            {
                var lastAction = _undoStack.Pop();
                if (lastAction is IShape || lastAction is TextBlock)
                {
                    _redoStack.Push(lastAction);
                    selectedPainter.Pop();
                    RedrawCanvas();
                    UpdateUndoRedoButtonState();
                }
                //else if (lastAction is string) // Layer
                //{
                //    Layers.Remove(lastAction as string);
                //    _paintersLayer.RemoveAt(_paintersLayer.Count - 1);
                //    DrawArea.Children.RemoveAt(DrawArea.Children.Count - 1);
                //    selectedLayer = FindCanvasByName(Layers.LastOrDefault());
                //    selectedPainter = _paintersLayer.LastOrDefault();
                //}
            }
        }

        private void RedoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_redoStack.Count > 0)
            {
                var redoAction = _redoStack.Pop();
                if (redoAction is IShape || redoAction is TextBlock)
                {
                    _undoStack.Push(redoAction);
                    selectedPainter.Push(redoAction);
                    RedrawCanvas();
                    UpdateUndoRedoButtonState();
                }
                //else if (redoAction is string) // Layer
                //{
                //    string canvasName = redoAction as string;
                //    Canvas canvasToAdd = new Canvas();
                //    canvasToAdd.Name = canvasName;
                //    Canvas.SetZIndex(canvasToAdd, Layers.Count() - 1);
                //    canvasToAdd.Background = Brushes.Transparent;

                //    Layers.Add(canvasName);
                //    _paintersLayer.Add(new Stack<object>());
                //    DrawArea.Children.Add(canvasToAdd);
                //    Canvas.SetZIndex(tempCanvas, Layers.Count() + 1);

                //    selectedLayer = FindCanvasByName(canvasName);
                //    selectedPainter = _paintersLayer.LastOrDefault();
                //}
            }
        }

        private void UpdateUndoRedoButtonState()
        {
            UndoBtn.IsEnabled = _undoStack.Count > 0;
            RedoBtn.IsEnabled = _redoStack.Count > 0;
        }

        private void AddLayerBtn_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(Layers.Count);
            string canvasName = "myLayer" + Layers.Count().ToString();

            Canvas canvasToAdd = new Canvas();
            canvasToAdd.Name = canvasName;
            Canvas.SetZIndex(canvasToAdd, Layers.Count() - 1);
            canvasToAdd.Background = Brushes.Transparent;

            Layers.Insert(0, canvasName);
            _paintersLayer.Insert(0, new Stack<object>());
            DrawArea.Children.Add(canvasToAdd);
            Canvas.SetZIndex(tempCanvas, Layers.Count() + 1);

            selectedLayer = FindCanvasByName(Layers[0]);
            selectedPainter = _paintersLayer[0];
            LayerListBox.SelectedItem = canvasName;

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

        private void NewBtn_Click(object sender, RoutedEventArgs e)
        {
            Restart();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Keyboard.IsKeyDown(Key.N))
            {
                Restart();
            }
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Keyboard.IsKeyDown(Key.S))
            {
                Save();
            }
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Keyboard.IsKeyDown(Key.L))
            {
                Load();
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

        private void FontStyleCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)FontStyleCombobox.SelectedItem;
            if (selectedItem != null)
            {
                textFontStyle = selectedItem.Content.ToString();
            }
        }

        private void FontSizeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)FontSizeCombobox.SelectedItem;
            if (selectedItem != null)
            {
                if (int.TryParse(selectedItem.Content.ToString(), out int size))
                {
                    textFontSize = size;
                }
            }
        }

        private void RedrawCanvas()
        {
            selectedLayer.Children.Clear();
            foreach (var item in selectedPainter.Reverse())
            {
                if (item is IShape shape)
                {
                    selectedLayer.Children.Add(shape.Convert());
                }
                else if (item is TextBlock textBlock)
                {
                    selectedLayer.Children.Add(textBlock);
                }
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

        private void Restart()
        {
            Layers.Clear();
            _paintersLayer.Clear();
            selectedLayer = null;
            selectedPainter = null;
            _isDrawing = false;
            _start = new Point();
            _end = new Point();
            _strokeThickness = 1;
            _strokeType = new double[] { 1, 0 };
            _list.Clear();
            _undoStack.Clear();
            _redoStack.Clear();
            _lastElement = null;
            _allPainter.Clear();
            _painter = _prototypes[0];
            ChosenColor = Brushes.Black;
            DrawArea.Children.Clear();

            string canvasName = "myCanvas";
            Canvas canvasToAdd = new Canvas();
            canvasToAdd.Name = canvasName;
            canvasToAdd.Background = Brushes.White;
            Canvas.SetZIndex(canvasToAdd, 0);
            Layers.Add(canvasName);
            DrawArea.Children.Add(canvasToAdd);
            selectedLayer = FindCanvasByName("myCanvas");

            canvasName = "tempCanvas";
            canvasToAdd = new Canvas();
            canvasToAdd.Name = canvasName;
            canvasToAdd.Background = Brushes.Transparent;
            Canvas.SetZIndex(canvasToAdd, 1);
            canvasToAdd.MouseLeftButtonDown += (sender, e) => Canvas_MouseLeftButtonDown(sender, e);
            canvasToAdd.MouseMove += (sender, e) => Canvas_MouseMove(sender, e);
            canvasToAdd.MouseLeftButtonUp += (sender, e) => Canvas_MouseLeftButtonUp(sender, e);
            DrawArea.Children.Add(canvasToAdd);

            _paintersLayer.Add(new Stack<object>());
            selectedPainter = _paintersLayer[0];
        }

        private void Save()
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            };

            var serializeShapeList = JsonConvert.SerializeObject(_allPainter, settings);

            StringBuilder builder = new StringBuilder();
            builder.Append(serializeShapeList).Append("\n");
            string content = builder.ToString();

            var saveDialog = new System.Windows.Forms.SaveFileDialog();

            saveDialog.Filter = "JSON (*.json)|*.json";

            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = saveDialog.FileName;
                File.WriteAllText(path, content);
            }
        }

        private void Load()
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "JSON (*.json)|*.json";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = dialog.FileName;

                try
                {
                    string content = File.ReadAllText(path);

                    if (content.Length == 0)
                    {
                        return;
                    }

                    var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects };
                    _allPainter.Clear();
                    foreach (var layer in Layers)
                    {
                        Canvas newCanvas = FindCanvasByName(layer);
                        newCanvas.Children.Clear();
                    }

                    List<IShape> containers = JsonConvert.DeserializeObject<List<IShape>>(content, settings);
                    foreach (var item in containers)
                    {
                        _allPainter.Add(item);
                    }

                    foreach (var shape in _allPainter)
                    {
                        var element = shape.Convert();
                        selectedLayer.Children.Add(element);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading file: " + ex.Message);
                }
            }
        }
        #endregion

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                TextBlock textBlock = new TextBlock
                {
                    Text = textBox.Text,
                    Width = textBox.ActualWidth,
                    Height = textBox.ActualHeight,
                    Background = textBox.Background,
                    Foreground = textBox.Foreground,
                    FontSize = textBox.FontSize,
                    FontFamily = textBox.FontFamily,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(textBox.Margin.Left, textBox.Margin.Top, 0, 0)
                };

                selectedLayer.Children.Remove(textBox);
                selectedLayer.Children.Add(textBlock);
                selectedPainter.Push(textBlock);

                selectedLayer.Children.Clear();
                RedrawCanvas();

                _allPainter.Add((IShape)_painter.Clone());

                _undoStack.Push(textBlock);
                _redoStack.Clear();

            }
        }

        private void ChosenColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _isChosenColorClicked = true;
            _isBackgroundColorClicked = false;
        }

        private void BackgroundColorBtn_Click(object sender, RoutedEventArgs e)
        {
            _isChosenColorClicked = false;
            _isBackgroundColorClicked = true;
        }

        private void BackgrounFillCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _isBackgrounFillCheckBox = true;
        }

        private void BackgrounFillCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _isBackgrounFillCheckBox = false;
        }
    }
}
