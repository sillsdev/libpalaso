using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using SIL.Reporting;

namespace SIL.Windows.Forms.ImageToolbox.ImageGallery
{
	/// <summary>
	/// Thumbnail viewer is a this wrapper around either a ListViewThumbnailViewer or a WebThumbnailViewer
	/// (implemented in SIL.Windows.Forms.GeckoBrowserAdapter), which must be used by clients
	/// that are using GeckoFx.
	/// </summary>
	public class ThumbnailViewer : UserControl
	{
		private IThumbnailViewer _thumbnailViewer;

		public static bool UseWebViewer { get; set; }

		public ThumbnailViewer()
		{
			_thumbnailViewer = CreateViewer();
			_thumbnailViewer.SelectedIndexChanged += (sender, args) =>
			{
				if (SelectedIndexChanged != null)
					SelectedIndexChanged(sender, args);
			};
			_thumbnailViewer.LoadComplete += (sender, args) =>
			{
				if (LoadComplete != null)
					LoadComplete(sender, args);
			};
			_thumbnailViewer.TheControl.Dock = DockStyle.Fill;
			Controls.Add(_thumbnailViewer.TheControl);
			Clear(); // BL-2901 prevents thumbnail viewer from having a black background when displayed
		}

		public IThumbnailViewer CreateViewer()
		{
			if (UseWebViewer)
			{
				var path = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).LocalPath),
					"SIL.Windows.Forms.GeckoBrowserAdapter.dll");
				if (File.Exists(path))
				{
					var assembly = Assembly.LoadFile(path);
					if (assembly != null)
					{
						var browser = assembly.GetType("SIL.Windows.Forms.GeckoBrowserAdapter.WebThumbnailViewer");
						if (browser != null)
						{
							try
							{
								return (IThumbnailViewer)Activator.CreateInstance(browser);
							}
							catch (Exception e)
							{
								Logger.WriteMinorEvent("Could not create gecko viewer" + e);
#if DEBUG
								throw new Exception("Could not create gecko viewer", e);
#endif
								//Eat exceptions creating the GeckoFxWebBrowserAdapter
							}
						}
					}
				}
				Debug.Fail("could not create gecko-based thumbnail viewer");
			}
			// If we can't make a gecko one for any reason, the default one is only slightly imperfect.
			return new ListViewThumbnailViewer();
		}
		public Func<string, string> CaptionMethod
		{
			get { return _thumbnailViewer.CaptionMethod; }
			set { _thumbnailViewer.CaptionMethod = value; }
		}
		public void Clear() { _thumbnailViewer.Clear();}
		public void Closing() { _thumbnailViewer.Closing();}

		private bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (disposed)
				return;
			disposed = true;
			if (disposing)
			{
				if (_thumbnailViewer != null)
				{
					_thumbnailViewer.Dispose();
					_thumbnailViewer = null;
				}
			}
			base.Dispose(disposing);
		}

		public void LoadItems(IEnumerable<string> pathList) { _thumbnailViewer.LoadItems(pathList);}
		public bool HasSelection {
			get { return _thumbnailViewer.HasSelection; }
		}
		public string SelectedPath { get { return _thumbnailViewer.SelectedPath; } }
		public bool CanLoad { get { return _thumbnailViewer.CanLoad; } }

		public Color ThumbBorderColor
		{
			get { return _thumbnailViewer.ThumbBorderColor; }
			set { _thumbnailViewer.ThumbBorderColor = value; }
		}

		public int ThumbNailSize
		{
			get { return _thumbnailViewer.ThumbNailSize; }
			set { _thumbnailViewer.ThumbNailSize = value; }
		}

		public event EventHandler SelectedIndexChanged;
		public event EventHandler LoadComplete;
	}

	public interface IThumbnailViewer : IDisposable
	{
		Control TheControl { get; }
		Func<string, string> CaptionMethod { get; set; }
		void Clear();
		void Closing();
		void LoadItems(IEnumerable<string> pathList);
		bool HasSelection { get; }
		string SelectedPath { get; }
		bool CanLoad { get; }
		Color ThumbBorderColor { get; set; }
		int ThumbNailSize { get; set; }
		event EventHandler SelectedIndexChanged;
		event EventHandler LoadComplete;
	}
}
