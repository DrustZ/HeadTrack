using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        private Rectangle _roi = new Rectangle();
        private bool _captureInProgress = false;
        private bool _detected = false;
        private FaceDetect detector = new FaceDetect();
        private Camshift camshift = new Camshift();
        private FastDetector fastdetector = new FastDetector();
        private Smoother smoother = new Smoother(30.0f, true, 0.35f);
        private HeadPosition headposition = null;

        Stopwatch sw;

        Mat _hist = new Mat();
        Mat _hsv = new Mat();

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
                sw = Stopwatch.StartNew();
                _capture.Retrieve(_frame, 0);

                //Detect_With_Camshift();
                Detect_With_TemplateMatching();

                if (_roi.Height < 10 || _roi.Width < 10)
                    _detected = false;
                else
                {
                    List<float> pos = smoother.smooth(new List<float>(new float[] {_roi.X, _roi.Y, _roi.Height, _roi.Width} ));
                    _roi.X = (int)pos[0];
                    _roi.Y = (int)pos[1];
                    _roi.Height = (int)pos[2];
                    _roi.Width = (int)pos[3];
                    
                    //if (fastdetector._foundface)
                        CvInvoke.Rectangle(_frame, _roi, new Bgr(Color.Red).MCvScalar, 2);
                    if (headposition == null)
                        headposition = new HeadPosition(70.0f, _frame.Rows, _frame.Cols, 2);
                    if (!headposition.stable)
                        headposition.waitToStable(_roi);
                    else
                    {
                        headposition.TrackPosition(_roi);
                        Console.WriteLine("x : {0} y : {1} z : {2}", headposition.x, headposition.y, headposition.z);
                    }
                }
                cambox.Image = _frame;

                sw.Stop();
                Console.WriteLine("time elapse : {0}", sw.ElapsedMilliseconds);
            }
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_captureInProgress)
            {
                StartBtn.Content = "Start";
                _capture.Pause();
                _detected = false;

            } else
            {
                StartBtn.Content = "Stop";
                _capture.Start();
               
            }
            _captureInProgress = !_captureInProgress;
        }
        
        private void Detect_With_Camshift()
        {
            if (!_detected)
            {
                _roi = detector.detectFace(_frame, true);
                if (_roi.Height != 0)
                {
                    _detected = true;
                    camshift.initializeRect(_frame, _roi);
                    smoother.initialize(new List<float>(new float[] { _roi.X, _roi.Y, _roi.Height, _roi.Width }));
                }
            }
            else
            {
                _roi = camshift.camshift(_frame);
            }
        }

        private void Detect_With_TemplateMatching()
        {
            _roi = fastdetector.detect(_frame);
            smoother.initialize(new List<float>(new float[] { _roi.X, _roi.Y, _roi.Height, _roi.Width }));
        }
    }
}
