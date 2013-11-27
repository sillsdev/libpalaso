using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.ComponentModel.Design;
using System.Drawing.Design;

namespace Palaso.UI.WindowsForms.SuperToolTip
{
	public class SuperToolTipInfo
	{
		#region Private Members

		//palaso additions
		private Point _offsetForWhereToDisplay;

		//Background Color
		private Color _backgroundGradientBegin;
		private Color _backgroundGradientMiddle;
		private Color _backgroundGradientEnd;

		//Body related
		private string _bodyText;
		private Font _bodyFont;
		private Color _bodyTextForeColor;
		private Bitmap _bodyImage;

		//Header related
		private bool _showHeader;
		private string _headerText;
		private Font _headerFont;
		private Color _headerTextForeColor;
		private bool _showHeaderSeparator;

		//Footer related
		private bool _showFooter;
		private string _footerText;
		private Font _footerFont;
		private Color _footerTextForeColor;
		private bool _showFooterSeparator;
		private Bitmap _footerImage;

		#endregion

		#region Public Properties

		#region Background Color

		[NotifyParentProperty(true)]
		[DefaultValue(typeof(Color), "255, 250, 172")]
		public Color BackgroundGradientBegin
		{
			get { return _backgroundGradientBegin; }
			set { _backgroundGradientBegin = value; }
		}

		[NotifyParentProperty(true)]
		[DefaultValue(typeof(Color), "255, 207, 157")]
		public Color BackgroundGradientMiddle
		{
			get { return _backgroundGradientMiddle; }
			set { _backgroundGradientMiddle = value; }
		}

		[NotifyParentProperty(true)]
		[DefaultValue(typeof(Color), "255, 153, 0")]
		public Color BackgroundGradientEnd
		{
			get { return _backgroundGradientEnd; }
			set { _backgroundGradientEnd = value; }
		}

		#endregion

		#region Body Properties

		[NotifyParentProperty(true)]
		public Font BodyFont
		{
			// Note that the BodyFont property never
			// returns null.
			get
			{
				if (_bodyFont != null) return _bodyFont;
				return Control.DefaultFont;
			}
			set
			{
				_bodyFont = value;
			}
		}

		[NotifyParentProperty(true)]
		[DefaultValue("")]
		[Editor("MultilineStringEditor","UITypeEditor")]
		public string BodyText
		{
			get { return _bodyText; }
			set { _bodyText = value; }
		}


		[NotifyParentProperty(true)]
		[DefaultValue(typeof(Color), "DimGray")]
		public Color BodyForeColor
		{
			get { return _bodyTextForeColor; }
			set { _bodyTextForeColor = value; }
		}

		[NotifyParentProperty(true)]
		[DefaultValue(typeof(Bitmap), null)]
		public Bitmap BodyImage
		{
			get { return _bodyImage; }
			set
			{
				_bodyImage = value;

			}
		}

		#endregion

		#region Header Properties

		[NotifyParentProperty(true)]
		[DefaultValue(true)]
		public bool ShowHeader
		{
			get { return _showHeader; }
			set { _showHeader = value; }
		}

		[NotifyParentProperty(true)]
		public Font HeaderFont
		{
			// Note that the BodyFont property never
			// returns null.
			get
			{
				if (_headerFont != null) return _headerFont;
				return new Font(Control.DefaultFont, FontStyle.Bold);// Control.DefaultFont;
			}
			set
			{
				_headerFont = value;
			}
		}

		[NotifyParentProperty(true)]
		[DefaultValue("")]
		[Editor("MultilineStringEditor", "UITypeEditor")]
		public string HeaderText
		{
			get { return _headerText; }
			set
			{
				_headerText = value;
				if (!string.IsNullOrEmpty(value))   //jh
					ShowHeader = true;              //jh

			}
		}

		[NotifyParentProperty(true)]
		[DefaultValue(typeof(Color), "DimGray")]
		public Color HeaderForeColor
		{
			get { return _headerTextForeColor; }
			set { _headerTextForeColor = value; }
		}

		[NotifyParentProperty(true)]
		[DefaultValue(false)]
		public bool ShowHeaderSeparator
		{
			get { return _showHeaderSeparator; }
			set { _showHeaderSeparator = value; }
		}
		#endregion

		#region Footer Properties

		[NotifyParentProperty(true)]
		[DefaultValue(false)]
		public bool ShowFooter
		{
			get { return _showFooter; }
			set { _showFooter = value; }
		}

		[NotifyParentProperty(true)]
		public Font FooterFont
		{
			// Note that the BodyFont property never
			// returns null.
			get
			{
				if (_footerFont != null) return _footerFont;
				return Control.DefaultFont;
			}
			set
			{
				_footerFont = value;
			}
		}

		[NotifyParentProperty(true)]
		[DefaultValue("")]
		[Editor("MultilineStringEditor", "UITypeEditor")]
		public string FooterText
		{
			get { return _footerText; }
			set { _footerText = value; }
		}

		[NotifyParentProperty(true)]
		[DefaultValue(typeof(Color), "DimGray")]
		public Color FooterForeColor
		{
			get { return _footerTextForeColor; }
			set { _footerTextForeColor = value; }
		}

		[NotifyParentProperty(true)]
		[DefaultValue(typeof(Bitmap), null)]
		public Bitmap FooterImage
		{
			get { return _footerImage; }
			set
			{
				_footerImage = value;
			}
		}

		[NotifyParentProperty(true)]
		[DefaultValue(false)]
		public bool ShowFooterSeparator
		{
			get { return _showFooterSeparator; }
			set { _showFooterSeparator = value; }
		}

		/// <summary>
		/// Set this when you need to control just where the tt is going to display
		/// </summary>
		public Point OffsetForWhereToDisplay
		{
			get
			{
				return _offsetForWhereToDisplay;
			}
			set
			{
				_offsetForWhereToDisplay = value;
			}
		}

		#endregion

		#endregion

		#region ResetXXX & ShouldSerializeXXX

		private bool ShouldSerializeBodyFont()
		{
			return _bodyFont != null;
		}
		private void ResetBodyFont()
		{
			BodyFont = null;
		}

		private bool ShouldSerializeHeaderFont()
		{
			return _headerFont != null;
		}
		private void ResetHeaderFont()
		{
			HeaderFont = null;
		}

		private bool ShouldSerializeFooterFont()
		{
			return _footerFont != null;
		}
		private void ResetFooterFont()
		{
			FooterFont = null;
		}

		#endregion

		#region Constructors

		public SuperToolTipInfo()
		{
			InitializePrivateMembers();
		}

		#endregion

		#region Public Methods

		public override string ToString()
		{
			return "(SuperToolTipInfo)";
		}

		#endregion

		#region Helpers

		private void InitializePrivateMembers()
		{
			//colors
			_backgroundGradientBegin = Color.FromArgb(255, 255, 255);
			_backgroundGradientMiddle = Color.FromArgb(242, 246, 251);
			_backgroundGradientEnd = Color.FromArgb(202, 218, 239);

			//Default Body
			_bodyText = string.Empty;
			_bodyImage = null;
			_bodyTextForeColor = Color.DimGray;

			//Default Header
			_showHeader = false;
			_headerText = string.Empty;
			_headerTextForeColor = Color.DimGray;
			_showHeaderSeparator = false;

			//Default Footer
			_showFooter = false;
			_footerText = string.Empty;
			_footerImage = null;
			_footerTextForeColor = Color.DimGray;
			_footerImage = null;
			_showFooterSeparator = false;

		}

		internal void Reset()
		{
			InitializePrivateMembers();
		}

		#endregion

	}

}
