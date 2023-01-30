using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class TouchController : MonoBehaviour {

        public Control control;

        Vector2 cameraMovement;
        byte okres;
        //float propX;
        float propY;


        private void Awake() {
            //propX = (float)Screen.width / Screen.height;
            propY = (float)Screen.height / Screen.width;
        }

        private void Update() {
            int amount = Input.touchCount;
            if (amount == 1)
                TouchSingle();
            else if (amount == 2)
                TouchMulti();
            else if (okres > 0)
                okres--;

            if (cameraMovement.magnitude > 0.05f) {
                cameraMovement *= 0.98f;
                control.MoveCamera(cameraMovement);
            }
        }

        void TouchSingle() {
            Touch dotyk = Input.GetTouch(0);

            Vector2 punkt = Camera.main.ScreenToViewportPoint(dotyk.deltaPosition);
            punkt = new Vector2(punkt.x, punkt.y * propY);

            //Vector2 punkt = dotyk.deltaPosition;
            if (dotyk.phase == TouchPhase.Moved && (okres == 4 || punkt.magnitude > 0.18f * Time.deltaTime)) {
                okres = 4;
                //float dt = Time.deltaTime / dotyk.deltaTime;
                //Debug.Log(cameraMovement);
                //if (dt == 0 || float.IsNaN(dt) || float.IsInfinity(dt))
                //    dt = 0.1f;
                //Vector2 fixedTouch = punkt * dt;
                const float q = 15f;
                cameraMovement = new Vector2(-punkt.x * q, -punkt.y * q);
                control.MoveCamera(cameraMovement);
            }
            else if (dotyk.phase == TouchPhase.Ended && okres <= 0) {
                control.SelectObject(dotyk.position);
                okres = 4;
                cameraMovement = Vector3.zero;
            }

        }

        void TouchMulti() {
            cameraMovement = Vector3.zero;
            okres = 4;

            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            control.SetCameraView(deltaMagnitudeDiff * 0.01f);
        }

    }

}
