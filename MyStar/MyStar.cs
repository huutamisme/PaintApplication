using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Shapes;
using System.Windows.Shapes;
using System.Diagnostics;

namespace MyStar
{
    public class MyStar : IShape
    {
        private Point _start;
        private Point _end;
        private SolidColorBrush _brush;
        private int _strokeThickness;
        private double[] _strokeDashArray;
        private double _angle = 0;

        public string Name => "Star";

        public void AddFirst(Point point)
        {
            _start = point;
        }

        public void AddSecond(Point point)
        {
            _end = point;
        }

        public void AddColor(SolidColorBrush solidColorBrush)
        {
            _brush = solidColorBrush;
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
            double centerX = (_start.X + _end.X) / 2;
            double centerY = (_start.Y + _end.Y) / 2;
            double radius = Math.Sqrt(Math.Pow(_end.X - centerX, 2) + Math.Pow(_end.Y - centerY, 2));

            PathFigure star = new PathFigure();
            star.StartPoint = new Point(centerX, centerY - radius);

            double angle = -Math.PI / 2;
            double increment = Math.PI / 5;

            for (int i = 0; i < 5; i++)
            {
                LineSegment line = new LineSegment();
                line.Point = new Point(centerX + radius * Math.Cos(angle), centerY + radius * Math.Sin(angle));
                star.Segments.Add(line);

                angle += increment;

                line = new LineSegment();
                line.Point = new Point(centerX + (radius / 2) * Math.Cos(angle), centerY + (radius / 2) * Math.Sin(angle));
                star.Segments.Add(line);

                angle += increment;
            }
            star.IsClosed = true;

            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(star);

            Path path = new Path();
            path.Data = geometry;
            path.Stroke = _brush;
            path.StrokeThickness = _strokeThickness;
            path.StrokeDashArray = new DoubleCollection(_strokeDashArray);

            return path;
        }

    }
}
