using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class Mouse : MonoBehaviour {

        public Control control;

        void Update() {

            if (Input.GetMouseButtonDown(0))
                control.SelectObject(Input.mousePosition);

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.L))
                Control.contender.DestroyUnit(Control.contender.army[0].data);
            if (Input.GetKeyDown(KeyCode.M))
                Control.contender.army[0].data.hero.SetExp(Engine.requiredExpForLevel);
#endif

            control.SetCameraView(-Input.mouseScrollDelta.y);
            control.MoveCamera(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
        }

    }

}
