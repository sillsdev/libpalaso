using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SIL.Lift;
using SIL.UiBindings;

namespace SIL.DictionaryServices.Model
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

		public LexRelationType(string id, Multiplicities multiplicity, TargetTypes targetType)
		{
			ID = id;
			TargetType = targetType;
			Multiplicity = multiplicity;
		}

		public string ID { get; }

		public Multiplicities Multiplicity { get; }

		public TargetTypes TargetType { get; }
	}

	public class LexRelation: IPalasoDataObjectProperty,
							  IValueHolder<string>,
							  IReferenceContainer,
							  IReportEmptiness,
								IExtensible
	{

		public List<string> EmbeddedXmlElements = new List<string>();

		//private LexRelationType _type;
		//private PalasoDataObject _target;
		private string _targetId;
		private PalasoDataObject _parent;
		//
		//        public LexRelation()
		//        {
		//        }

		public LexRelation(string fieldId, string targetId, PalasoDataObject parent)
		{
			FieldId = fieldId;
			_targetId = targetId ?? string.Empty;
			_parent = parent;

			Traits = new List<LexTrait>();
			Fields = new List<LexField>();
		}

		/// <summary>
		/// Set to string.emtpy to clear the relation
		/// </summary>
		public string Key
		{
			get => _targetId;
			set => _targetId = value ?? string.Empty;
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
			set => _parent = value;
		}

		public string FieldId { get; set; }

		#region IReferenceContainer Members

		public string TargetId
		{
			get => _targetId;
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
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("relation"));

			//tell our parent, which then handles getting us saved eventually
			_parent?.NotifyPropertyChanged("relation");
		}

		#region IReportEmptiness Members

		public bool ShouldHoldUpDeletionOfParentObject => false;

		public void RemoveEmptyStuff()
		{
			//nothing to do...
		}

		public bool ShouldCountAsFilledForPurposesOfConditionalDisplay => !string.IsNullOrEmpty(Key);

		public bool ShouldBeRemovedFromParentDueToEmptiness => string.IsNullOrEmpty(Key);

		public string Value
		{
			get => TargetId;
			set => TargetId = value;
		}

		#endregion

		#region INotifyPropertyChanged Members

		///<summary>
		///Occurs when a property value changes.
		///</summary>
		///
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Implementation of IExtensible

		public List<LexTrait> Traits { get; private set; }
		public List<LexField> Fields { get; private set; }

		#endregion

		public IPalasoDataObjectProperty Clone()
		{
			var clone = new LexRelation(FieldId, _targetId, null);
			clone.EmbeddedXmlElements = new List<string>(EmbeddedXmlElements);
			clone.Traits.AddRange(Traits.Select(t => t.Clone()));
			clone.Fields.AddRange(Fields.Select(f => (LexField)f.Clone()));
			return clone;
		}

		public override bool Equals(object other)
		{
			return Equals(other as LexRelation);
		}

		public bool Equals(IPalasoDataObjectProperty other)
		{
			return Equals(other as LexRelation);
		}

		public bool Equals(LexRelation other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (!EmbeddedXmlElements.SequenceEqual(other.EmbeddedXmlElements)) return false;
			if ((FieldId != null && !FieldId.Equals(other.FieldId)) || (other.FieldId != null && !other.FieldId.Equals(FieldId))) return false;
			if ((_targetId != null && !_targetId.Equals(other._targetId)) || (other._targetId != null && !other._targetId.Equals(_targetId))) return false;
			if (!Traits.SequenceEqual(other.Traits)) return false;
			if (!Fields.SequenceEqual(other.Fields)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 19;
				hash *= 23 + EmbeddedXmlElements.GetHashCode();
				hash *= 23 + FieldId.GetHashCode();
				hash *= 23 + _targetId.GetHashCode();
				hash *= 23 + Traits.GetHashCode();
				hash *= 23 + Fields.GetHashCode();
				return hash;
			}
		}
	}

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
			List<LexRelation> condemed = new List<LexRelation>();
			foreach (LexRelation relation in Relations)
			{
				if (relation.ShouldBeRemovedFromParentDueToEmptiness)
				{
					condemed.Add(relation);
				}
			}

			foreach (LexRelation relation in condemed)
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

		public virtual bool Equals(IPalasoDataObjectProperty other)
		{
			return Equals((object)other);
		}

		public override bool Equals(Object obj)
		{
			if (obj == null) return false;
			if (obj.GetType() != typeof(LexRelationCollection)) return false;
			return Equals((LexRelationCollection)obj);
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 19;
				hash *= 59 + Relations.GetHashCode();
				return hash;
			}
		}

		public bool Equals(LexRelationCollection other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (!Relations.SequenceEqual(other.Relations)) return false;
			return true;
		}
	}
}