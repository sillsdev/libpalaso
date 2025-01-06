using System.Collections.Generic;

namespace SIL.Core.ClearShare
{
	/// <summary>
	/// The word "work" in isolation doesn't suggest the right sense, here. Think of "derived-work" or "work of fiction".
	///
	/// Typically an application would assign a single "Work" object to each document.
	/// </summary>
	public class Work
	{
		public Work()
		{
			Contributions = new List<Contribution>();
			Licenses = new List<License>();
		}

		/// <summary>
		/// Most works will only have a single license, but some (e.g. software) may well be dual-licensed.
		/// </summary>
		public List<License> Licenses { get; private set;}

		/// <summary>
		/// Who did what, when?
		/// </summary>
		public List<Contribution> Contributions { get; private set; }


		//enhance: in one version of the design, a work had sub-works.
	}
}
