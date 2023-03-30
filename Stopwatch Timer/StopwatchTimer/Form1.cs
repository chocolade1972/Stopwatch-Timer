using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using DannyGeneral;

namespace StopwatchTimer
{
	public partial class Form1 : Form
	{
		private static readonly Stopwatch watch = new Stopwatch();
		private long diff = 0, previousTicks = 0, ticksDisplayed = 0;
		private OptionsFile optionsfile = new OptionsFile(Path.GetDirectoryName(Application.LocalUserAppDataPath) + "\\Settings.txt");
		private string result;
		private bool runOnStart = false;
		private bool countingDown = false;
		private TimeSpan ctimeSpan;
		private int savedMilliseconds = 0;
		private string settingsFile = Path.GetDirectoryName(Application.LocalUserAppDataPath) + "\\Settings.txt";


        public Form1()
		{
			InitializeComponent();

			richTextBox1.TabStop = false;
			richTextBox1.ReadOnly = true;
			richTextBox1.BackColor = Color.White;
			richTextBox1.Cursor = Cursors.Arrow;
			richTextBox1.Enter += RichTextBox1_Enter;

			if (File.Exists(settingsFile))
			{
				string[] lines = File.ReadAllLines(settingsFile);
				if (lines.Length > 0)
				{
					trackBarHours.Value = Convert.ToInt32(optionsfile.GetKey("trackbarhours"));
					trackBarMinutes.Value = Convert.ToInt32(optionsfile.GetKey("trackbarminutes"));
					trackBarSeconds.Value = Convert.ToInt32(optionsfile.GetKey("trackbarseconds"));
					savedMilliseconds = Convert.ToInt32(optionsfile.GetKey("milliseconds"));
					if (Convert.ToDateTime(optionsfile.GetKey("timetargetvalue")) <= dateTimePicker1.MinDate)
					{
						dateTimePicker1.Value = DateTime.Now;
                    }
					else
					{
						dateTimePicker1.Value = Convert.ToDateTime(optionsfile.GetKey("timetargetvalue"));
					}
					richTextBox1.Text = optionsfile.GetKey("result");
				}
			}

			TimeSpan ctimeSpan = new TimeSpan(0, trackBarHours.Value, trackBarMinutes.Value, trackBarSeconds.Value, 0);
			ticksDisplayed = ctimeSpan.Ticks;

			radioButton1.Checked = GetBool("radiobutton1");
			timeTargetchkbox.Checked = GetBool("timetargetcheckbox");

			timeTargetchkboxState();

			if (ticksDisplayed > 0 && radioButton1.Checked == false)
				radioButton2.Checked = true;

			if (ticksDisplayed == 0)
				radioButton1.Checked = true;

			/*if (trackBarHours.Value == 0 && trackBarMinutes.Value == 0 && trackBarSeconds.Value == 0)
			{
				btnPause.Enabled = false;
				btnReset.Enabled = false;
			}
			else
			{
				btnPause.Enabled = false;
				btnReset.Enabled = true;
			}*/
			// To check check what and how to change in the Pause
			// method if the timer is not reseted to 00 then when starting
			// the application the Pause button should be text CONTINUE
			// and to allowed to continue.

			if (trackBarHours.Value > 0 || trackBarMinutes.Value > 0 || trackBarSeconds.Value > 0)
			{
				btnPause.Text = "CONTINUE";
				btnStart.Text = "STOP";
			}
			else
			{
				radioButton2.Enabled = false;
				btnPause.Enabled = false;
				btnReset.Enabled = false;
			}

                runOnStart = GetBool("runonstart");
			if (runOnStart == true)
			{
				autoRunOnStart.Checked = true;
				StartOnRun();
			}
			else
			{
				autoRunOnStart.Checked = false;
			}
		}

		private void timeTargetchkboxState()
		{
			if (timeTargetchkbox.Checked == false)
			{
				dateTimePicker1.Enabled = false;
			}
			else
			{
				dateTimePicker1.Enabled = true;
			}
		}

		private void RichTextBox1_Enter(object sender, EventArgs e)
		{
			btnStart.Focus();
		}

		private void UpdateTime()
		{
			if (ticksDisplayed > 0)
				btnReset.Enabled = true;

			richTextBox1.Text = GetTimeString(watch.Elapsed);
			optionsfile.SetKey("result", result.ToString());
            optionsfile.SetKey("milliseconds", ctimeSpan.Milliseconds.ToString());
        }

