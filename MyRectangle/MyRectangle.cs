
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Shapes;
using System.Windows.Shapes;

namespace MyRectangle
{
    public class MyRectangle : IShape
    {
        private Point _topLeft;
        private Point _rightBottom;
        private SolidColorBrush _brush;
        private int _strokeThickness;
        private double[] _strokeDashArray;
        public string Name => "Rectangle";
        public void AddFirst(Point point)
        {
            _topLeft = point;
        }

        public void AddSecond(Point point)
        {
            _rightBottom = point;
        }
        public void AddColor(SolidColorBrush solidcolorbrush)
        {
            _brush = solidcolorbrush;
        }

        public void AddStrokeThickness(int strokeThickness)
        {
            _strokeThickness = strokeThickness;
        }

        public void AddStrokeDashArray(double[] strokeDashArray)
        {
            _strokeDashArray = strokeDashArray;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement Convert()
        {
            var item = new Rectangle()
            {   // TODO: end luon luon lon hon start
                Width = Math.Abs(_rightBottom.X - _topLeft.X),
                Height = Math.Abs(_rightBottom.Y - _topLeft.Y),
                StrokeThickness = _strokeThickness,
                Stroke = _brush,
                StrokeDashArray = new DoubleCollection(_strokeDashArray)
            };
            Canvas.SetLeft(item, Math.Min(_topLeft.X, _rightBottom.X));
            Canvas.SetTop(item, Math.Min(_topLeft.Y, _rightBottom.Y));
            return item;
        }
    }

}
