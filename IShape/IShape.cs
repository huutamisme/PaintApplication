
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
        UIElement Convert();
        string Name { get; }
    }

}
