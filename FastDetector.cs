using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Diagnostics;

namespace HeadTrack
{
    class FastDetector
    {
        private CascadeClassifier _faceCascade= null;
        Stopwatch watch;
        Mat _grayFrame, _template;
        Rectangle _roi, _searchRoi;
        Size resizedSize;
        bool _templateMatchingRunning = false;
        
        float _templateMatchingMaxDuration = 0.5f;
        float _templateMatchingStartTime, _templateMatchingCurrentTime = 0;
        int _resizeWidth = 640;
        float _scale = 1;

        public bool _foundface = false;

        public FastDetector(float maxDuration = 2.0f)
        {
            _grayFrame = new Mat();
            _template = new Mat();
            _faceCascade = new CascadeClassifier("D:/Codes/Lab/HeadTracking/HeadTrack/haarcascade_frontalface_default.xml");
            _roi = new Rectangle(0, 0, 0, 0);
            _searchRoi = new Rectangle();
            watch = new Stopwatch();
            watch.Start();
            _templateMatchingMaxDuration = maxDuration;
        }

        public Rectangle detect(Mat frame)
        {
            _scale = (float)Math.Min(_resizeWidth, frame.Cols) / frame.Cols;
            resizedSize = new Size((int)(_scale * frame.Cols), (int)(_scale * frame.Rows));
            Mat resizedFrame = new Mat();
            CvInvoke.Resize(frame, resizedFrame, resizedSize);
            CvInvoke.CvtColor(frame, resizedFrame, ColorConversion.Bgr2Gray);
            CvInvoke.EqualizeHist(resizedFrame, resizedFrame);
            if (!_foundface)
                detectFaceAllSizes(ref resizedFrame);
            else
            {
                detectFaceAroundRoi(ref resizedFrame);
                if (_templateMatchingRunning)
                    detectFaceTemplateMatching(ref resizedFrame);
            }
            return new Rectangle((int)(_roi.X/_scale), (int)(_roi.Y/_scale), (int)(_roi.Width/_scale), (int)(_roi.Height/_scale));
        }

        Rectangle biggestFace(Rectangle[] faces)
        {
            Rectangle biggest = faces[0];
            foreach (var face in faces)
            {
                if (biggest.Height * biggest.Width < face.Width * face.Height)
                    biggest = face;
            }
            return biggest;
        }

        Rectangle doubleRoiSize()
        {
            Rectangle returnRect = new Rectangle();
            // Double rect size
            returnRect.Width = _roi.Width * 2;
            returnRect.Height = _roi.Height * 2;

            // Center rect around original center
            returnRect.X = _roi.X - _roi.Width / 2;
            returnRect.Y = _roi.Y - _roi.Height / 2;

            // Handle edge cases
            if (returnRect.X < 0)
            {
                returnRect.Width += returnRect.X;
                returnRect.X = 0;
            }
            if (returnRect.Y < 0)
            {
                returnRect.Height += returnRect.Y;
                returnRect.Y = 0;
            }

            if (returnRect.X + returnRect.Width > resizedSize.Width)
            {
                returnRect.Width = resizedSize.Width - returnRect.X;
            }
            if (returnRect.Y + returnRect.Height > resizedSize.Height)
            {
                returnRect.Height = resizedSize.Height - returnRect.Y;
            }
            return returnRect;
        }

        void setFaceTemplate(ref Mat frame)
        {
            _template = new Mat(frame, new Rectangle(_roi.X + _roi.Width / 4, _roi.Y + _roi.Height / 4, _roi.Width / 2, _roi.Height / 2));
        }
        
        void detectFaceAllSizes(ref Mat frame)
        {
            Rectangle[] facesDetected = _faceCascade.DetectMultiScale(frame, 1.05, 3, new Size(frame.Rows / 5, frame.Cols / 5));
            if (facesDetected.Count() <= 0) return;
            _foundface = true;
            _roi = biggestFace(facesDetected);
            _searchRoi = doubleRoiSize();
            setFaceTemplate(ref frame);
        }
        
        void detectFaceAroundRoi(ref Mat frame)
        {
            // Detect faces sized +/-20% off biggest face in previous search
            Rectangle[] facesDetected = _faceCascade.DetectMultiScale(new Mat(frame, _searchRoi), 1.05, 3, new Size((int)(_roi.Width * 0.8f), (int)(_roi.Height * 0.8f)),
                                          new Size((int)(_roi.Width * 1.2f), (int)(_roi.Height * 1.2f)));

            if (facesDetected.Count() == 0)
            {
                // Activate template matching if not already started and start timer
                _templateMatchingRunning = true;
                if (_templateMatchingStartTime == 0)
                    _templateMatchingStartTime = watch.ElapsedMilliseconds;
                return;
            }

            // Turn off template matching if running and reset timer
            _templateMatchingRunning = false;
            _templateMatchingCurrentTime = _templateMatchingStartTime = 0;

            // Get detected face
            _roi = biggestFace(facesDetected);

            // Add roi offset to face
            _roi.X += _searchRoi.X;
            _roi.Y += _searchRoi.Y;

            // Get face template
            setFaceTemplate(ref frame);
            _searchRoi = doubleRoiSize();
        }

        void detectFaceTemplateMatching(ref Mat frame)
        {
            // Calculate duration of template matching
            float duration = (float)(watch.ElapsedMilliseconds - _templateMatchingStartTime) / 1000;

            // If template matching lasts for more than 2 seconds face is possibly lost
            // so disable it and redetect using cascades
            if (duration > _templateMatchingMaxDuration || _template.Rows <= 1 || _template.Cols <= 1) {
                _foundface = false;
                _templateMatchingRunning = false;
                _templateMatchingStartTime = 0;
                _roi.Width = _roi.Height = 0;
		        return;
            }

            // Template matching with last known face 
            //cv::matchTemplate(frame(m_faceRoi), m_faceTemplate, m_matchingResult, CV_TM_CCOEFF);
            using (Mat _matchResult = new Mat())
            {
                CvInvoke.MatchTemplate(new Mat(frame, _searchRoi), _template, _matchResult, TemplateMatchingType.SqdiffNormed);
                CvInvoke.Normalize(_matchResult, _matchResult, 0, 1, NormType.MinMax);
                double min = 0;
                double max = 0;
                Point minLoc = new Point();
                Point maxLoc = new Point();
                CvInvoke.MinMaxLoc(_matchResult, ref min, ref max, ref minLoc, ref maxLoc);

                // Add roi offset to face position
                minLoc.X += _searchRoi.X;
                minLoc.Y += _searchRoi.Y;

                // Get detected face
                _roi = new Rectangle(minLoc.X, minLoc.Y, _template.Cols, _template.Rows);
                _roi = doubleRoiSize();
            }
            // Get new face template
            setFaceTemplate(ref frame);

            // Calculate search roi
            _searchRoi = doubleRoiSize();
        }

    }
}
