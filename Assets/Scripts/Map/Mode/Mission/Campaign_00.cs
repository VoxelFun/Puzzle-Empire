using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class Campaign_00 : Campaign {

        protected override void TriggerEvent(bool load) {
            Data data = Info.contenders[1].army[0].data;
            data.damage = 0;
            data.health = 40;
        }

        protected override Mission[] GetMissions() {
            return new Mission[]{
                new DestroyEnemyArmy()
            };
        }

    }

}
