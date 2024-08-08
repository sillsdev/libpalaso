using System.Drawing;
using SIL.Core.ClearShare;

namespace SIL.Windows.Forms.ClearShare
{
	/// <summary>
	/// Describes a single license, under which many works can be licensed for use
	/// </summary>
	public class LicenseWithLogo : License
	{
		public Image Logo { get; private set; }

		/// ------------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			unchecked
			{
				int result = base.GetHashCode();
				result = (result * 397) ^ (Logo != null ? Logo.GetHashCode() : 0);
				return result;
			}
		}

		/// ------------------------------------------------------------------------------------
		public override bool Equals(object other)
		{
			if (other == null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			if (GetType() != other.GetType())
				return false;

			return AreContentsEqual(other as License);
		}
	}
}
