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
        public enum Orders
        {
            CALIBRATE,
            MARK,
            DONE,
            WORK,
            CREATE
        }

        private SpeechRecognitionEngine speechEngine;

        private RecognizerInfo recognizerInfo;

        private readonly MainEngine mainEngine;

        public SpeechRecognizer(MainEngine me)
        {
            mainEngine = me;
        }

        public void Start()
        {
            recognizerInfo = GetKinectRecognizer();

            mainEngine.AddTextToLog("SpeechRec: " + "trying to start");

            if (recognizerInfo != null)
            {
                this.speechEngine = new SpeechRecognitionEngine(recognizerInfo.Id);

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

                // ladowanie gramatyk
                speechEngine.LoadGrammar(gCalibration);
                speechEngine.LoadGrammar(gONN);

                speechEngine.SpeechRecognized += speechEngine_SpeechRecognized;
                speechEngine.SpeechRecognitionRejected += speechEngine_SpeechRecognitionRejected;

                speechEngine.SetInputToAudioStream(
                    mainEngine.GetKinectSensor().AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            else
            {
                // cos zle z Kinectem
                mainEngine.AddTextToLog("SpeechRec: " + "recognizerInfo = null");
            }
            mainEngine.AddTextToLog("SpeechRec: " + "started");
        }

        public void LoadGrammar(Grammar g)
        {
            this.speechEngine.LoadGrammar(g);
        }

        void speechEngine_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            // to do
            //mainEngine.AddTextToLog("SpeechRec: " + "not recognized");
        }

        void speechEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            const double ConfidenceThreshold = 0.3;

            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                if (e.Result.Semantics.Value != null)
                {
                    string semValue = e.Result.Semantics.Value.ToString();

                    switch (semValue)
                    {
                        case "CALIB_CALIBRATE":
                            mainEngine.AddTextToLog("SpeechRec: " + semValue);
                            if (mainEngine.GetAppState() == ApplicationState.Ready)
                            {
                                mainEngine.SetAppState(ApplicationState.Calibration);
                                mainEngine.service.send(Orders.CALIBRATE);
                                // no i komunikat do klienta...
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
                                if (mainEngine.GetAppState() == ApplicationState.Calibrated)
                                {
                                    mainEngine.AddTextToLog("skalibrowane");
                                    mainEngine.service.send(Orders.DONE);      
                                }
                                else if (mainEngine.GetAppState() == ApplicationState.Calibration)
                                {
                                    mainEngine.service.send(Orders.MARK);
                                }
                            }
                            else
                            {
                                // cos poszlo zle...
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
                                // no i komunikat do klienta...
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
                                // no i komunikat do klienta...
                            }
                            else
                            {
                                // ...
                            }
                            break;
                    }
                }
                else
                {
                    var semVal = e.Result.Semantics;

                    if (semVal.ContainsKey("OWN_MOVE_NAME"))
                    {
                        mainEngine.AddTextToLog("SpeechRec: move " + semVal["OWN_MOVE_NAME"].Value);

                        if (mainEngine.GetAppState() == ApplicationState.Working)
                        {
                            mainEngine.GetObjectManager().MoveTo(semVal["OWN_MOVE_NAME"].Value.ToString(),
                                mainEngine.GetSkeletonController().GetRightHandCoord());
                            // no i komunikat do klienta...
                        }
                        else
                        {
                            // ...
                        }
                    }
                    else if (semVal.ContainsKey("OWN_NEW_NAME"))
                    {
                        mainEngine.AddTextToLog("SpeechRec: new " + semVal["OWN_NEW_NAME"].Value);

                        if (mainEngine.GetAppState() == ApplicationState.Working)
                        {
                            mainEngine.GetObjectManager().AddUsedObject(semVal["OWN_NEW_NAME"].Value.ToString(),
                                mainEngine.GetSkeletonController().GetRightHandCoord());

                            SingleObject so = mainEngine.GetObjectManager().GetUsedObjectByName(semVal["OWN_NEW_NAME"].Value.ToString());
                            so.SetScreenPosition(mainEngine.GetCalibrator().ScaleKinectPositionToScreen(so.GetCentroidPosition()));
                            mainEngine.service.sendCreateObject(so.GetObjectName(), so.GetObjectType(), so.GetScreenCentroidPosition().Y, so.GetScreenCentroidPosition().X);
                            // no i komunikat do klienta...
                        }
                        else
                        {
                            // ...
                        }
                    }
                    else if (semVal.ContainsKey("OWN_REMOVE_NAME"))
                    {
                        mainEngine.AddTextToLog("SpeechRec: remove " + semVal["OWN_REMOVE_NAME"].Value);

                        if (mainEngine.GetAppState() == ApplicationState.Working)
                        {
                            mainEngine.GetObjectManager().RemoveUsedObject(semVal["OWN_REMOVE_NAME"].Value.ToString());
                            // no i komunikat do klienta...
                        }
                        else
                        {

                        }
                    }   
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
            gbROWN.Append("remove");
            gbROWN.Append(new SemanticResultKey("OWN_REMOVE_NAME", objWithNames));
            Grammar gROWN = new Grammar(gbROWN);

            // ladujemy wszystkie gramatyki
            speechEngine.LoadGrammar(gMOWN);
            speechEngine.LoadGrammar(gCOWN);
            speechEngine.LoadGrammar(gROWN);

            mainEngine.AddTextToLog("SpeechRec: " + "grammars loaded");
        }
    }
}
