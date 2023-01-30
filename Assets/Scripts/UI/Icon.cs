using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Script.UI {

    public class Icon : UIItem {

        [Header("Requirement")]
        public Image image;

        public void OnPointerEnter() {
            MyGUI.effect.Enter(this);
        }

        public void OnPointerExit() {
            MyGUI.effect.Exit(this);
        }

        public void SetColor(Color color) {
            image.color = color.image;
        }

        [System.Serializable]
        public class Color {
            public UnityEngine.Color image;

            public Color(Icon icon) => image = icon.image.color;
            public Color(UnityEngine.Color color) => image = color;
        }

    }

}