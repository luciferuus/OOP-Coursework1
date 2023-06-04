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
        Timer timer;
        Experiment experiment;
        WriteableBitmap bitCanvas;

        public MainWindow()
        {
            InitializeComponent();
            
        }

        #region Button events

        private void Start_Click(object sender, EventArgs e)
        {
            if (timer == null || timer.State == Timer.TimerStates.Stopped)
            {
                timer = new Timer();
                experiment = new Experiment(Experiment.ExperimentStates.Running);
                bitCanvas = new WriteableBitmap((int)DishPic.ActualHeight, (int)DishPic.ActualWidth, 100, 100, PixelFormats.Rgb24, null);
                DishPic.Source = bitCanvas;
                EventsSubscribe();
            }
        }

        private void StartTimed_Click(object sender, EventArgs e)
        {
            TextboxExperimentTime.BorderBrush = Brushes.Gray;
            if (timer == null || timer.State == Timer.TimerStates.Stopped)
            {
                if (GetTextboxInput() != null)
                {
                    timer = new Timer((int)GetTextboxInput(), (TimeMeasures)ComboboxTimeMode.SelectedIndex);
                    experiment = new Experiment(Experiment.ExperimentStates.Running);
                    DishPic.Source = new WriteableBitmap((int)DishPic.ActualHeight, (int)DishPic.ActualWidth, 100, 100, PixelFormats.Rgb24, null);
                    EventsSubscribe();
                }

            }
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            if (timer != null && timer.State != Timer.TimerStates.Stopped)
            {
                timer.Stop();
                EventsUnsubscribe();
            }
        }

        #endregion

        public void UpdateTimeTrackers(object sender, EventArgs e)
        {
            ElapsedLabel.Text = timer.GetElapsed();
            EstimatedLabel.Text = timer.GetEstimated();
        }

        public void UpdateOrganismLabels(object sender, EventArgs e)
        {
            LabelBacteriaCount.Text = experiment.organismTrackers[0].Population.ToString();
            LabelVirusCount.Text = experiment.organismTrackers[1].Population.ToString();
            LabelFungiCount.Text = experiment.organismTrackers[2].Population.ToString();
        }

        public void DrawDish(object sender, EventArgs e)
        {
            Random rnd = new Random();
            bitCanvas = new WriteableBitmap((int)DishPic.ActualHeight, (int)DishPic.ActualWidth, 100, 100, PixelFormats.Rgb24, null);
            bitCanvas.Lock();
            int Column;
            int Row;
            foreach (Experiment.OrganismTracker ot in experiment.organismTrackers)
            {
                for(int i = 0; i < ot.Population; i++)
                {
                    unsafe
                    {
                        IntPtr pBackBuffer = bitCanvas.BackBuffer;
                        Column = rnd.Next(0, bitCanvas.PixelHeight);
                        Row = rnd.Next(0, bitCanvas.PixelWidth);
                        pBackBuffer += Column;
                        pBackBuffer += Row;
                        int ColorToDraw = ot.ColorData[0] << 16;
                        ColorToDraw |= ot.ColorData[1] << 8;
                        ColorToDraw |= ot.ColorData[2] << 0;
                        *((int*)pBackBuffer) = ColorToDraw;
                    }
                    bitCanvas.AddDirtyRect(new Int32Rect(Column, Row, 1, 1));
                }
            }
            bitCanvas.Unlock();
        }

        public void TimerExecutioner(object sender, EventArgs e)
        {
            if(timer.State == Timer.TimerStates.Stopped)
            {
                timer.Stop();
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

        private void EventsSubscribe()
        {
            foreach (Experiment.OrganismTracker ot in experiment.organismTrackers)
            {
                timer.Tick.Elapsed += ot.Tick;
            }
            CompositionTarget.Rendering += UpdateTimeTrackers;
            CompositionTarget.Rendering += UpdateOrganismLabels;
            CompositionTarget.Rendering += TimerExecutioner;
            CompositionTarget.Rendering += DrawDish;
        }

        private void EventsUnsubscribe()
        {
            foreach(Experiment.OrganismTracker ot in experiment.organismTrackers)
            {
                timer.Tick.Elapsed -= ot.Tick;
            }
            CompositionTarget.Rendering -= UpdateTimeTrackers;
            CompositionTarget.Rendering -= UpdateOrganismLabels;
            CompositionTarget.Rendering -= TimerExecutioner;
            CompositionTarget.Rendering -= DrawDish;
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
                        this.Stop();

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

        public void Stop()
        {
            State = TimerStates.Stopped;
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
            Stopped,
            Running,
            OnTimer
        }

        public enum Species
        {
            Bacteria = 3,
            Virus = 5,
            Fungus = 7
        }

        public List<OrganismTracker> organismTrackers = new List<OrganismTracker>();

        public Experiment(ExperimentStates experimentState)
        {
            organismTrackers.Add(new OrganismTracker(Species.Bacteria, new int[3] { 0, 255, 0 }));
            organismTrackers.Add(new OrganismTracker(Species.Virus, new int[3] { 255, 0, 0 }));
            organismTrackers.Add(new OrganismTracker(Species.Fungus, new int[3] { 0, 0, 255 }));
        }

        public class OrganismTracker
        {
            public Species Species;
            public long Population;
            private int TickTracker = 0;
            public int[] ColorData;

            public OrganismTracker(Species species, int[] ColorData)
            {
                this.Species = species;
                this.ColorData = ColorData;
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
