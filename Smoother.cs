using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadTrack
{
    
    class Smoother
    {
        private float alpha, interval, lastUpdate;
        private List<float> sp, sp2;
        private bool interpolate = false;
        Stopwatch sw = new Stopwatch();
        public bool initialized = false;
        
        public Smoother(float inter, bool interpolate = false, float alp = 0.4f)
        {
            alpha = alp;
            interval = inter;
            this.interpolate = interpolate;
        }

        public void initialize(List<float> positions)
        {
            initialized = true;
            sw.Start();
            lastUpdate = sw.ElapsedMilliseconds;
            sp = new List<float>(positions);
            sp2 = new List<float>(positions);
        }

        public List<float> smooth(List<float> positions)
        {
            if (initialized)
            {
                // update
                for (var i = 0; i < positions.Count(); i++)
                {
                    sp[i] = alpha * positions[i] + (1 - alpha) * sp[i];
                    sp2[i] = alpha * sp[i] + (1 - alpha) * sp2[i];
                }

                // set time

                var msDiff = sw.ElapsedMilliseconds - lastUpdate;
                lastUpdate += msDiff;
                var newPositions = predict(msDiff);

                return newPositions;
            }
            return positions;
        }

        List<float> predict(float time)
        {
            var retPos = new List<float>();

            if (this.interpolate)
            {
                var step = time / interval;
                
                var stepLo = (int)step;
                var ratio = alpha / (1 - alpha);

                var a = (step - stepLo) * ratio;
                var b = (2 + stepLo * ratio);
                var c = (1 + stepLo * ratio);

                for (var i = 0; i < sp.Count(); i++)
                {
                    retPos.Add(a * (sp[i] - sp2[i]) + b * sp[i] - c * sp2[i]);
                }
            }
            else
            {
                var step = (int)(time / interval);
                var ratio = (alpha * step) / (1 - alpha);
                var a = 2 + ratio;
                var b = 1 + ratio;
                for (var i = 0; i < sp.Count(); i++)
                {
                    retPos.Add(a * sp[i] - b * sp2[i]);
                }
            }

            return retPos;
        }
    }
}
