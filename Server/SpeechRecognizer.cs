using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Server
{

    public class SpeechRecognizer
    {
        private SpeechRecognitionEngine speechEngine;

        public SpeechRecognizer(KinectSensor sensor, Grammar g = null)
        {
            RecognizerInfo ri = GetKinectRecognizer();

            if (ri != null)
            {
                this.speechEngine = new SpeechRecognitionEngine(ri.Id);

                if (g != null) speechEngine.LoadGrammar(g);

                speechEngine.SpeechRecognized += speechEngine_SpeechRecognized;
                speechEngine.SpeechRecognitionRejected += speechEngine_SpeechRecognitionRejected;

                speechEngine.SetInputToAudioStream(
                    sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            else
            {
                // cos zle z Kinectem
            }
        }

        public void LoadGrammar(Grammar g)
        {
            this.speechEngine.LoadGrammar(g);
        }

        void speechEngine_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void speechEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // to do
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        // tworzy gramatyke na podstawie zadanej reguly z pliku xml
        public static Grammar CreateGrammar(string ruleName)
        {
            Grammar grammar;

            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.SpeechGrammar)))
            {
                grammar = new Grammar(memoryStream, ruleName);
            }

            return grammar;
        }
    }
}
