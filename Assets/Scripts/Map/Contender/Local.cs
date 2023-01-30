using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class Local : Player {

        public override void DeselectUnit(Movement unit) {
            Destroy(unit.selectionObject);
        }

        public override void SelectUnit(Movement unit) {
            MapController.ArmySelectionSpriteNew(unit, Info.selectionObject);
        }

        #region Communique

        public override void ShowCommunique(string message) {
            Library.guiController.ShowInfo(message);
        }

        #endregion

        #region Hero

        public override void GetSkirmishHero() {
            Library.shopController.ShowHeroes(contender.race.GetHeroes());
        }

        #endregion

        #region Turn

        

        #endregion

    }

}
