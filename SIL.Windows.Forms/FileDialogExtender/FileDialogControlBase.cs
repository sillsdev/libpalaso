//  Copyright (c) 2006, Gustavo Franco
//  Copyright © Decebal Mihailescu 2007-2010

//  Email:  gustavo_franco@hotmail.com
//  All rights reserved.

//  Redistribution and use in source and binary forms, with or without modification,
//  are permitted provided that the following conditions are met:

//  Redistributions of source code must retain the above copyright notice,
//  this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice,
//  this list of conditions and the following disclaimer in the documentation
//  and/or other materials provided with the distribution.

//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER
//  REMAINS UNCHANGED.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JetBrains.Annotations;
using static System.Diagnostics.Process;

namespace SIL.Windows.Forms.FileDialogExtender
{
	#region Base class

	public partial class FileDialogControlBase : UserControl//, IMessageFilter
	{
		#region Delegates
		public delegate void PathChangedEventHandler(IWin32Window sender, string filePath);
		public delegate void FilterChangedEventHandler(IWin32Window sender, int index);
		#endregion

		#region Events
		//for weird reasons the designer wants the events public not protected
		[Category("FileDialogExtenders")]
		public event PathChangedEventHandler EventFileNameChanged;
		[Category("FileDialogExtenders")]
		public event PathChangedEventHandler EventFolderNameChanged;
		[Category("FileDialogExtenders")]
		public event FilterChangedEventHandler EventFilterChanged;
		[Category("FileDialogExtenders")]
		public event CancelEventHandler EventClosingDialog;
		#endregion

		#region Constants Declaration
		private const SetWindowPosFlags UFLAGSHIDE =
			SetWindowPosFlags.SWP_NOACTIVATE |
			SetWindowPosFlags.SWP_NOOWNERZORDER |
			SetWindowPosFlags.SWP_NOMOVE |
			SetWindowPosFlags.SWP_NOSIZE |
			SetWindowPosFlags.SWP_HIDEWINDOW;
		#endregion

		#region Variables Declaration

		NativeWindow _dlgWrapper;
		private AddonWindowLocation _StartLocation = AddonWindowLocation.Right;
		IntPtr _hFileDialogHandle = IntPtr.Zero;
		string _InitialDirectory = string.Empty;
		string _Filter = "All files (*.*)|*.*";
		string _DefaultExt = "jpg";
		string _FileName = string.Empty;
		int _FilterIndex = 1;
		bool _AddExtension = true;
		bool _CheckFileExists = true;
		bool _EnableOkBtn = true;
		bool _DereferenceLinks = true;
		bool _ShowHelp;
		RECT _OpenDialogWindowRect;
		IntPtr _hOKButton = IntPtr.Zero;
		private bool _hasRunInitMSDialog;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
		IntPtr _hListViewPtr;

		#endregion

		#region Constructors
		public FileDialogControlBase()
		{
			InitializeComponent();
		}
		#endregion

		#region Properties

		internal static uint OriginalDlgWidth { get; set; }

		internal static uint OriginalDlgHeight { get; set; }

		[Browsable(false)]
		[PublicAPI]
		public string[] FileDlgFileNames => DesignMode ? null : MSDialog.FileNames;

		[Browsable(false)]
		public FileDialog MSDialog { set; get; }

		[Category("FileDialogExtenders")]
		[DefaultValue(AddonWindowLocation.Right)]
		public AddonWindowLocation FileDlgStartLocation
		{
			get => _StartLocation;
			set
			{
				_StartLocation = value;
				if (DesignMode)
					Refresh();
			}
		}

		internal Size OriginalCtrlSize { get; set; }

		[Category("FileDialogExtenders")]
		[DefaultValue(FolderViewMode.Default)]
		public FolderViewMode FileDlgDefaultViewMode { get; set; } = FolderViewMode.Default;

		[Category("FileDialogExtenders")]
		[DefaultValue(FileDialogType.OpenFileDlg)]
		public FileDialogType FileDlgType { get; set; }

		[Category("FileDialogExtenders")]
		[DefaultValue("")]
		[PublicAPI]
		public string FileDlgInitialDirectory
		{
			get => DesignMode ? _InitialDirectory : MSDialog.InitialDirectory;
			set
			{
				_InitialDirectory = value;
				if (!DesignMode && MSDialog != null)
					MSDialog.InitialDirectory = value;
			}
		}

		[Category("FileDialogExtenders")]
		[DefaultValue("")]
		[PublicAPI]
		public string FileDlgFileName
		{
			get => DesignMode ? _FileName : MSDialog.FileName;
			set => _FileName = value;
		}

		[Category("FileDialogExtenders")]
		[DefaultValue("")]
		public string FileDlgCaption { get; set; } = "Save";

		[Category("FileDialogExtenders")]
		[DefaultValue("&Open")]
		public string FileDlgOkCaption { get; set; } = "&Open";

