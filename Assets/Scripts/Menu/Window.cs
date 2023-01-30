using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Menu {
    public class Window : MonoBehaviour {

        [Header("Window")]
        public new GameObject gameObject;

        protected Controller controller;

        public void Begin(Controller controller) {
            this.controller = controller;
            OnBegin();
        }

        public virtual void OnBegin() {
        }

        public virtual void OnEnd() {
        }

        public void CreateWindow(GameObject gameObject) {
            controller.CreateWindow(gameObject);
        }

        public void Return() {
            controller.Return();
        }

    }
}