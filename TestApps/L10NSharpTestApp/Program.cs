using System;
using System.IO;
using L10NSharp;
using SIL.IO;

namespace L10NSharpTestApp
{
	/// <summary>
	/// Minimal console app that demonstrates L10NSharp initialization and string lookup.
	/// Usage: L10NSharpTestApp [locale]
	/// Example: L10NSharpTestApp fr
	/// </summary>
	internal static class Program
	{
		static void Main(string[] args)
		{
			var locale = args.Length > 0 ? args[0].ToLowerInvariant() : "fr";

			Console.WriteLine("=== L10NSharp Test App ===");
			Console.WriteLine($"Requested locale: {locale}");
			Console.WriteLine();

			const string key = "AboutDialog.NoUpdates";
			const string english = "No Updates";

			// Locate the XLF for the requested locale.
			var xlfName = $"Palaso.{locale}.xlf";
			var xlfPath = FileLocationUtilities.GetFileDistributedWithApplication(true, xlfName);
			if (xlfPath == null)
			{
				Console.WriteLine($"No XLF found for locale '{locale}' ({xlfName}).");
				return;
			}

			var locDir = Path.GetDirectoryName(xlfPath);
			Console.WriteLine($"Loading XLF from: {locDir}");

			using var manager = LocalizationManager.Create(locale, "Palaso", "Palaso", "1.0.0",
				locDir, "SIL/L10NSharpTestApp", new[] { "SIL." }, null);

			Console.WriteLine($"Active UI locale : {LocalizationManager.UILanguageId}");
			var result = LocalizationManager.GetDynamicString("Palaso", key, english);
			Console.WriteLine($"[{key}]: \"{result}\"");
		}
	}
}
