using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using System.Reflection;

namespace Sandwich
{
	public class Plugin : GH_AssemblyInfo
	{
		public override string Name => "Sandwich";
		public override Bitmap Icon => null;
		public override string Description => "";
		public override Guid Id => new Guid("56F9640B-F425-43E9-B33E-E2835C25AE47");
		public override string AuthorName => "";
		public override string AuthorContact => "";
	}
	public class Priority : GH_AssemblyPriority
	{
		public override GH_LoadingInstruction PriorityLoad()
		{
			Instances.CanvasCreated += AppendSandwichInteraction;
			return GH_LoadingInstruction.Proceed;
		}

		private void AppendSandwichInteraction(GH_Canvas canvas)
		{
			Instances.CanvasCreated -= AppendSandwichInteraction;

			GH_DocumentEditor editor = Instances.DocumentEditor;

			var events = (System.ComponentModel.EventHandlerList)typeof(Control).GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(editor, null);
			object key = typeof(Control).GetField("EventKeyDown", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
			Delegate handlers = events[key];

			if (handlers != null)
			{
				foreach (Delegate handler in handlers.GetInvocationList())
				{
					if (handler == null)
						continue;
					var dele = (KeyEventHandler)Delegate.CreateDelegate(typeof(KeyEventHandler), editor, handler.Method, true);
					editor.KeyDown -= dele;
				}
			}
			
			Instances.DocumentEditor.KeyDown += SandwichInteraction.SetActiveInteraction;
			//Instances.ActiveCanvas.KeyDown += SandwichInteraction.SetActiveInteraction;

			if (handlers != null)
			{
				foreach (Delegate handler in handlers.GetInvocationList())
				{
					if (handler == null)
						continue;
					var dele = (KeyEventHandler)Delegate.CreateDelegate(typeof(KeyEventHandler), editor, handler.Method, true);
					editor.KeyDown += dele;
				}
			}
		}

	}
}
