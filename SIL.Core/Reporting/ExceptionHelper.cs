// Copyright (c) 2002-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using JetBrains.Annotations;

namespace SIL.Reporting
{
	/// <summary>
	/// Helper class that makes it easier to get information out of nested exceptions to
	/// display in the UI.
	/// </summary>
	public static class ExceptionHelper
	{
		/// <summary>
		/// Get the messages from all nested exceptions
		/// </summary>
		/// <param name="e">The exception</param>
		/// <returns>String with the messages of all nested exceptions</returns>
		public static string GetAllExceptionMessages(Exception e)
		{
			var strB = new StringBuilder();
			while (e != null)
			{
				strB.Append("\n\t");
				strB.Append(e.Message);
				e = e.InnerException;
			}

			strB.Remove(0, 2); // remove \n\t from beginning
			return strB.ToString();
		}

		/// <summary>
		/// Gets the exception types of all nested exceptions.
		/// </summary>
		/// <param name="e">The exception</param>
		/// <returns>String with the types of all nested exceptions. The string has the
		/// form type1(type2(type3...)).</returns>
		public static string GetExceptionTypes(Exception e)
		{
			var strB = new StringBuilder();
			int nTypes = 0;
			while (e != null)
			{
				if (nTypes > 0)
					strB.Append("(");
				strB.Append(e.GetType());
				e = e.InnerException;
				nTypes++;
			}

			for (; nTypes > 1; nTypes--) // don't need ) for first type
				strB.Append(")");

			return strB.ToString();
		}

		/// <summary>
		/// Gets a string with the stack traces of all nested exceptions. The stack
		/// for the inner most exception is displayed first. Each stack is preceded
		/// by the exception type, module name, method name and message.
		/// </summary>
		/// <param name="e">The exception</param>
		/// <returns>String with stack traces of all nested exceptions.</returns>
		[PublicAPI]
		public static string GetAllStackTraces(Exception e)
		{
			StringBuilder strB = new StringBuilder();
			while (e != null)
			{
				strB = new StringBuilder(
					string.Format("Stack trace for {0} in module {1}, {2} ({3}):\n{4}\n\n{5}",
					e.GetType().Name, e.Source, e.TargetSite.Name, e.Message, e.StackTrace,
					strB));
				e = e.InnerException;
			}

			return strB.ToString();
		}

		/// <summary>
		/// Gets the names of all the target sites of nested exceptions.
		/// </summary>
		/// <param name="e">The exception</param>
		/// <returns>String with the names of all the target sites.</returns>
		public static string GetTargetSiteNames(Exception e)
		{
			StringBuilder strB = new StringBuilder();
			int nSite = 0;
			while (e != null)
			{
				if (nSite > 0)
					strB.Append("/");
				strB.Append(e.TargetSite.Name);
			}

			return strB.ToString();
		}

		/// <summary>
		/// Gets the inner most exception
		/// </summary>
		/// <param name="e">The exception</param>
		/// <returns>Returns the inner most exception.</returns>
		public static Exception GetInnerMostException(Exception e)
		{
			while (e.InnerException != null)
				e = e.InnerException;

			return e;
		}

		/// <summary>
		/// Gets the help string.
		/// </summary>
		/// <param name="e">The exception</param>
		/// <returns>The help link</returns>
		public static string GetHelpLink(Exception e)
		{
			string helpLink = string.Empty;
			while (e != null)
			{
				if (!string.IsNullOrEmpty(e.HelpLink))
					helpLink = e.HelpLink;
				e = e.InnerException;
			}

			return helpLink;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the hierarchical exception info.
		/// </summary>
		/// <param name="error">The error.</param>
		/// <param name="innerMostException">The innermost exception or null if the error is
		/// the innermost exception</param>
		/// <returns>A string containing the text of the specified error</returns>
		/// ------------------------------------------------------------------------------------
		public static string GetHierarchicalExceptionInfo(Exception error,
			ref Exception innerMostException)
		{
			innerMostException = error.InnerException;
			var strBldr = new StringBuilder();
			strBldr.Append(GetExceptionText(error));

			if (error.InnerException != null)
			{
				strBldr.AppendLine("**Inner Exception:");
				strBldr.Append(GetHierarchicalExceptionInfo(error.InnerException, ref innerMostException));
			}
			return strBldr.ToString();
		}

		public static string GetExceptionText(Exception error)
		{
			var strBldr = new StringBuilder();

			strBldr.Append("Msg: ");
			strBldr.AppendLine(error.Message);
			strBldr.Append("Class: ");
			strBldr.AppendLine(error.GetType().ToString());

			try
			{
				var comException = error as COMException;
				if (comException != null)
				{
					strBldr.Append("COM message: ");
					strBldr.AppendLine(new Win32Exception(comException.ErrorCode).Message);
				}
			}
			catch
			{
			}

			try
			{
				strBldr.Append("Source: ");
				strBldr.AppendLine(error.Source);
			}
			catch
			{
			}

			try
			{
				if (error.TargetSite != null)
				{
					strBldr.Append("Assembly: ");
					strBldr.AppendLine(error.TargetSite.DeclaringType.Assembly.FullName);
				}
			}
			catch
			{
			}

			try
			{
				strBldr.Append("Stack: ");
				strBldr.AppendLine(error.StackTrace);
			}
			catch
			{
			}
			strBldr.AppendFormat("Thread: {0}", Thread.CurrentThread.Name);
			strBldr.AppendLine();

			strBldr.AppendFormat("Thread UI culture: {0}", Thread.CurrentThread.CurrentUICulture);
			strBldr.AppendLine();

			strBldr.AppendFormat("Exception: {0}", error.GetType());
			strBldr.AppendLine();

			try
			{
				if (error.Data.Count > 0)
				{
					strBldr.AppendLine("Additional Exception Information:");
					foreach (DictionaryEntry de in error.Data)
					{
						strBldr.AppendFormat("{0}={1}", de.Key, de.Value);
						strBldr.AppendLine();
					}
				}
			}
			catch
			{
			}

			return strBldr.ToString();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Executes the action, logging and ignoring any exceptions.
		/// </summary>
		/// <param name="action">The action.</param>
		/// ------------------------------------------------------------------------------------
		public static void LogAndIgnoreErrors(Action action)
		{
			LogAndIgnoreErrors(action, null);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Executes the action, logging and ignoring any exceptions.
		/// </summary>
		/// <param name="action">The action.</param>
		/// <param name="onError">Additional action to perform after logging the error.</param>
		/// ------------------------------------------------------------------------------------
		public static void LogAndIgnoreErrors(Action action, Action<Exception> onError)
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				Logger.WriteError(e);
				if (onError != null)
					onError(e);
			}
		}
	}
}
