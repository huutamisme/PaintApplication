
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Shapes;

namespace MyEllipse
{
    public class MyEllipse : IShape
    {
        private Point _topLeft;
        private Point _rightBottom;
        public string Name => "Ellipse";
        public void AddFirst(Point point)
        {
            _topLeft = point;
        }

        public void AddSecond(Point point)
        {
            _rightBottom = point;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement Convert()
        {
            var item = new Ellipse()
            {   // TODO: end luon luon lon hon start
                Width = _rightBottom.X - _topLeft.X,
                Height = _rightBottom.Y - _topLeft.Y,
                StrokeThickness = 1,
                Stroke = new SolidColorBrush(Colors.Blue)
            };
            Canvas.SetLeft(item, _topLeft.X);
            Canvas.SetTop(item, _topLeft.Y);
            return item;
        }
    }

}
