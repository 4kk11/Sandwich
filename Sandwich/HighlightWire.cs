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
using System.Drawing;
using System.Drawing.Drawing2D;
using Grasshopper.GUI.Canvas.Interaction;
using Rhino;

namespace Sandwich
{
	public class HighlightWire
	{
		public static void Init()
		{ 
			Instances.ActiveCanvas.CanvasPrePaintWires += PrePaintWire;
		}

		public static bool isActive;
		public static GraphicsPath path;

		private static void PrePaintWire(GH_Canvas canvas)
		{
			if (!isActive || path == null) return;
			Color col = Color.FromArgb(255, 0, 0);

			Pen edge = new Pen(col, 10);
			//edge.DashCap = System.Drawing.Drawing2D.DashCap.Flat;
			//edge.DashPattern = new float[] { 1.5f, 2f };
			canvas.Graphics.DrawPath(edge, path);

			edge.Dispose();
		}

		public static void Reset()
		{
			isActive = false;
			path = null;
			Instances.ActiveCanvas.CanvasPrePaintWires -= PrePaintWire;
		}
	}
}
