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
	public class WireUtility
	{
		public static IEnumerable<Wire> GetVisibleWires(GH_Canvas canvas)
		{
			foreach (IGH_DocumentObject obj in canvas.Document.Objects)
			{
				//ビューに入っていないものは除外する
				RectangleF rec = canvas.Viewport.VisibleRegion;

				if (obj is IGH_Param) //Paramの場合
				{
					IGH_Param target = (IGH_Param)obj;
					if (target.WireDisplay == GH_ParamWireDisplay.hidden) continue;
					foreach (IGH_Param source in target.Sources)
					{
						if (target != null && source != null)
						{
							Wire wire = new Wire(source, target);
							Region region = new Region(wire.path);
							
							if (region.IsVisible(rec))
								yield return wire;
						}
					}
				}
				else if (obj is IGH_Component component) //コンポーネントの場合
				{
					foreach (IGH_Param target in component.Params.Input)
					{
						if (target.WireDisplay == GH_ParamWireDisplay.hidden) continue;
						foreach (IGH_Param source in target.Sources)
						{
							if (target != null && source != null)
							{
								Wire wire = new Wire(source, target);
								Region region = new Region(wire.path);
								if (region.IsVisible(rec))
									yield return wire;
							}
						}
					}
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
			PointF inputGrip = target.Attributes.InputGrip;
			PointF outputGrip = source.Attributes.OutputGrip;
			path = GH_Painter.ConnectionPath(inputGrip, outputGrip, GH_WireDirection.left, GH_WireDirection.right);
			if (inputGrip.Y == outputGrip.Y)
			{
				//ワイヤーが直線になるとき、何故かRegionが作れなくなるので以下で対処
				path.Widen(new Pen(Color.White, 1.0f));
			}
			
		}
	}
}
