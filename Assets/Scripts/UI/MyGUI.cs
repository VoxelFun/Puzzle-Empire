using UnityEngine;
using UnityEngine.UI;

namespace Script.UI {

    public class MyGUI : MonoBehaviour {

        [Header("Colors")]
        public TileColor tileColor;
        public IconColor iconColor;

        UIItem lastItem;

        public static MyGUI effect;

        private void Awake() {
            effect = this;
            iconColor.Set(tileColor);
        }

        #region Icon

        public void Enter(Icon icon) {
            RecolorLastItem();
            icon.SetColor(iconColor.enter);
            lastItem = icon;
        }

        public void Exit(Icon icon) {
            icon.SetColor((Icon.Color)RestoreColor(icon, iconColor));
        }

        #endregion

        #region Tile

        public void RestoreColor(Tile tile) {
            tile.LastColor = tileColor.Default;
            Exit(tile);
        }

        public void Enter(Tile tile) {
            RecolorLastItem();
            tile.SetColor(tileColor.enter);
            lastItem = tile;
        }

        public void Exit(Tile tile) {
            tile.SetColor((Tile.Color)RestoreColor(tile, tileColor));
        }

        public void Select(Tile tile) {
            tile.LastColor = tileColor.select;
            tile.SetColor(tileColor.select);
        }

        #endregion

        #region UIItem

        private void RecolorLastItem() {
            if (lastItem)
                if (lastItem is Tile)
                    Exit(lastItem as Tile);
                else
                    Exit(lastItem as Icon);
        }

        private object RestoreColor(UIItem item, IUIItemColor itemColor) {
            return item.LastColor == null ? itemColor.Default : item.LastColor;
        }

        #endregion

        public interface IUIItemColor {
            object Default { get; }
            object Select { get; }
        }

        [System.Serializable]
        public class TileColor : IUIItemColor {
            public Tile.Color enter;
            public Tile.Color select;
            public Tile tile;

            public object Default => new Tile.Color(tile);
            public object Select => select;
        }

        [System.Serializable]
        public class IconColor : IUIItemColor {
            public Icon icon;

            [System.NonSerialized] public Icon.Color enter;

            public void Set(TileColor tileColor) {
                enter = new Icon.Color(tileColor.enter.image);
            }

            public object Default => new Icon.Color(icon);
            public object Select => throw new System.NotImplementedException();
        }

    }

    

}