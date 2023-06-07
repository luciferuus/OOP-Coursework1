using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Serialization;

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
        private Timer timer;
        private Experiment experiment;
        readonly WriteableBitmap bitCanvas;

        public MainWindow() //Window initialization
        {
            InitializeComponent();
            bitCanvas = new WriteableBitmap(350, 350, 100, 100, PixelFormats.Bgra32, null);
        }

        public Experiment Experiment { get; set; }

        public Timer Timer { get; set; }

        public WriteableBitmap BitCanvas => bitCanvas;

        #region Button events

        private void Start_Click(object sender, EventArgs e) //Handles clicking the "Start" button
        {
            if (Timer == null || Timer.State == Timer.TimerStates.Stopped)
            {
                byte[] blank = new byte[BitCanvas.PixelWidth * BitCanvas.PixelHeight * BitCanvas.Format.BitsPerPixel / 8];
                for (int i = 0; i < blank.Length - 4; i += 4)
                {
                    blank[i] = 0;
                    blank[i + 1] = 0;
                    blank[i + 2] = 0;
                    blank[i + 3] = 255;
                }

                Dispatcher.Invoke(() =>
                {
                    Int32Rect pix = new Int32Rect(0, 0, BitCanvas.PixelWidth, BitCanvas.PixelHeight);
                    BitCanvas.WritePixels(pix, blank, BitCanvas.PixelWidth * (BitCanvas.Format.BitsPerPixel / 8), 0);
                });

                Timer = new Timer();
                DishPic.Source = BitCanvas;
                Experiment = new Experiment(Experiment.ExperimentStates.Running, Dispatcher, BitCanvas);
                EventsSubscribe();
            } else
            {
                MessageBox.Show("Unable to start new experiment when one is already in process.", "Start denied", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        private void StartTimed_Click(object sender, EventArgs e) //Handles clicking the "Start" button in TimeExp zone
        {
            TextboxExperimentTime.BorderBrush = Brushes.Gray;
            if (Timer == null || Timer.State == Timer.TimerStates.Stopped)
            {
                if (GetTextboxInput() != null)
                {
                    byte[] blank = new byte[BitCanvas.PixelWidth * BitCanvas.PixelHeight * BitCanvas.Format.BitsPerPixel / 8];
                    for (int i = 0; i < blank.Length - 4; i += 4)
                    {
                        blank[i] = 0;
                        blank[i + 1] = 0;
                        blank[i + 2] = 0;
                        blank[i + 3] = 255;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        Int32Rect pix = new Int32Rect(0, 0, BitCanvas.PixelWidth, BitCanvas.PixelHeight);
                        BitCanvas.WritePixels(pix, blank, BitCanvas.PixelWidth * (BitCanvas.Format.BitsPerPixel / 8), 0);
                    });

                    Timer = new Timer((int)GetTextboxInput(), (TimeMeasures)ComboboxTimeMode.SelectedIndex);
                    DishPic.Source = BitCanvas;
                    Experiment = new Experiment(Experiment.ExperimentStates.OnTimer, Dispatcher, BitCanvas);
                    EventsSubscribe();
                } else
                {
                    MessageBox.Show("Time must be a whole number >= 0. Please try again.", "Input error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            } else
            {
                MessageBox.Show("Unable to start new experiment when one is already in process.", "Start denied", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Stop_Click(object sender, EventArgs e) //Handles clicking the "Stop" button
        {
            if (Timer != null && Timer.State != Timer.TimerStates.Stopped)
            {
                Timer.Stop();
                EventsUnsubscribe();
            } else
            {
                MessageBox.Show("To stop an experiment, there must be a running one.", "Nothing to stop", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        public void UpdateTimeTrackers(object sender, EventArgs e) //Updates labels keeping track of time
        {
            ElapsedLabel.Text = Timer.GetElapsed();
            EstimatedLabel.Text = Timer.GetEstimated();
        }

        public void UpdateOrganismLabels(object sender, EventArgs e) //Updates values in labels tracking population of organisms
        {
            LabelBacteriaCount.Text = Experiment.organismTrackers[0].PopulationString;
            LabelVirusCount.Text = Experiment.organismTrackers[1].PopulationString;
            LabelFungiCount.Text = Experiment.organismTrackers[2].PopulationString;
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
            foreach (Experiment.OrganismTracker ot in Experiment.organismTrackers)
            {
                Timer.Tick.Elapsed += ot.Tick;
            }
            CompositionTarget.Rendering += UpdateTimeTrackers;
            CompositionTarget.Rendering += UpdateOrganismLabels;
        }

        private void EventsUnsubscribe() //Unsubscribes OrganismTrackers from Timer tick and UI stuff from Rendering tick
        {
            foreach (Experiment.OrganismTracker ot in Experiment.organismTrackers)
            {
                Timer.Tick.Elapsed -= ot.Tick;
            }
            CompositionTarget.Rendering -= UpdateTimeTrackers;
            CompositionTarget.Rendering -= UpdateOrganismLabels;
        }

        private void ShowAbout(object sender, EventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();
        }
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
                        a.Minutes = 60; a.Hours--;
                    }
                    else if (a.Hours == 0 && a.Days > 0)
                    {
                        a.Hours = 24; a.Days--;
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
            if (this.State != TimerStates.Stopped)
            {
                Elapsed++;
            }
            switch (State)
            {
                case TimerStates.Limitless:

                    break;

                case TimerStates.Limited:
                    Estimated--;
                    if (Estimated.ToString() == "00:00:00:00")
                    {
                        this.State = TimerStates.Stopped;
                        this.Stop();
                    }
                    break;

                case TimerStates.Stopped:

                    break;
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
        Random random;

        public enum ExperimentStates //Possible states of the experiment
        {
            Stopped,
            Running,
            OnTimer
        }

        public enum Species //Organism species and their multiplying period
        {
            Bacteria = 1,
            Virus = 2,
            Fungus = 3
        }

        public List<OrganismTracker> organismTrackers = new List<OrganismTracker>(); //List of organism trackers

        public Experiment(ExperimentStates experimentState, Dispatcher dispatcher, WriteableBitmap writeableBitmap) //Creates an experiment with specific state and 3 trackers for 3 species
        {
            random = new Random();
            organismTrackers.Add(new OrganismTracker(Species.Bacteria, new byte[4] { 0, 255, 0, 255 }, dispatcher, writeableBitmap, random));
            organismTrackers.Add(new OrganismTracker(Species.Virus, new byte[4] { 0, 0, 255, 255 }, dispatcher, writeableBitmap, random));
            organismTrackers.Add(new OrganismTracker(Species.Fungus, new byte[4] { 255, 0, 0, 255 }, dispatcher, writeableBitmap, random));
        }

        public class OrganismTracker //Tracks one species of organisms
        {
            WriteableBitmap wb; //Canvas for pixels
            Random random; //Random
            DrawUtils drawUtils; //Tools used in drawing on WritableBitmap
            Dispatcher dispatcher; //Dispathcer. Used to execute code on UI thread
            public Species Species; //Species that are tracked
            public long Population; //Self-explanatory
            private int TickTracker = 0; //Tracks when to multiply
            public byte[] ColorData; //Color used during drawing the organism
            private long power { get; set; } = 1; //Amount of organisms represented by 1 pixel
            private long drawn = 0; //Amount of already drawn organisms
            private Func<long, long> progression = x => (long)Math.Round(Math.Sqrt(2 * x * 4 * Math.Pow(1.21, 16))); //Determines speed of organism multiplication
            private long ticks = 0; //Tick tracker for correct progression

            public OrganismTracker(Species species, byte[] ColorData, Dispatcher dispatcher, WriteableBitmap writeableBitmap, Random random) //Creates a tracker of a species and gives them a color
            {
                this.Species = species;
                this.ColorData = ColorData;
                this.dispatcher = dispatcher;
                this.wb = writeableBitmap;
                this.drawUtils = new DrawUtils(wb);
                this.random = random;
                Population = 1;
                DrawOrganisms((int)Population / (int)power);
            }

            async void Multiply() //Doubles population
            {
                ticks++;
                Population = progression(ticks);
                int toDraw = (int)((Population - drawn) / power);
                await dispatcher.InvokeAsync(() => DrawOrganisms(toDraw));
            }

            private void DrawOrganisms(int amount) //Draws specific amount of pixels on a bitmap
            {
                drawn += amount * power;
                int Column;
                int Row;
                for (int i = 0; i < amount; i++)
                {
                    Column = GetCoordinate(wb.PixelWidth / 2, wb.PixelWidth / 10);
                    Row = GetCoordinate(wb.PixelHeight / 2, wb.PixelHeight / 10);
                    Int32Rect pix = new Int32Rect(Row, Column, 1, 1);
                    wb.WritePixels(pix, ColorData, DrawUtils.GetStride(wb), 0);
                }
            }

            public void ClearBitmap() //Clears the bitmap
            {
                dispatcher.Invoke(() =>
                {
                    Int32Rect pix = new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight);
                    wb.WritePixels(pix, drawUtils.blank, DrawUtils.GetStride(wb), 0);
                });
            }

            private int GetCoordinate(int origin, double spread)
            {
                double x1 = 1 - random.NextDouble();
                double x2 = 1 - random.NextDouble();

                double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
                return (int)((y1 * spread) % 1000) + origin;
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
                    if (Population >= long.MaxValue - 100)
                    {
                        Population = long.MaxValue - 100;
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
                    for (int i = 0; i < blank.Length - 4; i += 4)
                    {
                        blank[i] = 0;
                        blank[i + 1] = 0;
                        blank[i + 2] = 0;
                        blank[i + 3] = 255;
                    }
                }

                public static int GetStride(WriteableBitmap wb) => wb.PixelWidth * (wb.Format.BitsPerPixel / 8); //Calculates stride, used for writing pixels into the bitmap
            }
        }
    }
}
