
using System.Windows;
using System.Windows.Media;

namespace Shapes
{
    public interface IShape : ICloneable
    {
        void AddFirst(Point point);
        void AddSecond(Point point);
        void AddColor(SolidColorBrush solidColorBrush);
        void AddStrokeThickness(int stroleThickness);
        void AddStrokeDashArray(double[] strokeDashArray);
        UIElement Convert();
        string Name { get; }
    }

}