		[Category("FileDialogExtenders")]
		[DefaultValue("jpg")]
		[PublicAPI]
		public string FileDlgDefaultExt
		{
			get => DesignMode ? _DefaultExt : MSDialog.DefaultExt;
			set => _DefaultExt = value;
		}

		[Category("FileDialogExtenders")]
		[DefaultValue("All files (*.*)|*.*")]
		[PublicAPI]
		public string FileDlgFilter
		{
			get => DesignMode ? _Filter : MSDialog.Filter;
			set => _Filter = value;
		}

		[Category("FileDialogExtenders")]
		[DefaultValue(1)]
		[PublicAPI]
		public int FileDlgFilterIndex
		{
			get => DesignMode ? _FilterIndex : MSDialog.FilterIndex;
			set => _FilterIndex = value;
		}

		[Category("FileDialogExtenders")]
		[DefaultValue(true)]
		[PublicAPI]
		public bool FileDlgAddExtension
		{
			get => DesignMode ? _AddExtension : MSDialog.AddExtension;
			set => _AddExtension = value;
		}

		[Category("FileDialogExtenders")]
		[DefaultValue(true)]
		public bool FileDlgEnableOkBtn
		{
			get => _EnableOkBtn;
			set
			{
				_EnableOkBtn = value;
				if (!DesignMode && MSDialog != null && _hOKButton != IntPtr.Zero)
					NativeMethods.EnableWindow(_hOKButton, _EnableOkBtn);
			}
		}

		[Category("FileDialogExtenders")]
		[DefaultValue(true)]
		[PublicAPI]
		public bool FileDlgCheckFileExists
		{
			get => DesignMode ? _CheckFileExists : MSDialog.CheckFileExists;
			set => _CheckFileExists = value;
		}

		[Category("FileDialogExtenders")]
		[DefaultValue(false)]
		[PublicAPI]
		public bool FileDlgShowHelp
		{
			get => DesignMode ? _ShowHelp : MSDialog.ShowHelp;
			set => _ShowHelp = value;
		}

		[Category("FileDialogExtenders")]
		[DefaultValue(true)]
		[PublicAPI]
		public bool FileDlgDereferenceLinks
		{
			get => DesignMode ? _DereferenceLinks : MSDialog.DereferenceLinks;
			set => _DereferenceLinks = value;
		}
		#endregion

