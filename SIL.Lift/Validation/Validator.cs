using System;
using System.Collections.Generic;
using System.Xml;
using Commons.Xml.Relaxng;
using SIL.Lift.Parsing;
using SIL.Progress;

namespace SIL.Lift.Validation
{
	[Flags]
	public enum ValidationOptions
	{
		CheckLIFT = 1,
		CheckGUIDs =2,
		All = 0xFFFF
	};

	/// <summary>
	/// Provide progress reporting for validation.
	/// </summary>
	/// <remarks>TODO: provide a single IProgressReport interface for SIL.Lift (or SIL.Core?).</remarks>
	public interface IValidationProgress
	{
		///<summary>
		/// Get/set the progress message.
		///</summary>
		string Status { set; get; }
		///<summary>
		/// Advance the progress bar n steps.
		///</summary>
		void Step(int n);
		/// <summary>
		/// Set the number of steps in the progress bar.
		/// </summary>
		int MaxRange { set; get; }
	}

	/// <summary>
	/// Trivial, nonfunctional implementation of IValidationProgress.
	/// </summary>
	/// <remarks>
	/// TODO: provide a single IProgressReport interface for SIL.Lift (or SIL.Core?), and a single trivial
	/// implementation thereof.
	/// </remarks>
	public class NullValidationProgress : IValidationProgress
	{
		private string _status = String.Empty;
		private int _max = 20;
		private int _step;

		#region IValidationProgress Members

		/// <summary>
		/// Get/set the status message string.
		/// </summary>
		public string Status
		{
			get { return _status; }
			set { _status = value; }
		}

		/// <summary>
		/// Advance the progress bar by n steps.
		/// </summary>
		public void Step(int n)
		{
			_step += n;
		}

		/// <summary>
		/// Get/set the number of steps in the progress bar.
		/// </summary>
		public int MaxRange
		{
			get { return _max; }
			set { _max = value; }
		}

		#endregion
	}

	///<summary>
	/// This class provides validation (via built-in RNG schema) of a LIFT file.
	///</summary>
	public class Validator
	{
		///<summary>
		/// Parse the given LIFT file and return a string containing all the validation errors
		/// (if any).  Progress reporting is supported.
		///</summary>
		public static string GetAnyValidationErrors(string path, IValidationProgress progress, ValidationOptions validationOptions)
		{
			string errors = "";
			if ((validationOptions & ValidationOptions.CheckLIFT) != 0)
			{
				errors += GetSchemaValidationErrors(path, progress);
			}

			if ((validationOptions & ValidationOptions.CheckGUIDs) != 0)
			{
				errors += GetDuplicateGuidErrors(path, progress);
			}

			return errors;
		}

		private static string GetDuplicateGuidErrors(string path, IValidationProgress progress)
		{
			progress.Status = "Checking for duplicate guids...";
			string errors="";
			HashSet<string> guids = new HashSet<string>();

			using (XmlTextReader reader = new XmlTextReader(path))
			{
				try
				{
					while (reader.Read())
					{
						if (reader.HasAttributes)
						{
							var guid = reader.GetAttribute("guid");
							if (!string.IsNullOrEmpty(guid))
							{
								if (guids.Contains(guid))
								{
									errors += Environment.NewLine + "Found duplicate GUID (Globally Unique Identifier): " + guid+". All GUIDs must be unique.";
								}
								else
								{
									guids.Add(guid);
								}
							}
						}
					}
				}
				catch (Exception error)
				{
					errors += error.Message;
				}
			}
			return errors;
		}


