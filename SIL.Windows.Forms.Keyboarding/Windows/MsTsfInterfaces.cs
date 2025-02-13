// Copyright (c) 2013-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SIL.Windows.Forms.Keyboarding.Windows
{
#region Structs used by COM API
	// Pack 0 so that it works for both 64 and 32bit
	[StructLayout(LayoutKind.Sequential, Pack = 0)]
	internal struct TfInputProcessorProfile
	{
		public TfProfileType ProfileType;
		public ushort LangId;
		public Guid ClsId;
		public Guid GuidProfile;
		public Guid CatId;
		public IntPtr HklSubstitute;
		public TfIppCaps Caps;
		public IntPtr Hkl;
		public TfIppFlags Flags;
	}

	// Pack 0 so that it works for both 64 and 32bit
	[StructLayout(LayoutKind.Sequential, Pack = 0)]
	internal struct TfLanguageProfile
	{
		public Guid ClsId;
		public ushort LangId;
		public Guid CatId;
		public bool IsActive;
		public Guid GuidProfile;
	}
#endregion

#region Enums
	internal enum TfProfileType : uint
	{
		Illegal = 0,
		InputProcessor = 1,
		KeyboardLayout = 2
	}

	[Flags]
	internal enum TfIppFlags : uint
	{
		Active = 1,
		Enabled = 2,
		SubstitutedByInputProcessor = 4
	}

	[Flags]
	internal enum TfIppCaps : uint
	{
		DisableOnTransitory = 0x00000001,
		SecureModeSupport = 0x00000002,
		UiElementEnabled = 0x00000004,
		ComlessSupport = 0x00000008,
		Wow16Support = 0x00000010
	}

	[Flags]
	internal enum TfIppMf : uint
	{
		ForProcess = 0x10000000,
		ForSession = 0x20000000,
		ForSystemAll = 0x40000000,
		EnableProfile = 0x00000001,
		DisableProfile = 0x00000002,
		DontCareCurrentInputLanguage = 0x00000004,
	}
#endregion

#region ITfInputProcessorProfiles
	/// <summary>
	/// These interfaces are now only used to retrieve nice keyboard names. All of the other keyboard switching is handled
	/// through .NET APIs (CurrentCulture)
	/// </summary>
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("1F02B6C5-7842-4EE6-8A0B-9A24183A95CA")]
	internal interface ITfInputProcessorProfiles
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Register(ref Guid rclsid);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Unregister(ref Guid rclsid);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void AddLanguageProfile(ref Guid rclsid, ushort langid, ref Guid guidProfile,
			[MarshalAs(UnmanagedType.LPWStr)] string desc, uint cchDesc,
			[MarshalAs(UnmanagedType.LPWStr)] string iconFile, uint cchFile, uint uIconIndex);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void RemoveLanguageProfile(ref Guid rclsid, ushort langid, ref Guid guidProfile);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		/* IEnumGUID */
		object EnumInputProcessorInfo();

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Struct)]
		Guid GetDefaultLanguageProfile(ushort langid, ref Guid catid, out Guid clsid);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SetDefaultLanguageProfile(ushort langid, ref Guid rclsid, ref Guid guidProfiles);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void ActivateLanguageProfile(ref Guid rclsid, ushort langid, ref Guid guidProfiles);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Struct)]
		Guid GetActiveLanguageProfile(ref Guid rclsid, out ushort langid);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.BStr)]
		string GetLanguageProfileDescription(ref Guid rclsid, ushort langid, ref Guid guidProfile);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		ushort GetCurrentLanguage();

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void ChangeCurrentLanguage(ushort langid);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetLanguageList(out IntPtr langIds);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IEnumTfLanguageProfiles EnumLanguageProfiles(ushort langid);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void EnableLanguageProfile(ref Guid rclsid, ushort langid, ref Guid guidProfile, [MarshalAs(UnmanagedType.VariantBool)] bool fEnable);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.VariantBool)]
		bool IsEnabledLanguageProfile(ref Guid rclsid, ushort langid, ref Guid guidProfile);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void EnableLanguageProfileByDefault(ref Guid rclsid, ushort langid, ref Guid guidProfile, [MarshalAs(UnmanagedType.VariantBool)] bool fEnable);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void SubstituteKeyboardLayout(ref Guid rclsid, ushort langid, ref Guid guidProfile, uint hkl);
	}

	/// <summary>
	/// InputProcessorProfiles Class
	/// </summary>
	[ComImport]
	[ClassInterface(ClassInterfaceType.None)]
	[TypeLibType(TypeLibTypeFlags.FCanCreate)]
	[Guid("33c53a50-f456-4884-b049-85fd643ecfed")]
	internal class TfInputProcessorProfilesClass : ITfInputProcessorProfiles
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern void Register(ref Guid rclsid);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern void Unregister(ref Guid rclsid);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern void AddLanguageProfile(ref Guid rclsid, ushort langid, ref Guid guidProfile,
			[MarshalAs(UnmanagedType.LPWStr)] string desc, uint cchDesc,
			[MarshalAs(UnmanagedType.LPWStr)] string iconFile, uint cchFile, uint uIconIndex);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern void RemoveLanguageProfile(ref Guid rclsid, ushort langid, ref Guid guidProfile);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		public virtual extern /* IEnumGUID */ object EnumInputProcessorInfo();

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Struct)]
		public virtual extern Guid GetDefaultLanguageProfile(ushort langid, ref Guid catid, out Guid clsid);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern void SetDefaultLanguageProfile(ushort langid, ref Guid rclsid, ref Guid guidProfiles);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern void ActivateLanguageProfile(ref Guid rclsid, ushort langid, ref Guid guidProfiles);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Struct)]
		public virtual extern Guid GetActiveLanguageProfile(ref Guid rclsid, out ushort langid);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.BStr)]
		public virtual extern string GetLanguageProfileDescription(ref Guid rclsid, ushort langid, ref Guid guidProfile);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern ushort GetCurrentLanguage();

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern void ChangeCurrentLanguage(ushort langid);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern int GetLanguageList(out IntPtr langIds);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		public virtual extern IEnumTfLanguageProfiles EnumLanguageProfiles(ushort langid);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern void EnableLanguageProfile(ref Guid rclsid, ushort langid, ref Guid guidProfile, [MarshalAs(UnmanagedType.VariantBool)] bool fEnable);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.VariantBool)]
		public virtual extern bool IsEnabledLanguageProfile(ref Guid rclsid, ushort langid, ref Guid guidProfile);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern void EnableLanguageProfileByDefault(ref Guid rclsid, ushort langid, ref Guid guidProfile, [MarshalAs(UnmanagedType.VariantBool)] bool fEnable);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern void SubstituteKeyboardLayout(ref Guid rclsid, ushort langid, ref Guid guidProfile, uint hkl);
	}
