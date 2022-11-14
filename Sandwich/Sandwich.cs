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
	public class SandwichInteraction : GH_DragInteraction
	{
		

		public SandwichInteraction(GH_Canvas canvas, GH_CanvasMouseEvent mouseEvent) : base(canvas, mouseEvent)
		{
			//なにかのイベントハンドラを登録したいときはここに書く
			HighlightWire.Init();
		}

		public static void SetActiveInteraction(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ControlKey)
			{
				GH_Canvas canvas = Grasshopper.Instances.ActiveCanvas;
				if (canvas.IsDocument)
				{
					if (canvas.ActiveInteraction is GH_DragInteraction && !(canvas.ActiveInteraction is SandwichInteraction))
					{
						canvas.ActiveInteraction = new SandwichInteraction(canvas, new GH_CanvasMouseEvent(canvas.Viewport,
							new MouseEventArgs(MouseButtons.None, 0, canvas.CursorControlPosition.X, canvas.CursorControlPosition.Y, 0)));
						e.Handled = true;
					}
				}
				
			}
		}

		private IGH_Attributes GetDragingAtt()　//ドラッグ中のオブジェクトのAttributeを取得
		{ 
			List<IGH_Attributes> atts = typeof(GH_DragInteraction).GetField("m_att", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this) as List<IGH_Attributes>;
			if (atts == null) return null;
			IGH_Attributes att = atts[0];
			return att;
		}


		private bool onWire(out Wire wire)　//重なっているかの判定
		{
			wire = null;
			IGH_Attributes att = GetDragingAtt();
			IGH_DocumentObject obj = att?.GetTopLevel.DocObject;
			if (obj != null && obj is IGH_Param) //(仮)IGH_Paramのみ
			{
				var wires = WireUtility.GetVisibleWires(Instances.ActiveCanvas);
				if (wires.Count() == 0) return false;

				RectangleF bounds = att.Bounds;

				foreach (Wire w in wires)
				{
					Region reg = new Region(w.path);
					if (reg.IsVisible(bounds))
					{
						//ドラッグ中のオブジェクトについているワイヤーはスキップ
						if (obj is IGH_Param param)
						{
							if (w.source == param || w.target == param) continue;
						}
						else if (obj is IGH_Component comp)
						{

						}

						wire = w;
						return true;
					}
				}
			}
			
			return false;
		}

		private void SandwichObject(IGH_DocumentObject obj, Wire wire)
		{
			if (obj == null || wire == null) return;
			if (obj is IGH_Param param) //ドラッグ中のオブジェクトがIGH_Paramの場合
			{
				wire.target.RemoveSource(wire.source);
				param.RemoveAllSources();

				for (int i = param.Recipients.Count-1; i >= 0; i--)
				{
					param.Recipients[i].RemoveSource(param);
				}

				if(param.Attributes.HasInputGrip) param.AddSource(wire.source);
				if (param.Attributes.HasOutputGrip) wire.target.AddSource(param);
			}
			else if (obj is IGH_Component comp) //ドラッグ中のオブジェクトがIGH_Componentの場合(保留)
			{
				return;
			}
			obj.ExpireSolution(true);
		}


		public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
		{
			
			if (onWire(out Wire wire))
			{
				//つなげ変える
				IGH_DocumentObject obj = GetDragingAtt().GetTopLevel.DocObject;
				SandwichObject(obj, wire);

				RhinoApp.WriteLine("onWire!!");
			}
			return base.RespondToMouseUp(sender, e);
		}

		public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
		{
			if (onWire(out Wire wire))
			{
				//ワイヤーをハイライト
				HighlightWire.path = wire.path;
				HighlightWire.isActive = true;
			}
			else
			{
				HighlightWire.path = null;
				HighlightWire.isActive = false;
			}

			return base.RespondToMouseMove(sender, e);
		}

		public override GH_ObjectResponse RespondToKeyUp(GH_Canvas sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.ControlKey)
			{
				//DragInteractionに戻す(SandwichInteractionは自動的に破棄される)
				GH_Canvas canvas = Grasshopper.Instances.ActiveCanvas;
				canvas.ActiveInteraction = new GH_DragInteraction(canvas, new GH_CanvasMouseEvent(canvas.Viewport,
								new MouseEventArgs(MouseButtons.None, 0, canvas.CursorControlPosition.X, canvas.CursorControlPosition.Y, 0)));
			}
			return base.RespondToKeyUp(sender,e);
		}

		public override void Destroy()
		{
			//コンストラクタで登録したイベントハンドラを破棄したいときはここに書く
			HighlightWire.Reset();
			base.Destroy();
		}

	}
}

/*
A = Instances.ActiveCanvas.ActiveInteraction;
this.Component.ExpireSolution(true);
*/
