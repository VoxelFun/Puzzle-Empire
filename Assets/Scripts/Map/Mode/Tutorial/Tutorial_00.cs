using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class Tutorial_00 : Tutorial {

        protected override Func<bool>[] GetConditions() {
            return new Func<bool>[]{
                () => !Library.guiController.IsVisible(),
                () => Scene.current == SceneName.Board,
            };
        }

    }

}


