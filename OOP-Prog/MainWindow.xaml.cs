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
            Stopped,
            Running
        }

        public TimerStates State { get; }
        DispatcherTimer dispatcherTimer;

        public Timer(int seconds)
        {

        }

        public Timer(int value, TimeMeasures measure)
        {

        }

        public void Tick(object sender, EventArgs e)
        {

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
