using System.Collections;
using System.Collections.Generic;
using Script.Spell;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Script.Board {

    public class Control : MonoBehaviour {

        [Header("Requirement")]
        public Board board;

        public static bool localPlayer;

        BaseControl control;

        private void Start() {
            RestoreControl();
        }

        private void Update() {
            if (!localPlayer)
                return;

            GameObject gameObject;
            if(Input.GetMouseButtonDown(0) && GetPointedObject(out gameObject))
                StartCoroutine(WaitForRelase(gameObject));
        }

        IEnumerator WaitForRelase(GameObject gameObject) {
            control.OnMouseButtondDown(gameObject);

            while (Input.GetMouseButton(0)) {
                yield return new WaitForEndOfFrame();
                control.OnMouseButton();
            }

            if (GetPointedObject(out gameObject))
                control.OnMouseButtondUp(gameObject);
        }

        #region Other

        public bool GetPointedObject(out GameObject gameObject) {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0) {
                gameObject = raycastResults[0].gameObject;
                return true;
            }
            gameObject = null;
            return false;
        }

        //public bool IsSpellUsed(Script.Spell.Spell spell) {
        //    return control is Spell && (control as Spell).spell == spell;
        //}

        public void RestoreControl() {
            SetControl(new Basic());
        }

        public void SetControl(BaseControl control) {
            this.control = control;
            control.OnBegin(board);
        }

        #endregion

        public abstract class BaseControl {
            protected Board board;

            public void OnBegin(Board board) {
                this.board = board;
            }

            public abstract void OnMouseButtondDown(GameObject gameObject);
            public abstract void OnMouseButton();
            public abstract void OnMouseButtondUp(GameObject gameObject);
        }

        public class Basic : BaseControl {
            bool end, gem, spell;
            string tag;
            float time;

            public override void OnMouseButton() {
                if (!end && spell && time < Time.timeSinceLevelLoad) {
                    Library.Board.controller.ShowSpellInfo(board.activeGem);
                    end = true;
                }
            }

            public override void OnMouseButtondDown(GameObject gameObject) {
                board.activeGem = gameObject.transform;

                tag = gameObject.tag;
                gem = tag == "Gem";
                spell = !gem && tag == "Spell";

                time = Time.timeSinceLevelLoad + 0.5f;
                //Main.Print("Down", time, tag, gem, spell);

                end = false;
            }

            public override void OnMouseButtondUp(GameObject gameObject) {
                //Main.Print("Up", end, gameObject.tag);
                if (end)
                    return;
                if (gameObject.tag == tag)
                    if (gem)
                        board.SelectGem(gameObject.transform);
                    else if (spell)
                        board.controller.TryToUseSpell(board.activeGem);
            }
        }

        public class Spell : BaseControl {

            public override void OnMouseButton() {
                
            }

            public override void OnMouseButtondDown(GameObject gameObject) {
                
            }

            public override void OnMouseButtondUp(GameObject gameObject) {
                if (gameObject.tag == "Gem")
                    board.controller.SelectGemBySpell(gameObject.transform);
                else
                    board.controller.control.RestoreControl();
            }
        }

    }

}
