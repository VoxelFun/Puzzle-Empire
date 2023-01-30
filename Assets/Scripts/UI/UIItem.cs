using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.UI {

    public abstract class UIItem : MonoBehaviour {

        object lastColor;

        public object LastColor { get => lastColor; set => lastColor = value; }
    }

}
