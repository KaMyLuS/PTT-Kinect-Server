using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Server
{
    class Calibrator
    {
        // punkty kalibracji, zaczynajac od lewego gornego rogu idac zgodnie z ruchem wskazowek zegara
        private Point3D[] calibrationPoints = new Point3D[6];

        // wskazuje na numer nastepnego punktu kalibracji
        private int nextPointIndex = 0;

        // czy proces kalibracji sie zakonczyl
        private bool calibrated = false;

        // ustawianie nastepnego punktu kalibracji
        public void SetNextCalibrationPoint(double x, double y, double z = 0)
        {
            if (!calibrated)
            {
                // tu wypada spr sensownosc punktu
                calibrationPoints[nextPointIndex] = new Point3D(x, y, z);
                nextPointIndex++;
            }
            if (nextPointIndex == 6) // tu jeszcze inne pierdoly trzeba sprawdzic (np. sensownosc punktow)
            {
                calibrated = true;
            }
        }

        public void Reset()
        {
            calibrated = false;
            nextPointIndex = 0;
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

        public Point3D ScaleScreenPositionToKinect(double x, double y)
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
