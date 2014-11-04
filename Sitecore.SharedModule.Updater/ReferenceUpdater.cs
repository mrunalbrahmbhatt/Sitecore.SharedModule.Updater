
/*
 This class will updates references of given items, with respect to given source(s) and destination(s).
 * 
 * Case coverd : Copy, Clone, Duplicate, Add from template, Item from branch.
 * 
 * Inspired from : http://reasoncodeexample.com/2013/01/13/changing-sitecore-item-references-when-creating-copying-duplicating-and-cloning/
 * 
 * Reasons to create :We implemented above solution but as number of site increate we feel slowness in whole environment while copying site.
 * Thus we need soluion which should be very fast,smart and supports multiple sources that makes it flexible for almost all scenario.
 * 
 * Fast and Smart : It is not using GetDecendent Api which is very expensive API in case of site copy in Foundry or duplicating whole site.
 * It only scans destination items(s) to process and only focuses on items which needs update/replacable means it calculates once if item is ignored then it will be ignoree for other items too to save time.
 * 
 * Multiple Source : In Sitecore foundry while creating site from other site type, we need multiple source like Site Root, Site Media Root to update references,
 * this can be applicable to any multi site solution.
 * 
 */

namespace Sitecore.SharedModule.Updater
{

	using Sitecore.Data;
	using Sitecore.Data.Fields;
	using Sitecore.Data.Items;
	using Sitecore.Diagnostics;
	using Sitecore.SecurityModel;
	using Sitecore.Xml.Patch;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;

	public class ReferenceUpdater
	{
		private Database _master = null;
		private Item _start = null;
		//List Items to process and can be replacable.
		private Dictionary<string, EqualItems> _result = new Dictionary<string, EqualItems>();
		private Dictionary<string, string> _roots = new Dictionary<string, string>();
		//List of ignored/excluded item e.g. device id or any id which is not required for replacement process. Also gains performance.
		private List<string> _ignoreList = new List<string>();
		private bool _deep = false;
		
		public int Count = 0;

	
		public string TimeTaken { get; set; }
		private Func<Field, bool> FieldFilter { get; set; }
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="start">Context Item/</param>
		/// <param name="roots"></param>
		/// <param name="deep"></param>
		public ReferenceUpdater(Item start, Dictionary<string, string> roots, bool deep = false)
		{
			_start = start;
			_roots = roots;
			_deep = deep;
			Count = 0;
		}

		/// <summary>
		/// Filter to exclude standard fields except Layout field.
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		private bool ExcludeStandardSitecoreFieldsExceptLayout(Field field)
		{
			Assert.ArgumentNotNull(field, "field");
			//for Sitecore 8 we can add final rendering field.
			return field.ID == FieldIDs.LayoutField || !field.Name.StartsWith("__");
		}
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="start">Starting Item from where reference update process</param>
		/// <param name="roots"> List multiple root. e.g. Soure->Destination, Source Media Library -> Destination,etc in case of multiple sites.</param>
		/// <param name="deep">Traverse all children under selected start item.</param>
		

		/// <summary>
		/// Start Reference Update process.
		/// </summary>
		public void Start()
		{
			Stopwatch stopWatch = new Stopwatch();

			stopWatch.Start();

			if (FieldFilter == null)
			{
				//Default if not supplied from outside.
				FieldFilter = ExcludeStandardSitecoreFieldsExceptLayout;
			}

			if (_start != null && _roots != null)
			{
				_master = _start.Database;
				FixReferences(_start, _roots, _deep);
			}

			stopWatch.Stop();

			// Get the elapsed time as a TimeSpan value.
			TimeSpan ts = stopWatch.Elapsed;
			// Format and display the TimeSpan value. 
			string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
				ts.Hours, ts.Minutes, ts.Seconds,
				ts.Milliseconds / 10);

			TimeTaken = elapsedTime;
		}


