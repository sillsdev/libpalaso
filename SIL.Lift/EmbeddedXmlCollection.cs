// Copyright (c) 2009-2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIL.Lift
{
	public class EmbeddedXmlCollection: IPalasoDataObjectProperty
	{
		private List<string>     _values;
		private PalasoDataObject _parent;

		public EmbeddedXmlCollection()
		{
			_values = new List<string>();
		}

		public PalasoDataObject Parent
		{
			set => _parent = value;
		}

		public List<string> Values
		{
			get => _values;
			set => _values = value;
		}

		public IPalasoDataObjectProperty Clone()
		{
			var clone = new EmbeddedXmlCollection();
			clone._values.AddRange(_values);
			return clone;
		}

		public override bool Equals(object other)
		{
			return Equals(other as EmbeddedXmlCollection);
		}

		public bool Equals(IPalasoDataObjectProperty other)
		{
			return Equals(other as EmbeddedXmlCollection);
		}

		public bool Equals(EmbeddedXmlCollection other)
		{
			if (other == null) return false;
			if (!_values.SequenceEqual(other._values)) return false; //order is relevant
			return true;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 47;
				hash *= 61 + _values?.GetHashCode() ?? 0;
				return hash;
			}
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			foreach (var part in Values)
			{
				builder.Append(part.ToString() + " ");
			}
			return builder.ToString().Trim();
		}
	}
}