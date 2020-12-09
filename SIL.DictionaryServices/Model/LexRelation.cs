using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SIL.Lift;
using SIL.UiBindings;

namespace SIL.DictionaryServices.Model
{
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
			// For this class we want a hash code based on the the object's reference so that we
			// can store and retrieve the object in the LiftLexEntryRepository. However, this is
			// not ideal and Microsoft warns: "Do not use the hash code as the key to retrieve an
			// object from a keyed collection."
			// https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode?view=netframework-4.8#remarks
			return base.GetHashCode();
		}
	}
}