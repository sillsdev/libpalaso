// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.Collections.Generic;
using System.Text;

namespace SIL.Email
{
	public class ThunderbirdEmailProvider : LinuxEmailProvider
	{
		protected override string EmailCommand => "thunderbird";

		protected override string FormatString => "-compose \"to='{0}',subject='{1}',body='{2}'{3}{4}{5}\"";

		private static string GetArguments(IList<string> arguments, string prefix = "")
		{
			var toBuilder = new StringBuilder();

			foreach (var argument in arguments)
			{
				if (toBuilder.Length > 0)
					toBuilder.Append(",");
				toBuilder.Append($"{prefix}{argument}");
			}

			return toBuilder.ToString();
		}

		protected override string GetToRecipients(IList<string> recipientTo)
		{
			return GetArguments(recipientTo);
		}

		protected override string GetCcRecipients(IList<string> recipients)
		{
			return recipients.Count > 0 ? $",cc='{GetArguments(recipients)}'" : null;
		}

		protected override string GetBccRecipients(IList<string> recipients)
		{
			return recipients.Count > 0 ? $",bcc='{GetArguments(recipients)}'" : null;
		}

		protected override string GetAttachments(IList<string> attachments)
		{
			return attachments.Count > 0 ? $",attachment='{GetArguments(attachments, "file://")}'" : null;
		}
	}
}