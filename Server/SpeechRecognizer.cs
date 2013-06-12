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

        private RecognizerInfo recognizerInfo;

        private readonly MainEngine mainEngine;

        public SpeechRecognizer(MainEngine me)
        {
            mainEngine = me;
        }

        public void Start(Grammar g = null)
        {
            recognizerInfo = GetKinectRecognizer();

            if (recognizerInfo != null)
            {
                this.speechEngine = new SpeechRecognitionEngine(recognizerInfo.Id);

                if (g != null) speechEngine.LoadGrammar(g);

                speechEngine.SpeechRecognized += speechEngine_SpeechRecognized;
                speechEngine.SpeechRecognitionRejected += speechEngine_SpeechRecognitionRejected;

                speechEngine.SetInputToAudioStream(
                    mainEngine.GetKinectSensor().AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
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
            // to do
        }

        void speechEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            const double ConfidenceThreshold = 0.3;

            if (e.Result.Confidence >= ConfidenceThreshold && e.Result.Semantics.Value != null)
            {
                string semValue = e.Result.Semantics.Value.ToString();

                switch (semValue)
                {
                    case  "CALIB_CALIBRATE":
                        mainEngine.AddTextToLog("SpeechRec: " + semValue);

                        if (mainEngine.GetAppState() == ApplicationState.Ready)
                        {
                            mainEngine.SetAppState(ApplicationState.Calibration);
                        }
                        else
                        {
                            // jakies inne przypadki? np. ponowna kalibracja czy cos...
                        }
                        break;

                    case "CALIB_MARK":
                        mainEngine.AddTextToLog("SpeechRec: " + semValue);

                        if (mainEngine.GetAppState() == ApplicationState.Calibration)
                        {
                            mainEngine.GetCalibrator().SetNextCalibrationPoint(
                                mainEngine.GetSkeletonController().GetRightHandCoord());
                        }
                        else
                        {
                            // albo juz po kalibracji albo cos poszlo zle...
                        }
                        break;

                    case "ONN_MOVE":
                        mainEngine.AddTextToLog("SpeechRec: " + semValue);
                        break;

                    case "ONN_THERE":
                        mainEngine.AddTextToLog("SpeechRec: " + semValue);

                        if (mainEngine.GetAppState() == ApplicationState.Working)
                        {
                            mainEngine.GetObjectManager().SelectedMoveTo(mainEngine.GetSkeletonController().GetRightHandCoord());
                        }
                        else
                        {
                            // ...
                        }
                        break;

                    case "ONN_REMOVE":
                        mainEngine.AddTextToLog("SpeechRec: " + semValue);

                        if (mainEngine.GetAppState() == ApplicationState.Working)
                        {
                            mainEngine.GetObjectManager().RemoveSelectedObject();
                        }
                        else
                        {
                            // ...
                        }
                        break;

                    default:
                        var semVal = e.Result.Semantics;

                        if (semVal["OWN_MOVE_NAME"] != null)
                        {
                            mainEngine.AddTextToLog("SpeechRec: " + semVal["OWN_MOVE_NAME"]);

                            if (mainEngine.GetAppState() == ApplicationState.Working)
                            {
                                mainEngine.GetObjectManager().MoveTo(semVal["OWN_MOVE_NAME"].ToString(),
                                    mainEngine.GetSkeletonController().GetRightHandCoord());
                            }
                            else
                            {
                                // ...
                            }
                        }
                        else if (semVal["OWN_NEW_NAME"] != null)
                        {
                            mainEngine.AddTextToLog("SpeechRec: " + semVal["OWN_NEW_NAME"]);

                            if (mainEngine.GetAppState() == ApplicationState.Working)
                            {
                                mainEngine.GetObjectManager().AddUsedObject(semVal["OWN_NEW_NAME"].ToString(),
                                    mainEngine.GetSkeletonController().GetRightHandCoord());
                            }
                            else
                            {
                                // ...
                            }
                        }
                        else if (semVal["OWN_REMOVE_NAME"] != null)
                        {
                            mainEngine.AddTextToLog("SpeechRec: " + semVal["OWN_NEW_NAME"]);

                            mainEngine.GetObjectManager().RemoveUsedObject(semVal["OWN_NEW_NAME"].ToString());
                        }
                        break;
                }
            }
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
        public static Grammar CreateGrammarFromXML(string ruleName = null)
        {
            Grammar grammar;

            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.SpeechGrammar)))
            {
                if (ruleName != null) grammar = new Grammar(memoryStream, ruleName);
                else grammar = new Grammar(memoryStream);
            }

            return grammar;
        }

        public void CreateAndLoadGrammarWithObjectsNames(string[] objNames)
        {
            // komendy do kalibracji
            Choices calibration = new Choices();
            calibration.Add(new SemanticResultValue("calibrate", "CALIB_CALIBRATE"));
            calibration.Add(new SemanticResultValue("mark", "CALIB_MARK"));
              
            GrammarBuilder gbCalibration = new GrammarBuilder { Culture = recognizerInfo.Culture };
            gbCalibration.Append(calibration); 
            Grammar gCalibration = new Grammar(gbCalibration);

            // do obslugi bez wykorzystywania nazw obiektow
            Choices objNoNames = new Choices();
            objNoNames.Add(new SemanticResultValue("move", "ONN_MOVE"));
            objNoNames.Add(new SemanticResultValue("there", "ONN_THERE"));
            objNoNames.Add(new SemanticResultValue("remove", "ONN_REMOVE"));

            GrammarBuilder gbONN = new GrammarBuilder { Culture = recognizerInfo.Culture };
            gbONN.Append(objNoNames);
            Grammar gONN = new Grammar(gbONN);

            // przesuwanie obiektow z wykorzystaniem nazw
            Choices objWithNames = new Choices(objNames);
            GrammarBuilder gbMOWN = new GrammarBuilder { Culture = recognizerInfo.Culture };
            gbMOWN.Append("move");
            gbMOWN.Append(new SemanticResultKey("OWN_MOVE_NAME", objWithNames));
            Grammar gMOWN = new Grammar(gbMOWN);

            // tworzenie obiektow z wykorzystaniem nazw, bez podawania kierunku
            GrammarBuilder gbCOWN = new GrammarBuilder { Culture = recognizerInfo.Culture };
            gbCOWN.Append("new");
            gbCOWN.Append(new SemanticResultKey("OWN_NEW_NAME", objWithNames));
            Grammar gCOWN = new Grammar(gbCOWN);

            // usuwanie obiektow z wykorzystaniem nazw
            GrammarBuilder gbROWN = new GrammarBuilder { Culture = recognizerInfo.Culture };
            gbROWN.Append("new");
            gbROWN.Append(new SemanticResultKey("OWN_REMOVE_NAME", objWithNames));
            Grammar gROWN = new Grammar(gbROWN);

            // ladujemy wszystkie gramatyki
            speechEngine.LoadGrammar(gCalibration);
            speechEngine.LoadGrammar(gONN);
            speechEngine.LoadGrammar(gMOWN);
            speechEngine.LoadGrammar(gCOWN);
            speechEngine.LoadGrammar(gROWN);
        }
    }
}
