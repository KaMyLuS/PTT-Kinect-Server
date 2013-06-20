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
    private static int count = -1;

    private string _name;
    public MainEngine engine;

    private string[] markers = new string[] {"top left", "top", "top right", "bottom right", "bottom", "bottom left"};


    public KinectService()
    {
        MainWindow window = MainWindow.instance;
        engine = new MainEngine(null, window.TBLog);
        window.mainEngine = engine;
        engine.service = this;
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
            default: break;
        }

        if (myJson != null)
        {
            System.Threading.Thread.Sleep(5000);
            count++;
            System.Diagnostics.Debug.WriteLine(myJson.ToString());
            Broadcast(myJson.ToString());
        }
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        var msg = "";

        JObject myJson = JObject.Parse(e.Data);
        System.Diagnostics.Debug.WriteLine("onmes: "+myJson.ToString());

        if (myJson["type"].ToString().Contains("configure:"))
        {
            myJson["type"] = myJson["type"].ToString().Replace("configure:", "reconfigured:");
        }
        else if (myJson["type"].ToString().Equals("calibration:listen_to_start"))
        {
            send(SpeechRecognizer.Orders.CALIBRATE); //tylko dla mockowania, do usuniecia jak bedzie kinect, on wysle sygnal po rozpoznaniu slowa
            while (count < markers.Length)
            {
                send(SpeechRecognizer.Orders.MARK);  //tylko dla mockowania, do usuniecia jak bedzie kinect, on wysle sygnal po rozpoznaniu slowa
            }

            send(SpeechRecognizer.Orders.DONE);
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
    }
  }
}
