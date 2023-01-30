using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.UI;

namespace Script.Menu {

    public abstract class Option : Window {

        protected Tile[] tiles;

        protected abstract int CurrentId { get; }

        public override void OnBegin() {
            tiles = GetComponentsInChildren<Tile>();
            Select(CurrentId);
        }

        public void Change(int id) {
            Select(id);
            Deselect(CurrentId);

            OnChange(id);
        }

        protected abstract void OnChange(int id);

        private void Deselect(int id) {
            MyGUI.effect.RestoreColor(tiles[id]);
        }

        private void Select(int id) {
            MyGUI.effect.Select(tiles[id]);
        }

    }

}