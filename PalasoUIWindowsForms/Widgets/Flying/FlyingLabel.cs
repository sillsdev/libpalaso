using System;
using System.Drawing;
using System.Windows.Forms;

namespace WeSay.UI
{
	public class MovingLabel: Label
	{
		private Point _to;
		private Point _from;
		private Animator _animator;
		public event EventHandler Finished;

		public MovingLabel()
		{
			Visible = false;
			BackColor = Color.Transparent; //didn't work
			InitializeAnimator();
		}

		private void InitializeAnimator()
		{
			_animator = new Animator();
			CubicBezierCurve c = new CubicBezierCurve(new PointF(0, 0),
													  new PointF(0.5f, 0f),
													  new PointF(.5f, 1f),
													  new PointF(1, 1));
			_animator.PointFromDistanceFunction = c.GetPointOnCurve;

			_animator.Duration = 200; // 750;
			_animator.FrameRate = 30;
			_animator.SpeedFunction = Animator.SpeedFunctions.SinSpeed;
			_animator.Animate += OnAnimator_Animate;
			_animator.Finished += OnAnimator_Finished;
		}

		private void OnAnimator_Animate(object sender, Animator.AnimatorEventArgs e)
		{
			Location = new Point(Animator.GetValue(e.Point.X, _from.X, _to.X),
								 Animator.GetValue(e.Point.Y, _from.Y, _to.Y));
		}

		private void OnAnimator_Finished(object sender, EventArgs e)
		{
			Visible = false;
			_animator.Stop();
			_animator.Reset();
			if (Finished != null)
			{
				Finished.Invoke(this, null);
			}
		}

		public void Go(string word, Point from, Point to)
		{
			_from = from;
			_to = to;

			Text = word;
			Location = from;
			Visible = true;
			BringToFront();
			_animator.Start();
		}
	}
}