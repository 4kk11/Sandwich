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
using Grasshopper.GUI.Canvas.Interaction;
using Rhino;


namespace Sandwich
{
	public class SandwichInteraction : GH_DragInteraction
	{
		

		public SandwichInteraction(GH_Canvas canvas, GH_CanvasMouseEvent mouseEvent) : base(canvas, mouseEvent)
		{ 
			//なにかのイベントハンドラを登録したいときはここに書く

		}

		public static void SetActiveInteraction(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ControlKey)
			{
				GH_Canvas canvas = Grasshopper.Instances.ActiveCanvas;
				if (canvas.IsDocument)
				{
					if (canvas.ActiveInteraction is GH_DragInteraction &&
						!(canvas.ActiveInteraction is SandwichInteraction))
					{
						canvas.ActiveInteraction = new SandwichInteraction(canvas, new GH_CanvasMouseEvent(canvas.Viewport,
							new MouseEventArgs(MouseButtons.None, 0, canvas.CursorControlPosition.X, canvas.CursorControlPosition.Y, 0)));
						e.Handled = true;
					}
				}
				
			}
		}

		private IGH_Attributes GetDragingAtt()
		{ 
			List<IGH_Attributes> atts = typeof(GH_DragInteraction).GetField("m_att", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this) as List<IGH_Attributes>;
			if (atts == null) return null;
			IGH_Attributes att = atts[0];
			return att;
		}


		private bool onWire()
		{
			IGH_Attributes att = GetDragingAtt();
			IGH_DocumentObject obj = att?.GetTopLevel.DocObject;
			if (obj != null)
			{
				var wires = WireUtility.GetVisibleWirePaths(Instances.ActiveCanvas);
				if (wires.Count() == 0) return false;

				RectangleF bounds = att.Bounds;

				foreach (var w in wires)
				{
					Region reg = new Region(w);
					if (reg.IsVisible(bounds))
					{
						return true;
					}
				}
			}

			return false;
		}

		


		public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
		{
			if (onWire())
			{
				RhinoApp.WriteLine("onWire!!");
			}
			return base.RespondToMouseUp(sender, e);
		}

		public override GH_ObjectResponse RespondToKeyUp(GH_Canvas sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ControlKey)
			{
				GH_Canvas canvas = Grasshopper.Instances.ActiveCanvas;
				canvas.ActiveInteraction = new GH_DragInteraction(canvas, new GH_CanvasMouseEvent(canvas.Viewport,
								new MouseEventArgs(MouseButtons.None, 0, canvas.CursorControlPosition.X, canvas.CursorControlPosition.Y, 0)));
			}
			return base.RespondToKeyUp(sender,e);
		}

		public override void Destroy()
		{
			//コンストラクタで登録したイベントハンドラを破棄したいときはここに書く
			base.Destroy();
		}

	}
}

/*
A = Instances.ActiveCanvas.ActiveInteraction;
this.Component.ExpireSolution(true);
*/
