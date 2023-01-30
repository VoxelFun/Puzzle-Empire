using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Script.Map {

    public class Control : MonoBehaviour {

#if UNITY_EDITOR || UNITY_STANDALONE
        [Header("Settings")]
        public bool touch;
#endif

        [Header("State")]
        public Movement followedUnit;

        [Header("Requirement")]
        public Camera[] cameras;
        public Transform cameraTransform;

        public static int amountOfBlocks;
        public static bool block;
        public static Contender contender;
        public static Field lastSelectedField;
        public static bool localPlayer;

        float orthographicSize;
        float startHeight;
        float startOrthographicSize;
        float cameraDiffrence = cameraDiffrenceY;

        const float cameraMovementSpeed = 600;
        const int cameraDiffrenceY = 64;

        public void Begin() {
            Library.control = this;
            orthographicSize = startOrthographicSize = Camera.main.orthographicSize;
            startHeight = cameraTransform.position.y;

#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.LogWarning((touch ? "Touch" : "Mouse") + " control enabled");
            if (!touch)
                gameObject.AddComponent<Mouse>().control = this;
            else
#endif
                gameObject.AddComponent<TouchController>().control = this;

            SetCameraView(0);
        }

		private void LateUpdate() {
            if (followedUnit)
                FollowUnit();
		}

		#region Block

		public static void SetBlock(bool value) {
            amountOfBlocks += value ? 1 : -1;
            block = amountOfBlocks != 0;
        }

        public static void SetLocalPlayer(bool value) {
            localPlayer = value;
        }

        #endregion

        #region Camera

        public void FocusCameraOnField(Field field) {
            Vector2Int position = field.position;
            MoveCamera(new Vector3(position.x, cameraTransform.position.y, position.y - cameraDiffrence));
        }

        public void MoveCamera(Vector2 direction) {
            if (followedUnit)
                return;

            direction *= Time.deltaTime * cameraMovementSpeed * (orthographicSize / startOrthographicSize);

            Vector3 position = cameraTransform.position;
            position = Vector3.Lerp(position, position + new Vector3(direction.x, 0, direction.y), 0.2f);

            MoveCamera(position);
        }

        public void MoveCamera(Vector3 position) {
            MapInfo.CameraLimit limit = Library.mapInfo.cameraLimit;
            cameraTransform.position = new Vector3(
                Mathf.Clamp(position.x, limit.minX, limit.maxX),
                position.y,
                Mathf.Clamp(position.z, limit.minY - cameraDiffrence, limit.maxY - cameraDiffrence)
            );
        }

        public void SetCameraView(float value) {
            orthographicSize = Mathf.Clamp(orthographicSize + value * 10, 30, 170);
            value = orthographicSize / startOrthographicSize;

            //cameraDiffrence = cameraDiffrenceY - cameraDiffrenceY * (1 - value) * 1.5f;
            cameraDiffrence = cameraDiffrenceY * value;
            MoveCamera(new Vector3(cameraTransform.position.x, startHeight * (0.3f + value * 0.7f), cameraTransform.position.z));

            foreach (Camera camera in cameras)
                camera.orthographicSize = orthographicSize;
        }

        #endregion

        #region Camera - Follow

        public void FollowUnit(){
            Vector3 position = followedUnit.transform.position;
            MoveCamera(new Vector3(position.x, cameraTransform.position.y, position.z - cameraDiffrence));
        }

        public void StartFollowingUnit(Movement unit) {
            followedUnit = unit;
        }

        public void StopFollowingUnit() {
            followedUnit = null;
        }

        #endregion

        public void SelectObject(Vector3 screenPoint) {
            if (block || !localPlayer || UI())
                return;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(screenPoint);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Layer.firstLayer)) {
                Movement movement = hit.transform.GetComponent<Movement>();
                if (movement.contender == contender)
                    contender.SelectUnit(movement);
                else if(contender.IsAnyUnitSelected())
                    contender.TryToMoveControl(movement.field);
                return;
            }

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Layer.mapObject)) {
                Field field = hit.transform.GetComponent<Field>();

                if (contender.IsAnyUnitSelected())
                    contender.TryToMoveControl(field);
                else if(field) {
                    lastSelectedField = field;
                    contender.SetLastUsedField(field);
                    if(field.owner == contender)
                        if (!field.place)
                            Library.shopController.Show(contender.race.buildings);
                        else
                            field.place.ActiveAction();
                }

                return;
            }
        }

        #region Other

        public bool UI() {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current) {
                position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        #endregion

    }

}
