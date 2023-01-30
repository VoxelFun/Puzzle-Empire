using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Script.Map {

    public class UI : MonoBehaviour {

        [Header("General")]
        public new Transform transform;

        #region Campaign

        public void ShowMission() {
            Library.guiController.ShowInfo(Library.mapInfo.GetCampaign().GetMissionText());
            Return();
        }

        #endregion

        #region Game

        [Header("Game Layer")]
        public GameObject game;

        public void HideGameLayer() {
            game.SetActive(false);
        }

        public void ShowGameLayer() {
            game.SetActive(true);
        }

        #endregion

        #region GameOver

        [Header("Game Over")]
        public GameObject gameOverObject;

        public void ShowGameOver(string message) {
            HideGameLayer();
            Text text = Instantiate(gameOverObject, transform).GetComponentInChildren<Text>();
            text.text = message;
        }
        
        #endregion

        #region Gold

        [Header("Gold")]
        public Text gold;

        public void UpdateGold(int value) {
            gold.text = value.ToString();
        }

        #endregion

        #region Menu

        [Header("Menu")]
        public GameObject menu;
        public Transform menuButtonParent;
        public Text turn;

        public void DestroyLoadButton() {
            Destroy(menuButtonParent.Find("Load").gameObject);
        }

        public void DestroyMissionButton() {
            Destroy(menuButtonParent.GetChild(0).gameObject);
        }

        public void HideMenu() {
            menu.SetActive(false);
            Library.controller.EndPause();
            Library.ui.CloseLayer();
        }

        public void ShowMenu() {
            menu.SetActive(true);
            BeginNewLayer(HideMenu, true);
            turn.text = "Turn: " + (Library.controller.turn + 1);
        }

        #endregion

        #region PlayerTools

        [Header("Player Tools")]
        public GameObject playerTools;

        public void ShowPlayerTools() {
            playerTools.SetActive(Control.localPlayer);
        }

        #endregion

        #region ReturnLayer

        [Header("Return Layer")]
        public GameObject contendersNavBar;
        public GameObject returningLayer;

        System.Action hide;
        bool returnButton;

        public void BeginNewLayer(System.Action hide, bool returnButton) {
            this.hide = hide;
            this.returnButton = returnButton;
            ChangeLayerStatus(true);
        }

        private void ChangeLayerStatus(bool enable) {
            contendersNavBar.SetActive(!enable);
            returningLayer.SetActive(enable && returnButton);
            Control.SetBlock(enable);
        }

        public void CloseLayer() {
            ChangeLayerStatus(false);

        }

        public void Return() {
            hide();
        }


        #endregion


    }

}


