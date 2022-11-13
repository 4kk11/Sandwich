using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Grasshopper.GUI.Canvas.Interaction;
using Rhino;


namespace Sandwich
{
	public class WireUtility
	{
		public static IEnumerable<GraphicsPath> GetVisibleWirePaths(GH_Canvas canvas)
		{
			foreach (IGH_DocumentObject obj in canvas.Document.Objects)
			{
				if (obj is IGH_Param target)
				{
					foreach (IGH_Param source in target.Sources)
					{
						GraphicsPath path = GH_Painter.ConnectionPath(target.Attributes.InputGrip, source.Attributes.OutputGrip, GH_WireDirection.left, GH_WireDirection.right);
						yield return path;
					}
				}
			}
		}
	}
}
