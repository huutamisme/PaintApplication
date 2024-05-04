using System;
using System.Windows;
using System.Windows.Media;

namespace Main
{
    public interface IShape
    {
        string Name { get; }
        string Icon { get; }
        SolidColorBrush Brush { get; set; }
        int Thickness { get; set; }

        Double RotateAngle { get; set; }

        double xleftTop { get; set; }
        double yleftTop { get; set; }

        double xRightBottom { get; set; }
        double yRightBottom { get; set; }

        DoubleCollection StrokeDash { get; set; }

        void HandleStart(double x, double y);
        void HandleEnd(double x, double y);
        IShape Clone();

        UIElement Draw(int strokeThickness, DoubleCollection strokeDashArray, SolidColorBrush solidcolorbrush);
    }
}
