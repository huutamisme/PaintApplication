using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using Main;

namespace MyArrowDown
{
    public class MyArrowDown : CShape, IShape
    {
        public SolidColorBrush Brush { get; set; }
        public int Thickness { get; set; }
        public DoubleCollection StrokeDash { get; set; }
        public string Name => "ArrowDown";
        public string Icon => "Images/pentagon.png";
        public void HandleEnd(double x, double y)
        {
            _rightBottom.X = x;
            _rightBottom.Y = y;
        }

        public void HandleStart(double x, double y)
        {
            _leftTop.X = x;
            _leftTop.Y = y;
        }

        public IShape Clone()
        {
            MyArrowDown temp = new MyArrowDown();

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

        public UIElement Draw(int strokeThickness, DoubleCollection strokeDashArray, SolidColorBrush solidcolorbrush)
        {
            double centerX = (_leftTop.X + _rightBottom.X) / 2;
            double centerY = (_leftTop.Y + _rightBottom.Y) / 2;
            double arrowHeight = Math.Abs(_leftTop.Y - _rightBottom.Y) / 2;

            PathFigure arrow = new PathFigure();
            arrow.StartPoint = new Point(centerX, centerY + arrowHeight); 

            LineSegment line1 = new LineSegment(new Point(centerX - arrowHeight / 2, centerY), true); 
            LineSegment line2 = new LineSegment(new Point(centerX - arrowHeight / 4, centerY), true); 
            LineSegment line3 = new LineSegment(new Point(centerX - arrowHeight / 4, centerY - arrowHeight), true); 
            LineSegment line4 = new LineSegment(new Point(centerX + arrowHeight / 4, centerY - arrowHeight), true); 
            LineSegment line5 = new LineSegment(new Point(centerX + arrowHeight / 4, centerY), true); 
            LineSegment line6 = new LineSegment(new Point(centerX + arrowHeight / 2, centerY), true); 
            LineSegment line7 = new LineSegment(new Point(centerX, centerY + arrowHeight), true); 

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

            return path;
        }

        override public CShape deepCopy()
        {
            MyArrowDown temp = new MyArrowDown();

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
