using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.UI;

namespace Script.Menu {

    public class Difficulty : Option {

        protected override int CurrentId => (int)Preference.difficulty;

        protected override void OnChange(int id) {
            Preference.SaveDifficulty((Preference.Difficulty)id);
        }

    }

}