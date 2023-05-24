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
        public MainWindow()
        {
            InitializeComponent();

        }
    }

    public class Timer
    {
        public enum TimerStates
        {
            Limited,
            Limitless
        }

        public struct Time
        {
            int Days;
            int Hours;
            int Minutes;
            int Seconds;

            public static Time operator --(Time a)
            {
                if (a.Seconds > 0) { a.Seconds--; }
                while (true)
                {
                    if (a.Hours == 0)
                    {
                        a.Days--;
                        a.Hours = 23;
                    }
                    else if (a.Minutes == 0)
                    {
                        a.Hours--;
                        a.Minutes = 59;
                    }
                    else if (a.Seconds == 0)
                    {
                        a.Minutes--;
                        a.Seconds = 59;
                    }
                    else
                    {
                        break;
                    }
                }
                return a;
            }
        }

        public TimerStates state { get; private set; }

        private Time time { get; set; }

        DispatcherTimer dispatcherTimer;

        public Timer()
        {
            state = TimerStates.Limitless;
            Initialize();
        }

        public Timer(int value, TimeMeasures measure)
        {
            state = TimerStates.Limited;
            Initialize();
        }

        public void Tick(object sender, EventArgs e)
        {

        }

        private void Initialize()
        {
            
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += Tick;
            dispatcherTimer.Interval = new TimeSpan(1);
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
    }
}
