using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using SIL.Reporting;

namespace SIL.i18n
{
#if NET
	[System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
	public class StringCatalog
	{
		private Dictionary<string, string> _catalog;
		private static StringCatalog _singleton;
		private static Font _font;
		private static bool _inInternationalizationTestMode;

		/// <summary>
		/// Construct with no actual string file
		/// </summary>
		public StringCatalog(): this(String.Empty, 9)
		{
		}
		/// <summary>
		/// Construct with no actual string file
		/// </summary>
		public StringCatalog(string labelFontName, float labelFontSizeInPoints)
		{
			Init();
			SetupUIFont(labelFontName, labelFontSizeInPoints );
		}

		private enum State
		{
			InMsgId,
			InMsgStr,
			Reset
		} ;

		public StringCatalog(string pathToPoFile, string labelFontName, float labelFontSizeInPoints)
		{
			Init();
			_inInternationalizationTestMode = pathToPoFile == "test";
			if (!_inInternationalizationTestMode)
			{
				using (var reader = File.OpenText(pathToPoFile))
				{
					string id = "";
					string message = "";
					string line = reader.ReadLine();
					var state = State.Reset;

					while (line != null)
					{
						switch (state)
						{
							case State.Reset:
								if (line.StartsWith("msgid"))
								{
									state = State.InMsgId;
									id = GetStringBetweenQuotes(line);
								}
								break;

							case State.InMsgId:
								if (line.StartsWith("msgstr"))
								{
									state = State.InMsgStr;
									message = GetStringBetweenQuotes(line);
								}
								else if (line.StartsWith("\""))
								{
									id += GetStringBetweenQuotes(line);
								}
								break;

							case State.InMsgStr:
								if (string.IsNullOrEmpty(line))
								{
									state = State.Reset;
									if (!(string.IsNullOrEmpty(id) || string.IsNullOrEmpty(message) || _catalog.ContainsKey(id)))
									{
										_catalog.Add(id.Trim(), message.Trim());
									}
									id = "";
									message = "";
								}
								else if (line.StartsWith("\""))
								{
									message += GetStringBetweenQuotes(line);
								}
								break;
						}
						line = reader.ReadLine();
					}
					if (!(string.IsNullOrEmpty(id) || string.IsNullOrEmpty(message) || _catalog.ContainsKey(id)))
					{
						_catalog.Add(id, message);
					}
				}
			}

			SetupUIFont(labelFontName,  labelFontSizeInPoints);
		}

		private void SetupUIFont(string labelFontName, float labelFontSizeInPoints)
		{
			if (_inInternationalizationTestMode)
			{
				LabelFont = new Font(FontFamily.GenericSansSerif, 9);
				return;
			}

			LabelFont = new Font(FontFamily.GenericSansSerif, (float) 8.25, FontStyle.Regular);
			if(!String.IsNullOrEmpty(labelFontName ))
			{
				try
				{
					LabelFont = new Font(labelFontName, labelFontSizeInPoints, FontStyle.Regular);
				}
				catch (Exception)
				{
					ErrorReport.NotifyUserOfProblem(
						"Could not find the requested UI font '{0}'.  Will use a generic font instead.",
						labelFontName);
				}
			}
		}

		public static string Get(string id)
		{
			return Get(id,  String.Empty);
		}

		/// <summary>
		/// Clients should use this rather than running string.Format themselves,
		/// because this has error checking and a helpful message, should the number
		/// of parameters be wrong.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="translationNotes">just for the string scanner's use</param>
		/// <param name="args">arguments to the string, used in string.format</param>
		/// <returns></returns>
		public static string GetFormatted(string id, string translationNotes, params object[] args)
		{
			//todo: this doesn't notice if the catalog has too few arugment slots, e.g.
			//if it says "blah" when it should say "blah{0}"

			try
			{
				var s = Get(id, translationNotes);
				try
				{
					s = String.Format(s, args);
					return s;
				}
				catch(Exception e)
				{
					Reporting.ErrorReport.NotifyUserOfProblem(
						"There was a problem localizing\r\n'{0}'\r\ninto this UI language... check number of parameters. The code expects there to be {1}.  The current localized string is\r\n'{2}'.\r\nThe error was {3}", id, args.Length, s, e.Message);

					return "!!"+s; // show it without the formatting
				}
			}
			catch(Exception)
			{
				return "Error localizing string '" + id + "' to this UI language";
			}
		}

		public static string Get(string id, string translationNotes)
		{
			if (!String.IsNullOrEmpty(id) && id[0] == '~')
			{
				id = id.Substring(1);
			}
			if (_singleton == null) //todo: this should not be needed
			{
				return id;
			}

			if (_inInternationalizationTestMode)
			{
				return "*"+_singleton[id];
			}
			else
			{
				return _singleton[id];
			}
		}


		private void Init()
		{
			_singleton = this;
			_catalog = new Dictionary<string, string>();
		}

		private static string GetStringBetweenQuotes(string line)
		{
			int s = line.IndexOf('"');
			int f = line.LastIndexOf('"');
			return line.Substring(s + 1, f - (s + 1));
		}

		public string this[string id]
		{
			get
			{
				if (_catalog.ContainsKey(id))
				{
					return _catalog[id];
				}
				//REVIEW: What's this about?  It was   id = id.Replace("&&", "&");  which was removing the && we need when it gets to the UI
				var idWithSingleAmpersand  =id.Replace("&&", "&");
				if (_catalog.ContainsKey(idWithSingleAmpersand))
				{
					return _catalog[idWithSingleAmpersand];
				}
				return id;
			}
		}

		public static Font LabelFont
		{
			get
			{
				if (_font == null)
				{
					_font = new Font(FontFamily.GenericSansSerif, 9);
				}
				return _font;
			}
			set
			{
				_font = value;
			}
		}

		// Font resizing is deprecated - obsolete API DG 2011-12
		public static Font ModifyFontForLocalization(Font incoming)
		{
			return incoming;
		}
	}
}