using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Lift;

namespace SIL.DictionaryServices.Model
{
	public class LexRelationCollection: IPalasoDataObjectProperty, IReportEmptiness
	{
		#region IParentable Members

		public PalasoDataObject Parent { set; get; }

		#endregion

		public List<LexRelation> Relations { get; private set; } = new List<LexRelation>();

		//
		//        public bool IsEmpty
		//        {
		//            get
		//            {
		//                foreach (LexRelation relation in _relations)
		//                {
		//                    if (!relation.ShouldCountAsFilledForPurposesOfConditionalDisplay)
		//                    {
		//                        return false;
		//                    }
		//                }
		//                return true;
		//            }
		//        }

		#region IReportEmptiness Members

		public bool ShouldHoldUpDeletionOfParentObject => false; //don't hold up deleting just because of these

		public bool ShouldCountAsFilledForPurposesOfConditionalDisplay
		{
			get
			{
				foreach (LexRelation relation in Relations)
				{
					if (relation.ShouldCountAsFilledForPurposesOfConditionalDisplay)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool ShouldBeRemovedFromParentDueToEmptiness
		{
			get
			{
				//if we can find one child that thinks he is non-empty, then we too should stick around
				foreach (LexRelation relation in Relations)
				{
					if (!relation.ShouldBeRemovedFromParentDueToEmptiness)
					{
						return false;
					}
				}
				return true;
			}
		}

		public void RemoveEmptyStuff()
		{
			//we do this in two passes because you can't remove items from a collection you are iterating over
			List<LexRelation> condemned = new List<LexRelation>();
			foreach (LexRelation relation in Relations)
			{
				if (relation.ShouldBeRemovedFromParentDueToEmptiness)
				{
					condemned.Add(relation);
				}
			}

			foreach (LexRelation relation in condemned)
			{
				Relations.Remove(relation);
			}
		}

		#endregion

		public IPalasoDataObjectProperty Clone()
		{
			var clone = new LexRelationCollection();
			clone.Relations = new List<LexRelation>(Relations);
			return clone;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as LexRelationCollection);
		}

		public virtual bool Equals(IPalasoDataObjectProperty other)
		{
			return Equals(other as LexRelationCollection);
		}

		public bool Equals(LexRelationCollection other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (!Relations.SequenceEqual(other.Relations)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// For this class we want a hash code based on the the object's reference so that we
			// can store and retrieve the object in the LiftLexEntryRepository. However, this is
			// not ideal and Microsoft warns: "Do not use the hash code as the key to retrieve an
			// object from a keyed collection."
			// https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode?view=netframework-4.8#remarks
			return base.GetHashCode();
		}
	}
}