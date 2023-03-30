using DannyGeneral;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StopwatchTimer
{
    public partial class StopwatchUserControl : UserControl
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

        public StopwatchUserControl()
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

        private void RichTextBox1_Enter(object sender, EventArgs e)
        {
            btnStart.Focus();
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
            timeTarget(ctimeSpan);

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

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (btnStart.Text == "START" && !radioButton2.Checked)
            {
                watch.Reset();

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
            else
            {
                OnStartINIT();
            }
        }

        private void OnStartINIT()
        {
            radioButton2.Enabled = false;
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
            }
            else
            {
                btnPause.Text = "PAUSE";
                watch.Start();
                timer1.Enabled = true;
            }
            //}
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            watch.Reset();

            diff = 0;
            previousTicks = 0;
            ticksDisplayed = 0;
            trackBarHours.Value = 0;
            trackBarMinutes.Value = 0;
            trackBarSeconds.Value = 0;

            if (trackBarHours.Value == 0 && trackBarMinutes.Value == 0 && trackBarSeconds.Value == 0)
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

            UpdateTime();
        }

        private void trackBarHours_Scroll(object sender, EventArgs e)
        {
            TimeSpan ctimeSpan = new TimeSpan(ticksDisplayed);
            TimeSpan htimeSpan = new TimeSpan(ctimeSpan.Days, trackBarHours.Value, ctimeSpan.Minutes, ctimeSpan.Seconds, ctimeSpan.Milliseconds);

            ticksDisplayed = htimeSpan.Ticks;

            TrackbarsScrollStates();

            optionsfile.SetKey("trackbarhours", trackBarHours.Value.ToString());

            UpdateTime();
        }

        private void trackBarMinutes_Scroll(object sender, EventArgs e)
        {
            TimeSpan ctimeSpan = new TimeSpan(ticksDisplayed);
            TimeSpan mtimeSpan = new TimeSpan(ctimeSpan.Days, ctimeSpan.Hours, trackBarMinutes.Value, ctimeSpan.Seconds, ctimeSpan.Milliseconds);

            ticksDisplayed = mtimeSpan.Ticks;

            TrackbarsScrollStates();

            optionsfile.SetKey("trackbarminutes", trackBarMinutes.Value.ToString());

            UpdateTime();
        }

        private void trackBarSeconds_Scroll(object sender, EventArgs e)
        {
            TimeSpan ctimeSpan = new TimeSpan(ticksDisplayed);
            TimeSpan stimeSpan = new TimeSpan(ctimeSpan.Days, ctimeSpan.Hours, ctimeSpan.Minutes, trackBarSeconds.Value, ctimeSpan.Milliseconds);

            ticksDisplayed = stimeSpan.Ticks;

            TrackbarsScrollStates();

            optionsfile.SetKey("trackbarseconds", trackBarSeconds.Value.ToString());

            UpdateTime();
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

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            countingDown = false;
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



        private void StopwatchUserControl_Load(object sender, EventArgs e)
        {
            optionsfile.SetKey("trackbarhours", trackBarHours.Value.ToString());
            optionsfile.SetKey("trackbarminutes", trackBarMinutes.Value.ToString());
            optionsfile.SetKey("trackbarseconds", trackBarSeconds.Value.ToString());
            optionsfile.SetKey("timetargetvalue", dateTimePicker1.Value.ToString());
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
