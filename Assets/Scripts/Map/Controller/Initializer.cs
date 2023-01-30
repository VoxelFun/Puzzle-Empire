using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class Initializer : MonoBehaviour {

        public Control control;
        public Controller controller;
        public UI ui;

        public ShopController shopController;

        [Header("Things saved to static")]
        public Material accessibleFieldMaterial;
        public GameObject contenderObject;
        public GameObject selectionObject;
        public GameObject healthStatus;
        public Transform canvasWorld;

#if UNITY_EDITOR
        [Header("UNITY EDITOR")]
        public GameObject map;
#endif

        private void Start() {
            Library.ui = ui;
            Library.controller = controller;
            Library.shopController = shopController;

            Info.selectionObject = selectionObject;
            Info.healthStatus = healthStatus;
            Info.canvasWorld = canvasWorld;
            Info.accessibleFieldMaterial = accessibleFieldMaterial;

            Info.startIncome = Human.stock.mainBuilding.gameObject.GetComponent<Structure>().info.income;

#if UNITY_EDITOR
            if(Scene.current == SceneName.Editor)
                global::Info.map = map ? map : global::Info.campaign ? Library.general.campaignMaps[0].gameObject : Library.general.skirmishMaps[0].gameObject;
#endif
            if (global::Info.load) {
                Progress.LoadGlobalVariables();
                int id = global::Info.mapId;
                global::Info.map = global::Info.campaign ? Library.general.campaignMaps[id].gameObject : Library.general.skirmishMaps[id].gameObject;
            }

            MapMode mapMode = Instantiate(global::Info.map).GetComponent<MapMode>();
            Library.mapInfo = mapMode.info;
            if(!global::Info.load)
                mapMode.OnBegin();
            control.Begin();

            if (!global::Info.campaign)
                Library.ui.DestroyMissionButton();
            if (!Progress.IsAnyMapSaved())
                ui.DestroyLoadButton();

            Info.contendersAmount = global::Info.players.Length;
            Info.contenders = new Contender[Info.contendersAmount];
            for (int i = 0; i < Info.contendersAmount; i++) {
                GameObject gameObject = Instantiate(contenderObject);
                gameObject.name = "Contender_" + i;

                Contender contender = gameObject.GetComponent<Contender>();
                Global.Player player = global::Info.players[i];

                contender.info = player;
                contender.color = player.color;
                contender.raceName = player.race;
                contender.teamId = player.team;

                switch (contender.raceName) {
                    case ContenderRace.Humans:
                        contender.race = Human.stock;
                        break;
                    case ContenderRace.Undeads:
                        contender.race = Undead.stock;
                        break;
                }

                switch (player.type) {
                    case PlayerType.Local:
                        gameObject.AddComponent<Local>();
                        break;
                    case PlayerType.Computer:
                        gameObject.AddComponent<Computer>();
                        break;
                    case PlayerType.Neutral:
                        gameObject.AddComponent<Neutral>();
                        break;
                }

                contender.Begin();
                Info.contenders[i] = contender;
            }

            for (int i = 0; i < Info.contendersAmount; i++) {
                Field field = Library.mapInfo.startFields[i];
                Contender contender = Info.contenders[i];
                contender.SetLastUsedField(field);

                if(!global::Info.campaign || (mapMode as Campaign).players[i].mainBuilding) {
                    field.Occupy(contender);
                    field.CreateStructure(contender.race.mainBuilding);
                }
                if (!global::Info.load && global::Info.campaign){
                    //if (i == 0)
                    //    field.CreateUnit(contender.race.units[0], contender);
                    //if (i == 0)
                    //    field.CreateUnit(contender.race.units[0], contender);
                    //field.CreateUnit(contender.race.units[0], contender);
                    Campaign.Player player = (mapMode as Campaign).players[i];

                    foreach (Field item in player.ownedFields)
                        item.TryToOccupy(contender);

                    foreach (Campaign.Player.ArmyInfo armyInfo in player.buildings)
                        armyInfo.field.CreateStructure(contender.race.buildings[armyInfo.id]);
                    foreach (Campaign.Player.ArmyInfo armyInfo in player.heroes)
                        armyInfo.field.CreateUnit(contender.race.GetHeroes()[armyInfo.id], contender);
                    foreach (Campaign.Player.ArmyInfo armyInfo in player.units)
                        armyInfo.field.CreateUnit(contender.race.units[armyInfo.id], contender);
                }
            }

            if (global::Info.load)
                Progress.Load();

            //foreach (Transform item in mapInfo.transform.GetChild(0)) {
            //    item.GetComponent<Field>().Occupy(Info.contenders[0]);
            //}
            //foreach (Movement movement in Info.contenders[0].army) {
            //    movement.data.SetHealth(20);
            //}

            Control.contender = Info.contenders[Info.contenderId];
            shopController.Begin(Human.stock, Undead.stock);
            controller.BeginPlayerTurn(false);

            global::Info.load = false;
            Main.SetCanvasScaling();
        }

    }

}
