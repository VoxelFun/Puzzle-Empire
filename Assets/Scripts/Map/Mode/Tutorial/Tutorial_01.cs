using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class Tutorial_01 : Tutorial {

        protected override System.Func<bool>[] GetConditions() {
            return new System.Func<bool>[]{
                () => !Library.guiController.IsVisible(),
            };
        }

    }

}