		private string GetTimeString(TimeSpan elapsed)
		{
			result = string.Empty;
			diff = elapsed.Ticks - previousTicks;

			if (radioButton1.Checked == true)
			{
				ticksDisplayed += diff;
			}
			else
			{
				if (countingDown)
				{
					ticksDisplayed += diff;
				}
				else
				{
					ticksDisplayed -= diff;
				}
			}

			if (ticksDisplayed < 0)
			{
				ticksDisplayed = 0;

				watch.Stop();
				btnStart.Text = "START";
				btnPause.Text = "PAUSE";
				btnPause.Enabled = false;
				if (trackBarHours.Value == 0 && trackBarMinutes.Value == 0 && trackBarSeconds.Value == 0 && ticksDisplayed == 0)
				{
					btnReset.Enabled = false;
					radioButton2.Enabled = false;
					radioButton1.Checked = true;
				}

				timer1.Enabled = false;
			}

			ctimeSpan = new TimeSpan(ticksDisplayed);

			if (timeTargetchkbox.Checked)
			{
				timeTarget(ctimeSpan);
			}

			if (trackBarHours.Value != ctimeSpan.Hours) { trackBarHours.Value = ctimeSpan.Hours; }
			if (trackBarMinutes.Value != ctimeSpan.Minutes) { trackBarMinutes.Value = ctimeSpan.Minutes; }
			if (trackBarSeconds.Value != ctimeSpan.Seconds) { trackBarSeconds.Value = ctimeSpan.Seconds; }

			result = string.Format("{0:00}:{1:00}:{2:00}.{3:000}",
				ctimeSpan.Hours,
				ctimeSpan.Minutes,
				ctimeSpan.Seconds,
				ctimeSpan.Milliseconds);

			previousTicks = elapsed.Ticks;

			return result;
		}

		private void timeTarget(TimeSpan ctimeSpan)
		{
			if (dateTimePicker1.Value.Hour == ctimeSpan.Hours
				&& dateTimePicker1.Value.Minute == ctimeSpan.Minutes
				&& dateTimePicker1.Value.Second == ctimeSpan.Seconds
				&& timeTargetchkbox.Checked == true)
			{
				//ticksDisplayed = 0;
				if (btnPause.Text == "PAUSE")
				{
					btnPause.Text = "CONTINUE";
					watch.Stop();
					timer1.Enabled = false;
					timeTargetchkbox.Checked = false;
				}
			}
			else
			{
				if (btnStart.Text == "STOP")
				{
					btnPause.Text = "PAUSE";
					watch.Start();
					timer1.Enabled = true;
				}
			}
			timeTargetchkboxState();
		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			// When starting.
			if (btnStart.Text == "START" && !radioButton2.Checked)
			{
				if (trackBarHours.Value > 0 || trackBarMinutes.Value > 0 || trackBarSeconds.Value > 0)
				{
                    watch.Start();
                    btnStart.Text = "STOP";
                    btnPause.Enabled = true;
                    btnReset.Enabled = true;
                    timer1.Enabled = true;
                    radioButton2.Enabled = true;
                }
				else
				{
					radioButton2.Enabled = true;
					// To check why radioButton2 is enabled true when
					// running the application and everything is set to
					// 00:00:00:00 and all trackBars are on value 0.
					// in that case radioButton2 should be enabled false !!!!!

					trackBarHours.Value = 0;
					trackBarMinutes.Value = 0;
					trackBarSeconds.Value = 0;

					TimeSpan ctimeSpan = new TimeSpan(0, trackBarHours.Value, trackBarMinutes.Value, trackBarSeconds.Value, 0);
					diff = 0;
					previousTicks = 0;
					ticksDisplayed = ctimeSpan.Ticks;

					watch.Start();
					btnStart.Text = "STOP";
					btnPause.Enabled = true;
					btnReset.Enabled = true;
					timer1.Enabled = true;
				}
			}
			else
			{
                // When stopping.
                radioButton2.Enabled = false;
				radioButton1.Checked = true;
                watch.Stop();
                btnStart.Text = "START";
                btnPause.Text = "PAUSE";
                btnPause.Enabled = false;
                btnReset.Enabled = false;
                trackBarHours.Value = 0;
                trackBarMinutes.Value = 0;
                trackBarSeconds.Value = 0;

                TimeSpan ctimeSpan = new TimeSpan(0, trackBarHours.Value, trackBarMinutes.Value, trackBarSeconds.Value, 0);
                diff = 0;
                previousTicks = 0;
                ticksDisplayed = ctimeSpan.Ticks;
                watch.Reset();
                timer1.Enabled = false;
                UpdateTime();
            }

			// When clicking the btnStart button here to check if the
			// one of the trackBars values more then 0 to start the timer
			// from this current value time and not from 00:00:00.000 
			// to think about it to fix it first. then to fix all buttons
			// like radioButtons 1 & 2 and to fix and make the datetimepicker 
			// and checkbox Pause on time target !!!!!
		}

