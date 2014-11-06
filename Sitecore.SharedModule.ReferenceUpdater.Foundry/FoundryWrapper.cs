using Sitecore.Data.Items;
using Sitecore.Foundry.Sites;
using System.Collections.Generic;

namespace Sitecore.SharedModule.ReferenceUpdater.Foundry
{
	public class FoundryWrapper
	{
		public static Item GetHome()
		{
			var home = Sitecore.Context.Database.GetItem(Sitecore.Context.Site.StartPath);
			if (home != null)
				return home;
			return null;
		}

		/// <summary>
		/// Returns the Context Database as Foundry does not always return the correct Database
		/// </summary>
		/// <returns></returns>
		public static Sitecore.Data.Database GetContextDatabase()
		{
			if (Sitecore.Foundry.Configuration.Settings.Instance.LiveMode)
				return Sitecore.Foundry.Context.ContentDatabase;
			else
				return Sitecore.Foundry.Context.CurrentDatabase;
		}

		public static Dictionary<string, string> GetRoots(Item contextItem)
		{
			Dictionary<string, string> roots = new Dictionary<string, string>();
			var siteDetail = FoundryWrapper.GetSiteDetailFromItem(contextItem);
			if (contextItem != null && siteDetail.SiteContext != null)
			{
				if (siteDetail.SiteRoot != null && siteDetail.SiteTemplateRoot != null)
					roots.Add(siteDetail.SiteTemplateRoot.Paths.Path, siteDetail.SiteRoot.Paths.Path);

				if (siteDetail.SiteMediaRoot != null && siteDetail.SiteTemplateMediaRoot != null)
					roots.Add(siteDetail.SiteTemplateMediaRoot.Paths.Path, siteDetail.SiteMediaRoot.Paths.Path);
			}
			return roots;
		}


		public static Dictionary<string, string> GetRoots(Item contextItem, Item sourceItem)
		{
			SiteDetail dest = GetSiteDetailFromItem(contextItem);
			SiteDetail source = GetSiteDetailFromItem(sourceItem);

			Dictionary<string, string> roots = GetRoots(contextItem);

			if (source.SiteRoot != null && dest.SiteRoot != null)
			{
				if (source.SiteRoot.ID != dest.SiteRoot.ID)
				{
					roots.Add(source.SiteRoot.Paths.Path, dest.SiteRoot.Paths.Path);
				}

				if (source.SiteMediaRoot.ID != dest.SiteMediaRoot.ID)
				{
					roots.Add(source.SiteMediaRoot.Paths.Path, dest.SiteMediaRoot.Paths.Path);
				}
			}

			roots.Add(sourceItem.Paths.Path, contextItem.Paths.Path);

			return roots;
		}

		public static SiteDetail GetSiteDetailFromItem(Item i)
		{
			SiteDetail sd = new SiteDetail();

			var siteContext = Sitecore.Foundry.Context.Server.Sites.GetSite(i);
			if (siteContext != null)
			{
				sd.currentItem = i;
				sd.SiteContext = siteContext;
				sd.SiteRoot = siteContext.RootItem;
				sd.SiteMediaRoot = siteContext.MediaLibrary.Item;

				var siteType = siteContext.SiteType;
				if (siteType != null)
				{
					if (siteType.SiteTemplate != null && siteType.SiteTemplate.ItemFromContentDatabase != null)
					{

						sd.SiteTemplateRoot = siteType.SiteTemplate.ItemFromContentDatabase;
						sd.SiteTemplateMediaRoot = siteType.MediaLibraryTemplateItem;
					}
				}
			}
			return sd;
		}
	}

	public class SiteDetail
	{
		public Item currentItem { get; set; }

		public SiteContext SiteContext { get; set; }

		public Item SiteTemplateRoot { get; set; }
		public Item SiteTemplateMediaRoot { get; set; }

		public Item SiteRoot { get; set; }
		public Item SiteMediaRoot { get; set; }
	}
}
