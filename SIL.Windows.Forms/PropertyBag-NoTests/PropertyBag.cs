/**********************************************************************************************
 * Project: Our Word!
 * File:    PropertyBag.cs
 * Author:  John Wimbish & Others as Noted Below
 * Created: 20 Aug 2007
 * Purpose: Make a PropertyGrid usable (e.g., localizable, etc).
 * Legal:   Copyright (c) 2004-08, John S. Wimbish. All Rights Reserved.
 *
 * The code, unless otherwise noted:
 *    Copyright (C) 2002  Tony Allowatt
 *    Last Update: 12/14/2002
 *    From CodeProject.com article: "Bending the .NET PropertyGrid to Your Will"
 *        www.codeproject.com/cs/miscctrl/bending_property.asp
 *
 * The ability to pass in an array of strings for an enum:
 *    Edited by David Thielen - 05/27/2007
 *    From his blog at www.davidthielen.info/programming/2007/05/passing_an_enum.html
 *
 * JSW: I've added "ID", so that "name" and "description" can be localized. GetValue and
 *    SetValue should thus use "ID", rather than "name" as in Tony's article.
 *
 *********************************************************************************************/
#region Header: Using, etc.

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;

#endregion

namespace SIL.Windows.Forms
{
	public class PropertySpec
	{
		// Attrs -----------------------------------------------------------------------------
		#region Attr{g}: string ID - Unlike "name", this isn't changing due to localizations
		public string ID
		{
			get
			{
				Debug.Assert(!string.IsNullOrEmpty(m_sID));
				return m_sID;
			}
		}
		private readonly string m_sID;
		#endregion
		#region Attr{g/s}: string Name - Gets or sets the name of this property.
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}
		private string name;
		#endregion
		#region Attr{g/s}: object DefaultValue - the default value for this property
		public object DefaultValue
		{
			get
			{
				return defaultValue;
			}
			set
			{
				defaultValue = value;
			}
		}
		private object defaultValue;
		#endregion
		#region Attr{g/s}: string Description - the help text description of this property.
		public string Description
		{
			get
			{
				return description;
			}
			set
			{
				description = value;
			}
		}
		private string description;
		#endregion
		#region Attr{g/s}: string Category - the category name of this property.
		public string Category
		{
			get
			{
				return category;
			}
			set
			{
				category = value;
			}
		}
		private string category;
		#endregion
		#region Attr{g}: string[] EnumValues - Allowed values if an enum.
		public string[] EnumValues
		{
			get
			{
				return enumValues;
			}
			protected set
			{
				enumValues = value;
			}
		}
		private string[] enumValues;
		#endregion

		#region Attr{g/s}: bool DontLocalizeName
		public bool DontLocalizeName
		{
			get
			{
				return m_bDontLocalizeName;
			}
			set
			{
				m_bDontLocalizeName = value;
			}
		}
		bool m_bDontLocalizeName = false;
		#endregion
		#region Attr{g/s}: bool DontLocalizeCategory
		public bool DontLocalizeCategory
		{
			get
			{
				return m_bDontLocalizeCategory;
			}
			set
			{
				m_bDontLocalizeCategory = value;
			}
		}
		bool m_bDontLocalizeCategory = false;
		#endregion
		#region Attr{g/s}: bool DontLocalizeHelp
		public bool DontLocalizeHelp
		{
			get
			{
				return m_bDontLocalizeHelp;
			}
			set
			{
				m_bDontLocalizeHelp = value;
			}
		}
		bool m_bDontLocalizeHelp = false;
		#endregion
		#region Attr{g/s}: bool DontLocalizeEnums
		public bool DontLocalizeEnums
		{
			get
			{
				return m_bDontLocalizeEnums;
			}
			set
			{
				m_bDontLocalizeEnums = value;
			}
		}
		bool m_bDontLocalizeEnums = false;
		#endregion

		private Attribute[] attributes;
		private string editor;
		private Type type;
		private Type typeConverter;

		#region Properties

		/// <summary>
		/// Gets or sets a collection of additional Attributes for this property.  This can
		/// be used to specify attributes beyond those supported intrinsically by the
		/// PropertySpec class, such as ReadOnly and Browsable.
		/// </summary>
		public Attribute[] Attributes
		{
			get { return attributes; }
			set { attributes = value; }
		}

		/// <summary>
		/// Gets or sets the type converter type for this property.
		/// </summary>
		public Type ConverterTypeName
		{
			get { return typeConverter; }
			set { typeConverter = value; }
		}

		/// <summary>
		/// Gets or sets the fully qualified name of the editor type for this property.
		/// </summary>
		public string EditorTypeName
		{
			get { return editor; }
			set { editor = value; }
		}

		/// <summary>
		/// Gets or sets the type of this property.
		/// </summary>
		public Type TypeName
		{
			get { return type; }
			set { type = value; }
		}
		#endregion

