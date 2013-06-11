﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace Server
{
    class WebSocketServer
    {
        private static HttpServer _httpsv;

        public static void Init()
        {
            _httpsv = new HttpServer(4649);
            //_httpsv.Sweeped = false;
            //_httpsv.AddWebSocketService<Echo>("/Echo");
            _httpsv.AddWebSocketService<Chat>("/Chat");

            _httpsv.OnGet += (sender, e) =>
            {
                onGet(e);
            };

            _httpsv.OnError += (sender, e) =>
            {
                Console.WriteLine(e.Message);
            };

            _httpsv.Start();
            Console.WriteLine("HTTP Server listening on port: {0} service path:", _httpsv.Port);
            foreach (var path in _httpsv.ServicePaths)
                Console.WriteLine("  {0}", path);
            Console.WriteLine();

            Console.WriteLine("Press any key to stop server...");
            Console.ReadLine();

            _httpsv.Stop();
        }

        private static byte[] getContent(string path)
        {
            if (path == "/")
                path += "index.html";

            return _httpsv.GetFile(path);
        }

        private static void onGet(HttpRequestEventArgs eventArgs)
        {
            var request = eventArgs.Request;
            var response = eventArgs.Response;
            var content = getContent(request.RawUrl);
            if (content != null)
            {
                response.WriteContent(content);
                return;
            }

            response.StatusCode = (int)HttpStatusCode.NotFound;
        } 
    }
}
