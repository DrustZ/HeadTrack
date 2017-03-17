using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace HeadTrack
{
    class HeadPosition
    {
        float head_width_cm = 16.0f;
        float head_height_cm = 20.0f;
        // angle between side of face and diagonal across
        float head_small_angle;
        float head_diag_cm; // diagonal of face in real space
        float tan_hsa, cos_hsa, sin_hsa;
        float fov_width, tan_fov_width;
        float camheight_cam, camwidth_cam, distance_from_camera_to_screen;

        List<float> headDiagonal = new List<float>();

        public float x, y, z; //position in cm
        public bool stable = false;

        public HeadPosition(float fov, float cam_h, float cam_w, float dist_from_cam_to_screen = 9.0f)
        {
            head_small_angle = (float)Math.Atan(head_width_cm / head_height_cm);
            head_diag_cm = (float)Math.Sqrt((head_width_cm * head_width_cm) + (head_height_cm * head_height_cm));
            sin_hsa = (float)Math.Sin(head_small_angle); //precalculated sine
            cos_hsa = (float)Math.Cos(head_small_angle); //precalculated cosine
            tan_hsa = (float)Math.Tan(head_small_angle); //precalculated tan

            fov_width = (float)(fov* Math.PI / 180.0);
            tan_fov_width = 2 * (float)Math.Tan(fov_width / 2);
            distance_from_camera_to_screen = dist_from_cam_to_screen;
            camheight_cam = cam_h;
            camwidth_cam  = cam_w;
        }

        public void waitToStable(Rectangle facetrackrObj)
        {
            stable = false;

            // calculate headdiagonal
            var headdiag = (float)Math.Sqrt(facetrackrObj.Width * facetrackrObj.Width + facetrackrObj.Height * facetrackrObj.Height);

            if (headDiagonal.Count() < 6)
            {
                headDiagonal.Add(headdiag);
            }
            else
            {
                headDiagonal.RemoveAt(0);
                headDiagonal.Add(headdiag);
                if ((headDiagonal.Max() - headDiagonal.Min()) < 5)
                {
                    stable = true;
                }
            }
        }

        public void TrackPosition(Rectangle facetrackrObj)
        {

            var w = facetrackrObj.Width;
            var h = facetrackrObj.Height;
            var fx = facetrackrObj.X;
            var fy = facetrackrObj.Y;
            float head_diag_cam = (float)Math.Sqrt((w * w) + (h * h));
            // calculate cm-distance from screen
            z = (head_diag_cm * this.camwidth_cam) / (tan_fov_width * head_diag_cam);

            // calculate cm-position relative to center of screen
            x = -((fx / this.camwidth_cam) - 0.5f) * z * tan_fov_width;
            y = -((fy / this.camheight_cam) - 0.5f) * z * tan_fov_width * (this.camheight_cam / this.camwidth_cam);
            
            // Transformation from position relative to camera, to position relative to center of screen
            y = y + distance_from_camera_to_screen;
        }
    }
}
