using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Server
{
    public enum ApplicationState
    {
        NotReady,
        Ready,
        Calibration,
        Calibrated,
        Working
    }

    public class MainEngine
    {
        private ApplicationState appState = ApplicationState.NotReady;

        private readonly Calibrator calibrator;
        private readonly ObjectManager objectManager;
        private readonly SkeletonController skeletonController;
        private readonly SpeechRecognizer speechRecognizer;
        public KinectService service { get;  set; } 

        private readonly KinectSensor kinectSensor;

        public bool clientConnected = false;

        public bool IsKinectConnected
        {
            get
            {
                if (this.kinectSensor != null && this.kinectSensor.Status == KinectStatus.Connected)
                    return true;
                return false;
            }
        }

        public bool IsClientConnected
        {
            get
            {
                return clientConnected;
            }

            set
            {
                clientConnected = value;

                if (this.IsKinectConnected && value) this.appState = ApplicationState.Ready;
                else this.appState = ApplicationState.NotReady;
            }
        }

        // TextBox do wypisywania logow aplikacji
        private TextBox tbLog;
        
        // 'filtr' waznosci logow 
        private const int LogLevel = 0;

        public MainEngine(KinectSensor sensor, TextBox log)
        {
            this.tbLog = log;
            this.kinectSensor = sensor;

            calibrator = new Calibrator(this);
            objectManager = new ObjectManager(this);
            skeletonController = new SkeletonController(this);
            speechRecognizer = new SpeechRecognizer(this);
        }

        /* dopisuje text do logu, dodajac znak nowej linii na koncu
         * logLvl - poziom waznosci danego wpisu, 0 - najmniej wazny */
        public void AddTextToLog(string text, int logLvl = 0)
        {
            if(this.tbLog != null && logLvl >= MainEngine.LogLevel)
                tbLog.Dispatcher.Invoke(DispatcherPriority.Normal,
                    new Action(() => { tbLog.Text += text + '\n'; }));
        }

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

        public KinectSensor GetKinectSensor()
        {
            return kinectSensor;
        }

        public ApplicationState GetAppState()
        {
            lock (this)
            {
                return appState;
            }
        }

        public void Start()
        {
            if (this.IsKinectConnected)
            {
                // przygotowanie streamingu danych o szkielecie
                kinectSensor.SkeletonStream.Enable();
                kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(skeletonController.sensor_SkeletonFrameReady);

                // odpalamy czujnik Kinecta
                kinectSensor.Start();

                // tu trzeba odpalic webServerSocket

                speechRecognizer.Start();

                /* Ogolnie idea jest taka:
                 * 1. WebSocketServer (WSS) sobie dziala i czeka na podlaczenie klienta
                 * 2. Po podlaczeniu ustawia w mainEngine, ze klient polaczony
                 * 3. WSS sobie slucha (w osobnym watku) komunikatow i na ich podstawie wykonuje dzialania, np.
                 *      gdy przyjda info o dostepnych obiektach to je dodaje przez ObjectManagera oraz do gramatyki SpeechRecognizera,
                 *      a takze zmienia stan aplikacji w MainEngine
                 * 4. SpeechRecognizer rowniez sobie slucha komunikatow i wywoluje odpowiednie polecenia, np. przesuwanie obiektow,
                 *      zmiana stanu w MainEngine, itp.
                 *      */
            }
            else
            {
                // mozna tu rzucic jakis wyjatek, ze Kinect nie podlaczony czy cos
            }
        }

        public static void MoveCursorTo(int x, int y)
        {
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x, y);
        }

        public static void MoveCursorTo(Point p)
        {
            System.Windows.Forms.Cursor.Position = p;
        }

        public void SetAppState(ApplicationState appS)
        {
            this.appState = appS;
        }

        public void SetAppStateToWorking()
        {
            if (this.appState == ApplicationState.Calibrated)
                this.appState = ApplicationState.Working;
            else
            {
                // ...
            }
        }

        public void SetAppStateToCalibrated()
        {
            if (this.appState == ApplicationState.Calibration)
                this.appState = ApplicationState.Calibrated;
            else
            {
                // ...
            }
        }

        public void SetAppStateToCalibration()
        {
            if (this.appState == ApplicationState.Ready)
                this.appState = ApplicationState.Calibration;
            else
            {
                // ...
            }
        }

        public void SetAppStateToReady()
        {
            if (this.IsClientConnected && this.IsKinectConnected)
                this.appState = ApplicationState.Ready;
            else
            {
                // ...
            }
        }
    }
}
