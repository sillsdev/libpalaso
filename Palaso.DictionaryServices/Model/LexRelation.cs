using System.Collections.Generic;
using System.ComponentModel;
using Palaso.Lift;
using Palaso.UiBindings;

namespace Palaso.DictionaryServices.Model
{
	public class LexRelationType
	{
		public enum Multiplicities
		{
			One,
			Many
		}

		public enum TargetTypes
		{
			Entry,
			Sense
		}

		private readonly string _id;
		private readonly Multiplicities _multiplicity;
		private readonly TargetTypes _targetType;

		public LexRelationType(string id, Multiplicities multiplicity, TargetTypes targetType)
		{
			_id = id;
			_targetType = targetType;
			_multiplicity = multiplicity;
		}

		public string ID
		{
			get { return _id; }
		}

		public Multiplicities Multiplicity
		{
			get { return _multiplicity; }
		}

		public TargetTypes TargetType
		{
			get { return _targetType; }
		}
	}

	public class LexRelation: IParentable,
							  IValueHolder<string>,
							  IReferenceContainer,
							  IReportEmptiness
	{
		//private LexRelationType _type;
		private string _fieldId;
		//private PalasoDataObject _target;
		private string _targetId;
		private PalasoDataObject _parent;
		//
		//        public LexRelation()
		//        {
		//        }

		public LexRelation(string fieldId, string targetId, PalasoDataObject parent)
		{
			_fieldId = fieldId;
			_targetId = targetId ?? string.Empty;
			_parent = parent;
		}

		/// <summary>
		/// Set to string.emtpy to clear the relation
		/// </summary>
		public string Key
		{
			get { return _targetId; }
			set { _targetId = value ?? string.Empty; }
		}

		//        public LexRelationType Type
		//        {
		//            get
		//            {
		//                return _type;
		//            }
		//            set
		//            {
		//                _type = value;
		//            }
		//        }

		#region IParentable Members

		public PalasoDataObject Parent
		{
			set { _parent = value; }
		}

		public string FieldId
		{
			get { return _fieldId; }
			set { _fieldId = value; }
		}

		#region IReferenceContainer Members

		public string TargetId
		{
			get { return _targetId; }
			set
			{
				if (value == TargetId)
				{
					return;
				}

				if (value == null)
				{
					_targetId = string.Empty;
				}
				else
				{
					_targetId = value;
				}
				NotifyPropertyChanged();
			}
		}

		#endregion

		#endregion

		private void NotifyPropertyChanged()
		{
			//tell any data binding. These would update the display of this data.
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs("relation"));
			}

			//tell our parent, which then handles getting us saved eventually
			if (_parent != null)
			{
				_parent.NotifyPropertyChanged("relation");
			}
		}

		#region IReportEmptiness Members

		public bool ShouldHoldUpDeletionOfParentObject
		{
			get { return false; }
		}

		public void RemoveEmptyStuff()
		{
			//nothing to do...
		}

		public bool ShouldCountAsFilledForPurposesOfConditionalDisplay
		{
			get { return !string.IsNullOrEmpty(Key); }
		}

		public bool ShouldBeRemovedFromParentDueToEmptiness
		{
			get { return string.IsNullOrEmpty(Key); }
		}

		public string Value
		{
			get { return TargetId; }
			set { TargetId = value; }
		}

		#endregion

		#region INotifyPropertyChanged Members

		///<summary>
		///Occurs when a property value changes.
		///</summary>
		///
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}

	public class LexRelationCollection: IParentable, IReportEmptiness
	{
		private PalasoDataObject _parent;
		private List<LexRelation> _relations = new List<LexRelation>();

		#region IParentable Members

		public PalasoDataObject Parent
		{
			set { _parent = value; }
			get { return _parent; }
		}

		#endregion

		public List<LexRelation> Relations
		{
			get { return _relations; }
			set { _relations = value; }
		}

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

		public bool ShouldHoldUpDeletionOfParentObject
		{
			get { return false; //don't hold up deleting just because of these
			}
		}

		public bool ShouldCountAsFilledForPurposesOfConditionalDisplay
		{
			get
			{
				foreach (LexRelation relation in _relations)
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
				foreach (LexRelation relation in _relations)
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
			List<LexRelation> condemed = new List<LexRelation>();
			foreach (LexRelation relation in _relations)
			{
				if (relation.ShouldBeRemovedFromParentDueToEmptiness)
				{
					condemed.Add(relation);
				}
			}

			foreach (LexRelation relation in condemed)
			{
				_relations.Remove(relation);
			}
		}

		#endregion
	}
}