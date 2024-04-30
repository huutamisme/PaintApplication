using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using Main;
using System.Windows.Media.Media3D;

namespace MyStar
{
    public class MyStar : CShape, IShape
    {
        public SolidColorBrush Brush { get; set; }
        public int Thickness { get; set; }
        public DoubleCollection StrokeDash { get; set; }
        public string Name => "Star";
        public string Icon => "Images/pentagon.png";

        public double xleftTop { get; set; }
        public double yleftTop { get; set; }

        public double xRightBottom { get; set; }
        public double yRightBottom { get; set; }

        public void HandleEnd(double x, double y)
        {
            xRightBottom = x;
            yRightBottom = y;
            _rightBottom.X = x;
            _rightBottom.Y = y;
        }

        public void HandleStart(double x, double y)
        {
            xleftTop = x;
            yleftTop = y;
            _leftTop.X = x;
            _leftTop.Y = y;
        }

        public IShape Clone()
        {
            MyStar temp = new MyStar();

            temp.LeftTop = this._leftTop.deepCopy();
            temp.RightBottom = this._rightBottom.deepCopy();
            temp._rotateAngle = this._rotateAngle;
            temp.Thickness = this.Thickness;
            temp.xleftTop = this.xleftTop;
            temp.xRightBottom = this.xRightBottom;
            temp.yleftTop = this.yleftTop;
            temp.yRightBottom = this.yRightBottom;

            if (this.Brush != null)
                temp.Brush = this.Brush.Clone();

            if (this.StrokeDash != null)
                temp.StrokeDash = this.StrokeDash.Clone();
            return temp;
        }

        public UIElement Draw(int strokeThickness, DoubleCollection strokeDashArray, SolidColorBrush solidcolorbrush)
        {
            double width = Math.Abs(_rightBottom.X - _leftTop.X);
            double height = Math.Abs(_rightBottom.Y - _leftTop.Y);
            double centerX = (_leftTop.X + _rightBottom.X) / 2;
            double centerY = (_leftTop.Y + _rightBottom.Y) / 2;
            double radius = Math.Min(Math.Abs(_rightBottom.X - centerX), Math.Abs(_rightBottom.Y - centerY));

            PathFigure star = new PathFigure();
            star.StartPoint = new Point(centerX, centerY - radius);

            double angle = -Math.PI / 2;
            double increment = Math.PI / 5;

            for (int i = 0; i < 10; i++)
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
            path.Stroke = solidcolorbrush;
            path.StrokeThickness = strokeThickness;
            path.StrokeDashArray = strokeDashArray;

            RotateTransform transform = new RotateTransform(this._rotateAngle);
            transform.CenterX = width * 1.0 / 2;
            transform.CenterY = height * 1.0 / 2;
            path.RenderTransform = transform;

            return path;
        }


        override public CShape deepCopy()
        {
            MyStar temp = new MyStar();

            temp.LeftTop = this._leftTop.deepCopy();
            temp.RightBottom = this._rightBottom.deepCopy();
            temp._rotateAngle = this._rotateAngle;
            temp.Thickness = this.Thickness;

            if (this.Brush != null)
                temp.Brush = this.Brush.Clone();

            if (this.StrokeDash != null)
                temp.StrokeDash = this.StrokeDash.Clone();
            return temp;
        }

    }
}
