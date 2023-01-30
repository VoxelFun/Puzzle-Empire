using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class Neutral : Player {

        public override void GetSkirmishHero() {
            throw new System.NotImplementedException();
        }

        #region Turn

        public override void OnBeginTurn() {
            Library.controller.EndTurn();
        }

        #endregion

    }

}


