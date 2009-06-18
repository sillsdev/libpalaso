using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Palaso.Media.Tests
{
	public partial class Form1 : Form
	{
		private readonly AudioIrrKlangSession _recorder;
		private string _path;

		public Form1()
		{
//            _recorder = new AudioIrrKlangSession(_path);
			InitializeComponent();
			soundFieldControl1.Path = Path.GetTempFileName().Replace(".tmp",".wav");
//            UpdateScreen();
//            timer1.Enabled = true;
		}

		private void shortSoundFieldControl1_Load(object sender, EventArgs e)
		{
			this.shortSoundFieldControl1.Path = Path.GetTempFileName().Replace(".tmp", ".wav");
		}

		private void shortSoundFieldControl2_Load(object sender, EventArgs e)
		{
			this.shortSoundFieldControl2.Path = Path.GetTempFileName().Replace(".tmp", ".wav");
		}

//
//        private void UpdateScreen()
//        {
//            _recordButton.Enabled = _recorder.CanRecord;
//            _stopButton.Enabled = _recorder.CanStop;
//            _playButton.Enabled = _recorder.CanPlay && File.Exists(_path);
////            _playButton.Enabled = true;
////            _stopButton.Enabled = true;
//        }
//
//        private void _recordButton_Click(object sender, EventArgs e)
//        {
//           if(File.Exists(_path))
//               File.Delete(_path);
//
//            _recorder.StartRecording();
//            UpdateScreen();
//        }
//
//        private void _playButton_Click(object sender, EventArgs e)
//        {
//            _recorder.Play();
//            UpdateScreen();
//        }
//
//        private void _stopButton_Click(object sender, EventArgs e)
//        {
//            if(_recorder.IsRecording)
//            {
//                _recorder.StopRecording();
//                //_recorder.SaveAsWav(_path);
//            }
//            else
//            {
//                _recorder.StopPlaying();
//            }
//            UpdateScreen();
//        }
//
//        private void timer1_Tick(object sender, EventArgs e)
//        {
//            UpdateScreen();
//        }
//
//        private void button1_Click(object sender, EventArgs e)
//        {
//            _recorder.StartRecording();
//            Thread.Sleep(3000);
//           _recorder.StopRecording();
//            UpdateScreen();
//            _recorder.Play();
//            UpdateScreen();
//        }
//
//        private void Form1_Load(object sender, EventArgs e)
//        {
//           // _recorder.Test();
//        }
	}

	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
	}
}