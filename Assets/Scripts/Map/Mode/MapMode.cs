using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class MapMode : MonoBehaviour {

        [Header("Requirement")]
        public MapInfo info;

        public virtual string GetName() {
            return name;
        }

        public virtual void OnBegin() { }

    }
}