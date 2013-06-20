using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media.Media3D;

namespace Server
{
    public class ObjectManager
    {
        // zbior wszystkich obiektow, ktorych mozna uzyc w programie
        Dictionary<string, SingleObject> possibleObjects = new Dictionary<string,SingleObject>();

        // obiekty faktycznie istniejace na planszy
        Dictionary<string, SingleObject> usedObjects = new Dictionary<string,SingleObject>();

        // margines pomiedzy obiektami (wg wsp. ekranowych), potrzebny przy relatywnym pozycjonowaniu
        double screenGutter;

        // margines wg odleglosci Kinecta
        double kinectGutter;

        // aktualnie zaznaczony obiekt
        SingleObject selectedObject;

        MainEngine mainEngine;

        public ObjectManager(MainEngine engine)
        {
            this.mainEngine = engine;
        }

        public void SetScreenGutter(double g)
        {
            this.screenGutter = g;
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
            if (possibleObjects.ContainsKey(name))
            {
                SingleObject obj = possibleObjects[name];
                if (!ExistsUsedObject(name))
                {
                    obj.KinectMoveTo(kx, ky, kz);
                    usedObjects.Add(name, obj);
                    mainEngine.AddTextToLog("ObjectManager: dodano na plansze obiekt o nazwie " + name);
                }
                else
                {
                    mainEngine.AddTextToLog("ObjectManager: na planszy juz istnieje obiekt o nazwie " + name);
                }
            }
            else
            {
                mainEngine.AddTextToLog("ObjectManager: nie obsluguje obiektu o nazwie " + name
                    + " na pozycje (" + kx
                    + "," + ky + "," + kz + ")");
            }
        }

        public void AddUsedObject(string name, Point3D point)
        {
            AddUsedObject(name, point.X, point.Y, point.Z);
        }

        // usuwanie obiektu z planszy
        public void RemoveUsedObject(string name)
        {
            if (ExistsUsedObject(name))
            {
                SingleObject obj = usedObjects[name];
                usedObjects.Remove(name);
                if (selectedObject == obj) selectedObject = null;

                mainEngine.AddTextToLog("ObjectManager: usunieto z planszy obiekt " + name);
            }
            else
            {
                mainEngine.AddTextToLog("ObjectManager: nie istnieje na planszy obiekt do usuniecia " + name);
                // ...
            }
        }

        public void RemoveSelectedObject()
        {
            if (selectedObject != null)
                usedObjects.Remove(selectedObject.GetObjectName());
        }

        // przesuwanie danego obiektu wg wspolrzednych Kinecta
        public void MoveTo(string name, double x, double y, double z)
        {
            if (ExistsUsedObject(name))
            {
                SingleObject obj = usedObjects[name];
                obj.KinectMoveTo(x, y, z);
                obj.SetScreenPosition(mainEngine.GetCalibrator().ScaleKinectPositionToScreen(x, y, z));
                // tu wypada wyslac komunikat do klienta o zmianie polozenia

                mainEngine.AddTextToLog("ObjectManager: przesunieto obiekt " + name + " na pozycje (" + x 
                    + "," + y + "," + z + ")");
            }
            else
            {
                mainEngine.AddTextToLog("ObjectManager: nie ma obiektu do przesuniecia o nazwie " + name);
                // ...
            }
        }

        public void MoveTo(string name, Point3D point)
        {
            MoveTo(name, point.X, point.Y, point.Z);
        }

        // przesuwanie aktualnie zaznaczonego obiektu
        public void SelectedMoveTo(Point3D point)
        {
            if (selectedObject != null)
            {
                selectedObject.KinectMoveTo(point);
                selectedObject.SetScreenPosition(mainEngine.GetCalibrator().ScaleKinectPositionToScreen(point));
            }
            else
            {
                // ...
            }
        }

        public SingleObject GetUsedObjectByName(string name)
        {
            return usedObjects[name];
        }

        public string[] GetPossibleObjectsNames()
        {
            return possibleObjects.Keys.ToArray<string>();
        }

        public void SelectObject(string name)
        {
            selectedObject = usedObjects[name];
        }

        public void DeselectObject()
        {
            selectedObject = null;
        }

        public SingleObject GetSelectedObject()
        {
            return selectedObject;
        }

        bool ExistsUsedObject(string name)
        {
            if (usedObjects.ContainsKey(name)) return true;
            return false;
        }
    }
}
