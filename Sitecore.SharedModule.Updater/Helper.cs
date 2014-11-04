


namespace Sitecore.SharedModule.Updater
{
	using Sitecore.Data;
	using Sitecore.Data.Items;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	/// <summary>
	/// Helper class contains extentions.
	/// </summary>
	static class Helper
	{
		/// <summary>
		/// Replace string with ignore case option.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		/// <param name="ignoreCase"></param>
		/// <returns></returns>
		public static string Replace(this string source, string oldValue, string newValue, bool ignoreCase)
		{
			if (String.IsNullOrEmpty(source))
				return source;

			if (!ignoreCase)
				return source.Replace(oldValue, newValue);
			else
			{
				oldValue = oldValue.Replace("[", @"\[").Replace("]", @"\]");
				var regex = new Regex(oldValue, RegexOptions.IgnoreCase);
				return regex.Replace(source, newValue);
			}
		}

		public static StringBuilder Replace(this StringBuilder source, string oldValue, string newValue, bool ignoreCase)
		{
			return new StringBuilder(Replace(source.ToString(), oldValue, newValue, ignoreCase));
		}
		/// <summary>
		/// If null return false or any.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <returns></returns>
		public static bool OrAny<T>(this IEnumerable<T> list)
		{
			if (list != null && list.Count() > 0)
				return list.Any();
			return false;
		}

		/// <summary>
		/// ID extenstion to get item.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="db"></param>
		/// <returns></returns>
		public static Item GetItem(this ID id, Database db)
		{
			if (id.OrEmpty() != string.Empty)
				return db.GetItem(id);
			return null;
		}

		/// <summary>
		/// String to ID
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static ID OrID(this object s)
		{
			if (s.OrEmpty() != string.Empty)
			{

				if (ShortID.IsShortID(s.ToString()))
				{
					return (new ShortID(s.ToString())).ToID();
				}


				if (ID.IsID(s.ToString()))
				{
					return (new ID(s.ToString()));
				}
			}

			return ID.Null;
		}
		/// <summary>
		/// converts object to string,If null return empty.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string OrEmpty(this object s)
		{
			if (s != null)
				return s.ToString();
			return string.Empty;
		}
	}
}
