using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Server
{
    class SingleObject
    {
        // pozycja srodka obiektu wg wspolrzednych Kinecta
        private Point centroidPosition;

        private string objectName;
        private string objectType;

        // pozycja srodka obiektu wg wspolrzednych ekranu docelowego
        Point ScreenCentroidPosition
        {
            get
            {
                // TO DO !!
                return new Point(0, 0);
            }
        }


        SingleObject(string type, string name, double x = 0, double y = 0)
        {
            this.centroidPosition = new Point(x, y);
            this.objectType = type;
            this.objectName = name;
        }

        SingleObject(string type, string name, Point pos)
        {
            this.centroidPosition = pos;
            this.objectType = type;
            this.objectName = name;
        }

        public void MoveTo(double x, double y)
        {
            this.centroidPosition.X = x;
            this.centroidPosition.Y = y;
        }

        public void MoveTo(Point newPos)
        {
            this.centroidPosition = newPos;
        }

        public Point GetCentroidPosition()
        {
            return centroidPosition;
        }
    }
}
