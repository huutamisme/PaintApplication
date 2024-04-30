using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Text;
using System.Windows.Shapes;
using Main;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Microsoft.VisualBasic.Logging;
using System.Runtime.Intrinsics.X86;
using System.ComponentModel;

namespace DemoPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public MainWindow()
        {
            InitializeComponent();
            SetWindowSizeToScreenSize();
            ChosenColor = Brushes.Black;
            BackgroundColor = Brushes.Black;
            _strokeThickness = 1;
            _strokeType = new DoubleCollection() { 1, 0 };

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

            _undoStacksLayer.Add(new Stack<object>());
            _undoStack = _undoStacksLayer[0];

            _redoStacksLayer.Add(new Stack<object>());
            _redoStack = _redoStacksLayer[0];

            _isDrawing = false;
            IsTextFormatChosen = false;
            IsLayerBtnClicked = false;
            IsEdit = false;
            IsSelectArea = false;
            _isTexting = false;
            _isChosenColorClicked = true;
            _isBackgroundColorClicked = false;
            _isBackgrounFillCheckBox = false;
            _isLayerBtnClicked = false;

            DataContext = this;
        }

        #region Khai báo biến

        public ObservableCollection<String> Layers { get; set; } = new ObservableCollection<String>();
        public ObservableCollection<Stack<object>> _paintersLayer { get; set; } = new ObservableCollection<Stack<object>>();
        public ObservableCollection<Stack<object>> _undoStacksLayer { get; set; } = new ObservableCollection<Stack<object>>();
        public ObservableCollection<Stack<object>> _redoStacksLayer { get; set; } = new ObservableCollection<Stack<object>>();

        Canvas selectedLayer;
        Stack<object> selectedPainter = new Stack<object>(); // mảng chứa các hình đã vẽ trong 1 Layer
        Stack<object> _undoStack = new Stack<object>(); // mảng undo trong 1 Layer
        Stack<object> _redoStack = new Stack<object>(); // mảng redo trong 1 Layer
        bool _isDrawing;
        bool _isTexting;
        bool _isChosenColorClicked;
        bool _isBackgroundColorClicked;
        bool _isBackgrounFillCheckBox;
        Point _start;
        Point _end;
        int _strokeThickness;
        DoubleCollection _strokeType;
        String textFontStyle;
        int textFontSize;

        #region biến binding

        private bool _isLayerBtnClicked;

        public bool IsLayerBtnClicked
        {
            get { return _isLayerBtnClicked; }
            set
            {
                _isLayerBtnClicked = value;
                OnPropertyChanged(nameof(IsLayerBtnClicked));
            }
        }


        private bool _isTextFormatChosen;

        public bool IsTextFormatChosen
        {
            get { return _isTextFormatChosen; }
            set
            {
                _isTextFormatChosen = value;
                OnPropertyChanged(nameof(IsTextFormatChosen));
            }
        }

        private bool _isEdit;

        public bool IsEdit
        {
            get { return _isEdit; }
            set
            {
                _isEdit = value;
                OnPropertyChanged(nameof(IsEdit));
            }
        }

        private bool _isSelectArea;
        public bool IsSelectArea
        {
            get { return _isSelectArea; }
            set
            {
                _isSelectArea = value;
                OnPropertyChanged(nameof(IsSelectArea));
            }
        }    

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        // memory
        private IShape copiedShape = null;
        private IShape cutShape = null;
        private List<IShape> _chosedShapes = new List<IShape>();

        List<UIElement> _list = new List<UIElement>();
        UIElement _lastElement;
        List<IShape> _prototypes = new List<IShape>();  //chứa tất cả các shape load từ file dll
        List<IShape> _allPainter = new List<IShape>(); // chứa tất cả những hình đã vẽ
        IShape _painter = null;

        // for editing
        private double editPreviousX = -1;
        private double editPreviousY = -1;
        private List<ControlPoint> _controlPoints = new List<ControlPoint>();


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
            if(IsLayerBtnClicked)
            {
                _isTexting = false;
                IsLayerBtnClicked = false;
                textFormattingPopup.IsOpen = !textFormattingPopup.IsOpen;
            }    
            IShape item = (IShape)(sender as Button)!.Tag;
            _painter = item;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsLayerBtnClicked)
            {
                _isTexting = true;
                _isDrawing = false;
                foreach (var shape in _prototypes)
                {
                    if(shape.Name.Equals("Rectangle"))
                    {
                        _painter = shape;
                    }    
                }
                _start = e.GetPosition(selectedLayer);
            }
            else
            {
                _isTexting = false;
                _isDrawing = true;
                _start = e.GetPosition(selectedLayer);
                _isDrawing = true;
               // Point pos = e.GetPosition(selectedLayer);
                //_painter.HandleStart(_start.X, _start.Y);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            bool isChange = false;
            if (_chosedShapes.Count == 1)
            {
                CShape Cshape = (CShape)_chosedShapes[0];
                Point currentPosition = e.GetPosition(selectedLayer);
                for (int i = 0; i < _controlPoints.Count; i++)
                {
                    if (_controlPoints[i].isHovering(Cshape.getRotateAngle(), currentPosition.X, currentPosition.Y))
                    {
                        // xác định hướng mà con trỏ chuột sẽ mở rộng từ đỉnh đó để thể hiện hướng
                        switch (_controlPoints[i].getEdge(Cshape.getRotateAngle()))
                        {
                            case "topleft" or "bottomright":
                                {
                                    Mouse.OverrideCursor = System.Windows.Input.Cursors.SizeNWSE;
                                    break;
                                }
                            case "topright" or "bottomleft":
                                {
                                    Mouse.OverrideCursor = System.Windows.Input.Cursors.SizeNESW;
                                    break;
                                }
                            case "top" or "bottom":
                                {
                                    Mouse.OverrideCursor = System.Windows.Input.Cursors.SizeNS;
                                    break;
                                }
                            case "left" or "right":
                                {
                                    Mouse.OverrideCursor = System.Windows.Input.Cursors.SizeWE;
                                    break;
                                }
                        }
                        if (_controlPoints[i].type == "move" || _controlPoints[i].type == "rotate")
                        {
                            Mouse.OverrideCursor = System.Windows.Input.Cursors.Hand;
                        }

                        isChange = true;
                        break;
                    }
                };

                if (!isChange)
                {
                    Mouse.OverrideCursor = null;
                }
            }
            if (this.IsEdit)
            {
                if (_chosedShapes.Count < 1)
                    return;

                if (Mouse.LeftButton != MouseButtonState.Pressed)
                    return;

                Point curPos = e.GetPosition(selectedLayer);
                double Dx, Dy;

                if (editPreviousX == -1 || editPreviousY == -1)
                {
                    editPreviousX = curPos.X;
                    editPreviousY = curPos.Y;
                    return;
                }

                // distance of x and y of new coordinate compared to the old one
                Dx = curPos.X - editPreviousX;
                Dy = curPos.Y - editPreviousY;

                // for multiple chosed, move each of the shape to the relative position
                if (_chosedShapes.Count > 1)
                {
                    _chosedShapes.ForEach(E =>
                    {
                        CShape C = (CShape)E;

                        C.LeftTop.X += Dx;
                        C.LeftTop.Y += Dy;
                        C.RightBottom.X += Dx;
                        C.RightBottom.Y += Dy;
                    });
                }
                else
                {
                    CShape shape = (CShape)_chosedShapes[0];
                    _controlPoints.ForEach(ctrlPoint =>
                    {
                        List<cord> edges = new List<cord>()
                        {
                        new cord(shape.LeftTop),      // 0 xt
                        new cordY(shape.LeftTop),      // 1 yt
                        new cord(shape.RightBottom),  // 2 xb
                        new cordY(shape.RightBottom)   // 3 yb
						};

                        List<int> rotate0 = new List<int>
                        {
                        0, 1, 2, 3
                        };
                        List<int> rotate90 = new List<int>
                        {
                        //xt, yt, xb, xb
                        3, 0, 1, 2
                        };
                        List<int> rotate180 = new List<int>
                        {
                        //xt, yt, xb, xb
                        2, 3, 0, 1
                        };
                        List<int> rotate270 = new List<int>
                        {
                        //xt, yt, xb, xb
                        1, 2, 3, 0
                        };

                        List<List<int>> rotateList = new List<List<int>>()
                        {
                        rotate0,
                        rotate90,
                        rotate180,
                        rotate270
                        };

                        double rot = shape.getRotateAngle();
                        int index = 0;

                        if (rot > 0)
                            while (true)
                            {
                                rot -= 90;
                                if (rot < 0)
                                    break;
                                index++;

                                if (index == 4)
                                    index = 0;
                            }
                        else
                            while (true)
                            {
                                rot += 90;
                                if (rot > 0)
                                    break;
                                index--;
                                if (index == -1)
                                    index = 3;
                            };

                        if (ctrlPoint.isHovering(shape.getRotateAngle(), curPos.X, curPos.Y))
                        {
                            switch (ctrlPoint.type)
                            {
                                case "rotate":
                                    {
                                        Trace.WriteLine("Rotating");
                                        const double RotateFactor = 180.0 / 270;
                                        double alpha = Math.Abs(Dx + Dy);

                                        Point2D v = shape.getCenterPoint();

                                        double xv = editPreviousX - v.X;
                                        double yv = editPreviousY - v.Y;

                                        double angle = Math.Atan2(Dx * yv - Dy * xv, Dx * xv + Dy * yv);

                                        if (angle > 0)
                                        {
                                            Trace.WriteLine("The shape" + shape.getRotateAngle());
                                            shape.setRotateAngle(shape.getRotateAngle() - alpha * RotateFactor);
                                        }
                                        else
                                        {
                                            Trace.WriteLine("The shape" + shape.getRotateAngle());
                                            shape.setRotateAngle(shape.getRotateAngle() + alpha * RotateFactor);
                                        }
                                        break;
                                    }

                                case "move":
                                    {
                                        shape.LeftTop.X = shape.LeftTop.X + Dx;
                                        shape.LeftTop.Y = shape.LeftTop.Y + Dy;
                                        shape.RightBottom.X = shape.RightBottom.X + Dx;
                                        shape.RightBottom.Y = shape.RightBottom.Y + Dy;
                                        break;
                                    }

                                case "diag":
                                    {
                                        Point2D handledXY = ctrlPoint.handle(shape.getRotateAngle(), Dx, Dy);

                                        switch (index)
                                        {
                                            case 1:
                                                handledXY.X *= -1;
                                                break;
                                            case 2:
                                                {
                                                    handledXY.Y *= -1;
                                                    handledXY.X *= -1;
                                                    break;
                                                }
                                            case 3:
                                                {
                                                    handledXY.Y *= -1;
                                                    break;
                                                }
                                        }


                                        switch (ctrlPoint.getEdge(shape.getRotateAngle()))
                                        {
                                            case "topleft":
                                                {
                                                    edges[rotateList[index][0]].setCord(handledXY.X);
                                                    edges[rotateList[index][1]].setCord(handledXY.Y);
                                                    edges[rotateList[index][2]].setCord(-handledXY.X);
                                                    edges[rotateList[index][3]].setCord(-handledXY.Y);
                                                    break;
                                                }
                                            case "topright":
                                                {
                                                    edges[rotateList[index][2]].setCord(handledXY.X);
                                                    edges[rotateList[index][1]].setCord(handledXY.Y);
                                                    edges[rotateList[index][0]].setCord(-handledXY.X);
                                                    edges[rotateList[index][3]].setCord(-handledXY.Y);
                                                    break;
                                                }
                                            case "bottomright":
                                                {
                                                    edges[rotateList[index][2]].setCord(handledXY.X);
                                                    edges[rotateList[index][3]].setCord(handledXY.Y);
                                                    edges[rotateList[index][0]].setCord(-handledXY.X);
                                                    edges[rotateList[index][1]].setCord(-handledXY.Y);
                                                    break;
                                                }
                                            case "bottomleft":
                                                {
                                                    edges[rotateList[index][0]].setCord(handledXY.X);
                                                    edges[rotateList[index][3]].setCord(handledXY.Y);
                                                    edges[rotateList[index][2]].setCord(-handledXY.X);
                                                    edges[rotateList[index][1]].setCord(-handledXY.Y);
                                                    break;
                                                }
                                            case "right":
                                                {
                                                    edges[rotateList[index][2]].setCord(handledXY.X);
                                                    edges[rotateList[index][0]].setCord(-handledXY.X);
                                                    break;
                                                }
                                            case "left":
                                                {
                                                    edges[rotateList[index][0]].setCord(handledXY.X);
                                                    edges[rotateList[index][2]].setCord(-handledXY.X);
                                                    break;
                                                }
                                            case "top":
                                                {
                                                    edges[rotateList[index][1]].setCord(handledXY.Y);
                                                    edges[rotateList[index][3]].setCord(-handledXY.Y);
                                                    break;
                                                }
                                            case "bottom":
                                                {
                                                    edges[rotateList[index][3]].setCord(handledXY.Y);
                                                    edges[rotateList[index][1]].setCord(-handledXY.Y);
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                            }
                        }

                    });
                }


                editPreviousX = curPos.X;
                editPreviousY = curPos.Y;

                RedrawCanvas();
                return;
            }
            if (_isDrawing)
            {
                _end = e.GetPosition(selectedLayer);
                selectedLayer.Children.Clear();
                RedrawCanvas();
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    _end = new Point(_start.X + (_end.Y - _start.Y), _start.Y + (_end.Y - _start.Y));
                }
                _painter.Brush = ChosenColor;
                _painter.StrokeDash = _strokeType;
                _painter.Thickness = _strokeThickness;
                _painter.HandleStart(_start.X, _start.Y);
                _painter.HandleEnd(_end.X, _end.Y);
                selectedLayer.Children.Add(_painter.Draw(_strokeThickness, _strokeType, ChosenColor));
                
                UndoBtn.IsEnabled = true;
            }
            if (_isTexting)
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.IBeam;
                _end = e.GetPosition(selectedLayer);
                selectedLayer.Children.Clear();
                RedrawCanvas();
                _painter.Brush = Brushes.Black;
                _painter.StrokeDash = new DoubleCollection { 4, 4 };
                _painter.Thickness = 1;
                _painter.HandleStart(_start.X, _start.Y);
                _painter.HandleEnd(_end.X, _end.Y);
                selectedLayer.Children.Add(_painter.Draw(1, new  DoubleCollection { 4, 4 }, Brushes.Black));

                UndoBtn.IsEnabled = true;
                
            }
            else
            {
                Mouse.OverrideCursor = null;
            }

        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this._prototypes.Count == 0)
                return;
            _isDrawing = false;
            if (this.IsEdit)
            {
                if (e.ChangedButton != MouseButton.Left)
                    return;

                Point curPosition = e.GetPosition(selectedLayer);
                foreach (var item in selectedPainter)
                {
                    if(item is TextBlock)
                    {
                        return;
                    }    
                    CShape temp = (CShape)item;
                    if (temp.isHovering(curPosition.X, curPosition.Y))
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            if (!_chosedShapes.Contains((IShape)item))
                            {
                                this._chosedShapes.Add((IShape)item);
                            }
                            else
                            {
                                this._chosedShapes.Remove((IShape)item);
                            }
                        }
                        else
                        {
                            _chosedShapes.Clear();
                            this._chosedShapes.Add((IShape)item);
                        }
                        RedrawCanvas();
                        break;
                    }
                }

                this.editPreviousX = -1;
                this.editPreviousY = -1;

                return;
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

                return;
            }

            Point pos = e.GetPosition(selectedLayer);
            _painter.HandleEnd(pos.X, pos.Y);

            // Ddd to shapes list & save it color + thickness
            _painter.Brush = ChosenColor;
            _painter.Thickness = _strokeThickness;
            _painter.StrokeDash = _strokeType;
            selectedPainter.Push((IShape)_painter.Clone());
            _allPainter.Add((IShape)_painter.Clone());
            _undoStack.Push((IShape)_painter.Clone());
            _redoStack.Clear();

            RedrawCanvas();
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
            IsTextFormatChosen = !IsTextFormatChosen;

        }

        private void LayerBtn_Click(object sender, RoutedEventArgs e)
        {
            LayersPopup.IsOpen = !LayersPopup.IsOpen;
            IsLayerBtnClicked = !IsLayerBtnClicked;
            Debug.WriteLine(IsLayerBtnClicked);
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
                if (_chosedShapes.Count > 0)
                {
                    _chosedShapes.ForEach(shape =>
                    {
                        shape.Brush = ChosenColor;
                    });
                    RedrawCanvas();
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

            _undoStacksLayer.Insert(0, new Stack<object>());
            _redoStacksLayer.Insert(0, new Stack<object>());

            selectedLayer = FindCanvasByName(Layers[0]);
            selectedPainter = _paintersLayer[0];
            _undoStack = _undoStacksLayer[0];
            _redoStack = _redoStacksLayer[0];
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
                    _undoStack = _undoStacksLayer[selectedIndex];
                    _redoStack = _redoStacksLayer[selectedIndex];
                    UpdateUndoRedoButtonState();
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
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Keyboard.IsKeyDown(Key.O))
            {
                Load();
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown((Key)Key.C))
            {
                handleCopy();
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown((Key)Key.X))
            {
                handleCut();
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown((Key)Key.V))
            {
                handlePaste();
            }
        }

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
                if (_chosedShapes.Count > 0)
                {
                    _chosedShapes.ForEach(shape =>
                    {
                        shape.Thickness = _strokeThickness;
                    });
                    RedrawCanvas();
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
                    _strokeType = new DoubleCollection { 1, 0 };
                    break;
                case "Dash":
                    _strokeType = new DoubleCollection { 5, 3 };
                    break;
                case "Dot":
                    _strokeType = new DoubleCollection { 1, 1 };
                    break;
                case "DashDotDot":
                    _strokeType = new DoubleCollection { 5, 3, 1, 1, 1, 1 };
                    break;
                default:
                    break;
            }
            if (_chosedShapes.Count > 0)
            {
                _chosedShapes.ForEach(shape =>
                {
                    shape.StrokeDash = _strokeType;
                });
                RedrawCanvas();
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
                    selectedLayer.Children.Add(shape.Draw(shape.Thickness, shape.StrokeDash, shape.Brush));
                }
                else if (item is TextBlock textBlock)
                {
                    selectedLayer.Children.Add(textBlock);
                }
                else if (item is UIElement uIElement)
                {
                    selectedLayer.Children.Add(uIElement);
                }
            }


            if (IsEdit && _chosedShapes.Count > 0)
            {
                _chosedShapes.ForEach(shape =>
                {
                    CShape cShape = (CShape)shape;
                    selectedLayer.Children.Add(cShape.controlOutline());
                    Trace.WriteLine(cShape.getRotateAngle());
                    if (_chosedShapes.Count == 1)
                    {
                        List<ControlPoint> controlPoints = cShape.GetControlPoints();
                        this._controlPoints = controlPoints;
                        controlPoints.ForEach(Ctrl =>
                        {
                            selectedLayer.Children.Add(Ctrl.drawPoint(cShape.getRotateAngle(), cShape.getCenterPoint()));
                        });
                    }
                });
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
            _undoStacksLayer.Clear();
            _redoStacksLayer.Clear();
            selectedLayer = null;
            selectedPainter = null;
            _isDrawing = false;
            _start = new Point();
            _end = new Point();
            _strokeThickness = 1;
            _strokeType = new DoubleCollection() { 1, 0 };
            _list.Clear();
            _undoStack.Clear();
            _redoStack.Clear();
            _lastElement = null;
            _allPainter.Clear();
            _painter = _prototypes[0];
            ChosenColor = Brushes.Black;
            BackgroundColor = Brushes.Black;
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

            _undoStacksLayer.Add(new Stack<object>());
            _undoStack = _undoStacksLayer[0];

            _redoStacksLayer.Add(new Stack<object>());
            _redoStack = _redoStacksLayer[0];

            IsLayerBtnClicked = false;
            IsEdit = false;
            IsTextFormatChosen = false;
            IsSelectArea = false;
            _isTexting = false;
            _isChosenColorClicked = true;
            _isBackgroundColorClicked = false;
            _isBackgrounFillCheckBox = false;
            _isLayerBtnClicked = false;

            StrokeTypeCB.SelectedIndex = 0;
            StrokeThicknessCB.SelectedIndex = 0;
            FontStyleCombobox.SelectedIndex = 0;
            FontSizeCombobox.SelectedIndex = 0;
            BackgrounFillCheckBox.IsChecked = false;
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

                    Restart();

                    List<IShape> containers = JsonConvert.DeserializeObject<List<IShape>>(content, settings);
                    foreach (var item in containers)
                    {
                        _allPainter.Add(item);
                    }

                    foreach (var shape in _allPainter)
                    {
                        var element = shape.Draw(shape.Thickness, shape.StrokeDash, shape.Brush);
                        selectedLayer.Children.Add(element);
                        selectedPainter.Push(element);
                        _undoStack.Push(element);
                        _redoStack.Clear();
                        UpdateUndoRedoButtonState();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading file: " + ex.Message);
                }
            }
        }

        #endregion

        private void EditShapeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.IsEdit = !this.IsEdit;
            if (IsEdit)
            {
                this.Cursor = System.Windows.Input.Cursors.Hand;
            }
            else
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
            if (!this.IsEdit)
                this._chosedShapes.Clear();
        }
        private void SelectAreaBtn_Click(object sender, RoutedEventArgs e)
        {
            IsSelectArea = !IsSelectArea;
        }
        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void handlePaste()
        {
            if (copiedShape == null && cutShape == null)
            {
                return;
            }
            // handle patse for copy
            if (copiedShape != null)
            {
                var point = System.Windows.Forms.Control.MousePosition;
                IShape temp = copiedShape.Clone();

                var width = temp.xRightBottom - temp.xleftTop;
                var height = temp.yleftTop - temp.yRightBottom;
                temp.HandleStart(point.X, point.Y - 195);
                temp.HandleEnd(point.X + width, point.Y + height - 195);
                selectedPainter.Push(temp.Clone());
                _allPainter.Add(temp.Clone());
                _undoStack.Push(temp.Clone());
                _redoStack.Clear();

                RedrawCanvas();
            }
            // handle paste for cut
            else
            {
                var point = System.Windows.Forms.Control.MousePosition;
                IShape temp = cutShape.Clone();

                _allPainter.Remove(cutShape);

                // remove element from stack
                List<object> tempList = selectedPainter.ToList();

                tempList.Remove(cutShape);

                selectedPainter = new Stack<object>(tempList);


                var width = temp.xRightBottom - temp.xleftTop;
                var height = temp.yleftTop - temp.yRightBottom;
                temp.HandleStart(point.X, point.Y - 195);
                temp.HandleEnd(point.X + width, point.Y + height - 195);
                selectedPainter.Push(temp.Clone());
                _allPainter.Add(temp.Clone());
                _undoStack.Push(temp.Clone());
                _redoStack.Clear();

                cutShape = null;
                // trigger isEdit = false
                IsEdit = false;
                RedrawCanvas();
            }

        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            handleCut();
        }
        private void handleCut()
        {
            if (_chosedShapes.Count != 1)
            {
                return;
            }
            copiedShape = null;
            cutShape = _chosedShapes[0];

        }
 
        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            handleCopy();
        }
        private void handleCopy()
        {
            if (_chosedShapes.Count != 1)
            {
                return;
            }
            cutShape = null;
            copiedShape = _chosedShapes[0];
        }
    }
}
