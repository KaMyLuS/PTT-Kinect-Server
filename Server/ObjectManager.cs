using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    class ObjectManager
    {
        // zbior wszystkich obiektow, ktorych mozna uzyc w programie
        Dictionary<string, SingleObject> possibleObjects = new Dictionary<string,SingleObject>();

        // obiekty faktycznie istniejace na planszy
        Dictionary<string, SingleObject> usedObjects = new Dictionary<string,SingleObject>();

        // m.in. do przeliczania wspolrzednych Kinect<->ekran
        Calibrator calibrator;

        // margines pomiedzy obiektami (wg wsp. ekranowych), potrzebny przy relatywnym pozycjonowaniu
        double screenGutter;

        // margines wg odleglosci Kinecta
        double kinectGutter;

        public ObjectManager(Calibrator calib, double gutter)
        {
            this.calibrator = calib;
            this.screenGutter = gutter;
        }

        public void CreateNewPossibleObject(string type, string name, int width, int height)
        {
            SingleObject obj = new SingleObject(type, name, width, height);
            possibleObjects.Add(name, obj);
        }

        public void AddPossibleObject(SingleObject obj)
        {
            if (obj != null) possibleObjects.Add(obj.GetObjectName(), obj);
        }

        // dodawanie obiektu 'na plansze' wg wspolrzednych Kinecta
        public void AddUsedObject(string name, double kx, double ky, double kz)
        {
            SingleObject obj = possibleObjects[name];
            if (obj != null)
            {
                obj.KinectMoveTo(kx, ky, kz);
                usedObjects.Add(name, obj);
            }
        }

        // usuwanie obiektu z planszy
        public void DeleteUsedObject(string name)
        {
            SingleObject obj = usedObjects[name];
            if (obj != null) usedObjects.Remove(name);
        }

        // przesuwanie danego obiektu wg wspolrzednych Kinecta
        public void MoveTo(string name, double x, double y, double z)
        {
            SingleObject obj = usedObjects[name];
            if (obj != null)
            {
                obj.KinectMoveTo(x, y, z);
                obj.SetScreenPosition(calibrator.ScaleKinectPositionToScreen(x, y, z));
                // tu wypada wyslac komunikat do klienta o zmianie polozenia
            }
        }

        public SingleObject GetUsedObjectByName(string name)
        {
            return usedObjects[name];
        }
    }
}
