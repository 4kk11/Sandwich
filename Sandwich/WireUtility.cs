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
		public static IEnumerable<Wire> GetVisibleWires(GH_Canvas canvas)
		{
			foreach (IGH_DocumentObject obj in canvas.Document.Objects)
			{
				if (obj is IGH_Param target) //Paramの場合
				{
					foreach (IGH_Param source in target.Sources)
					{
						if (target != null && source != null)
							yield return new Wire(source, target);
					}
				}
				else if (obj is IGH_Component component) //コンポーネントの場合
				{ 
					
				}
			}
		}
	}

	public class Wire
	{
		public IGH_Param source { get; private set; }
		public IGH_Param target { get; private set; }
		public GraphicsPath path { get; private set; }

		public Wire(IGH_Param _source, IGH_Param _target)
		{
			source = _source;
			target = _target;
			path = GH_Painter.ConnectionPath(target.Attributes.InputGrip, source.Attributes.OutputGrip, GH_WireDirection.left, GH_WireDirection.right);
		}
	}
}
