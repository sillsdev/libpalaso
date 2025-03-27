using System;
using System.IO;
using System.Windows.Forms;

namespace SIL.Media.SoundFieldControlTestApp
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
//            _recorder = new AudioIrrKlangSession(_path);
			InitializeComponent();
			soundFieldControl1.Path = Path.GetTempFileName().Replace(".tmp",".wav");
//            UpdateScreen();
//            timer1.Enabled = true;


	/*didn't work       var map = new ColorMap[1];
			map[0] =new ColorMap();
			map[0].OldColor = Color.Black;
			map[0].NewColor = Color.Red;
			bitmapButton1.ImageAttributes.SetGamma(.2f);
			bitmapButton1.ImageAttributes.SetBrushRemapTable(map);
	 */
		}

	  /*  protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);


			Image image = new Bitmap(@"C:\dev\HearThis\artwork\test.bmp");

			var attrs = new ImageAttributes();
			var map = new ColorMap[1];
			map[0] = new ColorMap();
			map[0].OldColor = Color.Red;
			map[0].NewColor = Color.Blue;

			attrs.SetBrushRemapTable(map);
		   e.Graphics.DrawImage(image, new Rectangle(20, 100, 200, 200), 0, 0, 48, 48, GraphicsUnit.Pixel, attrs);
		}
		*/

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
//        private void MainForm_Load(object sender, EventArgs e)
//        {
//           // _recorder.Test();
//        }
	}
}