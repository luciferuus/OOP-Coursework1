namespace OOP_ProgTests
{
    [TestClass]
    public class TimerTests
    {
        [TestClass]
        public class TimeStructTests
        {
            [TestMethod]
            public void TestOperations() 
            { 
                OOP_Prog.Timer.Time time = new OOP_Prog.Timer.Time(0, 1, 0, 0);
                time--;
                Assert.AreEqual(new OOP_Prog.Timer.Time(0, 0, 59, 59), time);
                time++; time++;
                Assert.AreEqual(new OOP_Prog.Timer.Time(0, 1, 0, 1), time);
            }

            [TestMethod]
            public void TestToString()
            {
                OOP_Prog.Timer.Time time = new OOP_Prog.Timer.Time(1, 0, 0, 0);
                for(int i = 0; i < 5; i++)
                {
                    time--;
                }
                Assert.AreEqual("00:23:59:55", time.ToString());
            }
        }

        [TestMethod]
        public void TestLimitless() 
        {
            OOP_Prog.Timer timer = new OOP_Prog.Timer();
            Assert.AreEqual(OOP_Prog.Timer.TimerStates.Limitless, timer.State);

            Assert.AreEqual("Infinity", timer.GetEstimated());

            DbgTickChecks dbgTickChecks = new DbgTickChecks();
            timer.Tick.Elapsed += dbgTickChecks.TickReciever;
            while(dbgTickChecks.ticksRecieved < 3) { }
            Assert.AreEqual("00:00:00:03", timer.GetElapsed());

            timer.Stop();
            Assert.AreEqual(OOP_Prog.Timer.TimerStates.Stopped, timer.State);
        }

        [TestMethod]
        public void TestLimited()
        {
            OOP_Prog.Timer timer = new OOP_Prog.Timer(1, TimeMeasures.Days);
            Assert.AreEqual(OOP_Prog.Timer.TimerStates.Limited, timer.State);
            DbgTickChecks dbgTickChecks = new DbgTickChecks();
            timer.Tick.Elapsed += dbgTickChecks.TickReciever;
            while(dbgTickChecks.ticksRecieved < 3) { }
            timer.Tick.Elapsed -= dbgTickChecks.TickReciever;
            Assert.AreEqual("00:23:59:57", timer.GetEstimated());
            
            timer.Stop();
            timer = new OOP_Prog.Timer(3, TimeMeasures.Seconds);
            dbgTickChecks.Reset();
            timer.Tick.Elapsed += dbgTickChecks.TickReciever;
            while(dbgTickChecks.ticksRecieved < 3) { }
            Assert.AreEqual(OOP_Prog.Timer.TimerStates.Stopped, timer.State);
            Assert.AreEqual("00:00:00:00", timer.GetEstimated());
        }
    }

    [TestClass]
    public class ExperimentTests
    {
        [TestMethod]
        public void TestOrgTracker()
        {
            ReExperiment.OrganismTracker bacterias = new ReExperiment.OrganismTracker(ReExperiment.Species.Bacteria, new Random());
            ReExperiment.OrganismTracker viruses = new ReExperiment.OrganismTracker(ReExperiment.Species.Virus, new Random());
            ReExperiment.OrganismTracker fungi = new ReExperiment.OrganismTracker(ReExperiment.Species.Fungus, new Random());
            Assert.AreEqual(ReExperiment.Species.Bacteria, bacterias.Species);
            Assert.AreEqual("1", bacterias.PopulationString);

            DbgTickChecks dbgTickChecks = new DbgTickChecks();
            OOP_Prog.Timer timer = new OOP_Prog.Timer();
            timer.Tick.Elapsed += dbgTickChecks.TickReciever;
            timer.Tick.Elapsed += bacterias.Tick;
            timer.Tick.Elapsed += viruses.Tick;
            timer.Tick.Elapsed += fungi.Tick;
            while(dbgTickChecks.ticksRecieved < 5) { }
            timer.Stop();
            Assert.AreEqual("10", bacterias.PopulationString);
            Assert.AreEqual("6", viruses.PopulationString);
            Assert.AreEqual("4", fungi.PopulationString);
        }

        [TestMethod]
        public void TestExperiment()
        {
            ReExperiment experiment = new ReExperiment(ReExperiment.ExperimentStates.Running);
            Assert.AreEqual(3, experiment.organismTrackers.Count);
            Assert.AreEqual(ReExperiment.Species.Bacteria, experiment.organismTrackers[0].Species);
            Assert.AreEqual(ReExperiment.Species.Virus, experiment.organismTrackers[1].Species);
            Assert.AreEqual(ReExperiment.Species.Fungus, experiment.organismTrackers[2].Species);
        }

        class ReExperiment
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

            public ReExperiment(ExperimentStates experimentState) //Creates an experiment with specific state and 3 trackers for 3 species
            {
                random = new Random();
                organismTrackers.Add(new OrganismTracker(Species.Bacteria, random));
                organismTrackers.Add(new OrganismTracker(Species.Virus, random));
                organismTrackers.Add(new OrganismTracker(Species.Fungus, random));
            }

            public class OrganismTracker //Tracks one species of organisms
            {
                Random random; //Random
                DrawUtils drawUtils; //Tools used in drawing on WritableBitma
                public Species Species; //Species that are tracked
                public long Population; //Self-explanatory
                private int TickTracker = 0; //Tracks when to multiply
                public long power = 1; //Amount of organisms represented by 1 pixel
                private long drawn = 0; //Amount of already drawn organisms
                private Func<long, long> progression = x => (long)Math.Round(Math.Sqrt(x * 4 * Math.Pow(1.1, 16))); //Determines speed of organism multiplication
                private long ticks = 0; //Tick tracker for correct progression

                public OrganismTracker(Species species, Random random) //Creates a tracker of a species and gives them a color
                {
                    this.Species = species;
                    this.drawUtils = new DrawUtils();
                    this.random = random;
                    Population = 1;
                    DrawOrganisms((int)Population / (int)power);
                }

                void Multiply() //Doubles population
                {
                    ticks++;
                    Population = progression(ticks);
                    int toDraw = (int)((Population - drawn) / power);
                    DrawOrganisms(toDraw);
                }

                private void DrawOrganisms(int amount) //Draws specific amount of pixels on a bitmap
                {
                    drawn += amount * power;
                }

                private int GetCoordinate(int origin, double spread)
                {
                    double x1 = 1 - random.NextDouble();
                    double x2 = 1 - random.NextDouble();

                    double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
                    return (int)((y1 * spread) % 1000) + origin;
                }

                public void Tick(object? sender, EventArgs e) //Tick, must be subscribed to tick in timer
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
                    public DrawUtils()
                    {
                        
                    }

                    public static int GetStride(int width, int BPP) => width * (BPP / 8); //Calculates stride, used for writing pixels into the bitmap
                }
            }
        }
    }

    class DbgTickChecks
    {
        public int ticksRecieved { get; private set; }

        public DbgTickChecks() => this.ticksRecieved = 0;

        public void TickReciever(object? sender, EventArgs e) => this.ticksRecieved++;

        public void Reset() => this.ticksRecieved = 0;
    }
}