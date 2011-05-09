using System;

namespace Palaso.UI.WindowsForms.ClearShare
{
	/// <summary>
	/// Records a single contribution of a single individual to a single "work".
	/// If the person performed another role, they get another contribution record.
	/// </summary>
	public sealed class Contribution : ICloneable
	{
		/// <summary>
		/// This will sometimes come from a controlled vocabulary of known people,
		/// but we don't need to model that here
		/// </summary>
		public string ContributorName { get; set; }

		/// <summary>
		/// The choices for this will come from the owning Work,
		/// but we don't model that here.
		/// </summary>
		public License ApprovedLicense{ get; set; }

		/// <summary>
		/// This will sometimes come from a controlled vocabulary, but that must
		/// be ensured elsewhere
		/// </summary>
		public Role Role { get; set; }

		/// <summary>
		/// The date the contribution was made. Seems like it would be rarely used,
		/// but an early SayMore user asked for this specifically.
		/// </summary>
		/// <remarks>olac would like us to control the format of this, but we would either
		/// * need UI which can be flexible (just a year, or a date), or
		/// * parse the string from the UI in such a way that we get it into a standard
		/// format
		/// </remarks>
		public string Date { get; set; }

		/// <summary>
		/// Normally a short note about the contribution (e.g., a more specific description
		/// of the role played)
		/// </summary>
		public string Comments { get; set; }

		private readonly int _hashCode;

		/// ------------------------------------------------------------------------------------
		public Contribution()
		{
			_hashCode = Guid.NewGuid().GetHashCode();
		}

		/// ------------------------------------------------------------------------------------
		public Contribution(string name, Role role) : this()
		{
			ContributorName = name;
			Role = role;
		}

		/// ------------------------------------------------------------------------------------
		public bool IsEmpty
		{
			get
			{
				return (string.IsNullOrEmpty(ContributorName) && Role == null &&
					ApprovedLicense == null && string.IsNullOrEmpty(Date) &&
					string.IsNullOrEmpty(Comments));
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Call this sparingly since it has to create an OlacSystem each time it is invoked.
		/// This returns true if Role is successfully set. Otherwise, false.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool SetRoleByCode(string roleCode)
		{
			if (roleCode != null)
			{
				Role role;
				if ((new OlacSystem()).TryGetRoleByCode(roleCode, out role))
				{
					Role = role;
					return true;
				}
			}

			return false;
		}

		/// ------------------------------------------------------------------------------------
		public object Clone()
		{
			// TODO: Deal with license if necessary.
			return new Contribution(ContributorName,
				(Role != null ? Role.Clone() : null)) { Date = Date, Comments = Comments };
		}

		/// ------------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return _hashCode;
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

			return AreContentsEqual(other as Contribution);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns true if the contents of this Contribution are the same as those of the
		/// specified Contribution.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool AreContentsEqual(Contribution other)
		{
			if (other == null)
				return false;

			bool rolesEqual = ((Role == null && other.Role == null) ||
				(Role != null && Role.Equals(other.Role)));

			bool licensesEqual = ((ApprovedLicense == null && other.ApprovedLicense == null) ||
				(ApprovedLicense != null && ApprovedLicense.Equals(other.ApprovedLicense)));

			return (rolesEqual && licensesEqual &&
				ContributorName == other.ContributorName &&
				Date == other.Date && Comments == other.Comments);
		}

		/// ------------------------------------------------------------------------------------
		public override string ToString()
		{
			return ContributorName;
		}
	}
}
