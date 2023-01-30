using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class MapInfo : MonoBehaviour {

        [Header("State")]
        public Mirror mirror;
        public Field[] startFields;

        [Header("State - Place")]
        public Place[] goldMines;
        public Place[] villages;

        [Header("State - other")]
        public CameraLimit cameraLimit;

        [Header("Requirement")]
        public new Transform transform;
        public new GameObject gameObject;
        public MapMode mode;

        public Campaign GetCampaign() {
            return (Campaign)mode;
        }

        public Field[] GetFields(){
            return transform.GetChild(0).GetComponentsInChildren<Field>();
        }

        [System.Serializable]
        public class CameraLimit {
            public int maxX = int.MinValue;
            public int maxY = int.MinValue;
            public int minX = int.MaxValue;
            public int minY = int.MaxValue;
        }

    }

    public enum Mirror {
        None, TwoSide, FourSide
    }

}
