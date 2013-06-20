using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Media.Media3D;
using Microsoft.Kinect;

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

        // usrednione max/min wspolrzedne dloni
        double avMaxX, avMaxY, avMinX, avMinY;
        double kinectWidth, kinectHeight;
        double xScale, yScale;

        // wysokosc i szerokosc ekranu
        int screenHeight = 600, screenWidth = 800;

        private readonly MainEngine mainEngine;

        public Calibrator(MainEngine me)
        {
            mainEngine = me;
        }

        private void computeCalibrationCoeffs()
        {
            avMaxX = (calibrationPoints[2].X + calibrationPoints[3].X) / 2;
            avMinX = (calibrationPoints[0].X + calibrationPoints[5].X) / 2;
            avMinY = (calibrationPoints[0].Y + calibrationPoints[1].Y + calibrationPoints[2].Y) / 3;
            avMaxY = (calibrationPoints[3].Y + calibrationPoints[4].Y + calibrationPoints[5].Y) / 3;

            kinectWidth = avMaxX - avMinX;
            kinectHeight = avMaxY - avMinY;

            xScale = screenWidth / kinectWidth ;
            yScale = screenHeight / kinectHeight;
        }

        // ustawianie nastepnego punktu kalibracji
        public void SetNextCalibrationPoint(double x, double y, double z = 0)
        {
            if (!calibrated)
            {
                // tu wypada spr sensownosc punktu
                calibrationPoints[nextPointIndex] = new Point3D(x, y, z);
                nextPointIndex++;

                mainEngine.AddTextToLog("Punkt kalibracji: " + x.ToString() + " " + y.ToString() + " " + z.ToString());
            }
            if (nextPointIndex == 6) // tu jeszcze inne rzeczy trzeba sprawdzic (np. sensownosc punktow)
            {
                if (mainEngine.GetAppState() == ApplicationState.Calibration)
                {
                    computeCalibrationCoeffs();
                    mainEngine.SetAppState(ApplicationState.Calibrated);
                    calibrated = true;
                    mainEngine.AddTextToLog("Kalibracja zakonczona!");
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
            double xLen = kinectPos.X - avMinX;
            double yLen = kinectPos.Y - avMinY;
            double xScreenLen = xLen * xScale;
            double yScreenLen = yLen * yScale;
            Point res = new Point((int)xScreenLen, (int)yScreenLen);
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

        public void SetScreenWidth(int width)
        {
            screenWidth = width;
        }

        public void SetScreenHeight(int height)
        {
            screenHeight = height;
        }
    }
}
