namespace Palaso.Email
{
	public class ThunderbirdEmailProvider : LinuxEmailProvider
	{
		protected override string EmailCommand
		{
			get
			{
				return "thunderbird";
			}
		}

		protected override string FormatStringNoAttachments
		{
			get
			{
				return "-compose \"to='{0}',subject='{1}',body='{2}'\"";
			}
		}

		protected override string FormatStringWithAttachments
		{
			get
			{
				return "-compose \"to='{0}',subject='{1}',attachment='{2}',body='{3}'\"";
			}
		}

		protected virtual string FormatStringAttachFile
		{
			get
			{
				return "file://{0}";
			}
		}

	}
}