﻿// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Drawing;
using System.Windows.Forms;
using SIL.Reporting;

namespace SIL.Windows.Forms.Widgets
{
	public class BetterLinkLabel : BetterLabel
	{
		public event LinkLabelLinkClickedEventHandler LinkClicked;

		public BetterLinkLabel()
		{
			ReadOnly = false;
			Enabled = true;
			SetStyle(ControlStyles.UserPaint, false);
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			Cursor = Cursors.Hand;
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			ForeColor = Color.Blue;//TODO
			Font = new Font(Font, FontStyle.Underline);
		}

		/// <summary>
		/// The url to launch
		/// </summary>
		public string URL { get; set; }

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			if(!string.IsNullOrEmpty(URL))
			{
				try
				{
					System.Diagnostics.Process.Start(URL);
				}
				catch(Exception)
				{
					ErrorReport.NotifyUserOfProblem(string.Format("Could not follow that link to {0}. Your computer is not set up to follow links of that kind, but you can try typing it into your web browser.",URL));
				}
			}
			else if (LinkClicked != null)
			{
				LinkClicked(this, new LinkLabelLinkClickedEventArgs(new LinkLabel.Link()));
			}
		}
	}
}