		private void FixReferences(Item start, Dictionary<string, string> roots, bool deep = false)
		{
			Count++;
			ScanItem(start);
			if (deep && start.HasChildren)
			{
				var childrens = start.Children;
				foreach (Item c in childrens)
				{
					FixReferences(c, roots, deep);
				}
			}
		}
		/// <summary>
		/// Scan Item 
		/// </summary>
		/// <param name="contextItem"></param>
		private void ScanItem(Item contextItem)
		{
			if (contextItem == null)
				return;
			IEnumerable<Field> fields = GetFieldsToProcess(contextItem);
			foreach (Field field in fields)
			{
				foreach (Item itemVersion in GetVersionsToProcess(contextItem))
				{
					Field itemVersionField = itemVersion.Fields[field.ID];
					//Process field
					ProcessField(itemVersionField);
				}
			}
		}

		public void ReplaceItemReferences(Item item)
		{
			IEnumerable<Field> fields = GetFieldsToProcess(item);
			foreach (Field field in fields)
			{
				foreach (Item itemVersion in GetVersionsToProcess(item))
				{
					Field itemVersionField = itemVersion.Fields[field.ID];
					ProcessField(itemVersionField);
				}
			}
		}
		private string GetInitialFieldValue(Field field)
		{
			return field.GetValue(true, true);
		}

