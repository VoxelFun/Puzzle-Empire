using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class Campaign_01 : Campaign {

        protected override Mission[] GetMissions() {
            return new Mission[]{
                new DestroyEnemyArmy()
            };
        }

        protected override void OnStartCampaign() {
            SetGold(0, 2000);
        }

    }

}
