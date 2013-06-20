using System;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server {

  public class KinectService : WebSocketService
  {
    private static int _num = 0;
    private static int count = 0;

    private string _name;
    public MainEngine engine;

    private string[] markers = new string[] {"top left", "top", "top right", "bottom right", "bottom", "bottom left"};


    public KinectService()
    {
        MainWindow window = MainWindow.instance;
        engine = window.mainEngine;
        engine.service = this;
        engine.clientConnected = true;
        engine.SetAppStateToReady();
    }

    private string getName()
    {
        return QueryString.Contains("name")
               ? QueryString["name"]
               : "";//"anon#" + getNum();
    }

    private int getNum()
    {
      return Interlocked.Increment(ref _num);
    }

    protected override void OnOpen()
    {
      _name = getName();
    }

    public void send(SpeechRecognizer.Orders order)
    {
        JObject myJson = null;

        switch (order)
        {
            case SpeechRecognizer.Orders.CALIBRATE:
                string markerSet = "[";
                
                for (int i = 0; i < markers.Length - 1; i++)
                {
                    markerSet += "\"" + markers[i] + "\", ";
                }

                markerSet += "\"" + markers[markers.Length - 1] + "\"]";

                myJson = JObject.Parse("{ \"type\": \"calibration:start\", \"message\": { \"markers\": " + markerSet + " } }");
                break;
            case SpeechRecognizer.Orders.MARK:
                myJson = JObject.Parse("{ \"type\": \"calibration:next_marker\", \"message\": { \"marker\": \"" + markers[count] + "\" } }");
                break;
            case SpeechRecognizer.Orders.DONE:
                myJson = JObject.Parse("{ \"type\": \"calibration:done\", \"message\": {} }");
                break;
            case SpeechRecognizer.Orders.WORK:
                myJson = JObject.Parse("{ \"type\": \"work:start\", \"message\": {} }");
                break;
            default: break;
        }

        if (myJson != null)
        {
            //System.Threading.Thread.Sleep(5000);
            count++;
            System.Diagnostics.Debug.WriteLine(myJson.ToString());
            Broadcast(myJson.ToString());
        }
    }

    public void sendCreateObject(string name, string type, int top, int left)
    {
        JObject myJson = JObject.Parse("{ \"type\": \"object:create\", \"message\": { \"name\": \"" + name + "\", \"type\": \"" + type + "\", \"top\": " + top + ", \"left\": " + left + " } }");
        Broadcast(myJson.ToString());
    }

    public void sendMoveObject(string name, string top, string left)
    {
        JObject myJson = JObject.Parse("{ \"type\": \"object:set_active\", \"message\": { \"name\": \"" + name + "\", \"top\": " + top + ", \"left\": " + left + " } }");
        Broadcast(myJson.ToString());
    }

    public void sendRemoveObject(string name)
    {
        JObject myJson = JObject.Parse("{ \"type\": \"object:remove\", \"message\": { \"name\": \"" + name + "\" } }");
        Broadcast(myJson.ToString());
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        var msg = "";

        JObject myJson = JObject.Parse(e.Data);
        System.Diagnostics.Debug.WriteLine("onmes: "+myJson.ToString());

        if (myJson["type"].ToString().Contains("configure:"))
        {
            if (myJson["type"].ToString().Equals("configure:object_types"))
            {
                foreach(JObject obj in myJson["message"])
                {
                    //engine.AddTextToLog(obj["name"] + " " + obj["width"] + " " + obj["height"]);  
                    engine.GetObjectManager().AddObjectType(obj["name"].ToString(), int.Parse(obj["width"].ToString()), int.Parse(obj["height"].ToString()));
                }      
            }
            else if (myJson["type"].ToString().Equals("configure:objects"))
            {
                foreach (JObject obj in myJson["message"])
                {
                    //engine.AddTextToLog(obj["name"] + " " + obj["width"] + " " + obj["height"]);
                    string type = obj["type"].ToString();
                    if (engine.GetObjectManager().ExistsObjectType(type))
                    {
                        ObjectType objType = engine.GetObjectManager().GetObjectTypes()[type];
                        engine.GetObjectManager().AddPossibleObject(new SingleObject(type, obj["name"].ToString(), objType.GetWidth(), objType.GetHeight())); 
                    }
                    else
                    {
                        // error: nie ma takiego typu
                    }
                }
            }

            myJson["type"] = myJson["type"].ToString().Replace("configure:", "reconfigured:");
        }
        else if (myJson["type"].ToString().Equals("calibration:listen_to_start"))
        {
            /*send(SpeechRecognizer.Orders.CALIBRATE); //tylko dla mockowania, do usuniecia jak bedzie kinect, on wysle sygnal po rozpoznaniu slowa
            while (count < markers.Length)
            {
                send(SpeechRecognizer.Orders.MARK);  //tylko dla mockowania, do usuniecia jak bedzie kinect, on wysle sygnal po rozpoznaniu slowa
            }

            send(SpeechRecognizer.Orders.DONE);
            return;*/
            engine.GetSpeechRecognizer().CreateAndLoadGrammarWithObjectsNames(engine.GetObjectManager().GetPossibleObjectsNames());
        }
        else if (myJson["type"].ToString().Equals("work:init"))
        {
            send(SpeechRecognizer.Orders.WORK);
            if (engine.SetAppStateToWorking())
            {
                engine.AddTextToLog("Aplikacja w pelni gotowa do dzialania!");
            }
            else
            {
                engine.AddTextToLog("Wystapil blad przy aktywacji aplikacji! Zresetuj program!");
            }
            return;
        }
        else if (myJson["type"].ToString().Equals("object:set_active"))
        {
            engine.GetObjectManager().SelectObject(myJson["name"].ToString());
            return;
        }


        if (_name.IsEmpty())
        {
            msg = myJson.ToString();
        }
        else
        {
            msg = String.Format("{0}: {1}", _name, myJson.ToString());
        }

        if (engine != null)
        {
            engine.AddTextToLog(msg);
        }

        Broadcast(msg);
        System.Diagnostics.Debug.WriteLine("aaa");
    }

    protected override void OnClose(CloseEventArgs e)
    {
        var msg = String.Format("{0} got logged off...", _name);
        Broadcast(msg);
        engine.clientConnected = false;
    }
  }
}
