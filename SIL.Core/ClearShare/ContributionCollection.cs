using System.Collections.Generic;
using System.Text;

namespace SIL.Core.ClearShare
{	/// ----------------------------------------------------------------------------------------
	public interface IAutoCompleteValueProviderWeird
	{
		/// ------------------------------------------------------------------------------------
		string GetValueForKey(string key);
	}

	/// ----------------------------------------------------------------------------------------
	public interface IAutoCompleteValueProvider
	{
		/// ------------------------------------------------------------------------------------
		IEnumerable<string> GetValuesForKey(string key);
	}

	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This class is an ordinary generic list of Contribution objects but also implements
	/// the IAutoCompleteValueProvider, which allows it to deliver a customized string of
	/// contributors for the auto-complete system.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class ContributionCollection : List<Contribution>, IAutoCompleteValueProviderWeird
	{
		/// ------------------------------------------------------------------------------------
		public ContributionCollection()
		{
		}

		/// ------------------------------------------------------------------------------------
		public ContributionCollection(IEnumerable<Contribution> collection)
		{
			AddRange(collection);
		}

		#region IAutoCompleteValueProvider implementation
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This method will provide the auto-complete value gatherer a string of contributor
		/// names to store for name fields' auto-complete lists. Since the gatherer has the
		/// ability to parse a single string containing delimited names, we'll go through
		/// all the contributions in our list and return a single string containing all names
		/// delimited with a semicolon. However, if the particular key passed to us is not
		/// for contributions, then return null.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string GetValueForKey(string key)
		{
			if (key != "contributions")
				return null;

			var bldr = new StringBuilder();
			foreach (var c in this)
				bldr.AppendFormat("{0}; ", c.ContributorName);

			if (bldr.Length == 0)
				return null;

			bldr.Length -= 2;
			return bldr.ToString();
		}

		#endregion
	}
}
