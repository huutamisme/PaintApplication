using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Shapes;
using System.Windows.Shapes;

namespace MyArrowRight
{
    public class MyArrowRight : IShape
    {
        private Point _topLeft;
        private Point _rightBottom;
        private SolidColorBrush _brush;
        private int _strokeThickness;
        private double[] _strokeDashArray;
        public string Name => "ArrowRight";
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
            double centerX = (_topLeft.X + _rightBottom.X) / 2;
            double centerY = (_topLeft.Y + _rightBottom.Y) / 2;
            double arrowWidth = Math.Abs(_topLeft.X - _rightBottom.X) / 2;

            PathFigure arrow = new PathFigure();
            arrow.StartPoint = new Point(centerX + arrowWidth, centerY); 

            LineSegment line1 = new LineSegment(new Point(centerX, centerY - arrowWidth / 2), true);
            LineSegment line2 = new LineSegment(new Point(centerX, centerY - arrowWidth / 4), true); 
            LineSegment line3 = new LineSegment(new Point(centerX - arrowWidth, centerY - arrowWidth / 4), true);
            LineSegment line4 = new LineSegment(new Point(centerX - arrowWidth, centerY + arrowWidth / 4), true); 
            LineSegment line5 = new LineSegment(new Point(centerX, centerY + arrowWidth / 4), true); 
            LineSegment line6 = new LineSegment(new Point(centerX, centerY + arrowWidth / 2), true);
            LineSegment line7 = new LineSegment(new Point(centerX + arrowWidth, centerY), true);

            arrow.Segments.Add(line1);
            arrow.Segments.Add(line2);
            arrow.Segments.Add(line3);
            arrow.Segments.Add(line4);
            arrow.Segments.Add(line5);
            arrow.Segments.Add(line6);
            arrow.Segments.Add(line7);

            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(arrow);

            Path path = new Path();
            path.Data = geometry;
            path.Stroke = _brush;
            path.StrokeThickness = _strokeThickness;
            path.StrokeDashArray = new DoubleCollection(_strokeDashArray);

            return path;
        }

    }
}
