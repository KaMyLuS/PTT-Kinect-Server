using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Drawing;

namespace Server
{
    public class SingleObject
    {
        // pozycja srodka obiektu wg wspolrzednych Kinecta
        private Point3D centroidPosition;

        private string objectName;
        private string objectType;

        // pozycja srodka obiektu na ekranie
        private Point screenCentroidPosition = new Point(0, 0);

        // wymiary obiektu
        private int width;
        private int height;

        public SingleObject(string type, string name, int width = 0, int height = 0, double x = 0, double y = 0, double z = 0)
        {
            this.centroidPosition = new Point3D(x, y, z);
            this.objectType = type;
            this.objectName = name;
            this.width = width;
            this.height = height;
        }

        SingleObject(string type, string name, Point3D pos)
        {
            this.centroidPosition = pos;
            this.objectType = type;
            this.objectName = name;
        }

        public void KinectMoveTo(double x, double y, double z = 0)
        {
            this.centroidPosition.X = x;
            this.centroidPosition.Y = y;
            this.centroidPosition.Z = z;
        }

        public void SetScreenPosition(int x, int y)
        {
            screenCentroidPosition.X = x;
            screenCentroidPosition.Y = y;
        }

        public void SetScreenPosition(Point pos)
        {
            screenCentroidPosition = pos;
        }

        public Point3D GetCentroidPosition()
        {
            return centroidPosition;
        }

        public Point GetScreenCentroidPosition()
        {
            return screenCentroidPosition;
        }

        public string GetObjectName()
        {
            return objectName;
        }

        public string GetObjectType()
        {
            return objectType;
        }
    }
}
