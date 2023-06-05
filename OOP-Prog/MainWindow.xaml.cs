using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        public Experiment experiment;
        readonly WriteableBitmap bitCanvas;

        public MainWindow() //Window initialization
        {
            InitializeComponent();
            bitCanvas = new WriteableBitmap(350, 350, 100, 100, PixelFormats.Bgra32, null);
        }

        #region Button events

        private void Start_Click(object sender, EventArgs e) //Handles clicking the "Start" button
        {
            if (timer == null || timer.State == Timer.TimerStates.Stopped)
            {
                timer = new Timer();
                experiment = new Experiment(Experiment.ExperimentStates.Running, Dispatcher, bitCanvas);
                DishPic.Source = bitCanvas;
                EventsSubscribe();
            }
        }

        private void StartTimed_Click(object sender, EventArgs e) //Handles clicking the "Start" button in TimeExp zone
        {
            TextboxExperimentTime.BorderBrush = Brushes.Gray;
            if (timer == null || timer.State == Timer.TimerStates.Stopped)
            {
                if (GetTextboxInput() != null)
                {
                    timer = new Timer((int)GetTextboxInput(), (TimeMeasures)ComboboxTimeMode.SelectedIndex);
                    experiment = new Experiment(Experiment.ExperimentStates.OnTimer, Dispatcher, bitCanvas);
                    EventsSubscribe();
                }

            }
        }

        private void Stop_Click(object sender, EventArgs e) //Handles clicking the "Stop" button
        {
            if (timer != null && timer.State != Timer.TimerStates.Stopped)
            {
                timer.Stop();
                EventsUnsubscribe();
            }
        }

        #endregion

        public void UpdateTimeTrackers(object sender, EventArgs e) //Updates labels keeping track of time
        {
            ElapsedLabel.Text = timer.GetElapsed();
            EstimatedLabel.Text = timer.GetEstimated();
        }

        public void UpdateOrganismLabels(object sender, EventArgs e) //Updates values in labels tracking population of organisms
        {
            LabelBacteriaCount.Text = experiment.organismTrackers[0].PopulationString;
            LabelVirusCount.Text = experiment.organismTrackers[1].PopulationString;
            LabelFungiCount.Text = experiment.organismTrackers[2].PopulationString;
        }

        public void TimerExecutioner(object sender, EventArgs e) //Checks if timer ran out, the stops it
        {
            if (timer.State == Timer.TimerStates.Stopped)
            {
                timer.Stop();
            }
        }

        public int? GetTextboxInput() //Gets the value from the textbox in TimeExp zone and checks it if it is valid
        {
            int input;
            try
            {
                input = Convert.ToInt32(TextboxExperimentTime.Text);
                if (input <= 0)
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

        private void EventsSubscribe() //Subscribes OrganismTrackers to Timer tick and UI stuff to Rendering tick
        {
            foreach (Experiment.OrganismTracker ot in experiment.organismTrackers)
            {
                timer.Tick.Elapsed += ot.Tick;
            }
            CompositionTarget.Rendering += UpdateTimeTrackers;
            CompositionTarget.Rendering += UpdateOrganismLabels;
            CompositionTarget.Rendering += TimerExecutioner;
        }

        private void EventsUnsubscribe() //Unsubscribes OrganismTrackers from Timer tick and UI stuff from Rendering tick
        {
            foreach (Experiment.OrganismTracker ot in experiment.organismTrackers)
            {
                timer.Tick.Elapsed -= ot.Tick;
            }
            CompositionTarget.Rendering -= UpdateTimeTrackers;
            CompositionTarget.Rendering -= UpdateOrganismLabels;
            CompositionTarget.Rendering -= TimerExecutioner;
        }

        public class Timer
        {
            public enum TimerStates //Possible states of the timer
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

            public TimerStates State { get; private set; } //Current state of the timer

            private Time Elapsed { get; set; } //Tracks time elapsed from the start

            private Time Estimated { get; set; } //Track estimated time

            public System.Timers.Timer Tick; //THE timer

            private Time Zero = new Time(0, 0, 0, 0); //Used for comparason in timers with time limit

            public Timer() //Creates a timer with no time limit
            {
                State = TimerStates.Limitless;
                Initialize();
            }

            public Timer(int value, TimeMeasures measure) //Creates timer with a time limit
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

            private void InternalTick(object sender, EventArgs e) //Things that happen inside the timer on tick
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

            public string GetElapsed() //Returns elapsed time
            {
                return Elapsed.ToString();
            }

            public string GetEstimated() //Returns estimated time
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

            public void Stop() //Stops the timer
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
            ExperimentStates State; //Current state of the experiment

            public enum ExperimentStates //Possible states of the experiment
            {
                Stopped,
                Running,
                OnTimer
            }

            public enum Species //Organism species and their multiplying period
            {
                Bacteria = 5,
                Virus = 8,
                Fungus = 11
            }

            public List<OrganismTracker> organismTrackers = new List<OrganismTracker>(); //List of organism trackers

            public Experiment(ExperimentStates experimentState, Dispatcher dispatcher, WriteableBitmap writeableBitmap) //Creates an experiment with specific state and 3 trackers for 3 species
            {
                organismTrackers.Add(new OrganismTracker(Species.Bacteria, new byte[4] { 0, 255, 0, 255 }, dispatcher, writeableBitmap));
                organismTrackers.Add(new OrganismTracker(Species.Virus, new byte[4] { 0, 0, 255, 255 }, dispatcher, writeableBitmap));
                organismTrackers.Add(new OrganismTracker(Species.Fungus, new byte[4] { 255, 0, 0, 255 }, dispatcher, writeableBitmap));
            }

            public class OrganismTracker //Tracks one species of organisms
            {
                WriteableBitmap wb;
                Random random = new Random();
                DrawUtils drawUtils;
                Dispatcher dispatcher;
                public Species Species; //Species that are tracked
                public long Population; //Self-explanatory
                public long PreviousStep;
                private int TickTracker = 0; //Tracks when to multiply
                public byte[] ColorData; //Color used during drawing the organism
                public long power; //Amount of organisms represented by 1 pixel

                public OrganismTracker(Species species, byte[] ColorData, Dispatcher dispatcher, WriteableBitmap writeableBitmap) //Creates a tracker of a species and gives them a color
                {
                    this.Species = species;
                    this.ColorData = ColorData;
                    this.dispatcher = dispatcher;
                    this.wb = writeableBitmap;
                    this.drawUtils = new DrawUtils(wb);
                    PreviousStep = 1;
                    Population = 1;
                    DrawOrganisms(1);
                }

                async void Multiply() //Doubles population
                {
                    PreviousStep = Population;
                    Population *= 2;
                    if (power == 1 && Population == 128) { power = 128; }
                    else if (Population / power == 128) { power *= 128; }
                    Clear();
                    int toDraw = (int)(Population / power);
                    await dispatcher.InvokeAsync(() => DrawOrganisms(toDraw));
                }

                private void DrawOrganisms(int amount) //Draws specific amount of pixels on a bitmap
                {
                    int Column;
                    int Row;
                    random = new Random();
                    for (int i = 0; i < amount; i++)
                    {
                        Column = random.Next(0, wb.PixelWidth);
                        Row = random.Next(0, wb.PixelHeight);
                        Int32Rect pix = new Int32Rect(Row, Column, 1, 1);
                        wb.WritePixels(pix, ColorData, DrawUtils.GetStride(wb), 0);
                    }
                }

                private void Clear() //Clears the bitmap
                {
                    dispatcher.Invoke(() =>
                    {
                        Int32Rect pix = new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight);
                        wb.WritePixels(pix, drawUtils.blank, DrawUtils.GetStride(wb), 0);
                    });
                }

                public void Tick(object sender, EventArgs e) //Tick, must be subscribed to tick in timer
                {
                    TickTracker++;
                    if (TickTracker == (int)Species)
                    {
                        TickTracker = 0;
                        Multiply();
                    }
                }

                public string PopulationString //Returns population as a string
                {
                    get
                    {
                        if (Population >= long.MaxValue)
                        {
                            return "OVERCROWDED";
                        }
                        else
                        {
                            return Population.ToString();
                        }
                    }
                }

                private class DrawUtils //Utilities for drawing on a WritableBitmap
                {
                    readonly public byte[] blank; //Array of white pixels. Used for clearing the bitmap

                    public DrawUtils(WriteableBitmap wb)
                    {
                        blank = new byte[wb.PixelWidth * wb.PixelHeight * wb.Format.BitsPerPixel / 8];
                        for (int i = 0; i < blank.Length - 1; i++)
                        {
                            blank[i] = 255;
                        }
                        blank[blank.Length - 1] = 255;
                    }

                    public static int GetStride(WriteableBitmap wb) => wb.PixelWidth * (wb.Format.BitsPerPixel / 8); //Calculates stride, used for writing pixels into the bitmap
                }
            }
        }
    }
}
