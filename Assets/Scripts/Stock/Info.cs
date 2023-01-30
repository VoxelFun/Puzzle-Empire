using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.Global;

public static class Info {

    public static MapPack aggressor;
    public static MapPack defender;

    public static bool campaign = true;
    public static bool load = false;
    public static GameObject map;
    public static int mapId;
    public static Player[] players = {
        new Player(PlayerType.Local, ContenderRace.Humans, 0, ContenderColor.Red, 0),
        new Player(PlayerType.Local, ContenderRace.Undeads, 1, ContenderColor.Blue, 1),
        };

}
