
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Shapes;

namespace MyLine
{
    public class MyLine : IShape
    {
        public Point _start;
        public Point _end;
        public SolidColorBrush _brush;
        public int _strokeThickness;
        public double[] _strokeDashArray;
        public string Name => "Line";

        public void AddFirst(Point point)
        {
            _start = point;
        }
        public void AddSecond(Point point)
        {
            _end = point;
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
            return new Line()
            {
                X1 = _start.X,
                Y1 = _start.Y,
                X2 = _end.X,
                Y2 = _end.Y,
                StrokeThickness = _strokeThickness,
                Stroke = _brush,
                StrokeDashArray = new DoubleCollection(_strokeDashArray)
            }; 
        }
    }

}
