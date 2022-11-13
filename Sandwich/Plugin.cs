using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.GUI.Canvas;

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

			Instances.DocumentEditor.KeyDown += SandwichInteraction.SetActiveInteraction;
			
		}

	}
}
