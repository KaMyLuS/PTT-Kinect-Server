using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // manager Kinectow (moze byc wiele urzadzen)
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();

        // format pobierania obrazu z kamery Kinecta
        private const ColorImageFormat ImageFormat = ColorImageFormat.RgbResolution640x480Fps30;

        // bitmapa przechowujaca klatke z kamery, mozliwa do wyswietlenia w programie
        private WriteableBitmap colorCameraBitmap;

        // klatka w postaci pobranej z kamery
        private byte[] colorCameraPixels;

        // glowny silnik 
        public MainEngine mainEngine;
        public static MainWindow instance;

        public MainWindow()
        {
            InitializeComponent();
            instance = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SensorChooserUI.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.KinectChanged += new EventHandler<KinectChangedEventArgs>(sensorChooser_KinectChanged);
            this.sensorChooser.Start();
            WebSocketServer webSocketServer = new WebSocketServer();
            webSocketServer.Init();
        }

        void sensorChooser_KinectChanged(object sender, KinectChangedEventArgs e)
        {
            try
            {
                this.UninitializeSensor(e.OldSensor);
                this.InitializeSensor(e.NewSensor);
            }
            catch (InvalidOperationException)
            {
                // cos zle z sensorem
            }   
        }

        private void ImKinectVideo_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }

        private void InitializeSensor(KinectSensor sensor)
        {
            if (null == sensor)
            {
                return;
            }

            // przygotowanie pol, wlasciwosci, itp. pod streaming z kamery
            sensor.ColorStream.Enable(ImageFormat);
            this.colorCameraPixels = new byte[sensor.ColorStream.FramePixelDataLength];
            this.colorCameraBitmap = new WriteableBitmap(sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight, 96.0, 96.0,
                PixelFormats.Bgr32, null);
            ImKinectVideo.Source = this.colorCameraBitmap;
            sensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(sensor_ColorFrameReady);

            mainEngine = new MainEngine(sensor, TBLog);
            mainEngine.Start();
        }

        void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            throw new NotImplementedException();
        }

        // wywolywane, gdy odebrano klatke z kamery
        void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    colorFrame.CopyPixelDataTo(colorCameraPixels);
                    this.colorCameraBitmap.WritePixels(new Int32Rect(0, 0, this.colorCameraBitmap.PixelWidth, this.colorCameraBitmap.PixelHeight),
                        colorCameraPixels, this.colorCameraBitmap.PixelWidth * sizeof(int), 0);
                }
            }
        }

        private void UninitializeSensor(KinectSensor sensor)
        {
            if (null == sensor)
            {
                return;
            }


        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            TBLog.Text = "";
        }
    }
}
