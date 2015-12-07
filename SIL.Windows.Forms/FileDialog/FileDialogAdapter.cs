// --------------------------------------------------------------------------------------------
// <copyright from='2011' to='2011' company='SIL International'>
// Copyright (c) 2011-2015 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html).
// </copyright>
// --------------------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SIL.Utils.FileDialog
{
	public abstract class FileDialogAdapter: IFileDialog, IDisposable
	{
		protected IFileDialog m_dlg;

		#region IFileDialog implementation
		public event EventHandler Disposed
		{
			add { m_dlg.Disposed += value; }
			remove { m_dlg.Disposed -= value; }
		}

		public event CancelEventHandler FileOk
		{
			add { m_dlg.FileOk += value; }
			remove { m_dlg.FileOk -= value; }
		}

		public event EventHandler HelpRequest
		{
			add { m_dlg.HelpRequest += value; }
			remove { m_dlg.HelpRequest -= value; }
		}

		public void Reset()
		{
			m_dlg.Reset();
		}

		public DialogResult ShowDialog()
		{
			return m_dlg.ShowDialog();
		}

		public DialogResult ShowDialog(IWin32Window owner)
		{
			return m_dlg.ShowDialog(owner);
		}

		public bool AddExtension
		{
			get { return m_dlg.AddExtension; }
			set { m_dlg.AddExtension = value; }
		}

		public bool CheckFileExists
		{
			get { return m_dlg.CheckFileExists; }
			set { m_dlg.CheckFileExists = value; }
		}

		public bool CheckPathExists
		{
			get { return m_dlg.CheckPathExists; }
			set { m_dlg.CheckPathExists = value; }
		}

		public string DefaultExt
		{
			get { return m_dlg.DefaultExt; }
			set { m_dlg.DefaultExt = value; }
		}

		public string FileName
		{
			get { return m_dlg.FileName; }
			set { m_dlg.FileName = value; }
		}

		public string[] FileNames
		{
			get { return m_dlg.FileNames; }
		}

		public string Filter
		{
			get { return m_dlg.Filter; }
			set { m_dlg.Filter = value; }
		}

		public int FilterIndex
		{
			get { return m_dlg.FilterIndex; }
			set { m_dlg.FilterIndex = value; }
		}

		public string InitialDirectory
		{
			get { return m_dlg.InitialDirectory; }
			set { m_dlg.InitialDirectory = value; }
		}

		public bool RestoreDirectory
		{
			get { return m_dlg.RestoreDirectory; }
			set { m_dlg.RestoreDirectory = value; }
		}

		public bool ShowHelp
		{
			get { return m_dlg.ShowHelp; }
			set { m_dlg.ShowHelp = value; }
		}

		public bool SupportMultiDottedExtensions
		{
			get { return m_dlg.SupportMultiDottedExtensions; }
			set { m_dlg.SupportMultiDottedExtensions = value; }
		}

		public string Title
		{
			get { return m_dlg.Title; }
			set { m_dlg.Title = value; }
		}

		public bool ValidateNames
		{
			get { return m_dlg.ValidateNames; }
			set { m_dlg.ValidateNames = value; }
		}
		#endregion

		#region Disposable stuff
		#if DEBUG
		/// <summary/>
		~FileDialogAdapter()
		{
			Dispose(false);
		}
		#endif

		/// <summary/>
		public bool IsDisposed { get; private set; }

		/// <summary/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary/>
		protected virtual void Dispose(bool fDisposing)
		{
			System.Diagnostics.Debug.WriteLineIf(!fDisposing, "****** Missing Dispose() call for " + GetType() + ". *******");
			if (fDisposing && !IsDisposed)
			{
				// dispose managed and unmanaged objects
				var disposable = m_dlg as IDisposable;
				if (disposable != null)
					disposable.Dispose();
			}
			m_dlg = null;
			IsDisposed = true;
		}
		#endregion
	}
}