		///<summary>
		/// Validate the LIFT file contained in the XmlTextReader.  Progress reporting is
		/// supported.
		///</summary>
		static private string GetSchemaValidationErrors(string path,IValidationProgress progress)
		{
			using (XmlTextReader documentReader = new XmlTextReader(path))
			{
				progress.Status = "Checking for Schema errors...";
				var resourceStream = typeof (LiftMultiText).Assembly.GetManifestResourceStream("SIL.Lift.Validation.lift.rng");
				if (resourceStream == null)
					throw new Exception();
				RelaxngValidatingReader reader = new RelaxngValidatingReader(
					documentReader, new XmlTextReader(resourceStream));
				reader.ReportDetails = true;
				string lastGuy = "lift";
				int line = 0;
				int step = 0;
				if (progress != null)
				{
					try
					{
						if (documentReader.BaseURI != null)
						{
							string sFilePath = documentReader.BaseURI.Replace("file://", "");
							if (sFilePath.StartsWith("/"))
							{
								// On Microsoft Windows, the BaseURI may be "file:///C:/foo/bar.lift"
								if (sFilePath.Length > 3 &&
									Char.IsLetter(sFilePath[1]) && sFilePath[2] == ':' && sFilePath[3] == '/')
								{
									sFilePath = sFilePath.Substring(1);
								}
							}
							System.IO.FileInfo fi = new System.IO.FileInfo(sFilePath);
							// Unfortunately, XmlTextReader gives access to only line numbers,
							// not actual file positions while reading.  A check of 8 Flex
							// generated LIFT files showed a range of 43.9 - 52.1 chars per
							// line.  The biatah sample distributed with WeSay has an average
							// of only 23.1 chars per line.  We'll compromise by guessing 33.
							// The alternative is to read the entire file to get the actual
							// line count.
							int maxline = (int) (fi.Length/33);
							if (maxline < 8)
								progress.MaxRange = 8;
							else if (maxline < 100)
								progress.MaxRange = maxline;
							else
								progress.MaxRange = 100;
							step = (maxline + 99)/100;
						}
						if (step <= 0)
							step = 1;
					}
					catch
					{
						step = 100;
					}
				}
				try
				{
					while (!reader.EOF)
					{
						// Debug.WriteLine(reader.v
						reader.Read();
						lastGuy = reader.Name;
						if (progress != null && reader.LineNumber != line)
						{
							line = reader.LineNumber;
							if (line%step == 0)
								progress.Step(1);
						}
					}
				}
				catch (Exception e)
				{
					if (reader.Name == "version" && (lastGuy == "lift" || lastGuy == ""))
					{
						return String.Format(
							"This file claims to be version {0} of LIFT, but this version of the program uses version {1}",
							reader.Value, LiftVersion);
					}
					string m = string.Format("{0}\r\nError near: {1} {2} '{3}'", e.Message, lastGuy, reader.Name, reader.Value);
					return m;
				}
				return null;
			}
		}

		///<summary>
		/// Get the LIFT version handled by this code in the form of a string.
		///</summary>
		public static string LiftVersion
		{
			get
			{
				return "0.13";
			}
		}

		///<summary>
		/// Check the given LIFT file for validity, throwing an exception if an error is found.
		/// Progress reporting is supported.
		///</summary>
		///<exception cref="LiftFormatException"></exception>
		public static void CheckLiftWithPossibleThrow(string pathToLiftFile)
		{
			string errors = GetAnyValidationErrors(pathToLiftFile, new NullValidationProgress(), ValidationOptions.All);
			if (!String.IsNullOrEmpty(errors))
			{
				errors = string.Format("Sorry, the dictionary file at {0} does not conform to the current version of the LIFT format ({1}).  The RNG validator said: {2}.\r\n\r\n If this file was generated by a program such as Lexique Pro, FieldWorks, or WeSay it's very important that you notify the relevant developers of the problem.",
									   pathToLiftFile, LiftVersion, errors);
				throw new LiftFormatException(errors);
			}
		}

		///<summary>
		/// Get the LIFT version of the given file, throwing an exception if it cannot be found.
		///</summary>
		///<exception cref="LiftFormatException"></exception>
		public static string GetLiftVersion(string pathToLift)
		{
			string liftVersionOfRequestedFile = String.Empty;

			XmlReaderSettings readerSettings = new XmlReaderSettings();
			readerSettings.ValidationType = ValidationType.None;
			readerSettings.IgnoreComments = true;

			using (XmlReader reader = XmlReader.Create(pathToLift, readerSettings))
			{
				if (reader.IsStartElement("lift"))
					liftVersionOfRequestedFile = reader.GetAttribute("version");
			}
			if (String.IsNullOrEmpty(liftVersionOfRequestedFile))
			{
				throw new LiftFormatException(String.Format("Cannot import {0} because this was not recognized as well-formed LIFT file (missing version).", pathToLift));
			}
			return liftVersionOfRequestedFile;
		}
	}


	/// <summary>
	/// I don't know how I ended up with these different progress models, but anyhow this adapts from the normal, log-based one to the Validation one needed by the validator.
	/// </summary>
	public class ValidationProgress : IValidationProgress
	{
		private readonly IProgress _progress;

		public ValidationProgress(IProgress progress)
		{
			_progress = progress;
		}

		public string Status
		{
			get { return "not supported"; }
			set{ _progress.WriteStatus(value);}
		}

		public void Step(int n)
		{
		}

		public int MaxRange { get; set; }
	}

}