using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArmDuinoBase.Model
{
    public class CommandRecognizer : SpeechRecognitionEngine, INotifyPropertyChanged
    {
        public SpeechSynthesizer Synth;
        private bool voiceControlActivated;
        public bool VoiceControlActivaded
        {
            get
            {
                return voiceControlActivated;
            }
            set
            {
                voiceControlActivated = value;
                NotifyPropertyChanged("VoiceControlActivated");
            }
        }
        private bool initialized;
        public bool Initialized
        {
            get
            {
                return initialized;
            }
            set
            {
                initialized = value;
                NotifyPropertyChanged("Initialized");
            }
        }
        private string language;
        public string Language
        {
            get
            {
                return language;
            }
            set
            {
                language = value;
                NotifyPropertyChanged("Language");
            }
        }
        private bool busy;
        public bool Busy
        {
            get
            {
                return busy;
            }
            set
            {
                busy = value;
                NotifyPropertyChanged("Busy");
            }
        }
        private string whatIHeard;
        public string WhatIHeard
        {
            get
            {
                return whatIHeard;
            }
            set
            {
                whatIHeard = value;
                NotifyPropertyChanged("WhatIHeard");
            }
        }

        private event CommandRecognizedEventHandler commandRecognized;
        private object commandRecognizedEventLock = new object();
        public event CommandRecognizedEventHandler CommandRecognized
        {
            add
            {
                lock (commandRecognizedEventLock)
                {
                    commandRecognized -= value;
                    commandRecognized += value;
                }
            }
            remove
            {
                lock (commandRecognizedEventLock)
                {
                    commandRecognized -= value;
                }
            }
        }
 

        public CommandRecognizer(string Language) :base(new System.Globalization.CultureInfo(Language))
        {
            this.Language = Language;
            this.Synth = new SpeechSynthesizer();
        }

        public void InitializeSpeechRecognition()
        {
            //Activation commands
            GrammarBuilder activateBuilder = new GrammarBuilder("Ok robot, activa el control por voz");
            activateBuilder.Culture = new System.Globalization.CultureInfo(Language);
            Grammar activate = new Grammar(activateBuilder);
            activate.Name = "activate";

            GrammarBuilder deactivateBuilder = new GrammarBuilder("Ok robot, desactiva el control por voz");
            deactivateBuilder.Culture = new System.Globalization.CultureInfo(Language);
            Grammar deactivate = new Grammar(deactivateBuilder);
            deactivate.Name = "deactivate";

            LoadGrammar(activate);
            LoadGrammar(deactivate);

            GrammarBuilder gestureBuilder = new GrammarBuilder("Ok robot, activate gesture control");
            gestureBuilder.Culture = new System.Globalization.CultureInfo(Language);
            Grammar activateGesture = new Grammar(gestureBuilder);
            activate.Name = "activateGesture";

            GrammarBuilder deGestureBuilder = new GrammarBuilder("Ok robot, deactivate gesture control");
            deGestureBuilder.Culture = new System.Globalization.CultureInfo(Language);
            Grammar deactivateGesture = new Grammar(deGestureBuilder);
            deactivate.Name = "deactivateGesture";

            LoadGrammar(activateGesture);
            LoadGrammar(deactivateGesture);
            RequestRecognizerUpdate();
            SpeechRecognized += _recognizer_SpeechRecognized;
            SpeechHypothesized += CommandRecognizer_SpeechHypothesized;
        }

        void CommandRecognizer_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            WhatIHeard = e.Result.Text;
        }



        void _recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            WhatIHeard = "";
            if (e.Result.Text == "Ok robot, activa el control por voz" && e.Result.Confidence >= 0.5)
            {
                Synth.SpeakAsync("Cotrol por voz activado");
                voiceControlActivated = true;
            }
            if (e.Result.Text == "Ok robot, desactiva el control por voz" && e.Result.Confidence >= 0.5)
            {
                Synth.SpeakAsync("Control por voz desactivado");
                voiceControlActivated = false;
            }
            if (e.Result.Text == "Ok robot, activate gesture control" && e.Result.Confidence >= 0.5)
            {
                Synth.SpeakAsync("Gesture control activated");
            }
            if (e.Result.Text == "Ok robot, deactivate gesture control" && e.Result.Confidence >= 0.5)
            {
                Synth.SpeakAsync("Gesture control deactivated");
            }
            if (voiceControlActivated == true && e.Result.Confidence >= 0.5) commandRecognized(this, e.Result.Text);
        }



        public void LoadCommand(SpokenCommand armcommand)
        {
            GrammarBuilder grBuilder = new GrammarBuilder(armcommand.Name);
            grBuilder.Culture =  new System.Globalization.CultureInfo(Language);
            Grammar spokenCommandGr = new Grammar(grBuilder);
            spokenCommandGr.Name = armcommand.Name;
            LoadGrammar(spokenCommandGr);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
