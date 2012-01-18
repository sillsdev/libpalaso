//  Copyright (c) 2006, Gustavo Franco
//  Copyright © Decebal Mihailescu 2010

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
using System.Windows.Forms;
using System.ComponentModel;
using Microsoft.Win32;

namespace Palaso.UI.WindowsForms.FileDialogExtender
{
	public static class Extensions
	{
		#region extension Methods
		public static DialogResult ShowDialog(this FileDialog fdlg, FileDialogControlBase ctrl, IWin32Window owner) //where T : FileDialogControlBase, new()
		{
			ctrl.FileDlgType =(fdlg is SaveFileDialog)?FileDialogType.SaveFileDlg: FileDialogType.OpenFileDlg;
			if (ctrl.ShowDialogExt(fdlg, owner) == DialogResult.OK)
				return DialogResult.OK;
			else
				return DialogResult.Ignore;
		}
		#endregion
	}

	//see http://msdn.microsoft.com/en-us/magazine/cc300434.aspx
	public static class FileDialogPlaces
	{
		private static readonly string TempKeyName = "TempPredefKey_" + Guid.NewGuid().ToString();
		private const string Key_PlacesBar = @"Software\Microsoft\Windows\CurrentVersion\Policies\ComDlg32\PlacesBar";
		private static RegistryKey _fakeKey;
		private static IntPtr _overriddenKey;
		private static object[] m_places;

		public static void SetPlaces(this FileDialog fd, object[] places)
		{
			if (fd == null)
				return;
			if (m_places == null)
				m_places = new object[5];
			else
				m_places.Initialize();

			if (places != null)
			{
				for (int i = 0; i < m_places.GetLength(0); i++)
				{
					m_places[i] = places[i];

				}
			}
			if (_fakeKey != null)
				ResetPlaces(fd);
			SetupFakeRegistryTree();
			if (fd != null)
				fd.Disposed += (object sender, EventArgs e) => { if (m_places != null && fd != null) ResetPlaces(fd); };
		}

		static FileDialogPlaces()
		{
		}

		static public void ResetPlaces(this FileDialog fd)
		{
			if (_overriddenKey != IntPtr.Zero)
			{
				ResetRegistry(_overriddenKey);
				_overriddenKey = IntPtr.Zero;
			}
			if (_fakeKey != null)
			{
				_fakeKey.Close();
				_fakeKey = null;
			}
			//delete the key tree
			Registry.CurrentUser.DeleteSubKeyTree(TempKeyName);
			m_places = null;
		}

		private static void SetupFakeRegistryTree()
		{
			_fakeKey = Registry.CurrentUser.CreateSubKey(TempKeyName);
			_overriddenKey = InitializeRegistry();

			// at this point, m_TempKeyName equals places key
			// write dynamic places here reading from Places
			RegistryKey reg = Registry.CurrentUser.CreateSubKey(Key_PlacesBar);
			for (int i = 0; i < m_places.GetLength(0); i++)
			{
				if (m_places[i] != null)
				{
					reg.SetValue("Place" + i.ToString(), m_places[i]);
				}
			}
		}

		//public static IntPtr GetRegistryHandle(RegistryKey registryKey)
		//{
		//    Type type = registryKey.GetType();
		//    FieldInfo fieldInfo = type.GetField("hkey", BindingFlags.Instance | BindingFlags.NonPublic);
		//    return (IntPtr)fieldInfo.GetValue(registryKey);
		//}
		static readonly UIntPtr HKEY_CURRENT_USER = new UIntPtr(0x80000001u);
		private static IntPtr InitializeRegistry()
		{
			IntPtr hkMyCU;
			NativeMethods.RegCreateKeyW(HKEY_CURRENT_USER, TempKeyName, out hkMyCU);
			NativeMethods.RegOverridePredefKey(HKEY_CURRENT_USER, hkMyCU);
			return hkMyCU;
		}


		static void ResetRegistry(IntPtr hkMyCU)
		{
			NativeMethods.RegOverridePredefKey(HKEY_CURRENT_USER, IntPtr.Zero);
			NativeMethods.RegCloseKey(hkMyCU);
			return;
		}

	}

	public enum Places
	{
		[Description("Desktop")]
		Desktop = 0,
		[Description("Program Files")]
		Programs = 2,
		[Description("Control Panel")]
		ControlPanel = 3,
		[Description("Printers")]
		Printers = 4,
		[Description("My Documents")]
		MyDocuments = 5,
		[Description("Favorites")]
		Favorites = 6,
		[Description("Startup folder")]
		StartupFolder = 7,
		[Description("Recent Files")]
		RecentFiles = 8,
		[Description("Send To")]
		SendTo = 9,
		[Description("Recycle Bin")]
		RecycleBin = 10,
		[Description("Start menu")]
		StartMenu = 12,
		[Description("My Computer")]
		MyComputer = 17,
		[Description("My Network Places")]
		MyNetworkPlaces = 18,
		[Description("Fonts")]
		Fonts = 20
	}
}
