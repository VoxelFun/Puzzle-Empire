using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Script.UI {

    public class Tile : UIItem {

        [Header("Requirement")]
        public Image image;

        public Text Text => GetComponentInChildren<Text>();

        public void OnPointerEnter() {
            MyGUI.effect.Enter(this);
        }

        public void OnPointerExit() {
            MyGUI.effect.Exit(this);
        }

        public void SetColor(Color color) {
            image.color = color.image;
            Text.color = color.text;
        }

        [System.Serializable]
        public class Color {
            public UnityEngine.Color image;
            public UnityEngine.Color text;

            public Color() {
            }

            public Color(Tile tile) {
                image = tile.image.color;
                text = tile.Text.color;
            }
        }

    }

}