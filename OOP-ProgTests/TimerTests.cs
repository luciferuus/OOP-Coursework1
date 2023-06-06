namespace OOP_ProgTests
{
    [TestClass]
    public class TimerTest
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
        public void TestLimitless() {
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
            OOP_Prog.Timer timer = new OOP_Prog.Timer(1, TimeMeasures.Hours);
            DbgTickChecks dbgTickChecks = new DbgTickChecks();
            timer.Tick.Elapsed += dbgTickChecks.TickReciever;
            while(dbgTickChecks.ticksRecieved < 3) { }
            timer.Tick.Elapsed -= dbgTickChecks.TickReciever;
            Assert.AreEqual("00:00:59:57", timer.GetEstimated());
            
            timer.Stop();
            timer = new OOP_Prog.Timer(3, TimeMeasures.Seconds);
            dbgTickChecks.Reset();
            timer.Tick.Elapsed += dbgTickChecks.TickReciever;
            while(dbgTickChecks.ticksRecieved < 3) { }
            Assert.AreEqual(OOP_Prog.Timer.TimerStates.Stopped, timer.State);
            Assert.AreEqual("00:00:00:00", timer.GetEstimated());
        }

        class DbgTickChecks
        {
            public int ticksRecieved { get; private set; }

            public DbgTickChecks() => this.ticksRecieved = 0;

            public void TickReciever(object? sender, EventArgs e) => this.ticksRecieved++;

            public void Reset() => this.ticksRecieved = 0;
        }
    }
}