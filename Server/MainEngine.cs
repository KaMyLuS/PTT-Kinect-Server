using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public enum ApplicationState
    {
        KinectNotConnected,
        KinectActive,
        ClientConnected,
        Calibration,
        Working
    }

    class MainEngine
    {
        private ApplicationState appState = ApplicationState.KinectNotConnected;

        private Calibrator calibrator;
        private ObjectManager objectManager;
        private SkeletonController skeletonController;
        private SpeechRecognizer speechRecognizer;
        private WebSocketServer webSocketServer;

        public Calibrator GetCalibrator()
        {
            return calibrator;
        }

        public ObjectManager GetObjectManager()
        {
            return objectManager;
        }

        public SkeletonController GetSkeletonController()
        {
            return skeletonController;
        }

        public SpeechRecognizer GetSpeechRecognizer()
        {
            return speechRecognizer;
        }

        public WebSocketServer GetWebSocketServer()
        {
            return webSocketServer;
        }
    }
}
