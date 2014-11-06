using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundry.Pipelines;
using Sitecore.Foundry.Pipelines.Sites.CreateSite;
using System.Collections.Generic;

namespace Sitecore.SharedModule.ReferenceUpdater.Foundry.Processors
{
	public class ReferenceUpdaterProcessor: CreateSiteProcessor, IRevertedProcessor<SitePipelineArgs>
	{
		private Item _source;
		private Item _target;
		private Item _sourceMedia;
		private Item _targetMedia;


		protected override string Message
		{
			get
			{
				return "Reference Replacement. ";
			}
		}
		public override void DoProcess(SitePipelineArgs args)
		{
			_source = args.SiteType.SiteTemplate.ItemFromContentDatabase;
			_target = args.Site.RootItem;
			_sourceMedia = args.SiteType.MediaLibraryTemplateItem;
			_targetMedia = args.Site.MediaLibrary.Item;

			Assert.ArgumentNotNull(_source, "source");
			Assert.ArgumentNotNull(_target, "target");

			Dictionary<string, string> roots = new Dictionary<string, string>();

			if (_source != null && _target != null)
				roots.Add(_source.Paths.Path, _target.Paths.Path);

			if (_sourceMedia != null && _targetMedia != null)
				roots.Add(_sourceMedia.Paths.Path, _targetMedia.Paths.Path);

			ReferenceUpdater refUpdater = new ReferenceUpdater(_target, roots, true);
				refUpdater.Start();

		}

		//Default Constructor
		public ReferenceUpdaterProcessor()
		{
		}

		public void Revert(SitePipelineArgs args)
		{
			//throw new System.NotImplementedException();
		}
	}
}