		/// <summary>
		/// Grab from sitecore dll.
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		private static string GetLayoutFieldValue(Field field)
		{
			string value = field.GetValue(true, true);
			Func<Field, string> func = XmlDeltas.WithEmptyValue("<r />");
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}
			if (!XmlPatchUtils.IsXmlPatch(value))
			{
				return value;
			}
			return XmlDeltas.ApplyDelta(func(field), value);
		}

		/// <summary>
		/// Process single field for Guid with different format and path, finally replace with equivalent destination item.
		/// </summary>
		/// <param name="field"></param>
		private void ProcessField(Field field)
		{
			string initialValue = string.Empty;
			string fieldValue = string.Empty;
			if (field.ID != FieldIDs.LayoutField)
			{
				initialValue = GetInitialFieldValue(field);
			}
			else
			{
				//Special Case.
				//Full Starndard + Delta.
				fieldValue = field.GetValue(true, true);
				//Patched Value
				initialValue = GetLayoutFieldValue(field);
			}

			if (string.IsNullOrEmpty(initialValue))
				return;

			//Get Keys : Ids and Paths
			var keys = GetKeys(initialValue);

			//Checks keys needs to be ignored otherwise equivalent item exist, if yes prepare list of equal items other wise add in ignore.
			GetReplaceableList(keys);

			//Replace with equivaent item list.
			if (_result.OrAny())
			{
				StringBuilder value = new StringBuilder(initialValue);
				foreach (var r in _result)
				{

					value = value.Replace(r.Value.Source.ID.Guid.ToString("D").ToUpper(), r.Value.Dest.ID.Guid.ToString("D").ToUpper());
					value = value.Replace(r.Value.Source.ID.Guid.ToString("D").ToLower(), r.Value.Dest.ID.Guid.ToString("D").ToLower());
					value = value.Replace(r.Value.Source.ID.Guid.ToString("N").ToUpper(), r.Value.Dest.ID.Guid.ToString("N").ToUpper());
					value = value.Replace(r.Value.Source.ID.Guid.ToString("N").ToLower(), r.Value.Dest.ID.Guid.ToString("N").ToLower());
					value = value.Replace(r.Value.Source.Paths.Path, r.Value.Dest.Paths.Path, true);
					value = value.Replace(r.Value.Source.Paths.Path.ToLower(), r.Value.Dest.Paths.Path.ToLower(), true);
					if (r.Value.Source.Paths.IsContentItem)
					{
						value.Replace(r.Value.Source.Paths.ContentPath, r.Value.Dest.Paths.ContentPath);
						value.Replace(r.Value.Source.Paths.ContentPath.ToLower(), r.Value.Dest.Paths.ContentPath.ToLower());
					}
				}

				//Special care taken to handler Standard Value and Delta of layout , thus to maintain inheritance otherwise it will break inhertiance
				// and any change to template will not reflect in derived pages.

				if (field.ID == FieldIDs.LayoutField)
				{

					//Difference of old value and new updated value, thus inheritance remain in place.
					//Try to get new patch.
					initialValue = XmlDeltas.ApplyDelta(fieldValue, initialValue);
				}
				UpdateFieldValue(field, initialValue, value);
			}
		}

		/// <summary>
		/// Updates field value with new processed.
		/// </summary>
		/// <param name="field"></param>
		/// <param name="initialValue"></param>
		/// <param name="value"></param>
		private void UpdateFieldValue(Field field, string initialValue, StringBuilder value)
		{
			if (initialValue.Equals(value.ToString()))
				return;
			using (new SecurityDisabler())
			{
				using (new EditContext(field.Item))
				{
					field.Value = value.ToString();
				}
			}
		}
		/// <summary>
		/// Prepares list of equivalent and replacable items.
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		private Dictionary<string, EqualItems> GetReplaceableList(List<string> keys)
		{
			foreach (string key in keys)
			{
				if (!_ignoreList.Contains(key))
				{
					if (!_result.ContainsKey(key))
					{
						Item kItem = GetItemFromString(key);
						if (kItem != null)
						{
							if (IsReplaceable(kItem.Paths.Path))
							{
								Item equalItem = GetEquivalentItem(kItem.Paths.Path);

								if (equalItem != null)
								{
									_result.Add(key, new EqualItems() { Source = kItem, Dest = equalItem });
									continue;
								}
							}
						}
						_ignoreList.Add(key);
					}
				}
			}

			return _result;
		}

		/// <summary>
		/// Gets real Item from ID or Path
		/// </summary>
		/// <param name="idOrPath"></param>
		/// <returns></returns>
		private Item GetItemFromString(string idOrPath)
		{
			if (!string.IsNullOrEmpty(idOrPath))
			{
				ID itemId = idOrPath.OrID();
				if (!itemId.IsNull)
					return itemId.GetItem(_master);

				return _master.GetItem(idOrPath.Trim('"'));

			}
			return null;
		}

		private IEnumerable<Item> GetVersionsToProcess(Item item)
		{
			return item.Versions.GetVersions(true);
		}
		
		//Reads all field of given item including Clone items.
		private IEnumerable<Field> GetFieldsToProcess(Item item)
		{
			item.Fields.ReadAll();
			return item.Fields.Where(FieldFilter).ToArray();
		}

		/// <summary>
		/// Gets Ids and Paths from given xml/field value.
		/// </summary>
		/// <param name="fieldValue"></param>
		/// <returns></returns>
		private List<string> GetKeys(string fieldValue)
		{
			//Regex for Guid/Path
			string pattern = "([a-zA-Z0-9]{8}-[a-zA-Z0-9]{4}-[a-zA-Z0-9]{4}-[a-zA-Z0-9]{4}-[a-zA-Z0-9]{12})|([a-zA-Z0-9]{32})|(\"/.*?\")";
			List<string> keys = new List<string>();
			Regex keyRegex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
			MatchCollection foundKeys = keyRegex.Matches(fieldValue);
			if (foundKeys != null && foundKeys.Count > 0)
			{
				foreach (Match m in foundKeys)
				{
					if (!keys.Contains(m.Value))
						keys.Add(m.Value);
				}
			}
			return keys.Where(k => !string.IsNullOrEmpty(k)).Distinct().ToList();
		}

		//Check each root item if any matchs then is replacable otherwise ignored.
		private bool IsReplaceable(string path)
		{
			if (_roots.OrAny())
			{
				foreach (var root in _roots)
				{
					if (path.ToLower().Contains(root.Key.ToLower()))
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns equivalent item.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private Item GetEquivalentItem(string path)
		{
			if (_roots.OrAny())
			{
				foreach (var root in _roots)
				{
					if (path.ToLower().Contains(root.Key.ToLower()))
					{
						string equalPath = path.Replace(root.Key, root.Value, true);
						return _master.GetItem(equalPath);
					}
				}
			}
			return null;
		}
	}

	class EqualItems
	{
		public Item Source { get; set; }
		public Item Dest { get; set; }
	}
}