		#region Virtuals
		//this is a hidden child window dor the whole dialog
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignMode && MSDialog != null)
			{
				MSDialog.FileOk += FileDialogControlBase_ClosingDialog;
				MSDialog.Disposed += FileDialogControlBase_DialogDisposed;
				MSDialog.HelpRequest += FileDialogControlBase_HelpRequest;
				FileDlgEnableOkBtn = _EnableOkBtn;//that's designed time value
				NativeMethods.SetWindowText(new HandleRef(_dlgWrapper,_dlgWrapper.Handle), FileDlgCaption);
				//will work only for open dialog, save dialog will be overriden internally by windows
				NativeMethods.SetWindowText(new HandleRef(this,_hOKButton), FileDlgOkCaption);//SetDlgItemText fails too
				//bool res = NativeMethods.SetDlgItemText(NativeMethods.GetParent(Handle), (int)ControlsId.ButtonOk, FileDlgOkCaption);
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (IsDisposed)
				return;
			if (MSDialog != null)
			{
				MSDialog.FileOk -= FileDialogControlBase_ClosingDialog;
				MSDialog.Disposed -= FileDialogControlBase_DialogDisposed;
				MSDialog.HelpRequest -= FileDialogControlBase_HelpRequest;
				MSDialog.Dispose();
				MSDialog = null;
			}
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		public virtual void OnFileNameChanged(IWin32Window sender, string fileName)
		{
			EventFileNameChanged?.Invoke(sender, fileName);
		}

		public void OnFolderNameChanged(IWin32Window sender, string folderName)
		{
			EventFolderNameChanged?.Invoke(sender, folderName);
			UpdateListView();
		}

		private void UpdateListView()
		{
			_hListViewPtr = NativeMethods.GetDlgItem(_hFileDialogHandle, (int)ControlsId.DefaultView);
			if (FileDlgDefaultViewMode != FolderViewMode.Default &&
			    _hFileDialogHandle != IntPtr.Zero)
			{
				NativeMethods.SendMessage(new HandleRef(this, _hListViewPtr), (int)Msg.WM_COMMAND,
					(IntPtr)(int)FileDlgDefaultViewMode, IntPtr.Zero);
			}
		}

		internal void OnFilterChanged(IWin32Window sender, int index)
		{
			EventFilterChanged?.Invoke(sender, index);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			if (DesignMode)
			{
				Graphics gr = e.Graphics;
				{
					HatchBrush hb = null;
					Pen p = null;
					try
					{
						switch (FileDlgStartLocation)
						{
							case AddonWindowLocation.Right:
								hb = new HatchBrush(HatchStyle.NarrowHorizontal, Color.Black, Color.Red);
								p = new Pen(hb, 5);
								gr.DrawLine(p, 0, 0, 0, Height);
								break;
							case AddonWindowLocation.Bottom:
								hb = new HatchBrush(HatchStyle.NarrowVertical, Color.Black, Color.Red);
								p = new Pen(hb, 5);
								gr.DrawLine(p, 0, 0, Width, 0);
								break;
							case AddonWindowLocation.BottomRight:
							default:
								hb = new HatchBrush(HatchStyle.Sphere, Color.Black, Color.Red);
								p = new Pen(hb, 5);
								gr.DrawLine(p, 0, 0, 4, 4);
								break;
						}
					}
					finally
					{
						p?.Dispose();
						hb?.Dispose();
					}
				}
			}
			base.OnPaint(e);
		}


		#endregion

		#region Methods
		public DialogResult ShowDialog()
		{
			return ShowDialog(null);
		}
		protected virtual void OnPrepareMSDialog()
		{
			InitMSDialog();
		}
		private void InitMSDialog()
		{
			MSDialog.InitialDirectory = _InitialDirectory.Length == 0 ? Path.GetDirectoryName(Application.ExecutablePath) : _InitialDirectory;
			MSDialog.AddExtension = _AddExtension;
			MSDialog.Filter = _Filter;
			MSDialog.FilterIndex = _FilterIndex;
			MSDialog.CheckFileExists = _CheckFileExists;
			MSDialog.DefaultExt = _DefaultExt;
			MSDialog.FileName = _FileName;
			MSDialog.DereferenceLinks = _DereferenceLinks;
			MSDialog.ShowHelp = _ShowHelp;
			_hasRunInitMSDialog = true;
		}

		public DialogResult ShowDialog(IWin32Window owner)
		{
			var returnDialogResult = DialogResult.Cancel;
			if (IsDisposed)
				return returnDialogResult;
			if (owner == null || owner.Handle == IntPtr.Zero)
				owner = new WindowWrapper(GetCurrentProcess().MainWindowHandle);

			OriginalCtrlSize = Size;
			MSDialog = FileDlgType == FileDialogType.OpenFileDlg ? new OpenFileDialog() :
				new SaveFileDialog() as FileDialog;
			_dlgWrapper = new WholeDialogWrapper(this);
			OnPrepareMSDialog();
			if (!_hasRunInitMSDialog)
				InitMSDialog();
			try
			{
				var autoUpgradeEnabledPropInfo =
					MSDialog.GetType().GetProperty("AutoUpgradeEnabled");
				autoUpgradeEnabledPropInfo?.SetValue(MSDialog, false, null);
				returnDialogResult = MSDialog.ShowDialog(owner);
			}
			catch (ObjectDisposedException)
			{
				// Sometimes if you open an animated .gif on the preview and the Form is closed, an
				// exception is thrown. Let's ignore this exception and keep closing the form.
			}
			catch (Exception ex)
			{
				MessageBox.Show("unable to get the modal dialog handle", ex.Message);
			}

			return returnDialogResult;
		}

		internal DialogResult ShowDialogExt(FileDialog fileDlg, IWin32Window owner)
		{
			var returnDialogResult = DialogResult.Cancel;
			if (IsDisposed)
				return returnDialogResult;
			if (owner == null || owner.Handle == IntPtr.Zero)
				owner = new WindowWrapper(GetCurrentProcess().MainWindowHandle);

			OriginalCtrlSize = Size;
			MSDialog = fileDlg;
			_dlgWrapper = new WholeDialogWrapper(this);

			try
			{
				var autoUpgradeInfo = MSDialog.GetType().GetProperty("AutoUpgradeEnabled");
				autoUpgradeInfo?.SetValue(MSDialog, false, null);
				returnDialogResult = MSDialog.ShowDialog(owner);
			}
			catch (ObjectDisposedException)
			{
				// Sometimes if you open an animated .gif on the preview and the Form is closed, an
				// exception is thrown. Let's ignore this exception and keep closing the form.
			}
			catch (Exception ex)
			{
				MessageBox.Show("unable to get the modal dialog handle", ex.Message);
			}
			return returnDialogResult;
		}
		#endregion


		#region event handlers
		void FileDialogControlBase_DialogDisposed(object sender, EventArgs e)
		{
			Dispose(true);
		}

		private void FileDialogControlBase_ClosingDialog(object sender, CancelEventArgs e)
		{
			if (EventClosingDialog != null)
			{
				EventClosingDialog(this, e);
			}
		}


		void FileDialogControlBase_HelpRequest(object sender, EventArgs e)
		{
			//this is a virtual call that should call the event in the subclass
			OnHelpRequested(new HelpEventArgs(new Point()));
		}

		#endregion
		#region helper types

		public class WindowWrapper : IWin32Window
		{
			public WindowWrapper(IntPtr handle)
			{
				Handle = handle;
			}

			public IntPtr Handle { get; }
		}
		#endregion
	}
	#endregion


}