using System;
using System.Windows.Forms;

namespace LaserGRBL
{
	public partial class JogForm : System.Windows.Forms.UserControl
	{
		GrblCore Core;

		public JogForm()
		{
			InitializeComponent();
		}

		public void SetCore(GrblCore core)
		{
			Core = core;

			UpdateFMax.Enabled = true;
			UpdateFMax_Tick(null, null);

			TbSpeed.Value = Math.Min((int)Settings.GetObject("Jog Speed", 1000), TbSpeed.Maximum);

            object jogStepDecimal = Settings.GetObject("Jog Step Float", null);
            if (jogStepDecimal != null)
                TbStep.Value = JogStepToTrackbar((decimal)jogStepDecimal);
            else
                // backwards compatibility
                TbStep.Value = JogStepToTrackbar((int)Settings.GetObject("Jog Step", 10));

			TbSpeed_ValueChanged(null, null); //set tooltip
			TbStep_ValueChanged(null, null); //set tooltip

            Core.OnJogStepChange += Core_OnJogStepChange;
		}

		private void OnJogButtonMouseDown(object sender, MouseEventArgs e)
		{
			Core.Jog((sender as DirectionButton).JogDirection);
		}

		private void TbSpeed_ValueChanged(object sender, EventArgs e)
		{
			TT.SetToolTip(TbSpeed, string.Format("Speed: {0}", TbSpeed.Value));
			LblSpeed.Text = String.Format("F{0}", TbSpeed.Value);
			Settings.SetObject("Jog Speed", TbSpeed.Value);
			Core.JogSpeed = TbSpeed.Value;
			needsave = true;
		}

		private void TbStep_ValueChanged(object sender, EventArgs e)
		{
            decimal newValue = TrackbarToJogStep(TbStep.Value);
            string displayValue = newValue.ToString("0.#");

            TT.SetToolTip(TbStep, string.Format("Step: {0}", displayValue));
			LblStep.Text = displayValue;
			Settings.SetObject("Jog Step Float", newValue);
			Core.JogStep = newValue;
			needsave = true;
		}

		private void BtnHome_Click(object sender, EventArgs e)
		{
			Core.JogHome();
		}

		bool needsave = false;
		private void OnSliderMouseUP(object sender, MouseEventArgs e)
		{
			if (needsave)
			{
				needsave = false;
				Settings.Save();
			}
		}

		int oldVal;
		private void UpdateFMax_Tick(object sender, EventArgs e)
		{
			int curVal = (int)Math.Max(Core.Configuration.MaxRateX, Core.Configuration.MaxRateY);
			if (oldVal != curVal)
			{
				TbSpeed.Value = Math.Min(TbSpeed.Value, curVal);
				TbSpeed.Maximum = curVal;
				TbSpeed.LargeChange = curVal / 10;
				TbSpeed.SmallChange = curVal / 20;
				oldVal = curVal;
			}
		}


        private void Core_OnJogStepChange(decimal current)
        {
            if (current != TrackbarToJogStep(TbStep.Value))
                TbStep.Value = JogStepToTrackbar(current);
        }

        // Trackbar is still an integer value, so the first 9 values will be 0.1 to 0.9,
        // 10 and above will be 1 to 200
        private int JogStepToTrackbar(decimal stepValue)
        {
            return stepValue < 1
              ? (int)Math.Truncate(stepValue * 10)
              : (int)Math.Truncate(stepValue) + 9;
        }

        private decimal TrackbarToJogStep(int trackbarValue)
        {
            return trackbarValue <= 10
                ? (decimal)trackbarValue / 10
                : trackbarValue - 9;
        }
	}

	public class DirectionButton : UserControls.ImageButton
	{
		private GrblCore.JogDirection mDir = GrblCore.JogDirection.N;

		public GrblCore.JogDirection JogDirection
		{
			get { return mDir; }
			set { mDir = value; }
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			if (Width != Height)
				Width = Height;

			base.OnSizeChanged(e);
		}
	}
}
