using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OOP_Prog
{
    public enum TimeMeasures
    {
        Seconds,
        Minutes,
        Hours,
        Days
    }

    public partial class MainWindow : Window
    {
        public bool ActiveTimer = false;
        Timer timer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UpdateTimeTrackers(object sender, EventArgs e)
        {
            ElapsedLabel.Text = timer.GetElapsed();
            EstimatedLabel.Text = timer.GetEstimated();
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveTimer == false)
            {
                ActiveTimer = true;
                timer = new Timer();
                timer.Dispatcher.Tick += new EventHandler(this.UpdateTimeTrackers);
            }
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            if(ActiveTimer == true)
            {
                ActiveTimer = false;
                timer.Terminate();
                timer.Dispatcher.Tick -= new EventHandler(this.UpdateTimeTrackers);
            }
        }

        #region Classes

        public class Timer
        {
            public enum TimerStates
            {
                Limited,
                Limitless
            }

            public struct Time //Stores days, hours, minutes and seconds
            {
                int Days;
                int Hours;
                int Minutes;
                int Seconds;

                public Time(int D, int H, int M, int S)
                {
                    Days = D;
                    Hours = H;
                    Minutes = M;
                    Seconds = S;
                }

                public static Time operator --(Time a) //Decrease by 1 second
                {
                    a.Seconds--;
                    while (true)
                    {
                        if (a.Hours == 0)
                        {
                            a.Days--; a.Hours = 23;
                        }
                        else if (a.Minutes == 0)
                        {
                            a.Hours--; a.Minutes = 59;
                        }
                        else if (a.Seconds == 0)
                        {
                            a.Minutes--; a.Seconds = 59;
                        }
                        else
                        {
                            break;
                        }
                    }
                    return a;
                }

                public static Time operator ++(Time a) //Increase by 1 second
                {
                    a.Seconds++;
                    while (true)
                    {
                        if (a.Seconds == 60)
                        {
                            a.Seconds = 0; a.Minutes++;
                        }
                        else if (a.Minutes == 60)
                        {
                            a.Minutes = 0; a.Hours++;
                        }
                        else if (a.Hours == 24)
                        {
                            a.Hours = 0; a.Days++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    return a;
                }

                public override string ToString()
                {
                    return $"{TStr(Days)}:{TStr(Hours)}:{TStr(Minutes)}:{TStr(Seconds)}";
                }

                private string TStr(int val) //Used in ToString() to output components of time in conventional way
                {
                    if (val < 10)
                    {
                        return "0" + val.ToString();
                    }
                    else
                    {
                        return val.ToString();
                    }
                }
            }

            public TimerStates State { get; private set; }

            private Time Elapsed { get; set; }

            private Time Estimated { get; set; }

            public DispatcherTimer Dispatcher;

            public Timer()
            {
                State = TimerStates.Limitless;
                Initialize();
            }

            public Timer(int value, TimeMeasures measure)
            {
                State = TimerStates.Limited;
                Initialize();
            }

            private void Tick(object sender, EventArgs e)
            {
                Elapsed++;
                switch (State)
                {
                    case TimerStates.Limitless:

                        break;

                    case TimerStates.Limited:

                        break;
                }
            }

            public string GetElapsed()
            {
                return Elapsed.ToString();
            }

            public string GetEstimated()
            {
                if (State == TimerStates.Limitless)
                {
                    return "Infinity";
                }
                else
                {
                    return Estimated.ToString();
                }
            }

            public void Terminate()
            {
                Dispatcher.Stop();
            }

            private void Initialize() //Performes setup common for all types of timers
            {
                Elapsed = new Time(0, 0, 0, 0);
                Dispatcher = new DispatcherTimer();
                Dispatcher.Tick += Tick;
                Dispatcher.Interval = new TimeSpan(0, 0, 1);
                Dispatcher.Start();
            }
        }

        public class Experiment
        {
            public enum ExperimentStates
            {
                Stopped,
                Running,
                OnTimer
            }

            class Bacreria
            {
                public Bacreria()
                {

                }
            }

            class Virus
            {
                public Virus()
                {

                }
            }

            class Fungus
            {
                public Fungus()
                {

                }
            }
        }

        #endregion
    }
}
