using System;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server {

  public class Chat : WebSocketService
  {
    private static int _num = 0;
    private static int ile = 0;

    private string _name;
    public MainEngine main;//{get; set;}

    public Chat()
    {
        //main = new MainEngine(
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

    protected override void OnMessage(MessageEventArgs e)
    {
        var msg = "";

        JObject myJson = JObject.Parse(e.Data);

        //Broadcast("\""+ile+ "\": " +myJson.ToString());//String.Format("[{0}] {1}", myJson["type"], myJson.ToString()));
        //Console.WriteLine("[{0}] {1}", myJson["type"], myJson.ToString());

        if (ile > 4)
        {
            myJson = JObject.Parse("{ \"type\": \"calibration:next_marker\", \"message\": { \"marker\": \"top\" } }");
        }
        else if (myJson["type"].ToString().Contains("configure:"))
        {
            myJson["type"] = myJson["type"].ToString().Replace("configure:", "reconfigured:");
            ile++;
        }
        else if (myJson["type"].ToString().Equals("calibration:listen_to_start"))
        {
            myJson = JObject.Parse("{ \"type\": \"calibration:start\", \"message\": { \"markers\": [\"top left\", \"top\", \"top right\", \"bottom right\", \"bottom\", \"bottom left\"] } }");
            ile++;
        }

        if (_name.IsEmpty())
        {
            msg = myJson.ToString();
        }
        else
        {
            msg = String.Format("{0}: {1}", _name, myJson.ToString());
        }

       // if (main != null)
        //{
            //main.AddTextToLog(msg);
        //}

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
