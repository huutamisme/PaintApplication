
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Main;
using System.Windows.Media.Media3D;
using System.Diagnostics;

namespace MyEllipse
{
    public class MyEllipse : CShape, IShape
    {
        public SolidColorBrush Brush { get; set; }
        public int Thickness { get; set; }
        public DoubleCollection StrokeDash { get; set; }
        public string Name => "Ellipse";
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
            MyEllipse temp = new MyEllipse();

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
            double width = Math.Abs(_rightBottom.X - _leftTop.X);
            double height = Math.Abs(_rightBottom.Y - _leftTop.Y);
            var item = new Ellipse()
            {   // TODO: end luon luon lon hon start
                Width = Math.Abs(_rightBottom.X - _leftTop.X),
                Height = Math.Abs(_rightBottom.Y - LeftTop.Y),
                StrokeThickness = Thickness,
                Stroke = Brush,
                StrokeDashArray = StrokeDash
            };
            Canvas.SetLeft(item, Math.Min(_leftTop.X, _rightBottom.X));
            Canvas.SetTop(item, Math.Min(_leftTop.Y, _rightBottom.Y));
            Trace.WriteLine("Here" + getRotateAngle());
            RotateTransform transform = new RotateTransform(this._rotateAngle);
            transform.CenterX = width * 1.0 / 2;
            transform.CenterY = height * 1.0 / 2;

            item.RenderTransform = transform;
            return item;
        }
    }

}
