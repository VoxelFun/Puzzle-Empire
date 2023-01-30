using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.Map;

namespace Script.Global {

    public class MapPack {

        public Field field;
        public Data[] datas;
        public int level = -1;

        public MapPack(Field field, Data[] datas) {
            this.field = field;
            this.datas = datas;
#if UNITY_EDITOR
            if(field)
#endif
            level = field.GetArmyLevel();
        }
    }

}
