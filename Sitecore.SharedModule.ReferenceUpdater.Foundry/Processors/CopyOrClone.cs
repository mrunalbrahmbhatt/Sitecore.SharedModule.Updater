using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Pipelines;
using System.Collections.Generic;
using System.Linq;

namespace Sitecore.SharedModule.ReferenceUpdater.Foundry.Processors
{
	public class CopyOrCloneItem : Sitecore.Shell.Framework.Pipelines.CopyItems
	{
		public virtual void ProcessFieldValues(CopyItemsArgs args)
		{
			Item sourceRoot = CopyItems.GetItems(args).FirstOrDefault();
			Assert.IsNotNull(sourceRoot, "sourceRoot is null.");

			Item targetItem = args.Copies.FirstOrDefault();
			Assert.IsNotNull(targetItem, "targetItem is null.");

			Dictionary<string, string> roots = new Dictionary<string, string>();
			roots = FoundryWrapper.GetRoots(targetItem, sourceRoot);

			if (targetItem != null && roots != null && roots.Count > 0)
			{
				ReferenceUpdater refUpdater = new ReferenceUpdater(targetItem, roots, true);
				refUpdater.Start();
			}
		}
	}
}
