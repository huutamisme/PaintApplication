using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using Main;
using System.Windows.Media.Media3D;

namespace MyArrowUp
{
    public class MyArrowUp : CShape, IShape
    {
        //public Point _topLeft;
        //public Point _rightBottom;
        //public SolidColorBrush _brush;
        //public int _strokeThickness;
        //public double[] _strokeDashArray;
        //public string Name => "ArrowUp";
        //public void AddFirst(Point point)
        //{
        //    _topLeft = point;
        //}

        //public void AddSecond(Point point)
        //{
        //    _rightBottom = point;
        //}
        //public void AddColor(SolidColorBrush solidcolorbrush)
        //{
        //    _brush = solidcolorbrush;
        //}

        //public void AddStrokeThickness(int strokeThickness)
        //{
        //    _strokeThickness = strokeThickness;
        //}

        //public void AddStrokeDashArray(double[] strokeDashArray)
        //{
        //    _strokeDashArray = strokeDashArray;
        //}
        //public object Clone()
        //{
        //    return MemberwiseClone();
        //}

        //public UIElement Convert()
        //{
        //    double centerX = (_topLeft.X + _rightBottom.X) / 2;
        //    double centerY = (_topLeft.Y + _rightBottom.Y) / 2;
        //    double arrowHeight = Math.Abs(_topLeft.Y - _rightBottom.Y) * 0.8;
        //    double arrowWidth = Math.Abs(_topLeft.X - _rightBottom.X) / 4;

        //    PathFigure arrow = new PathFigure();
        //    arrow.StartPoint = new Point(centerX + arrowWidth, centerY + arrowHeight / 2);

        //    LineSegment line1 = new LineSegment(new Point(centerX - arrowWidth, centerY + arrowHeight / 2), true);
        //    LineSegment line2 = new LineSegment(new Point(centerX - arrowWidth, centerY - arrowHeight / 2), true);
        //    LineSegment line3 = new LineSegment(new Point(centerX - arrowWidth * 2, centerY - arrowHeight / 2), true); 
        //    LineSegment line4 = new LineSegment(new Point(centerX, centerY - arrowHeight), true); 
        //    LineSegment line5 = new LineSegment(new Point(centerX + arrowWidth * 2, centerY - arrowHeight / 2), true); 
        //    LineSegment line6 = new LineSegment(new Point(centerX + arrowWidth, centerY - arrowHeight / 2), true); 
        //    LineSegment line7 = new LineSegment(new Point(centerX + arrowWidth, centerY + arrowHeight / 2), true);

        //    arrow.Segments.Add(line1);
        //    arrow.Segments.Add(line2);
        //    arrow.Segments.Add(line3);
        //    arrow.Segments.Add(line4);
        //    arrow.Segments.Add(line5);
        //    arrow.Segments.Add(line6);
        //    arrow.Segments.Add(line7);

        //    PathGeometry geometry = new PathGeometry();
        //    geometry.Figures.Add(arrow);

        //    Path path = new Path();
        //    path.Data = geometry;
        //    path.Stroke = _brush;
        //    path.StrokeThickness = _strokeThickness;
        //    path.StrokeDashArray = new DoubleCollection(_strokeDashArray);

        //    return path;
        //}
        public SolidColorBrush Brush { get; set; }
        public int Thickness { get; set; }
        public DoubleCollection StrokeDash { get; set; }
        public string Name => "ArrowUp";
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
            MyArrowUp temp = new MyArrowUp();

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
            double arrowHeight = Math.Abs(_leftTop.Y - _rightBottom.Y) / 2;

            PathFigure arrow = new PathFigure();
            arrow.StartPoint = new Point(centerX, centerY - arrowHeight);

            LineSegment line1 = new LineSegment(new Point(centerX - arrowHeight / 2, centerY), true);
            LineSegment line2 = new LineSegment(new Point(centerX - arrowHeight / 4, centerY), true);
            LineSegment line3 = new LineSegment(new Point(centerX - arrowHeight / 4, centerY + arrowHeight), true);
            LineSegment line4 = new LineSegment(new Point(centerX + arrowHeight / 4, centerY + arrowHeight), true);
            LineSegment line5 = new LineSegment(new Point(centerX + arrowHeight / 4, centerY), true);
            LineSegment line6 = new LineSegment(new Point(centerX + arrowHeight / 2, centerY), true);
            LineSegment line7 = new LineSegment(new Point(centerX, centerY - arrowHeight), true);

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
            MyArrowUp temp = new MyArrowUp();

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
