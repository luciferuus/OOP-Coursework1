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
        DishDrawer drawer;

        public MainWindow()
        {
            InitializeComponent();
            bitCanvas = new WriteableBitmap(350, 350, 100, 100, PixelFormats.Bgra32, null);
            drawer = new DishDrawer(bitCanvas);
        }

        #region Button events

        private void Start_Click(object sender, EventArgs e)
        {
            if (timer == null || timer.State == Timer.TimerStates.Stopped)
            {
                timer = new Timer();
                experiment = new Experiment(Experiment.ExperimentStates.Running);
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
            drawer.Tick(experiment);
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

        class DishDrawer
        {
            WriteableBitmap wb;
            Random rnd = new Random();
            byte tracker = 0;
            byte[] blank;

            public DishDrawer(WriteableBitmap writeableBitmap)
            {
                wb = writeableBitmap;
                blank = new byte[wb.PixelWidth * wb.PixelHeight * wb.Format.BitsPerPixel / 8];
                for(int i = 0; i < blank.Length - 1; i++)
                {
                    blank[i] = 255;
                }
                blank[blank.Length - 1] = 255;
            }

            public void Tick(Experiment exp)
            {
                if(tracker == 60)
                {
                    Clear();
                    DrawDish(exp);
                    tracker = 0;
                }
                tracker++;
            }

            void DrawDish(Experiment exp)
            {
                int Column;
                int Row;
                foreach (Experiment.OrganismTracker ot in exp.organismTrackers)
                {
                    for (int i = 0; i < ot.Population; i++)
                    {
                        Column = rnd.Next(0, wb.PixelWidth);
                        Row = rnd.Next(0, wb.PixelHeight);
                        Int32Rect pix = new Int32Rect(Row, Column, 1, 1);
                        wb.WritePixels(pix, ot.ColorData, GetStride(), 0);
                    }
                }
            }

            void Clear()
            {
                Int32Rect pix = new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight);
                wb.WritePixels(pix, blank, GetStride(), 0);
            }

            private int GetStride() => wb.PixelWidth * (wb.Format.BitsPerPixel / 8);
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
            Bacteria = 10,
            Virus = 17,
            Fungus = 22
        }

        public List<OrganismTracker> organismTrackers = new List<OrganismTracker>();

        public Experiment(ExperimentStates experimentState)
        {
            organismTrackers.Add(new OrganismTracker(Species.Bacteria, new byte[4] { 0, 255, 0 , 255}));
            organismTrackers.Add(new OrganismTracker(Species.Virus, new byte[4] { 0, 0, 255 , 255}));
            organismTrackers.Add(new OrganismTracker(Species.Fungus, new byte[4] { 255, 0, 0 , 255}));
        }

        public class OrganismTracker
        {
            public Species Species;
            public long Population;
            private int TickTracker = 0;
            public byte[] ColorData;

            public OrganismTracker(Species species, byte[] ColorData)
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
