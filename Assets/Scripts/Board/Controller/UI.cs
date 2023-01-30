using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Script.Board {

    public class UI : MonoBehaviour {

        [Header("Color")]
        public Color[] diamentsColors = new Color[Controller.diamondsAmount];
        public Color effectColor;
        public Color healthColor;
        public Color spellAvailable;
        public Color spellUnavailable;

        [Header("Requirement")]
        public GameObject effectObject;
        public Sprite[] effectSprites;
        public PlayerStatus[] playerStatuses;
        public Text leftMoves;
        public Status status;
        public GameObject statusChangeObject;
        public Transform turnAffiliation;

        #region Effect

        public void CreateEffect(Player player, Effect effect) {
            GameObject gameObject = Instantiate(effectObject, player.status.effectParent);
            gameObject.SetActive(true);

            effect.SetGameObject(gameObject);
            EffectInfo effectInfo = gameObject.GetComponent<EffectInfo>();
            effectInfo.effect = effect;
            effectInfo.image.sprite = effectSprites[(int)effect.name];
        }

        #endregion

        #region PlayerStatus

        public void UpdateDiamondAmount(Player player, int id) {
            player.status.amountOfDiamonds[id].text = player.GetDiamond(id).ToString();
        }

        public void UpdateDiamondAmount(Player player, int id, float value) {
            UpdateDiamondAmount(player, id);
            CreateStatusChange(player, diamentsColors[id], value);
        }

        public void UpdateDiamondsAmount(Player player) {
            for (int i = 0; i < Controller.diamondsAmount; i++)
                UpdateDiamondAmount(player, i);
        }

        public void UpdateHealth(Player player) {
            player.status.health.text = Mathf.CeilToInt(player.health) + "/" + player.maxHealth;
        }

        public void UpdateHealth(Player player, float value) {
            UpdateHealth(player);
            CreateStatusChange(player, healthColor, value);
        }

        #endregion

        #region Spells

        public void CreateSpell(Player player, Spell.Spell spell) {
            int amount = player.status.spells.Count;

            GameObject spellObject = Instantiate(player.status.spellObject, player.status.spellParent);
            Transform transform = spellObject.transform;
            spellObject.SetActive(true);

            spellObject.GetComponentInChildren<Text>().text = spell.GetName();
            player.status.spells.Add(transform.GetComponent<Image>());

            transform = transform.Find("Other");
            Transform parent = player.status.diamonds.transform;

            foreach (Gem gem in spell.cost.Keys) {
                Transform child = parent.GetChild((int)gem);
                GameObject gameObject = Instantiate(child.gameObject, transform);

                Transform diamont = gameObject.transform;
                diamont.position = new Vector3(child.position.x, 0);
                diamont.localPosition = new Vector3(diamont.localPosition.x, 0);

                gameObject.GetComponentInChildren<Text>().text = spell.cost[gem].ToString();
            }
        }

        public void EnableSpells(Player player, bool enable) {
            foreach (Image image in player.status.spells) {
                image.raycastTarget = enable;
            }
        }

        public void DestroySpell(Player player) {
            Destroy(player.status.spells[0].gameObject);
            player.status.spells.RemoveAt(0);
        }

        public void DestroySpellObject(Player player) {
            Destroy(player.status.spellObject);
        }

        public void UpdateSpellColor(Player player, int id, bool availability) {
            player.status.spells[id].color = availability ? spellAvailable : spellUnavailable;
        }

        #endregion

        #region Status

        public void CreateCommunique(string message) {
            StartCoroutine(MoveStatusChange(status, effectColor, message));
        }

        public void CreateStatusChange(Player player, EffectName name) {
            StartCoroutine(MoveStatusChange(player.status, effectColor, Main.AddSpacesToSentence(name.ToString())));
        }

        private void CreateStatusChange(Player player, Color color, float value) {
            string message = (Mathf.Round(value * 10) * 0.1f).ToString("0.0");
            if (value >= 0) message = "+" + message;
            StartCoroutine(MoveStatusChange(player.status, color, message));
        }

        private IEnumerator MoveStatusChange(Status status, Color color, string message) {
            const float duration = 2;
            Vector3 vector2 = new Vector3(0, 300);

            float delay = status.nextStatusChange - Time.timeSinceLevelLoad;
            if (delay < 0) delay = 0;
            status.nextStatusChange = Time.timeSinceLevelLoad + delay + 0.4f;
            yield return new WaitForSeconds(delay);

            GameObject gameObject = Instantiate(statusChangeObject, status.statusChangeParent);
            Transform transform = gameObject.transform;

            Text text = gameObject.GetComponent<Text>();
            text.text = message;

            float time = duration;
            while(time > 0) {
                color.a = time;

                text.color = color;
                transform.localPosition = Vector3.Lerp(Vector3.zero, vector2, 1 - time / duration);
                yield return new WaitForEndOfFrame();
                time -= Time.deltaTime;
            }

            Destroy(gameObject);
        }

        #endregion

        #region Turn

        public void UpdateLeftMoves(int value) {
            leftMoves.text = "Moves Left: " + value;
        }

        #endregion

        #region TurnAffiliation

        public void UpdateRotationOfTurnAffiliation() {
            turnAffiliation.Rotate(Vector3.forward, -45 * Time.deltaTime);
        }

        public void UpdatePositionOfTurnAffiliation(Player player) {
            turnAffiliation.localPosition = new Vector3(477 * player.sideMultiplier, turnAffiliation.localPosition.y);
        }

        #endregion

        #region Other

        public void UpdateDiamentParent(Player player) {
            player.status.diamonds.color = player.spellAvailability.Contains(true) ? spellAvailable : spellUnavailable;
        }

        #endregion

        [System.Serializable]
        public class Status {
            public Transform statusChangeParent;

            [System.NonSerialized] public float nextStatusChange;
        }

        [System.Serializable]
        public class PlayerStatus : Status {
            public Text[] amountOfDiamonds;
            public Image diamonds;
            public Transform effectParent;
            public Text health;
            public GameObject spellObject;
            public Transform spellParent;

            [System.NonSerialized] public List<Image> spells = new List<Image>();
        }

    }

    

}
