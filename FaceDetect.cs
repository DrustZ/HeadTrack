using Emgu.CV;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace HeadTrack
{
    class FaceDetect
    {
        private CascadeClassifier face = null;
        Stopwatch watch;
        Mat _grayFrame;
        Rectangle roi;

        public FaceDetect()
        {
            _grayFrame = new Mat();
            face = new CascadeClassifier("D:/Codes/Lab/HeadTracking/HeadTrack/haarcascade_frontalface_default.xml");
            roi = new Rectangle(0,0,0,0);
        }

        public Rectangle detectFace(Mat frame, bool debug = false)
        {
            if (debug)
                watch = Stopwatch.StartNew();
            CvInvoke.CvtColor(frame, _grayFrame, ColorConversion.Bgr2Gray);
            CvInvoke.EqualizeHist(_grayFrame, _grayFrame);
            Rectangle[] facesDetected = face.DetectMultiScale(
                        _grayFrame,
                        1.3,
                        7,
                        new System.Drawing.Size(70, 70));
            
            foreach (Rectangle face in facesDetected)
            {
                //CvInvoke.Rectangle(frame, face, new Bgr(System.Drawing.Color.Red).MCvScalar, 2);
                if (roi.Height * roi.Width < face.Height * face.Width)
                    roi = face;
            }

            if (facesDetected.Count() == 0) roi.Height = 0;

            if (debug)
            {
                Console.WriteLine("find {0} faces!", facesDetected.Count());
                watch.Stop();
                Console.WriteLine("time elapse : {0}", watch.ElapsedMilliseconds);
            }

            return roi;
        }
    }
}
