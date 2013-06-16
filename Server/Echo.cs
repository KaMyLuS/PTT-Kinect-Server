using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Example3 {

  public class Echo : WebSocketService
  {
    protected override void OnMessage(MessageEventArgs e)
    {
      Send("fajni jestesmy");
    }
  }
}
