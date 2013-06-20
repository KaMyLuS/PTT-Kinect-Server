using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Media.Media3D;

namespace Server
{
    public class SkeletonController
    {
        private Skeleton[] skeletons;

        private Joint rightHand;
        private Point3D rightHandCoord = new Point3D(0, 0, 0);

        private readonly MainEngine mainEngine;

        public SkeletonController(MainEngine me)
        {
            mainEngine = me;
        }

        public void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            // nie ma potrzeby odczytywac szkieletu gdy nie jest on potrzebny aplikacji
            if (mainEngine.GetAppState() == ApplicationState.Calibration 
                    || mainEngine.GetAppState() == ApplicationState.Working)
            {
                using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (skeletonFrame != null)
                    {
                        lock (this)
                        {
                            skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                            skeletonFrame.CopySkeletonDataTo(skeletons);

                            if (skeletons.All(s => s.TrackingState == SkeletonTrackingState.NotTracked))
                                return;

                            Skeleton firstTrackedSkeleton = skeletons.Where(s => s.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();
                            if (firstTrackedSkeleton != null && firstTrackedSkeleton.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked)
                            {
                                rightHand = firstTrackedSkeleton.Joints[JointType.HandRight];
                                rightHandCoord = new Point3D(rightHand.Position.X, rightHand.Position.Y, rightHand.Position.Z);

                                if (mainEngine.GetAppState() == ApplicationState.Working)
                                    MainEngine.MoveCursorTo(mainEngine.GetCalibrator().ScaleKinectPositionToScreen(
                                        rightHand.Position.X, rightHand.Position.Y, rightHand.Position.Z));

                                //mainEngine.AddTextToLog("SkelControl: hand tracked " + rightHand.Position.X.ToString()
                                   // + " " + rightHand.Position.Y.ToString() + " " + rightHand.Position.Z.ToString());
                            }
                        }
                    }
                }  
            }
        }

        public Joint GetRightHand()
        {
            return rightHand;
        }

        public Point3D GetRightHandCoord()
        {
            return rightHandCoord;
        }
    }
}
