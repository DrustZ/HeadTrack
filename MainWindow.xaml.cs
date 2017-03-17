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
using System.Drawing;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using System.Diagnostics;

namespace HeadTrack
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private VideoCapture _capture = null;
        private Mat _frame;
        private bool _captureInProgress = false;
        private FaceDetect detector = new FaceDetect();

        public MainWindow()
        {
            InitializeComponent();
            CvInvoke.UseOpenCL = false;
            try
            {
                _capture = new VideoCapture(1);
                _capture.ImageGrabbed += ProcessFrame;
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
            _frame = new Mat();
            if (_capture != null) _capture.FlipHorizontal = !_capture.FlipHorizontal;
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (_capture != null && _capture.Ptr != IntPtr.Zero)
            {
                _capture.Retrieve(_frame, 0);
                detector.detectFace(_frame, true);
                cambox.Image = _frame;

            }
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_captureInProgress)
            {
                StartBtn.Content = "Start";
                _capture.Pause();

            } else
            {
                StartBtn.Content = "Stop";
                _capture.Start();
            }
            _captureInProgress = !_captureInProgress;
        }

        
    }
}
