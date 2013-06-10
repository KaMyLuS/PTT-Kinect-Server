using System;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Example3 {

  public class Chat : WebSocketService
  {
    private static int _num = 0;

    private string _name;

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

        Console.WriteLine("[{0}] {1}", myJson["type"], myJson.ToString());

        if (false)
        {
            myJson = JObject.Parse("{ \"type\": \"calibration:next_marker\", \"message\": { \"marker\": \"top\" } }");
        }
        else if (myJson["type"].ToString().Contains("configure:"))
        {
            myJson["type"] = myJson["type"].ToString().Replace("configure:", "reconfigured:");
        }
        else if (myJson["type"].ToString().Equals("calibration:listen_to_start"))
        {
            myJson = JObject.Parse("{ \"type\": \"calibration:start\", \"message\": { \"markers\": [\"top left\", \"top\", \"top right\", \"bottom right\", \"bottom\", \"bottom left\"] } }");
        }

        if (_name.IsEmpty())
        {
            msg = myJson.ToString();
        }
        else
        {
            msg = String.Format("{0}: {1}", _name, myJson.ToString());
        }

        Console.WriteLine(msg);
        Broadcast(msg);
    }

    protected override void OnClose(CloseEventArgs e)
    {
      var msg = String.Format("{0} got logged off...", _name);
      Broadcast(msg);
    }
  }
}
