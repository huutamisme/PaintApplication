using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using Main;
using System.Windows.Media.Media3D;

namespace MyArrowLeft 
{
    public class MyArrowLeft : CShape, IShape
    {
        public Double RotateAngle { get; set; }

        public SolidColorBrush Brush { get; set; }
        public int Thickness { get; set; }
        public DoubleCollection StrokeDash { get; set; }
        public string Name => "ArrowLeft";
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
            MyArrowLeft temp = new MyArrowLeft();

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

        public UIElement Draw(int strokeThickness, DoubleCollection strokeDashArray, SolidColorBrush solidColorBrush)
        {
            double width = Math.Abs(_rightBottom.X - _leftTop.X);
            double height = Math.Abs(_rightBottom.Y - _leftTop.Y);
            double centerX = width / 2;
            double centerY = height / 2;
            double arrowHeight = Math.Abs(_leftTop.Y - _rightBottom.Y) / 2;
            double arrowWidth = Math.Abs(_leftTop.X - _rightBottom.X) / 2;

            Polygon arrow = new Polygon();
            arrow.Stroke = solidColorBrush;
            arrow.StrokeThickness = strokeThickness;
            arrow.StrokeDashArray = strokeDashArray;
            PointCollection points = new PointCollection();

            points.Add(new Point(centerX, centerY - arrowHeight / 2));
            points.Add(new Point(centerX, centerY - arrowHeight / 4));
            points.Add(new Point(centerX + arrowWidth, centerY - arrowHeight / 4));
            points.Add(new Point(centerX + arrowWidth, centerY + arrowHeight / 4));
            points.Add(new Point(centerX, centerY + arrowHeight / 4));
            points.Add(new Point(centerX, centerY + arrowHeight / 2));
            points.Add(new Point(centerX - arrowWidth, centerY));


            arrow.Points = points;

            if (_rightBottom.X > _leftTop.X && _rightBottom.Y > _leftTop.Y)
            {
                Canvas.SetLeft(arrow, _leftTop.X);
                Canvas.SetTop(arrow, _leftTop.Y);
            }
            else if (_rightBottom.X < _leftTop.X && _rightBottom.Y > _leftTop.Y)
            {
                Canvas.SetLeft(arrow, _rightBottom.X);
                Canvas.SetTop(arrow, _leftTop.Y);
            }
            else if (_rightBottom.X > _leftTop.X && _rightBottom.Y < _leftTop.Y)
            {
                Canvas.SetLeft(arrow, _leftTop.X);
                Canvas.SetTop(arrow, _rightBottom.Y);
            }
            else
            {
                Canvas.SetLeft(arrow, _rightBottom.X);
                Canvas.SetTop(arrow, _rightBottom.Y);
            }

            RotateTransform transform = new RotateTransform(RotateAngle);
            transform.CenterX = width * 1.0 / 2;
            transform.CenterY = height * 1.0 / 2;
            arrow.RenderTransform = transform;

            return arrow;
        }

        override public CShape deepCopy()
        {
            MyArrowLeft temp = new MyArrowLeft();

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
