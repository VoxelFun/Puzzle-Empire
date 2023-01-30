using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class Campaign_06 : Campaign {

        protected override Mission[] GetMissions() {
            return new Mission[]{
                new DefeatEnemy()
            };
        }

        protected override void OnStartCampaign() {
            SetGold(0, 150);
            SetGold(1, 100);
        }

    }

}