		// Constructors ----------------------------------------------------------------------
		#region Constructor(sID, sName, type)
		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="sID"></param>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">A Type that represents the type of the property.</param>
		public PropertySpec(string sID, string name, Type type)
			: this(sID, name, type, null, null, null)
		{ }
		#endregion
		#region Constructor(sID, sName, type, sCat)
		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="sID"></param>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">A Type that represents the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the
		/// property grid.</param>
		public PropertySpec(string sID, string name, Type type, string category)
			: this(sID, name, type, category, null, null)
		{ }
		#endregion
		#region Constructor(sID, sName, type, sCat, sDescr)
		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="sID"></param>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">A Type that represents the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the
		/// property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the
		/// property grid.</param>
		public PropertySpec(string sID, string name, Type type, string category, string description)
			: this(sID, name, type, category, description, null)
		{ }
		#endregion
		#region Constructor(sID, sName, type, sCat, sDescr, sDefaultValue)
		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="sID"></param>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">A Type that represents the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the
		/// property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the
		/// property grid.</param>
		/// <param name="defaultValue">The default value of the property, or null if there is
		/// no default value.</param>
		public PropertySpec(string sID, string name, Type type, string category, string description, object defaultValue)
		{
			this.m_sID = sID;

			this.name = name;
			this.type = type;
			this.category = category;
			this.description = description;
			this.defaultValue = defaultValue;
			attributes = null;
		}
		#endregion
		#region Constructor(sID, sName, type, sCat, sDescr, objDefaultValue, sEditor, typeConverter)
		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="sID"></param>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">A Type that represents the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the
		/// property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the
		/// property grid.</param>
		/// <param name="defaultValue">The default value of the property, or null if there is
		/// no default value.</param>
		/// <param name="editor">The fully qualified name of the type of the editor for this
		/// property.  This type must derive from UITypeEditor.</param>
		/// <param name="typeConverter">The Type that represents the type of the type
		/// converter for this property.  This type must derive from TypeConverter.</param>
		public PropertySpec(string sID, string name, Type type, string category, string description,
							object defaultValue, string editor, Type typeConverter)
			: this(sID, name, type, category, description, defaultValue)
		{
			this.editor = editor;
			this.typeConverter = typeConverter;
		}
		#endregion
		#region Constructor(sID, sName, type, sCat, sDescr, objDefaultValue, Type editor, typeConverter)
		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="sID"></param>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="type">A Type that represents the type of the property.</param>
		/// <param name="category">The category under which the property is displayed in the
		/// property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the
		/// property grid.</param>
		/// <param name="defaultValue">The default value of the property, or null if there is
		/// no default value.</param>
		/// <param name="editor">The Type that represents the type of the editor for this
		/// property.  This type must derive from UITypeEditor.</param>
		/// <param name="typeConverter">The Type that represents the type of the type
		/// converter for this property.  This type must derive from TypeConverter.</param>
		public PropertySpec(string sID, string name, Type type, string category,
							string description, object defaultValue, Type editor, Type typeConverter)
			: this(sID, name, type, category, description, defaultValue,
				   editor.AssemblyQualifiedName, typeConverter)
		{
		}

		#endregion
		#region Constructor(sID, sName, sCat, sDescr, string[] enumValues, sDefaultValue)
		/// <summary>
		/// Initializes a new instance of the PropertySpec class.
		/// </summary>
		/// <param name="sID"></param>
		/// <param name="name">The name of the property displayed in the property grid.</param>
		/// <param name="category">The category under which the property is displayed in the
		/// property grid.</param>
		/// <param name="description">A string that is displayed in the help area of the
		/// property grid.</param>
		/// <param name="enumValues">Allowed enum values.</param>
		/// <param name="defaultValue">The default value of the property, or null if there is
		/// no default value.</param>
		public PropertySpec(
			string sID,
			string name,
			string category,
			string description,
			string[] enumValues,
			string defaultValue)
			: this(sID, name, typeof(string), category, description, defaultValue,
				   (string)null, typeof(PropertyEnumStringConverter))
		{
			this.enumValues = enumValues;
		}
		#endregion