		private void btnReset_Click(object sender, EventArgs e)
		{
			if (btnPause.Text == "CONTINUE" && btnStart.Text == "STOP")
			{
                watch.Reset();
                diff = 0;
                previousTicks = 0;
                ticksDisplayed = 0;
                trackBarHours.Value = 0;
                trackBarMinutes.Value = 0;
                trackBarSeconds.Value = 0;
				btnReset.Enabled = false;
				radioButton1.Checked = true;
				radioButton2.Enabled = false;
                richTextBox1.Text = GetTimeString(watch.Elapsed);
            }
			else
			{
				if (btnStart.Text == "STOP")
				{
					watch.Reset();
					diff = 0;
					previousTicks = 0;
					ticksDisplayed = 0;
					trackBarHours.Value = 0;
					trackBarMinutes.Value = 0;
					trackBarSeconds.Value = 0;
					watch.Start();
				}
			}

			if((trackBarHours.Value > 0 || trackBarMinutes.Value > 0 || trackBarSeconds.Value > 0) && btnStart.Text == "START" && btnPause.Text == "PAUSE")
			{
                watch.Reset();
                diff = 0;
                previousTicks = 0;
                ticksDisplayed = 0;
                trackBarHours.Value = 0;
                trackBarMinutes.Value = 0;
                trackBarSeconds.Value = 0;
                btnReset.Enabled = false;
				radioButton2.Enabled = false;
				radioButton1.Checked = true;
                richTextBox1.Text = GetTimeString(watch.Elapsed);
            }
            //btnPause.Enabled = false;
            //btnReset.Enabled = false;
            //richTextBox1.Text = GetTimeString(watch.Elapsed);

            /*if (trackBarHours.Value == 0 && trackBarMinutes.Value == 0 && trackBarSeconds.Value == 0)
			{
				btnReset.Enabled = false;
			}
			else
			{
				btnReset.Enabled = true;
			}

			if (radioButton2.Checked && ticksDisplayed == 0)
			{
				countingDown = true;
				radioButton2.Checked = false;
				radioButton1.Checked = true;
			}

			if (btnPause.Text == "CONTINUE")
			{
				timer1.Enabled = false;
				btnPause.Enabled = false;
				btnReset.Enabled = false;
				btnStart.Text = "START";
				richTextBox1.Text = GetTimeString(watch.Elapsed);
			}
			else
			{
				UpdateTime();
			}*/
		}

		private void trackBarHours_Scroll(object sender, EventArgs e)
		{
			TimeSpan ctimeSpan = new TimeSpan(ticksDisplayed);
			TimeSpan htimeSpan = new TimeSpan(ctimeSpan.Days, trackBarHours.Value, ctimeSpan.Minutes, ctimeSpan.Seconds, ctimeSpan.Milliseconds);

			ticksDisplayed = htimeSpan.Ticks;

			TrackbarsScrollStates();

			optionsfile.SetKey("trackbarhours", trackBarHours.Value.ToString());

			UpdateTime();

            if (trackBarHours.Value > 0)
            {
                radioButton2.Enabled = true;
            }
        }

		private void trackBarMinutes_Scroll(object sender, EventArgs e)
		{
			TimeSpan ctimeSpan = new TimeSpan(ticksDisplayed);
			TimeSpan mtimeSpan = new TimeSpan(ctimeSpan.Days, ctimeSpan.Hours, trackBarMinutes.Value, ctimeSpan.Seconds, ctimeSpan.Milliseconds);

			ticksDisplayed = mtimeSpan.Ticks;

			TrackbarsScrollStates();

			optionsfile.SetKey("trackbarminutes", trackBarMinutes.Value.ToString());

			UpdateTime();

            if (trackBarMinutes.Value > 0)
            {
                radioButton2.Enabled = true;
            }
        }

		private void trackBarSeconds_Scroll(object sender, EventArgs e)
		{
			TimeSpan ctimeSpan = new TimeSpan(ticksDisplayed);
			TimeSpan stimeSpan = new TimeSpan(ctimeSpan.Days, ctimeSpan.Hours, ctimeSpan.Minutes, trackBarSeconds.Value, ctimeSpan.Milliseconds);

			ticksDisplayed = stimeSpan.Ticks;

			TrackbarsScrollStates();

			optionsfile.SetKey("trackbarseconds", trackBarSeconds.Value.ToString());

			UpdateTime();

			if(trackBarSeconds.Value > 0)
			{
				radioButton2.Enabled = true;
			}

			// If the btnPause button is text CONTINUE meaning in pause mode.
			// then when changing any of the trackBars values to keep it
			// pausing and the btnPause texto CONTINUE
			// if the pause button text is PAUSE then when changing here 
			// any of the trackBars values that it will continue regular.
		}

