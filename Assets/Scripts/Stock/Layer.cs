using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Layer {

    public const int all = ~0;
    public const int @default = 1;
    public const int mapObject = 1 << GameObject.mapObject;
    public const int firstLayer = 1 << GameObject.firstLayer;

    public static class GameObject{

        public const int mapObject = 8;
        public const int firstLayer = 9;

    }

}
