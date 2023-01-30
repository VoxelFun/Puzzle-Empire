using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class PlaceInfo : MonoBehaviour {

        [Header("Data")]
        public Place.Defence defence;
        public int income;
        public bool heal;

        [Header("Information")]
        public bool isTarget;

    }

}