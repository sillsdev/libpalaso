using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using L10NSharp;
using SIL.Windows.Forms.Miscellaneous;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems
{
	internal static class Extensions
	{
		internal static string ToValidVariantString(this string unknownString)
		{
			// var1-var2-var3
			// var1-privateUse1-x-privateUse2
			unknownString = unknownString.Trim();
			unknownString = Regex.Replace(unknownString, @"[ ,.]", "-");
			unknownString = Regex.Replace(unknownString, @"-+", "-");
			var tokens = unknownString.Split('-');
			var variants = new List<string>();
			var privateUse = new List<string>();
			bool haveSeenX = false;

			foreach (string token in tokens)
			{
				if (token == "x")
				{
					haveSeenX = true;
					continue;
				}
				if (!haveSeenX && StandardSubtags.IsValidRegisteredVariantCode(token))
				{
					variants.Add(token);
				}
				else
				{
					privateUse.Add(token);
				}
			}
			string combinedVariant = String.Join("-", variants.ToArray());
			string combinedPrivateUse = String.Join("-", privateUse.ToArray());
			string variantString = combinedVariant;
			if (!String.IsNullOrEmpty(combinedPrivateUse))
			{
				variantString = "x-" + combinedPrivateUse;
				if (!String.IsNullOrEmpty(combinedVariant))
				{
					variantString = combinedVariant + "-" + variantString;
				}
			}
			return variantString;
		}

		internal static WritingSystemDefinition CreateAndWarnUserIfOutOfDate(this IWritingSystemFactory wsFactory, string langTag)
		{
			WritingSystemDefinition ws;
			bool upToDate;
			WaitCursor.Show();
			try
			{
				upToDate = wsFactory.Create(langTag, out ws);
			}
			finally
			{
				WaitCursor.Hide();
			}

			if (!upToDate)
			{
				if (MessageBox.Show(Form.ActiveForm, LocalizationManager.GetString("WritingSystemSetupView.UnableToConnectToSldrText", "The application is unable to connect to the SIL Locale Data Repository to retrieve the latest information about this language. If you create this writing system, the default settings might be incorrect or out of date. Are you sure you want to create a new writing system?"),
					LocalizationManager.GetString("WritingSystemSetupView.UnableToConnectToSldrCaption", "Unable to connect to SLDR"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
				{
					return null;
				}
			}

			return ws;
		}
	}
}
