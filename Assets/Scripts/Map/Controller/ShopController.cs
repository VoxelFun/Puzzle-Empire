using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Script.Map {

    public class ShopController : MonoBehaviour {

        [Header("State")]

        [Header("Requirement - Screen Update")]
        public Text armorText;
        public Text damageText;
        public Text descriptionText;
        public Text healthText;
        public Text priceText;
        public Text rangeText;
        public Image preview;
        public GameObject statisticsGroup;

        [Header("Requirement")]
        public new GameObject gameObject;
        public GameObject iconsParent;
        public new Transform transform;
        public GameObject unitIcon;

        Transform[] iconsParents;
        Dictionary<Transform, Race.Information> unitsInformations = new Dictionary<Transform, Race.Information>();
        Dictionary<Race.Information[], Transform> categories = new Dictionary<Race.Information[], Transform>();

        bool buyingHero;
        Transform category;
        int temporaryIconId;
        Race.Information unitInformation;
        Field usedField;

        public void Begin(params Race[] races) {
            int amount = races.Length * 2;
            iconsParents = new Transform[amount];
            //iconsParents[0] = iconsParent.transform;
            for (int i = 0; i < amount; i++)
                iconsParents[i] = Main.CanvasChildNew(transform, iconsParent);

            for (int i = 0; i < races.Length; i++) {
                Race race = races[i];
                for (int j = 0; j < 2; j++) {

                    Race.Information[] informations = j == 0 ? race.units : race.buildings;
                    Transform parent = iconsParents[i * 2 + j];
                    categories.Add(informations, parent);

                    CreateIcons(informations, parent);
                }
            }

            unitIcon.SetActive(false);
            //Destroy(unitIcon);
            //UpdateScreen(races[0].units[0]);
        }

        private void CreateIcons(Race.Information[] informations, Transform parent) {
            temporaryIconId = parent.childCount;
            for (int k = 0; k < informations.Length; k++) {
                GameObject icon = Instantiate(unitIcon, parent);
                icon.GetComponent<Image>().sprite = informations[k].icon;
                icon.name = k.ToString();
                unitsInformations.Add(icon.transform, informations[k]);
                icon.SetActive(true);
            }
        }

        #region Screen

        public void UpdateScreen(Transform icon) {
            UpdateScreen(unitsInformations[icon]);
        }

        void UpdateScreen(Race.Information information) {
            Data data = information.GetData();

            if (data) {
                armorText.text = data.armor.ToString();
                damageText.text = data.damage.ToString();
                healthText.text = data.maxHealth.ToString();
                rangeText.text = data.range.ToString();
            }
            priceText.text = information.cost.ToString();
            statisticsGroup.SetActive(data);
            

            descriptionText.text = information.description;
            preview.sprite = information.sprite;
            unitInformation = information;
        }

        public void Show() {
            Library.ui.BeginNewLayer(Hide, !buyingHero);

            gameObject.SetActive(true);
            category.gameObject.SetActive(true);

            UpdateScreen(category.GetChild(0));
        }

        public void Show(Race.Information[] informations) {
            category = categories[informations];
            usedField = Control.lastSelectedField;

            CreateIcons(Control.contender.GetDeadHeroes().ToArray(), category);

            Show();
        }

        public void ShowHeroes(Race.Information[] informations) {
            usedField = Control.contender.GetMainBuilding().field;
            CreateIcons(informations, iconsParent.transform);
            category = iconsParent.transform;
            buyingHero = true;
            Show();
        }

        public void Hide() {
            gameObject.SetActive(false);
            category.gameObject.SetActive(false);

            int childCount = category.childCount;
            for (int i = temporaryIconId; i < childCount; i++) {
                Transform child = category.GetChild(i);
                unitsInformations.Remove(child);
                Destroy(child.gameObject);
            }
            buyingHero = false;

            Library.ui.CloseLayer();
        }

        #endregion

        public void Buy() {
            IMapObject mapObject = unitInformation.GetMapObject();
            if (!mapObject.IsCreatable(unitInformation, usedField)) {
                Control.contender.player.ShowCommunique("Not enough space");
                return;
            }
            if (!Control.contender.Pay(unitInformation.cost))
                return;
            mapObject.Create(unitInformation, usedField);
            if(!mapObject.IsCreatable(unitInformation, usedField))
                Hide();
        }

    }

}
