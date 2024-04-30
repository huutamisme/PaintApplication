using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Main
{
		public class rotatePoint : ControlPoint
		{
			public override string type => "rotate";
		}

		public class diagPoint : ControlPoint
		{
			public override string type => "diag";
		}

		public class oneSidePoint : ControlPoint
		{
			public override string type => "diag";
			// read this one again
			public override string getEdge(double angle)
			{
				string[] edge = { "top", "right", "bottom", "left" };
				int index = 0;
				if (centrePoint.X == point.X)
					if (centrePoint.Y > point.Y)
						index = 0;
					else
						index = 2;
				else
					if (centrePoint.Y == point.Y)
					if (centrePoint.X > point.X)
						index = 3;
					else
						index = 1;

				double rot = angle;

				if (rot > 0)
					while (true)
					{
						rot -= 90;
						if (rot < 0)
							break;
						index++;

						if (index == 4)
							index = 0;
					}
				else
					while (true)
					{
						rot += 90;
						if (rot > 0)
							break;
						index--;
						if (index == -1)
							index = 3;
					};

				return edge[index];
			}
		}
}
