using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Undo;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Canvas.Interaction;
using Rhino;
using System.Runtime.CompilerServices;

namespace Sandwich
{
	public class SandwichInteraction : GH_DragInteraction
	{
		private static IEnumerable<Wire> wires;

		public SandwichInteraction(GH_Canvas canvas, GH_CanvasMouseEvent mouseEvent) : base(canvas, mouseEvent)
		{
			//なにかのイベントハンドラを登録したいときはここに書く
			HighlightWire.Init();
			wires = WireUtility.GetVisibleWires(this.Canvas); //ワイヤーを全探査するのはctrlを押したときのみに留める
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
					}
					typeof(KeyEventArgs).GetField("keyData", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(e, Keys.Select);
				}
				e.Handled = true;
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

			List<IGH_UndoAction> undoActions = new List<IGH_UndoAction>(); //create undo actions

			if (obj is IGH_Param param) //ドラッグ中のオブジェクトがIGH_Paramの場合
			{
				//ワイヤーをはずす
				undoActions.AddRange(this.Canvas.Document.UndoUtil.CreateWireEvent("Remove target source", wire.target).Actions); 
				wire.target.RemoveSource(wire.source);

				undoActions.AddRange(this.Canvas.Document.UndoUtil.CreateWireEvent("Remove param source", param).Actions);
				param.RemoveAllSources();

				for (int i = param.Recipients.Count-1; i >= 0; i--)
				{
					IGH_Param recipient = param.Recipients[i];
					undoActions.AddRange(this.Canvas.Document.UndoUtil.CreateWireEvent("Remove param recipient", recipient).Actions);
					recipient.RemoveSource(param);
				}

				//ワイヤーをつける
				if (param.Attributes.HasInputGrip)
				{
					undoActions.AddRange(this.Canvas.Document.UndoUtil.CreateWireEvent("Add param source", param).Actions);
					param.AddSource(wire.source);
				}
				if (param.Attributes.HasOutputGrip)
				{
					undoActions.AddRange(this.Canvas.Document.UndoUtil.CreateWireEvent("Add target source", wire.target).Actions);
					wire.target.AddSource(param);
				}
			}
			else if (obj is IGH_Component comp) //ドラッグ中のオブジェクトがIGH_Componentの場合(保留)
			{
				return;
			}

			this.Canvas.Document.UndoUtil.RecordEvent("Sandwich", undoActions);
			obj.ExpireSolution(true);
		}


		public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
		{	
			
			if (onWire(out Wire wire))
			{
				//つなげ変える
				IGH_DocumentObject obj = GetDragingAtt().GetTopLevel.DocObject;
				SandwichObject(obj, wire);
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
				GH_Canvas canvas = this.Canvas;
				canvas.ActiveInteraction = new GH_DragInteraction(canvas, new GH_CanvasMouseEvent(canvas.Viewport,
								new MouseEventArgs(MouseButtons.None, 0, canvas.CursorControlPosition.X, canvas.CursorControlPosition.Y, 0)));
			}
			return base.RespondToKeyUp(sender,e);
		}

		public override void Destroy()
		{
			//コンストラクタで登録したイベントハンドラを破棄したいときはここに書く
			HighlightWire.Reset();
			wires = null;
			base.Destroy();
		}

	}
}

/*
A = Instances.ActiveCanvas.ActiveInteraction;
this.Component.ExpireSolution(true);
*/