		#region SMethod:PropertySpec CreateColorPropertySpec(sID, sName, sCat, sDescr, sDefault)
		static public PropertySpec CreateColorPropertySpec(
			string sID,
			string name,
			string category,
			string description,
			string defaultValue)
		{
			PropertySpec ps = new PropertySpec
				(
				sID,
				name,
				typeof(string),
				category,
				description,
				defaultValue,
				typeof(ColorTypeEditor),
				typeof(ColorConverter)
				);

			return ps;
		}
		#endregion

	}

	public class BoolPropertySpec : PropertySpec
	{
		#region Attr{g}: bool IsBoolProperty - T if enumValues represents a boolean value
		public bool IsBoolProperty
		{
			get
			{
				return m_bIsBoolProperty;
			}
		}
		bool m_bIsBoolProperty = false;
		#endregion
		#region Method: bool IsTrue(string sValue) - T if a Bool Property and the value matches enum[0]
		public bool IsTrue(string sValue)
		{
			if (m_bIsBoolProperty && sValue == EnumValues[0])
				return true;
			return false;
		}
		#endregion
		#region Method: string GetBoolString(b) - returns the enumValues string matching T/F
		public string GetBoolString(bool b)
		{
			if (!m_bIsBoolProperty)
				return null;

			return (b) ? EnumValues[0] : EnumValues[1];
		}
		#endregion

		#region Constructor(sID, sName, sCat, sDescr, string[] vsValues, sDefaultValue)
		public BoolPropertySpec(
			string sID,
			string sName,
			string sCategory,
			string sDescription,
			string[] vsValues,
			string sDefaultValue)
			: base(sID, sName, typeof(string), sCategory, sDescription, sDefaultValue,
				   (string)null, typeof(PropertyEnumStringConverter))
		{
			Debug.Assert(null != vsValues);
			Debug.Assert(vsValues.Length == 2);

			EnumValues = vsValues;

			m_bIsBoolProperty = true;
		}
		#endregion
	}

	public class EnumPropertySpec : PropertySpec
	{
		#region Attr{g}: string[] EnumIDs
		public string[] EnumIDs
		{
			get
			{
				return m_vsEnumIDs;
			}
		}
		string[] m_vsEnumIDs;
		#endregion
		#region Attr{g}: int[] EnumNumbers
		public int[] EnumNumbers
		{
			get
			{
				Debug.Assert(null != m_nEnumNumbers && m_nEnumNumbers.Length > 1);
				return m_nEnumNumbers;
			}
		}
		int[] m_nEnumNumbers;
		#endregion

		#region Method: int GetEnumNumberFor(string sValue)
		public int GetEnumNumberFor(string sValue)
		{
			for (int i = 0; i < EnumValues.Length; i++)
			{
				if (EnumValues[i] == sValue)
					return EnumNumbers[i];
			}
			return -1;
		}
		#endregion
		#region Method: string GetEnumValueFor(int nEnumNumber)
		public string GetEnumValueFor(int nEnumNumber)
		{
			for (int i = 0; i < EnumNumbers.Length; i++)
			{
				if (EnumNumbers[i] == nEnumNumber)
					return (string)EnumValues[i];
			}
			return "";
		}
		#endregion

		#region Constructor(...)
		public EnumPropertySpec(
			string sID,
			string sName,
			string sCategory,
			string sDescription,
			Type eNumbers,
			int[] vnNumbers,
			string[] vsValues,
			string sDefaultValue)
			: base(sID, sName, typeof(string), sCategory, sDescription, sDefaultValue,
				   (string)null, typeof(PropertyEnumStringConverter))
		{
			// Come up the the Enum IDs (that is, the names of the enums)
			Debug.Assert(eNumbers.IsEnum);
			ArrayList a = new ArrayList();
			MemberInfo[] vmi = eNumbers.GetMembers();
			foreach (MemberInfo mi in vmi)
			{
				// Exclude the stuff that is not an enum
				if (mi.DeclaringType.Name == "Enum" || mi.DeclaringType.Name == "Object")
					continue;
				if (mi.Name == "value__")
					continue;

				a.Add(mi);
			}
			m_vsEnumIDs = new string[a.Count];
			for (int i = 0; i < a.Count; i++)
				m_vsEnumIDs[i] = (a[i] as MemberInfo).Name;

			// The English Values
			Debug.Assert(null != vsValues);
			EnumValues = vsValues;

			// The numbers (e.g., (int)kLeft, (int)kCentered, (int)kRight)
			Debug.Assert(null != vnNumbers);
			m_nEnumNumbers = vnNumbers;

			Debug.Assert(vsValues.Length == m_nEnumNumbers.Length);
			Debug.Assert(m_vsEnumIDs.Length == m_nEnumNumbers.Length);
		}
		#endregion
	}

	public class PropertyEnumStringConverter : StringConverter
	{
		#region OMethod: bool GetStandardValuesExclusive(...)
		override public bool GetStandardValuesExclusive(ITypeDescriptorContext x)
		{
			return true;
		}
		#endregion
		#region OMethod: bool GetStandardValuesSupported(...)
		override public bool GetStandardValuesSupported(ITypeDescriptorContext x)
		{
			return true;
		}
		#endregion
		#region OMethod: StandardValuesCollection GetStandardValues(...)
		override public StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			PropertyBag.PropertySpecDescriptor psd = (PropertyBag.PropertySpecDescriptor)
													 context.PropertyDescriptor;
			return new StandardValuesCollection(psd.EnumValues);
		}
		#endregion
	}

	public class ColorConverter : StringConverter
	{
		#region Method: override bool GetStandardValuesSupported(...)
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}
		#endregion
		#region Method: override bool GetStandardValuesExclusive(...)
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return true;
		}
		#endregion
		#region Method: override StandardValuesCollection GetStandardValues
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			// Retrieve all of the "web" colors in the system
			Type type = typeof(Color);
			System.Reflection.PropertyInfo[] fields = type.GetProperties(
				System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.Static);

			// Loop through all of the colors, adding to the combo box items
			ArrayList a = new ArrayList();
			Color clr = new Color();
			int i = 0;
			foreach (System.Reflection.PropertyInfo pi in fields)
			{
				// The first one is "transparent", which isn't supported. So
				// we skip it.
				if (i > 0)
				{
					Color c = (Color)pi.GetValue(clr, null);

					a.Add(c);
				}
				i++;
			}

			// Convert to a vector
			string[] v = new string[a.Count];
			for (int k = 0; k < a.Count; k++)
				v[k] = ((Color)a[k]).Name;

			return new StandardValuesCollection(v);
		}
		#endregion
	}

	class ColorTypeEditor : UITypeEditor
	{
		#region OMethod: bool GetPaintValueSupported(...)
		public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
			return true;
		}
		#endregion
		#region OMethod: void PaintValue(...)
		public override void PaintValue(PaintValueEventArgs e)
		{
			// The item's color
			string sColorName = (string)e.Value;
			Color clr = Color.FromName(sColorName);

			// The rectangle to paint the color
			Rectangle r = e.Bounds;

			// Draw the color sample rectangle at the left
			e.Graphics.FillRectangle(new SolidBrush(clr), r);
		}
		#endregion
	}

	/// <summary>
	/// Provides data for the GetValue and SetValue events of the PropertyBag class.
	/// </summary>
	public class PropertySpecEventArgs : EventArgs
	{
		private PropertySpec property;
		private object val;

		/// <summary>
		/// Initializes a new instance of the PropertySpecEventArgs class.
		/// </summary>
		/// <param name="property">The PropertySpec that represents the property whose
		/// value is being requested or set.</param>
		/// <param name="val">The current value of the property.</param>
		public PropertySpecEventArgs(PropertySpec property, object val)
		{
			this.property = property;
			this.val = val;
		}

		/// <summary>
		/// Gets the PropertySpec that represents the property whose value is being
		/// requested or set.
		/// </summary>
		public PropertySpec Property
		{
			get { return property; }
		}

		/// <summary>
		/// Gets or sets the current value of the property.
		/// </summary>
		public object Value
		{
			get { return val; }
			set { val = value; }
		}
	}

	public class FontSizeConverter : Int16Converter
	{
		#region Method: override bool GetStandardValuesSupported(...)
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}
		#endregion
		#region Method: override bool GetStandardValuesExclusive(...)
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return true;
		}
		#endregion

		#region Method: override StandardValuesCollection GetStandardValues
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			int[] v = new int[20];

			for (int i = 0; i < v.Length; i++)
				v[i] = i + 8;

			return new StandardValuesCollection(v);
		}
		#endregion
	}

	public class FontNameConverter : StringConverter
	{
		#region Method: override bool GetStandardValuesSupported(...)
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}
		#endregion
		#region Method: override bool GetStandardValuesExclusive(...)
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return true;
		}
		#endregion

		#region Method: override StandardValuesCollection GetStandardValues
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			string[] v = new string[FontFamily.Families.Length];

			for (int i = 0; i < FontFamily.Families.Length; i++)
				v[i] = FontFamily.Families[i].Name;

			return new StandardValuesCollection(v);
		}
		#endregion
	}

	/// <summary>
	/// Represents the method that will handle the GetValue and SetValue events of the
	/// PropertyBag class.
	/// </summary>
	public delegate void PropertySpecEventHandler(object sender, PropertySpecEventArgs e);

	/// <summary>
	/// Represents a collection of custom properties that can be selected into a
	/// PropertyGrid to provide functionality beyond that of the simple reflection
	/// normally used to query an object's properties.
	/// </summary>
	public class PropertyBag : ICustomTypeDescriptor
	{
		#region EMBEDDED CLASS: PropertySpecCollection
		/// <summary>
		/// Encapsulates a collection of PropertySpec objects.
		/// </summary>
		[Serializable]
		public class PropertySpecCollection : IList
		{
			private ArrayList innerArray;

			/// <summary>
			/// Initializes a new instance of the PropertySpecCollection class.
			/// </summary>
			public PropertySpecCollection()
			{
				innerArray = new ArrayList();
			}

			/// <summary>
			/// Gets the number of elements in the PropertySpecCollection.
			/// </summary>
			/// <value>
			/// The number of elements contained in the PropertySpecCollection.
			/// </value>
			public int Count
			{
				get { return innerArray.Count; }
			}

			/// <summary>
			/// Gets a value indicating whether the PropertySpecCollection has a fixed size.
			/// </summary>
			/// <value>
			/// true if the PropertySpecCollection has a fixed size; otherwise, false.
			/// </value>
			public bool IsFixedSize
			{
				get { return false; }
			}

			/// <summary>
			/// Gets a value indicating whether the PropertySpecCollection is read-only.
			/// </summary>
			public bool IsReadOnly
			{
				get { return false; }
			}

			/// <summary>
			/// Gets a value indicating whether access to the collection is synchronized (thread-safe).
			/// </summary>
			/// <value>
			/// true if access to the PropertySpecCollection is synchronized (thread-safe); otherwise, false.
			/// </value>
			public bool IsSynchronized
			{
				get { return false; }
			}

			/// <summary>
			/// Gets an object that can be used to synchronize access to the collection.
			/// </summary>
			/// <value>
			/// An object that can be used to synchronize access to the collection.
			/// </value>
			object ICollection.SyncRoot
			{
				get { return null; }
			}

			/// <summary>
			/// Gets or sets the element at the specified index.
			/// In C#, this property is the indexer for the PropertySpecCollection class.
			/// </summary>
			/// <param name="index">The zero-based index of the element to get or set.</param>
			/// <value>
			/// The element at the specified index.
			/// </value>
			public PropertySpec this[int index]
			{
				get { return (PropertySpec)innerArray[index]; }
				set { innerArray[index] = value; }
			}

			/// <summary>
			/// Adds a PropertySpec to the end of the PropertySpecCollection.
			/// </summary>
			/// <param name="value">The PropertySpec to be added to the end of the PropertySpecCollection.</param>
			/// <returns>The PropertySpecCollection index at which the value has been added.</returns>
			public int Add(PropertySpec value)
			{
				int index = innerArray.Add(value);

				return index;
			}

			/// <summary>
			/// Adds the elements of an array of PropertySpec objects to the end of the PropertySpecCollection.
			/// </summary>
			/// <param name="array">The PropertySpec array whose elements should be added to the end of the
			/// PropertySpecCollection.</param>
			public void AddRange(PropertySpec[] array)
			{
				innerArray.AddRange(array);
			}

			/// <summary>
			/// Removes all elements from the PropertySpecCollection.
			/// </summary>
			public void Clear()
			{
				innerArray.Clear();
			}

			/// <summary>
			/// Determines whether a PropertySpec is in the PropertySpecCollection.
			/// </summary>
			/// <param name="item">The PropertySpec to locate in the PropertySpecCollection. The element to locate
			/// can be a null reference (Nothing in Visual Basic).</param>
			/// <returns>true if item is found in the PropertySpecCollection; otherwise, false.</returns>
			public bool Contains(PropertySpec item)
			{
				return innerArray.Contains(item);
			}

			/// <summary>
			/// Determines whether a PropertySpec with the specified name is in the PropertySpecCollection.
			/// </summary>
			/// <param name="name">The name of the PropertySpec to locate in the PropertySpecCollection.</param>
			/// <returns>true if item is found in the PropertySpecCollection; otherwise, false.</returns>
			public bool Contains(string name)
			{
				foreach (PropertySpec spec in innerArray)
					if (spec.Name == name)
						return true;

				return false;
			}

			/// <summary>
			/// Copies the entire PropertySpecCollection to a compatible one-dimensional Array, starting at the
			/// beginning of the target array.
			/// </summary>
			/// <param name="array">The one-dimensional Array that is the destination of the elements copied
			/// from PropertySpecCollection. The Array must have zero-based indexing.</param>
			public void CopyTo(PropertySpec[] array)
			{
				innerArray.CopyTo(array);
			}

			/// <summary>
			/// Copies the PropertySpecCollection or a portion of it to a one-dimensional array.
			/// </summary>
			/// <param name="array">The one-dimensional Array that is the destination of the elements copied
			/// from the collection.</param>
			/// <param name="index">The zero-based index in array at which copying begins.</param>
			public void CopyTo(PropertySpec[] array, int index)
			{
				innerArray.CopyTo(array, index);
			}

			/// <summary>
			/// Returns an enumerator that can iterate through the PropertySpecCollection.
			/// </summary>
			/// <returns>An IEnumerator for the entire PropertySpecCollection.</returns>
			public IEnumerator GetEnumerator()
			{
				return innerArray.GetEnumerator();
			}

			/// <summary>
			/// Searches for the specified PropertySpec and returns the zero-based index of the first
			/// occurrence within the entire PropertySpecCollection.
			/// </summary>
			/// <param name="value">The PropertySpec to locate in the PropertySpecCollection.</param>
			/// <returns>The zero-based index of the first occurrence of value within the entire PropertySpecCollection,
			/// if found; otherwise, -1.</returns>
			public int IndexOf(PropertySpec value)
			{
				return innerArray.IndexOf(value);
			}

			/// <summary>
			/// Searches for the PropertySpec with the specified name and returns the zero-based index of
			/// the first occurrence within the entire PropertySpecCollection.
			/// </summary>
			/// <param name="name">The name of the PropertySpec to locate in the PropertySpecCollection.</param>
			/// <returns>The zero-based index of the first occurrence of value within the entire PropertySpecCollection,
			/// if found; otherwise, -1.</returns>
			public int IndexOf(string name)
			{
				int i = 0;

				foreach (PropertySpec spec in innerArray)
				{
					if (spec.Name == name)
						return i;

					i++;
				}

				return -1;
			}

			/// <summary>
			/// Inserts a PropertySpec object into the PropertySpecCollection at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index at which value should be inserted.</param>
			/// <param name="value">The PropertySpec to insert.</param>
			public void Insert(int index, PropertySpec value)
			{
				innerArray.Insert(index, value);
			}

			/// <summary>
			/// Removes the first occurrence of a specific object from the PropertySpecCollection.
			/// </summary>
			/// <param name="obj">The PropertySpec to remove from the PropertySpecCollection.</param>
			public void Remove(PropertySpec obj)
			{
				innerArray.Remove(obj);
			}

			/// <summary>
			/// Removes the property with the specified name from the PropertySpecCollection.
			/// </summary>
			/// <param name="name">The name of the PropertySpec to remove from the PropertySpecCollection.</param>
			public void Remove(string name)
			{
				int index = IndexOf(name);
				RemoveAt(index);
			}

			/// <summary>
			/// Removes the object at the specified index of the PropertySpecCollection.
			/// </summary>
			/// <param name="index">The zero-based index of the element to remove.</param>
			public void RemoveAt(int index)
			{
				innerArray.RemoveAt(index);
			}

			/// <summary>
			/// Copies the elements of the PropertySpecCollection to a new PropertySpec array.
			/// </summary>
			/// <returns>A PropertySpec array containing copies of the elements of the PropertySpecCollection.</returns>
			public PropertySpec[] ToArray()
			{
				return (PropertySpec[])innerArray.ToArray(typeof(PropertySpec));
			}

			#region Explicit interface implementations for ICollection and IList
			/// <summary>
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			/// </summary>
			void ICollection.CopyTo(Array array, int index)
			{
				CopyTo((PropertySpec[])array, index);
			}

			/// <summary>
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			/// </summary>
			int IList.Add(object value)
			{
				return Add((PropertySpec)value);
			}

			/// <summary>
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			/// </summary>
			bool IList.Contains(object obj)
			{
				return Contains((PropertySpec)obj);
			}

			/// <summary>
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			/// </summary>
			object IList.this[int index]
			{
				get
				{
					return this[index];
				}
				set
				{
					this[index] = (PropertySpec)value;
				}
			}

			/// <summary>
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			/// </summary>
			int IList.IndexOf(object obj)
			{
				return IndexOf((PropertySpec)obj);
			}

			/// <summary>
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			/// </summary>
			void IList.Insert(int index, object value)
			{
				Insert(index, (PropertySpec)value);
			}

			/// <summary>
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			/// </summary>
			void IList.Remove(object value)
			{
				Remove((PropertySpec)value);
			}
			#endregion
		}
		#endregion
		#region EMBEDDED CLASS: PropertySpecDescriptor
		public class PropertySpecDescriptor : PropertyDescriptor
		{
			private PropertyBag bag;
			private PropertySpec item;

			#region Constructor(PropertySpec, PropertyBag, sName, Attribute[])
			public PropertySpecDescriptor(PropertySpec item, PropertyBag bag, Attribute[] attrs)
				:
					base( item.Name, attrs)
			{
				this.bag = bag;
				this.item = item;
			}
			#endregion

			#region VAttr{g}: string[] EnumValues
			public string[] EnumValues
			{
				get
				{
					return item.EnumValues;
				}
			}
			#endregion
			#region VAttr{g}: Type ComponentType
			public override Type ComponentType
			{
				get { return item.GetType(); }
			}
			#endregion
			#region VAttr{g}: bool IsReadOnly
			public override bool IsReadOnly
			{
				get { return (Attributes.Matches(ReadOnlyAttribute.Yes)); }
			}
			#endregion
			#region VAttr{g}: Type PropertyType
			public override Type PropertyType
			{
				get { return item.TypeName; }
			}
			#endregion

			#region OMethod: bool CanResetValue(object component)
			public override bool CanResetValue(object component)
			{
				if (item.DefaultValue == null)
					return false;
				else
					return !GetValue(component).Equals(item.DefaultValue);
			}
			#endregion
			#region OMethod: object GetValue(object component)
			public override object GetValue(object component)
			{
				// Have the property bag raise an event to get the current value
				// of the property.

				PropertySpecEventArgs e = new PropertySpecEventArgs(item, null);
				bag.OnGetValue(e);
				return e.Value;
			}
			#endregion
			#region OMethod: void ResetValue(object component)
			public override void ResetValue(object component)
			{
				SetValue(component, item.DefaultValue);
			}
			#endregion
			#region OMethod: void SetValue(object component, object value)
			public override void SetValue(object component, object value)
			{
				// Have the property bag raise an event to set the current value
				// of the property.

				PropertySpecEventArgs e = new PropertySpecEventArgs(item, value);
				bag.OnSetValue(e);
			}
			#endregion
			#region OMethod: bool ShouldSerializeValue(object component)
			public override bool ShouldSerializeValue(object component)
			{
				object val = GetValue(component);

				if (item.DefaultValue == null && val == null)
					return false;
				else
					return !val.Equals(item.DefaultValue);
			}
			#endregion
		}
		#endregion

		#region Method: PropertySpec FindPropertySpec(string sID)
		public PropertySpec FindPropertySpec(string sID)
		{
			foreach (PropertySpec ps in Properties)
			{
				if (ps.ID == sID)
					return ps;
			}
			return null;
		}
		#endregion


		#region Constructor() - Initializes a new instance of the PropertyBag class.
		public PropertyBag()
		{
			defaultProperty = null;
			properties = new PropertySpecCollection();
		}
		#endregion

		#region Attr{g/s}: string DefaultProperty - the name of the default property in the collection
		public string DefaultProperty
		{
			get
			{
				return defaultProperty;
			}
			set
			{
				defaultProperty = value;
			}
		}
		private string defaultProperty;
		#endregion
		#region Attr{g}: PropertySpecCollection Properties
		public PropertySpecCollection Properties
		{
			get
			{
				return properties;
			}
		}
		private PropertySpecCollection properties;
		#endregion

		/// <summary>
		/// Occurs when a PropertyGrid requests the value of a property.
		/// </summary>
		public event PropertySpecEventHandler GetValue;

		/// <summary>
		/// Occurs when the user changes the value of a property in a PropertyGrid.
		/// </summary>
		public event PropertySpecEventHandler SetValue;

		/// <summary>
		/// Raises the GetValue event.
		/// </summary>
		/// <param name="e">A PropertySpecEventArgs that contains the event data.</param>
		protected virtual void OnGetValue(PropertySpecEventArgs e)
		{
			if (GetValue != null)
				GetValue(this, e);
		}

		/// <summary>
		/// Raises the SetValue event.
		/// </summary>
		/// <param name="e">A PropertySpecEventArgs that contains the event data.</param>
		protected virtual void OnSetValue(PropertySpecEventArgs e)
		{
			if (SetValue != null)
				SetValue(this, e);
		}

		// Interface ICustomTypeDescriptor ---------------------------------------------------
		#region Interface ICustomTypeDescriptor
		// Most of the functions required by the ICustomTypeDescriptor are
		// merely pssed on to the default TypeDescriptor for this type,
		// which will do something appropriate.  The exceptions are noted
		// below.
		#region IMethod: AttributeCollection ICustomTypeDescriptor.GetAttributes()
		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return TypeDescriptor.GetAttributes(this, true);
		}
		#endregion
		#region IMethod: string ICustomTypeDescriptor.GetClassName()
		string ICustomTypeDescriptor.GetClassName()
		{
			return TypeDescriptor.GetClassName(this, true);
		}
		#endregion
		#region IMethod: string ICustomTypeDescriptor.GetComponentName()
		string ICustomTypeDescriptor.GetComponentName()
		{
			return TypeDescriptor.GetComponentName(this, true);
		}
		#endregion
		#region IMethod: TypeConverter ICustomTypeDescriptor.GetConverter()
		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return TypeDescriptor.GetConverter(this, true);
		}
		#endregion
		#region IMethod: EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(this, true);
		}
		#endregion
		#region IMethod: PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			// This function searches the property list for the property
			// with the same name as the DefaultProperty specified, and
			// returns a property descriptor for it.  If no property is
			// found that matches DefaultProperty, a null reference is
			// returned instead.

			PropertySpec propertySpec = null;
			if (defaultProperty != null)
			{
				int index = properties.IndexOf(defaultProperty);
				propertySpec = properties[index];
			}

			if (propertySpec != null)
				return new PropertySpecDescriptor(propertySpec, this, null);
			else
				return null;
		}
		#endregion
		#region IMethod: object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(this, editorBaseType, true);
		}
		#endregion
		#region IMethod: EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return TypeDescriptor.GetEvents(this, true);
		}
		#endregion
		#region IMethod: EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(this, attributes, true);
		}
		#endregion
		#region IMethod: PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
		}
		#endregion
		#region IMethod: PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			// Rather than passing this function on to the default TypeDescriptor,
			// which would return the actual properties of PropertyBag, I construct
			// a list here that contains property descriptors for the elements of the
			// Properties list in the bag.

			ArrayList props = new ArrayList();

			foreach (PropertySpec property in properties)
			{
				ArrayList attrs = new ArrayList();

				// If a category, description, editor, or type converter are specified
				// in the PropertySpec, create attributes to define that relationship.
				if (property.Category != null)
					attrs.Add(new CategoryAttribute(property.Category));

				if (property.Description != null)
					attrs.Add(new DescriptionAttribute(property.Description));

				if (property.EditorTypeName != null)
					attrs.Add(new EditorAttribute(property.EditorTypeName, typeof(UITypeEditor)));

				if (property.ConverterTypeName != null)
					attrs.Add(new TypeConverterAttribute(property.ConverterTypeName.FullName));

				// Additionally, append the custom attributes associated with the
				// PropertySpec, if any.
				if (property.Attributes != null)
					attrs.AddRange(property.Attributes);

				Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));

				// Create a new property descriptor for the property item, and add
				// it to the list.
				PropertySpecDescriptor pd = new PropertySpecDescriptor(property,
																	   this, attrArray);
				props.Add(pd);
			}

			// Convert the list of PropertyDescriptors to a collection that the
			// ICustomTypeDescriptor can use, and return it.
			PropertyDescriptor[] propArray = (PropertyDescriptor[])props.ToArray(
																	   typeof(PropertyDescriptor));
			return new PropertyDescriptorCollection(propArray);
		}
		#endregion
		#region Method: PropertyDescriptorCollection GetBagProperties(Attribute[] attributes)
		public PropertyDescriptorCollection GetBagProperties(Attribute[] attributes)
		{
			// Rather than passing this function on to the default TypeDescriptor,
			// which would return the actual properties of PropertyBag, I construct
			// a list here that contains property descriptors for the elements of the
			// Properties list in the bag.

			ArrayList props = new ArrayList();

			foreach (PropertySpec property in properties)
			{
				ArrayList attrs = new ArrayList();

				// If a category, description, editor, or type converter are specified
				// in the PropertySpec, create attributes to define that relationship.
				if (property.Category != null)
					attrs.Add(new CategoryAttribute(property.Category));

				if (property.Description != null)
					attrs.Add(new DescriptionAttribute(property.Description));

				if (property.EditorTypeName != null)
					attrs.Add(new EditorAttribute(property.EditorTypeName, typeof(UITypeEditor)));

				if (property.ConverterTypeName != null)
					attrs.Add(new TypeConverterAttribute(property.ConverterTypeName.FullName));

				// Additionally, append the custom attributes associated with the
				// PropertySpec, if any.
				if (property.Attributes != null)
					attrs.AddRange(property.Attributes);

				Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));

				// Create a new property descriptor for the property item, and add
				// it to the list.
				PropertySpecDescriptor pd = new PropertySpecDescriptor(property,
																	   this, attrArray);
				props.Add(pd);
			}

			// Convert the list of PropertyDescriptors to a collection that the
			// ICustomTypeDescriptor can use, and return it.
			PropertyDescriptor[] propArray = (PropertyDescriptor[])props.ToArray(
																	   typeof(PropertyDescriptor));
			return new PropertyDescriptorCollection(propArray);
		}
		#endregion
		#region IMethod: object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}
		#endregion
		#endregion
	}

	/// <summary>
	/// An extension of PropertyBag that manages a table of property values, in
	/// addition to firing events when property values are requested or set.
	/// </summary>
	public class PropertyTable : PropertyBag
	{
		private Hashtable propValues;

		/// <summary>
		/// Initializes a new instance of the PropertyTable class.
		/// </summary>
		public PropertyTable()
		{
			propValues = new Hashtable();
		}

		/// <summary>
		/// The property values being held.
		/// </summary>
		public Hashtable PropertyValues
		{
			get { return propValues; }
		}

		/// <summary>
		/// Gets or sets the value of the property with the specified name.
		/// <p>In C#, this property is the indexer of the PropertyTable class.</p>
		/// </summary>
		public object this[string key]
		{
			get { return propValues[key]; }
			set { propValues[key] = value; }
		}

		/// <summary>
		/// This member overrides PropertyBag.OnGetValue.
		/// </summary>
		protected override void OnGetValue(PropertySpecEventArgs e)
		{
			e.Value = propValues[e.Property.Name];
			base.OnGetValue(e);
		}

		/// <summary>
		/// This member overrides PropertyBag.OnSetValue.
		/// </summary>
		protected override void OnSetValue(PropertySpecEventArgs e)
		{
			propValues[e.Property.Name] = e.Value;
			base.OnSetValue(e);
		}
	}

	public class PropertyBagTypeConverter : TypeConverter
	{
		public override PropertyDescriptorCollection GetProperties(
			ITypeDescriptorContext context,
			object value,
			Attribute[] attributes)
		{
			PropertyBag bag = value as PropertyBag;
			if (null == bag)
				return null;

			return bag.GetBagProperties(attributes);
		}

		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}
	}
}