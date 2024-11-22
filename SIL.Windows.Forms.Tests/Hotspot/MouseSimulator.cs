using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SIL.Reflection;
using SIL.Windows.Forms.HotSpot;

public static class MouseSimulator
{
	private const int WM_LBUTTONDOWN = 0x0201;
	private const int WM_LBUTTONUP = 0x0202;
	private const int WM_MOUSEMOVE = 0x0200;

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

	public static void SimulateMouseDown(this HotSpot hotspot)
	{
		hotspot.Control.SimulateMouseDown(GetPositionFromCharIndex(hotspot.Control, hotspot.Offset));
	}

	public static void SimulateMouseDown(this Control control, Point pt)
	{
		control.SimulateMouseDown(pt.X, pt.Y);
	}

	public static void SimulateMouseDown(this Control control, int x, int y)
	{
		SendMessage(control.Handle, WM_LBUTTONDOWN, IntPtr.Zero, MakeLParam(x, y));
	}
	
	public static void SimulateMouseUp(this HotSpot hotspot)
	{
		hotspot.Control.SimulateMouseUp(GetPositionFromCharIndex(hotspot.Control, hotspot.Offset));
	}

	public static void SimulateMouseUp(this Control control, Point pt)
	{
		control.SimulateMouseUp(pt.X, pt.Y);
	}

	public static void SimulateMouseUp(this Control control, int x, int y)
	{
		SendMessage(control.Handle, WM_LBUTTONUP, IntPtr.Zero, MakeLParam(x, y));
	}
	
	public static void SimulateMouseClick(this HotSpot hotspot)
	{
		hotspot.Control.SimulateMouseClick(GetPositionFromCharIndex(hotspot.Control, hotspot.Offset));
	}

	public static void SimulateMouseClick(this Control control, Point pt)
	{
		control.SimulateMouseClick(pt.X, pt.Y);
	}

	public static void SimulateMouseClick(this Control control, int x, int y)
	{
		SimulateMouseDown(control, x, y);
		SimulateMouseUp(control, x, y);
	}

	public static void SimulateMouseMove(this HotSpot hotspot)
	{
		hotspot.Control.SimulateMouseMove(GetPositionFromCharIndex(hotspot.Control, hotspot.Offset));
	}

	public static void SimulateMouseMove(this Control control, Point pt)
	{
		control.SimulateMouseMove(pt.X, pt.Y);
	}

	public static void SimulateMouseMove(this Control control, int x, int y)
	{
		SendMessage(control.Handle, WM_MOUSEMOVE, IntPtr.Zero, MakeLParam(x, y));
	}
	public static void SimulateMouseHover(this Control control)
	{
		ReflectionHelper.CallMethod(control, "OnMouseHover", EventArgs.Empty);
	}

	private static IntPtr MakeLParam(int x, int y)
	{
		return (IntPtr)((y << 16) | (x & 0xFFFF));
	}

	private static Point GetPositionFromCharIndex(this TextBoxBase textBox, int offset)
	{
		var position = textBox.GetPositionFromCharIndex(offset);
		// the following is necessary to work around a .Net bug
		int x = (short)position.X;
		int y = (short)position.Y;
		return new Point(x, y);
	}
}