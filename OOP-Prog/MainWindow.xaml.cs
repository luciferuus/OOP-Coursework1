using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup.Localizer;
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
        Experiment experiment;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void UpdateTimeTrackers(object sender, EventArgs e)
        {
            ElapsedLabel.Text = timer.GetElapsed();
            EstimatedLabel.Text = timer.GetEstimated();
        }

        public void UpdateOrganismLabels(object sender, EventArgs e)
        {
            LabelBacteriaCount.Text = experiment.Bacterias.Population.ToString();
            LabelVirusCount.Text = experiment.Viruses.Population.ToString();
            LabelFungiCount.Text = experiment.Fungi.Population.ToString();
        }

        public void TimerExecutioner(object sender, EventArgs e)
        {
            if(timer.State == Timer.TimerStates.Stopped)
            {
                timer.Terminate();
                timer.Tick.Elapsed -= experiment.Bacterias.Tick;
                timer.Tick.Elapsed -= experiment.Viruses.Tick;
                timer.Tick.Elapsed -= experiment.Fungi.Tick;
                CompositionTarget.Rendering -= UpdateTimeTrackers;
                CompositionTarget.Rendering -= UpdateOrganismLabels;
                CompositionTarget.Rendering -= TimerExecutioner;
                ActiveTimer = false;
            }
        }

        public int? GetTextboxInput()
        {
            int input;
            try
            {
                input = Convert.ToInt32(TextboxExperimentTime.Text);
                if(input <= 0)
                {
                    throw new Exception("Experiment time must be >0");
                }
            }
            catch
            {
                TextboxExperimentTime.BorderBrush = Brushes.Red;
                return null;
            }
            return input;
        }

        private void Start_Click(object sender, EventArgs e)
        {
            if (ActiveTimer == false)
            {
                ActiveTimer = true;
                timer = new Timer();
                experiment = new Experiment(Experiment.ExperimentStates.Running);
                timer.Tick.Elapsed += experiment.Bacterias.Tick;
                timer.Tick.Elapsed += experiment.Viruses.Tick;
                timer.Tick.Elapsed += experiment.Fungi.Tick;
                CompositionTarget.Rendering += UpdateTimeTrackers;
                CompositionTarget.Rendering += UpdateOrganismLabels;
            }
        }

        private void StartTimed_Click(object sender, EventArgs e)
        {
            TextboxExperimentTime.BorderBrush = Brushes.Gray;
            if (ActiveTimer == false)
            {
                if (GetTextboxInput() != null)
                {
                    ActiveTimer = true;
                    timer = new Timer((int)GetTextboxInput(), (TimeMeasures)ComboboxTimeMode.SelectedIndex);
                    experiment = new Experiment(Experiment.ExperimentStates.Running);
                    timer.Tick.Elapsed += experiment.Bacterias.Tick;
                    timer.Tick.Elapsed += experiment.Viruses.Tick;
                    timer.Tick.Elapsed += experiment.Fungi.Tick;
                    CompositionTarget.Rendering += UpdateTimeTrackers;
                    CompositionTarget.Rendering += UpdateOrganismLabels;
                    CompositionTarget.Rendering += TimerExecutioner;
                }

            }
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            if(ActiveTimer == true)
            {
                ActiveTimer = false;
                timer.Tick.Elapsed -= experiment.Bacterias.Tick;
                timer.Tick.Elapsed -= experiment.Viruses.Tick;
                timer.Tick.Elapsed -= experiment.Fungi.Tick;
                timer.Terminate();
                CompositionTarget.Rendering -= UpdateTimeTrackers;
                CompositionTarget.Rendering -= UpdateOrganismLabels;
            }
        }
    }

    public class Timer
    {
        public enum TimerStates
        {
            Stopped,
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
                while (true)
                {
                    if (a.Seconds == 0 && a.Minutes > 0)
                    {
                        a.Seconds = 60; a.Minutes--;
                    }
                    else if (a.Minutes == 0 && a.Hours > 0)
                    {
                        a.Minutes = 59; a.Hours--;
                    }
                    else if (a.Hours == 0 && a.Days > 0)
                    {
                        a.Hours = 23; a.Days--;
                    }
                    else
                    {
                        break;
                    }
                }
                a.Seconds--;
                return a;
            }

            public static Time operator ++(Time a) //Increase by 1 second
            {
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
                a.Seconds++;
                return a;
            }

            /*
            public static bool operator ==(Time a, Time b)
            {
                if((a.Days == b.Days) && (a.Hours == b.Hours) && (a.Minutes == b.Minutes) && (a.Seconds == b.Seconds))
                {
                    return true;
                } else
                {
                    return false;
                }
            }

            public static bool operator !=(Time a, Time b)
            {
                if ((a.Days == b.Days) && (a.Hours == b.Hours) && (a.Minutes == b.Minutes) && (a.Seconds == b.Seconds))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            */

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

        public System.Timers.Timer Tick;

        private Time Zero = new Time(0, 0, 0, 0);

        public Timer()
        {
            State = TimerStates.Limitless;
            Initialize();
        }

        public Timer(int value, TimeMeasures measure)
        {
            State = TimerStates.Limited;
            switch (measure)
            {
                case TimeMeasures.Seconds:
                    Estimated = new Time(0, 0, 0, value);
                    break;

                case TimeMeasures.Minutes:
                    Estimated = new Time(0, 0, value, 0);
                    break;

                case TimeMeasures.Hours:
                    Estimated = new Time(0, value, 0, 0);
                    break;

                case TimeMeasures.Days:
                    Estimated = new Time(value, 0, 0, 0);
                    break;
            }
            Initialize();
        }

        private void InternalTick(object sender, EventArgs e)
        {
            switch (State)
            {
                case TimerStates.Limitless:

                    break;

                case TimerStates.Limited:
                    if (Estimated.Equals(Zero))
                    {
                        this.State = TimerStates.Stopped;
                        this.Terminate();

                    }
                    else
                    {
                        Estimated--;
                    }
                    break;

                case TimerStates.Stopped:

                    break;
            }
            if (this.State != TimerStates.Stopped)
            {
                Elapsed++;
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
            Tick.Stop();
        }

        private void Initialize() //Performes setup common for all types of timers
        {
            Elapsed = new Time(0, 0, 0, 0);
            Tick = new System.Timers.Timer(1000);
            Tick.Elapsed += InternalTick;
            Tick.AutoReset = true;
            Tick.Start();
        }
    }

    public class Experiment
    {
        ExperimentStates State;
        public enum ExperimentStates
        {
            Running,
            OnTimer
        }

        public enum Species
        {
            Bacteria = 1,
            Virus = 2,
            Fungus = 3
        }

        public OrganismTracker Bacterias;
        public OrganismTracker Viruses;
        public OrganismTracker Fungi;

        public Experiment(ExperimentStates experimentState)
        {
            Bacterias = new OrganismTracker(Species.Bacteria);
            Viruses = new OrganismTracker(Species.Virus);
            Fungi = new OrganismTracker(Species.Fungus);
        }

        public class OrganismTracker
        {
            public Species Species;
            public long Population;
            private int TickTracker = 0;

            public OrganismTracker(Species species)
            {
                this.Species = species;
                Population = 1;
            }

            void Multiply()
            {
                Population *= 2;
            }

            public void Tick(object sender, EventArgs e)
            {
                TickTracker++;
                if (TickTracker == (int)Species)
                {
                    TickTracker = 0;
                    Multiply();
                }
            }
        }
    }
}
