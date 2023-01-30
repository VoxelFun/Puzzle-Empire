using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Script.Map {

    public class Story {

        public StringReader reader;

        Transform speaker;
        List<string> speakersNames = new List<string>();
        List<Transform> speakers = new List<Transform>();
        List<Movement> speakersMovement = new List<Movement>();

        public Story(TextAsset textAsset) {
            reader = new StringReader(textAsset.text);
        }

        #region Speaker

        void AddSpeaker(string name) {
            Movement movement = FindSpeaker(name);

            speakersNames.Add(name);
            speakers.Add(movement.transform.GetChild(0));
            speakersMovement.Add(movement);
        }

        Movement FindSpeaker(string name) {
            foreach (var contender in Info.contenders) {
                foreach (var unit in contender.army) {
                    if (unit.name == name)
                        return unit;
                }
            }
            return null;
        }

        void StartSpeaking(string text, int id) {
            Movement movement = speakersMovement[id];

            Library.control.FocusCameraOnField(movement.field);
            speakers[id].localScale = Vector3.one * 2;

            speaker = speakers[id];
            Library.guiController.ShowInfo(movement.information.Name+"\n\n"+text, Move);
        }

        void StopSpeaking() {
            if (speaker)
                speaker.localScale = Vector3.one;
        }

        #endregion

        #region Story

        public void Begin() {
            Library.guiController.EditLayout(false, TextAnchor.UpperLeft, Skip);
            Library.ui.HideGameLayer();
            Control.SetBlock(true);
            Move();
        }

        void End() {
            StopSpeaking();

            speaker = null;
            speakers.Clear(); speakersNames.Clear();
            speakersMovement.Clear();
            Control.contender.FocusCamera();

            Control.SetBlock(false);
            Library.ui.ShowGameLayer();
            Library.guiController.RestoreLayout();
            Library.guiController.HideImmediately();
        }

        public void Move() {
            Move(true);
        }

        bool Move(bool update) {
            string line = reader.ReadLine();
            if (line == null || line.Length == 0) {
                End();
                return false;
            }
            if (update)
                Update(line);
            return true;
        }

        private void Skip() {
            while (Move(false));
        }

        void Update(string line) {
            string[] info = line.Split(';');

            string name = info[0];
            int id = speakersNames.IndexOf(name);
            if (id < 0) {
                id = speakersNames.Count;
                AddSpeaker(name);
            }

            StopSpeaking();
            StartSpeaking(info[1], id);
        }

        #endregion

    }

}