		private void TrackbarsScrollStates()
		{
			if (trackBarSeconds.Value == 0 && trackBarHours.Value == 0 && trackBarMinutes.Value == 0)
				btnReset.Enabled = false;
			if (trackBarSeconds.Value > 0 || trackBarHours.Value > 0 || trackBarMinutes.Value > 0)
				btnReset.Enabled = true;
		}

		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			optionsfile.SetKey("radiobutton1", radioButton1.Checked.ToString());
		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			optionsfile.SetKey("trackbarhours", trackBarHours.Value.ToString());
			optionsfile.SetKey("trackbarminutes", trackBarMinutes.Value.ToString());
			optionsfile.SetKey("trackbarseconds", trackBarSeconds.Value.ToString());
            optionsfile.SetKey("timetargetvalue", dateTimePicker1.Value.ToString());
        }

		private void btnPause_Click(object sender, EventArgs e)
		{
			Pause();
		}

		private void Pause()
		{
			//if (btnStart.Text == "STOP")
			//{
				if (btnPause.Text == "PAUSE")
				{
					btnPause.Text = "CONTINUE";
					watch.Stop();
					timer1.Enabled = false;
				radioButton2.Enabled = true;
				}
				else
				{
					btnPause.Text = "PAUSE";
					watch.Start();
					timer1.Enabled = true;
				    radioButton2.Enabled = true;
				}
			//}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			UpdateTime();
		}

		private void autoRunOnStart_CheckedChanged(object sender, EventArgs e)
		{
			if (autoRunOnStart.Checked)
			{
				runOnStart = true;
			}
			else
			{
				runOnStart = false;
			}

			optionsfile.SetKey("runonstart", runOnStart.ToString());
		}

		private void radioButton2_CheckedChanged(object sender, EventArgs e)
		{
			countingDown = false;
		}

		private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
		{
			btnStart.Focus();
			timeTarget(ctimeSpan);
            optionsfile.SetKey("timetargetvalue", dateTimePicker1.Value.ToString());
        }

		private void timeTargetchkbox_CheckedChanged(object sender, EventArgs e)
		{
			if (timeTargetchkbox.Checked == false)
			{
				dateTimePicker1.Enabled = false;
			}
			else
			{
				dateTimePicker1.Enabled = true;
			}
			optionsfile.SetKey("timetargetcheckbox", timeTargetchkbox.Checked.ToString());
		}

        private void btnSettingsFile_Click(object sender, EventArgs e)
        {
            //Process.Start(settingsFile);

            if (File.Exists(settingsFile))
            {
                Process.Start("explorer.exe", settingsFile);
            }

            /*RichTextBox rtx1 = new RichTextBox();
			rtx1.Size = new Size(380,330);
			rtx1.ScrollBars = RichTextBoxScrollBars.None;
			rtx1.BackColor = Color.Black;
			rtx1.ForeColor = Color.Yellow;
            rtx1.Font = new Font("Georgia", 16);
            this.Controls.Add(rtx1);
			rtx1.BringToFront();
            rtx1.Location = new Point((this.ClientSize.Width / 2) - (rtx1.Width / 2), (this.ClientSize.Height / 2) - (rtx1.Height / 2));
            rtx1.AppendText(File.ReadAllText(settingsFile));*/
        }

        private void StartOnRun()
		{
			watch.Reset();

			TimeSpan ctimeSpan = new TimeSpan(0, trackBarHours.Value, trackBarMinutes.Value, trackBarSeconds.Value, 0);
			diff = 0;
			previousTicks = 0;
			ticksDisplayed = ctimeSpan.Ticks;

			watch.Start();
			btnStart.Text = "STOP";
			btnPause.Text = "PAUSE";
			btnPause.Enabled = true;
			btnReset.Enabled = true;
			timer1.Enabled = true;
		}

		private bool GetBool(string keyname)
		{
			string radiobutton1 = optionsfile.GetKey(keyname);
			bool b = false;

			if (radiobutton1 != null)
			{
				bool.TryParse(radiobutton1.Trim(), out b);
			}

			return b;
		}
	}
}

// To save also the milliseconds to the OptionsFile settings.txt !!!!!