#endregion

#region ITfInputProcessorProfileMgr
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("71c6e74c-0f28-11d8-a82a-00065b84435c")]
	internal interface ITfInputProcessorProfileMgr
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void ActivateProfile(TfProfileType dwProfileType, ushort langid, ref Guid clsid, ref Guid guidProfile, IntPtr hkl, TfIppMf dwFlags);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void DeactivateProfile(TfProfileType dwProfileType, ushort langid, ref Guid clsid, ref Guid guidProfile, IntPtr hkl, TfIppMf dwFlags);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Struct)]
		TfInputProcessorProfile GetProfile(TfProfileType dwProfileType, ushort langid, ref Guid clsid, ref Guid guidProfile, IntPtr hkl);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IEnumTfInputProcessorProfiles EnumProfiles(short langid);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void ReleaseInputProcessor(ref Guid rclsid, uint dwFlags);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void RegisterProfile(ref Guid rclsid, ushort langid, ref Guid guidProfile,
			[MarshalAs(UnmanagedType.LPWStr)] string desc, uint cchDesc,
			[MarshalAs(UnmanagedType.LPWStr)] string iconFile, uint cchFile,
			uint uIconIndex, uint hklSubstitute, uint dwPreferredLayout,
			[MarshalAs(UnmanagedType.VariantBool)] bool bEnabledByDefault, uint dwFlags);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void UnregisterProfile(Guid rclsid, ushort langid, Guid guidProfile, uint dwFlags);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Struct)]
		TfInputProcessorProfile GetActiveProfile(ref Guid catid);
	}
