using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.Map;

public class General : MonoBehaviour {

    public Campaign[] campaignMaps;
    public Material[] contendersMaterials;
    public Skirmish[] skirmishMaps;

    private void Awake() {
        Library.general = this;
    }

    public bool IsNextMissionExists() {
        return Info.mapId < campaignMaps.Length;
    }

}
