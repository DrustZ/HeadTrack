using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;

namespace HeadTrack
{
    class Camshift
    {

        private Rectangle _roi = new Rectangle();
        Mat _hsv = new Mat();
        Mat _hist = new Mat();
        public RotatedRect _rotroi;

        public void initializeRect(Mat frame, Rectangle roi)
        {
            _roi = new Rectangle(roi.Location, roi.Size);
            using (Mat _obj = new Mat(frame, roi))
            using (Mat _mask = new Mat())
            using (Emgu.CV.Util.VectorOfMat vm = new Emgu.CV.Util.VectorOfMat())
            {
                CvInvoke.CvtColor(_obj, _hsv, ColorConversion.Bgr2HsvFull);

                CvInvoke.InRange(_hsv, new ScalarArray(new MCvScalar(0, 60, 32)), new ScalarArray(new MCvScalar(180, 255, 255)), _mask);
                vm.Push(_hsv);
                CvInvoke.CalcHist(vm, new int[] { 0 }, _mask, _hist, new int[] { 180 }, new float[] { 0, 180 }, false);
                CvInvoke.Normalize(_hist, _hist, 0, 255, NormType.MinMax);
            }
        }

        public Rectangle camshift(Mat frame)
        {
                using (Mat _bp = new Mat())
                using (Emgu.CV.Util.VectorOfMat vm = new Emgu.CV.Util.VectorOfMat())
                {
                    CvInvoke.CvtColor(frame, _hsv, ColorConversion.Bgr2HsvFull);
                    vm.Push(_hsv);
                    CvInvoke.CalcBackProject(vm, new int[] { 0 }, _hist, _bp, new float[] { 0, 180 });

                    MCvTermCriteria TermCriteria = new MCvTermCriteria() { Epsilon = 100 * Double.Epsilon, MaxIter = 10 };
                    _rotroi = CvInvoke.CamShift(_bp, ref _roi, TermCriteria);
                }
            return _roi;

        }
    }
}
