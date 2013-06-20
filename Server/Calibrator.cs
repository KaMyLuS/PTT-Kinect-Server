using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Media.Media3D;

namespace Server
{
    public class Calibrator
    {
        // punkty kalibracji, zaczynajac od lewego gornego rogu idac zgodnie z ruchem wskazowek zegara
        private Point3D[] calibrationPoints = new Point3D[6];

        // wskazuje na numer nastepnego punktu kalibracji
        private int nextPointIndex = 0;

        // czy proces kalibracji sie zakonczyl
        private bool calibrated = false;

        private readonly MainEngine mainEngine;

        public Calibrator(MainEngine me)
        {
            mainEngine = me;
        }

        // ustawianie nastepnego punktu kalibracji
        public void SetNextCalibrationPoint(double x, double y, double z = 0)
        {
            if (!calibrated)
            {
                // tu wypada spr sensownosc punktu
                calibrationPoints[nextPointIndex] = new Point3D(x, y, z);
                nextPointIndex++;

                mainEngine.AddTextToLog("Calibration point: " + x.ToString() + " " + y.ToString() + " " + z.ToString());
            }
            if (nextPointIndex == 6) // tu jeszcze inne pierdoly trzeba sprawdzic (np. sensownosc punktow)
            {
                calibrated = true;

                if (mainEngine.GetAppState() == ApplicationState.Calibration)
                {
                    mainEngine.SetAppState(ApplicationState.Calibrated);
                    mainEngine.AddTextToLog("Calibration end");
                }
                else
                {
                    // cos poszlo zle, skoro apka nie byla w stanie kalibracji a kalibracja sie odbywala...
                }
            }
        }

        public void SetNextCalibrationPoint(Point3D point)
        {
            SetNextCalibrationPoint(point.X, point.Y, point.Z);
        }

        public void Reset()
        {
            calibrated = false;
            nextPointIndex = 0;

            if (mainEngine.IsClientConnected && mainEngine.IsKinectConnected)
                mainEngine.SetAppState(ApplicationState.Ready);
            else mainEngine.SetAppState(ApplicationState.NotReady);
        }

        public bool IsCalibrated()
        {
            return calibrated;
        }

        public Point ScaleKinectPositionToScreen(Point3D kinectPos)
        {
            // TO DO!!
            Point res = new Point(0, 0);
            return res;
        }

        public Point ScaleKinectPositionToScreen(double x, double y, double z = 0)
        {
            return ScaleKinectPositionToScreen(new Point3D(x, y, z));
        }

        public Point3D ScaleScreenPositionToKinect(Point screenPos)
        {
            // TO DO !!
            Point3D res = new Point3D(0, 0, 0);
            return res;
        }

        public Point3D ScaleScreenPositionToKinect(int x, int y)
        {
            return ScaleScreenPositionToKinect(new Point(x, y));
        }

        public double ScaleKinectDistanceToScreen(double d)
        {
            // TO DO!
            return d;
        }

        public double ScaleScreenDistanceToKinect(double d)
        {
            // TO DO!
            return d;
        }
    }
}
