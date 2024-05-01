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
            var width = Math.Abs(_rightBottom.X - _leftTop.X);
            var height = Math.Abs(_rightBottom.Y - _leftTop.Y);

            var centerX = width / 2;
            var centerY = height / 2;

            // Create the Polygon
            Polygon starPolygon = new Polygon();
            starPolygon.Stroke = Brush;
            starPolygon.StrokeThickness = Thickness;

            double angleIncrement = Math.PI / 5;

            PointCollection starPoints = new PointCollection();

            // Tính toán tọa độ của các đỉnh của ngôi sao
            double halfWidth = width / 2;
            double halfHeight = height / 2;

            for (int i = 1; i <= 10; i++)
            {
                double halfWidth1;
                double halfHeight1;
                if (i % 2 == 0)
                {
                    halfHeight1 = halfHeight;
                    halfWidth1 = halfWidth;
                }
                else
                {
                    halfHeight1 = halfHeight / 2;
                    halfWidth1 = halfWidth / 2;
                }
                double angle = angleIncrement * i - Math.PI / 2; // Bắt đầu từ phía trên
                double x = centerX + halfWidth1 * Math.Cos(angle);
                double y = centerY + halfHeight1 * Math.Sin(angle);
                starPoints.Add(new Point(x, y));
            }

            starPolygon.Points = starPoints;

            if (_rightBottom.X > _leftTop.X && _rightBottom.Y > _leftTop.Y)
            {
                Canvas.SetLeft(starPolygon, _leftTop.X);
                Canvas.SetTop(starPolygon, _leftTop.Y);
            }
            else if (_rightBottom.X < _leftTop.X && _rightBottom.Y > _leftTop.Y)
            {
                Canvas.SetLeft(starPolygon, _rightBottom.X);
                Canvas.SetTop(starPolygon, _leftTop.Y);
            }
            else if (_rightBottom.X > _leftTop.X && _rightBottom.Y < _leftTop.Y)
            {
                Canvas.SetLeft(starPolygon, _leftTop.X);
                Canvas.SetTop(starPolygon, _rightBottom.Y);
            }
            else
            {
                Canvas.SetLeft(starPolygon, _rightBottom.X);
                Canvas.SetTop(starPolygon, _rightBottom.Y);
            }

            RotateTransform transform = new RotateTransform(this._rotateAngle);
            transform.CenterX = width * 1.0 / 2;
            transform.CenterY = height * 1.0 / 2;
            starPolygon.RenderTransform = transform;

            return starPolygon;
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
