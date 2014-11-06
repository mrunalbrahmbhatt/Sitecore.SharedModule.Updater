using Sitecore.Data.Items;
using Sitecore.Events;
using System;
using System.Collections.Generic;

namespace Sitecore.SharedModule.ReferenceUpdater.Foundry.Events
{
	public class AddFromTemplate
	{
		public void OnItemAdded(object sender, EventArgs args)
		{
			Item targetItem = Event.ExtractParameter(args, 0) as Item;

			if (targetItem == null)
				return;

			Dictionary<string, string> roots = new Dictionary<string, string>();
			roots = FoundryWrapper.GetRoots(targetItem);

			if (targetItem.Branch != null && targetItem.Branch.InnerItem.Children.Count == 1)
				roots.Add(targetItem.Branch.InnerItem.Children[0].Paths.Path, targetItem.Paths.Path);

			if (targetItem != null && roots!=null && roots.Count > 0)
			{
				ReferenceUpdater refUpdater = new ReferenceUpdater(targetItem, roots, true);
				refUpdater.Start();
			}
		}
	}
}
