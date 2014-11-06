using Sitecore.Buckets.Extensions;
using Sitecore.Buckets.Managers;
using Sitecore.Buckets.Pipelines.UI;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Globalization;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;

namespace Sitecore.SharedModule.ReferenceUpdater.Foundry.Processors
{
	public class DuplicateItem : ItemDuplicate
	{
		private Item _itemToDuplicate;

		public new void Execute(ClientPipelineArgs args)
		{
			Item targetItem = Duplicate(args);
			if (targetItem == null)
				return;

			if (_itemToDuplicate == null)
				return;


			Dictionary<string, string> roots = new Dictionary<string, string>();
			roots = FoundryWrapper.GetRoots(targetItem, _itemToDuplicate);

			if (targetItem != null && roots != null && roots.Count > 0)
			{
				ReferenceUpdater refUpdater = new ReferenceUpdater(targetItem, roots, true);
				refUpdater.Start();
			}
		}

		private Item Duplicate(ClientPipelineArgs args)
		{
			Item result = null;
			Assert.ArgumentNotNull(args, "args");
			Database database = Factory.GetDatabase(args.Parameters["database"]);
			Assert.IsNotNull(database, args.Parameters["database"]);
			string item = args.Parameters["id"];
			Item item1 = database.Items[item];
			if (item1 != null)
			{
				Item parent = item1.Parent;
				if (parent == null)
				{
					SheerResponse.Alert(Translate.Text("Cannot duplicate the root item."), new string[0]);
					args.AbortPipeline();
				}
				else if (!parent.Access.CanCreate())
				{
					object[] displayName = new object[] { item1.DisplayName };
					SheerResponse.Alert(Translate.Text("You do not have permission to duplicate \"{0}\".", displayName), new string[0]);
					args.AbortPipeline();
				}
				else
				{
					string[] strArrays = new string[] { AuditFormatter.FormatItem(item1) };
					Log.Audit(this, "Duplicate item: {0}", strArrays);
					Item parentBucketItemOrSiteRoot = item1.GetParentBucketItemOrSiteRoot();
					_itemToDuplicate = item1;
					if (!BucketManager.IsBucket(parentBucketItemOrSiteRoot))
					{
						result = Context.Workflow.DuplicateItem(item1, args.Parameters["name"]);
					}
					else
					{
						object[] objArray = new object[] { args, this };
						if (Event.RaiseEvent("item:bucketing:duplicating", objArray).Cancel)
						{
							Log.Info(string.Format("Event {0} was cancelled", "item:bucketing:duplicating"), this);
							args.AbortPipeline();
							return null;
						}
						Item item2 = Context.Workflow.DuplicateItem(item1, args.Parameters["name"]);
						Item item3 = CreateAndReturnDateFolderDestination(parentBucketItemOrSiteRoot, DateTime.Now, item1.ID);
						if (!IsBucketTemplateCheck(item1))
						{
							item3 = parentBucketItemOrSiteRoot;
						}
						ItemManager.MoveItem(item2, item3);
						object[] objArray1 = new object[] { args, this };
						Event.RaiseEvent("item:bucketing:duplicated", objArray1);
						Log.Info(string.Concat("Item ", item2.ID, " has been duplicated to another bucket"), this);
						result = item2;
					}
				}
			}
			else
			{
				SheerResponse.Alert(Translate.Text("Item not found."), new string[0]);
				args.AbortPipeline();
			}
			args.AbortPipeline();

			return result;
		}

		internal static Item CreateAndReturnDateFolderDestination(Item topParent, DateTime childItemCreationDateTime, ID itemToMove)
		{
			return BucketManager.Provider.CreateAndReturnDateFolderDestination(topParent, childItemCreationDateTime, itemToMove);
		}

		internal static bool IsBucketTemplateCheck(Item item)
		{
			TemplateItem template;
			if (item != null)
			{
				if (item.Fields[Sitecore.Buckets.Util.Constants.IsBucket] != null)
				{
					return item.Fields[Sitecore.Buckets.Util.Constants.BucketableField].Value.Equals("1");
				}
				if (item.Paths.FullPath.StartsWith("/sitecore/templates"))
				{
					if (item.Children[0] != null)
					{
						template = item.Children[0].Template;
					}
					else
					{
						template = null;
					}
					TemplateItem templateItem = template;
					if (templateItem != null)
					{
						TemplateItem templateItem1 = new TemplateItem(templateItem);
						if (templateItem.StandardValues != null && templateItem1.StandardValues[Sitecore.Buckets.Util.Constants.BucketableField] != null)
						{
							return templateItem1.StandardValues[Sitecore.Buckets.Util.Constants.BucketableField].Equals("1");
						}
					}
				}
			}
			return false;
		}
	}
}
