using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Script.Map {

    public abstract class Tutorial : MonoBehaviour {

        [Header("Requirement")]
        public TextAsset text;

        public static new GameObject gameObject;

        protected StringReader reader;

        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        WaitWhile waitWhile = new WaitWhile(() => Library.guiController.IsVisible());

        public void Start() {
            gameObject = transform.gameObject;

            StartCoroutine(Wait());
            DontDestroyOnLoad(gameObject);
        }

        private IEnumerator Wait() {
            //yield return wait;

            System.Func<bool>[] funcs = GetConditions();
            int amount = funcs.Length;

            reader = new StringReader(text.text);
            for (int i = 0; i < amount; i++) {
                System.Func<bool> func = funcs[i];

                while (!func())
                    yield return wait;
                yield return wait;

                Show();
                yield return waitWhile;
            }
            Destroy(gameObject);
        }

        protected abstract System.Func<bool>[] GetConditions();

        #region GUIController

        private void Hide() {
            Control.SetBlock(false);
            Library.ui.ShowGameLayer();
            Library.guiController.RestoreLayout();
            Library.guiController.HideImmediately();
        }

        private void Move() {
            Move(true);
        }

        private bool Move(bool update) {
            string line = reader.ReadLine();
            if (line == null || line.Length == 0) {
                Hide();
                return false;
            }
            if(update)
                Library.guiController.ShowInfo(line, Move);
            return true;
        }

        private void Show() {
            Library.guiController.EditLayout(true, TextAnchor.UpperLeft, Skip);
            Library.ui.HideGameLayer();
            Control.SetBlock(true);
            Move(true);
        }

        private void Skip() {
            while (Move(false)) ;
        }

        #endregion

    }

}