using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Main
{
    public class Point2D : IShape
    {
        public double X { get; set; }
        public double Y { get; set; }

        public string Icon { get; } // logo tuong ung voi icon

        public SolidColorBrush Brush { get; set; }  // mau sac
        public DoubleCollection StrokeDash { get; set; } // do day
        public string Name => "Point";
        public int Thickness { get; set; }

        // check whether a point is hover or not
        public bool isHovering(double x, double y)
        {
            return false;
        }

        // start drag
        public void HandleStart(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void HandleEnd(double x, double y)
        {
            X = x;
            Y = y;
        }

        // Point => start and end is the same
        public UIElement Draw(int thickness, DoubleCollection dash,SolidColorBrush brush)
        {
            Line line = new Line()
            {
                X1 = X,
                Y1 = Y,
                X2 = X,
                Y2 = Y,
                StrokeThickness = thickness,
                Stroke = brush,
                StrokeDashArray = dash
            };

            return line;
        }

        public IShape Clone()
        {
            return new Point2D();
        }

        // used for copy another point
        public Point2D deepCopy()
        {
            Point2D temp = new Point2D();
            temp.Y = this.Y;
            temp.X = this.X;
            return temp;
        }
    }
}
