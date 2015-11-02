using System.ComponentModel;

namespace SIL.Windows.Forms.SuperToolTip
{
	/// <summary>
	/// Class to wrap SuperToolTipInfo with a Boolean variable to use the
	/// SuperToolTip or not.
	/// </summary>
	[DefaultProperty("UseSuperToolTip")]
	public class SuperToolTipInfoWrapper
	{
		#region Private Members
		private bool _useSuperToolTip;
		private SuperToolTipInfo _superInfo;
		#endregion

		#region Public Properties
		[NotifyParentProperty(true)]
		[DefaultValue(false)]
		public bool UseSuperToolTip
		{
			get { return _useSuperToolTip; }
			set
			{
				_useSuperToolTip = value;
				if (_useSuperToolTip && _superInfo == null)
					_superInfo = new SuperToolTipInfo();

				if (!_useSuperToolTip) _superInfo = null;
			}
		}
		[NotifyParentProperty(true)]
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public SuperToolTipInfo SuperToolTipInfo
		{
			get { return _superInfo; }
			set { _superInfo = value; }
		}

		public void ResetSuperToolTipInfo()
		{
			_superInfo.Reset();
		}
		#endregion

		#region Public Methods
		public override string ToString()
		{
			if (_useSuperToolTip) return "(Super Info)";
			else return "(none)";
		}
		#endregion

	}

}