#endregion

#region IEnumTfInputProcessorProfiles
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("71c6e74d-0f28-11d8-a82a-00065b84435c")]
	internal interface IEnumTfInputProcessorProfiles
	{

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IEnumTfInputProcessorProfiles Clone();

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int Next(uint ulCount, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] TfInputProcessorProfile[] profiles);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Reset();

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Skip(uint ulCount);
	}
#endregion

#region IEnumTfLanguageProfiles
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("3d61bf11-ac5f-42c8-a4cb-931bcc28c744")]
	internal interface IEnumTfLanguageProfiles
	{

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IEnumTfLanguageProfiles Clone();

		// We cheat with this method. Usually it takes an array of TfLanguageProfile as
		// second parameter, but that is harder to marshal and since we always use it with
		// ulCount==1 we simply pass only one TfLanguageProfile.
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int Next(uint ulCount, out TfLanguageProfile profiles);
		//int Next(uint ulCount, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] TfLanguageProfile[] profiles);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Reset();

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Skip(uint ulCount);
	}
#endregion

#region ITfLanguageProfileNotifySink
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("43c9fe15-f494-4c17-9de2-b8a4ac350aa8")]
	internal interface ITfLanguageProfileNotifySink
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		//[return: MarshalAs(UnmanagedType.Bool)]
		bool OnLanguageChange(ushort langid);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void OnLanguageChanged();
	}
#endregion

#region ITfSource
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("4ea48a35-60ae-446f-8fd6-e6a8d82459f7")]
	internal interface ITfSource
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.U2)]
		ushort AdviseSink(ref Guid riid,
			[MarshalAs(UnmanagedType.Interface)]
			object punk);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void UnadviseSink(ushort dwCookie);
	}
#endregion

#region GUIDs
	internal class Guids
	{
		// in the COM interface definitions we have to use `ref Guid` so that the Guid gets
		// marshalled correctly. However, that prevents using constants. The workaround that I
		// came up with is to introduce the `Consts` property and return a new `Guids` object
		// every time. On Windows we got away without the `ref` when calling the method (although
		// our method definition had the `ref`), but that doesn't work on Linux.

		public static Guids Consts => new Guids();

		// Values from Wine project (http://source.winehq.org/git/wine.git/blob/HEAD:/dlls/uuid/uuid.c)
		public Guid TfcatTipKeyboard = new Guid(0x34745c63, 0xb2f0, 0x4784, 0x8b, 0x67, 0x5e, 0x12, 0xc8, 0x70, 0x1a, 0x31);
		public Guid TfcatTipSpeech = new Guid(0xB5A73CD1, 0x8355, 0x426B, 0xA1, 0x61, 0x25, 0x98, 0x08, 0xF2, 0x6B, 0x14);
		public Guid TfcatTipHandwriting = new Guid(0x246ecb87, 0xc2f2, 0x4abe, 0x90, 0x5b, 0xc8, 0xb3, 0x8a, 0xdd, 0x2c, 0x43);
		public Guid TfcatDisplayAttributeProvider = new Guid(0x046B8C80, 0x1647, 0x40F7, 0x9B, 0x21, 0xB9, 0x3B, 0x81, 0xAA, 0xBC, 0x1B);

		public Guid ITfLanguageProfileNotifySink = new Guid("43c9fe15-f494-4c17-9de2-b8a4ac350aa8");
	}
#endregion
}
