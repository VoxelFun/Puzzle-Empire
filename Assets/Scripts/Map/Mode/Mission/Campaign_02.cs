using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class Campaign_02 : Campaign {

        protected override Mission[] GetMissions() {
            return new Mission[]{
                new Survive(5)
            };
        }

        protected override void OnStartCampaign() {
            SetGold(1, 0, 450);
        }

    }

}
