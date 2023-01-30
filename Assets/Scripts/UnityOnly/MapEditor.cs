using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.Map;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Script.UnityOnly {

    public class MapEditor : MonoBehaviour {

        [Header("Map Information")]
        public MapInfo mapInfo;

        [Header("State - Places")]
        public List<Place> goldMines;
        public List<Place> villages;

        [Header("Requirement")]
        public GameObject angleLineObject;
        public GameObject fieldObject;
        public GameObject straightLineObject;
        public Material fieldMaterial;

        [Header("Requirement - Places")]
        public GameObject forest;
        public GameObject goldMine;
        public GameObject village;

        private System.Action<Vector3> modifyField;
        private System.Action<Vector3> modifyFields;

        private Transform fieldsTransform;
        private Transform linesTransform;

        private GameObject placeObject;
        private Campaign.Player player;
        private Race.Information information;
        private byte informationId;
        private bool isBuilding;
        private bool isHero;

        const float movementSpeed = 8 / 60f;

        void Awake() {
            if (!mapInfo)
                mapInfo = FindObjectOfType<MapInfo>();
            mapInfo.GetCampaign().enabled = false;
            mapInfo.transform.localScale = Vector3.one / Engine.fieldSize;

            fieldsTransform = mapInfo.transform.Find("Fields");
            linesTransform = mapInfo.transform.Find("Lines");

            goldMines = mapInfo.goldMines.ToList();
            villages = mapInfo.villages.ToList();

            switch (mapInfo.mirror) {
                case Mirror.None:
                    modifyFields = (pos) => modifyField(pos);
                    break;
                case Mirror.TwoSide:
                    modifyFields = (pos) => {
                        modifyField(pos);
                        modifyField(-pos);
                    };
                    break;
                case Mirror.FourSide:
                    modifyFields = (pos) => {
                        modifyField(pos);
                        modifyField(new Vector3(-pos.x, pos.y, pos.z));
                        modifyField(new Vector3(pos.x, pos.y, -pos.z));
                        modifyField(new Vector3(-pos.x, pos.y, -pos.z));
                    };
                    break;
            }

            Campaign campaign = GetCampaign();
            if (!campaign)
                return;

            informationId = byte.MaxValue;
            foreach (Campaign.Player player in campaign.players) {
                this.player = player;
                foreach (Campaign.Player.ArmyInfo armyInfo in player.buildings) {
                    information = Race.Get(player.kingdom.race).buildings[armyInfo.id];
                    CreateStructure(armyInfo.field);
                }
                foreach (Campaign.Player.ArmyInfo armyInfo in player.heroes) {
                    information = Race.Get(player.kingdom.race).GetHeroes()[armyInfo.id];
                    CreateUnit(armyInfo.field);
                }
                foreach (Campaign.Player.ArmyInfo armyInfo in player.units) {
                    information = Race.Get(player.kingdom.race).units[armyInfo.id];
                    CreateUnit(armyInfo.field);
                }
            }
            player = campaign.players[0];
        }

        public void End() {
            mapInfo.goldMines = goldMines.Where(o => o).ToArray();
            mapInfo.villages = villages.Where(o => o).ToArray();

            mapInfo.transform.localScale = Vector3.one;
            foreach (Field field in mapInfo.GetComponentsInChildren<Field>()) {
                field.army.Clear();
                field.amountOfUnits = 0;

                Vector2Int pos = field.position;
                mapInfo.cameraLimit.minX = Mathf.Min(mapInfo.cameraLimit.minX, pos.x);
                mapInfo.cameraLimit.minY = Mathf.Min(mapInfo.cameraLimit.minY, pos.y);

                mapInfo.cameraLimit.maxX = Mathf.Max(mapInfo.cameraLimit.maxX, pos.x);
                mapInfo.cameraLimit.maxY = Mathf.Max(mapInfo.cameraLimit.maxY, pos.y);
            }
        }

        void Update() {

            if (Input.GetMouseButtonDown(0))
                InvokeLeftMouseButtonAction();
            if (Input.GetMouseButtonDown(1))
                InvokeRightMouseButtonAction();

            if (Input.GetKeyDown(KeyCode.N))
                SetAction(AddStartField);
            if (Input.GetKeyDown(KeyCode.M))
                WipeStartFields();

            if (Input.GetKeyDown(KeyCode.F))
                CreatePlace(forest);
            if (Input.GetKeyDown(KeyCode.G))
                CreatePlace(goldMine);
            if (Input.GetKeyDown(KeyCode.V))
                CreatePlace(village);


            if (Input.GetKeyDown(KeyCode.Alpha0))
                SetAction(ForgotAssignedField);
            if(!Input.GetKey(KeyCode.LeftAlt))
                for (int i = 0; i < 4; i++)
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                        AssignFieldToPlayer(i);

            for (int i = 0; i < 10; i++)
                if(Input.GetKeyDown(KeyCode.Keypad0 + i)) {
                    CreateArmy(i, Input.GetKey(KeyCode.KeypadEnter), Input.GetKey(KeyCode.KeypadPlus));
                    break;
                }

            for (int i = 0; i < 10; i++)
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) && Input.GetKey(KeyCode.LeftAlt)) {
                    CreateArmy(i, Input.GetKey(KeyCode.LeftControl), Input.GetKey(KeyCode.LeftShift));
                    break;
                }

            if (Input.GetKeyDown(KeyCode.KeypadMinus))
                RemoveArmy(Input.GetKey(KeyCode.KeypadEnter));

            Camera.main.transform.position += movementSpeed * Time.deltaTime * Camera.main.fieldOfView * new Vector3(
                Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")
            );
            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView - Input.mouseScrollDelta.y * 2, 40, 120);
        }

        #region Army

        private void AddArmyToPlayer(Field field) {
            if (informationId == byte.MaxValue)
                return;
            Campaign.Player.ArmyInfo armyInfo = new Campaign.Player.ArmyInfo(field, informationId);
            if(isBuilding)
                Main.AddToArray(ref player.buildings, armyInfo);
            else if(isHero)
                Main.AddToArray(ref player.heroes, armyInfo);
            else
                Main.AddToArray(ref player.units, armyInfo);
        }

        private void CreateArmy(int id, bool isBuilding, bool isHero) {
            Race race = Human.stock;
            if (player.kingdom.race == ContenderRace.Undeads)
                race = Undead.stock;

            if (isBuilding) {
                information = race.buildings[id];
            }
            else if (isHero) {
                information = race.GetHeroes()[id];
            }
            else {
                information = race.units[id];
            }
            informationId = (byte)id;

            this.isBuilding = isBuilding;
            this.isHero = isHero;
            SetAction(CreateArmy);
        }

        public void CreateArmy(Vector3 position) {
            Field field = GetField(position);
            if (!field)
                return;

            if (isBuilding) {
                if(!field.place)
                    CreateStructure(field);
            }
            else if(field.IsAnyArmySlotEmpty(isHero))
                CreateUnit(field);

            AssignFieldToPlayer(position);
        }

        private void CreateStructure(Field field) {
            Transform transform = Instantiate(information.gameObject, field.transform).transform;
            transform.localPosition = field.position.ToVector3() / Engine.fieldSize - field.transform.position;

            field.place = transform.GetComponent<Structure>();

            transform.parent = null;
            AddArmyToPlayer(field);
        }

        private void CreateUnit(Field field) {
            Transform transform = Instantiate(information.gameObject, field.transform).transform;

            Movement movement = transform.GetComponent<Movement>();
            movement.transform.position = field.GetAvailablePosition() / Engine.fieldSize;
            field.AddUnit(movement);

            movement.information = movement.data.information = information;
            movement.SetRotation(field.GetRotation());

            transform.parent = null;
            AddArmyToPlayer(field);
        }

        private Campaign.Player.ArmyInfo[] GetArmyInfo() {
            return isBuilding ? player.buildings : isHero ? player.heroes : player.units;
        }

        private void RemoveArmy(bool isBuilding) {
            this.isBuilding = isBuilding;
            SetAction(RemoveArmy);
        }

        public void RemoveArmy(Vector3 position) {
            Field field = GetField(position);
            if (!field)
                return;

            if (isBuilding)
                RemoveStructure(field);
            else
                RemoveUnits(field);
        }

        private Campaign.Player.ArmyInfo[] RemoveFromArmyInfo(Campaign.Player.ArmyInfo[] armyInfo, Field field) {
            List<Campaign.Player.ArmyInfo> list = armyInfo.ToList();
            for (int i = armyInfo.Length - 1; i >= 0; i--) {
                if (armyInfo[i].field == field)
                    list.RemoveAt(i);
            }
            return list.ToArray();
        }

        private void RemoveStructure(Field field) {
            DestroyPlace(field);
            player.buildings = RemoveFromArmyInfo(player.buildings, field);
        }

        private void RemoveUnits(Field field) {
            for (int i = field.amountOfUnits - 1; i >= 0; i--) {
                Movement unit = field.army[i];
                field.RemoveUnit(unit);
                Destroy(unit.gameObject);

                if (unit.information.IsHero())
                    player.heroes = RemoveFromArmyInfo(player.heroes, field);
                else
                    player.units = RemoveFromArmyInfo(player.units, field);
            }
        }

        #endregion

        #region Connection

        void WipeEmptyConnections() {
            foreach (Field field in fieldsTransform.GetComponentsInChildren<Field>()) {
                List<Field.Connection> connections = field.connections.ToList();
                connections.RemoveAll(o => !o.field);
                field.connections = connections.ToArray();
            }
        }

#endregion

#region Control

        void InvokeLeftMouseButtonAction() {
            SetAction(CreateField);
        }

        void InvokeRightMouseButtonAction() {
            SetAction(DestroyField);
        }

#endregion

#region Field

        private void AssignFieldToPlayer(int id) {
            player = GetCampaign().players[id];
            SetAction(AssignFieldToPlayer);
        }

        private void AssignFieldToPlayer(Vector3 position) {
            Field field = GetField(position);
            if (!field)
                return;

            ForgotAssignedField(field);
            Main.AddToArray(ref player.ownedFields, field);
            SetFieldColor(field, player.kingdom.color);
        }

        private void ForgotAssignedField(Vector3 position) {
            Field field = GetField(position);
            if (!field)
                return;

            field.renderer.sharedMaterial = fieldMaterial;
            ForgotAssignedField(field);
        }

        private void ForgotAssignedField(Field field) {
            foreach (Campaign.Player item in GetCampaign().players)
                if (item.ownedFields.Contains(field)) {
                    Main.RemoveFromArray(ref item.ownedFields, field);
                    return;
                }
        }

        private void CreateField(Vector3 position) {
            if (GetField(position))
                return;

            GameObject gameObject = Instantiate(fieldObject, position, Quaternion.identity, fieldsTransform);
            Field thisField = gameObject.GetComponent<Field>();
            thisField.position = (position * Engine.fieldSize).ToVector2Int();

            foreach (Field field in GetFields(position, 3)) {

                if (field == thisField || field.connections.Any(o => o.field == thisField))
                    continue;

                Vector2Int diffrence = thisField.position - field.position;
                Vector2Int abs = diffrence.Abs();

                GameObject prefab; float z;
                if(abs.x + abs.y < 4 * Engine.fieldSize) {
                    prefab = straightLineObject;
                    z = abs.y * 45 / Engine.fieldSize;
                }
                else {
                    prefab = angleLineObject;
                    z = 90 - (diffrence.x + diffrence.y) * 22.5f / Engine.fieldSize;
                }
                Quaternion rotation = Quaternion.Euler(-90, 0, z);

                gameObject = Instantiate(prefab, Main.GetCenter(position, field.position.ToVector3() / Engine.fieldSize).Round(), rotation, linesTransform);
                Renderer renderer = gameObject.GetComponent<Renderer>();

                Main.AddToArray(ref field.connections, new Field.Connection(thisField, renderer));
                Main.AddToArray(ref thisField.connections, new Field.Connection(field, renderer));
            }
        }

        private void DestroyField(Vector3 position) {
            Field field = GetField(position);
            if (!field)
                return;

            foreach (Field.Connection connection in field.connections) {
                for (int i = 0; i < connection.field.connections.Length; i++) {
                    Field.Connection item = connection.field.connections[i];
                    if (item.field == field) {
                        Main.RemoveFromArray(ref connection.field.connections, i);
                        break;
                    }
                }
                Destroy(connection.line.gameObject);
            }

            Destroy(field.gameObject);

            ForgotAssignedField(field);
            RemoveStructure(field);
            RemoveUnits(field);

            WipeEmptyConnections();
        }

        private Field GetField(Vector3 position) {
            Field[] fields = GetFields(position, 1);
            if (fields.Length > 0)
                return fields[0];
            return null;
        }

        private Field[] GetFields(Vector3 position, int range) {
            return Physics.OverlapSphere(position, range, Layer.mapObject).Select(o => o.GetComponent<Field>()).ToArray();
        }

        private void ModifyFields() {
            modifyFields(Main.GetHit(Layer.@default).point.Round());
        }

        private void SetFieldColor(Field field, ContenderColor color) {
            field.renderer.sharedMaterial = Library.general.contendersMaterials[(int)color];
        }

#endregion

#region Place

        private void CreatePlace(Vector3 position) {
            Field field = GetField(position);
            if (!field)
                return;

            DestroyPlace(field);
            if (!placeObject)
                return;

            Transform transform = Instantiate(placeObject, field.transform).transform;
            string name = transform.name = placeObject.name;

            Place place = field.place = transform.GetComponent<Place>();
            field.place.field = field;

            if (name == "Gold Mine")
                goldMines.Add(place);
            else if (name == "Village")
                villages.Add(place);
        }

        private void CreatePlace(GameObject placeObject) {
            this.placeObject = placeObject;
            SetAction(CreatePlace);
        }

        private void DestroyPlace(Field field) {
            if (field.place)
                DestroyImmediate(field.place.gameObject);
        }

#endregion

#region StartField

        private void AddStartField(Vector3 position) {
            Field field = GetField(position);
            if (!field)
                return;
            Main.AddToArray(ref mapInfo.startFields, field);
        }

        private void WipeStartFields() {
            mapInfo.startFields = new Field[0];
        }

#endregion

#region Other

        void SetAction(System.Action<Vector3> action) {
            modifyField = action;
            ModifyFields();
        }

        Campaign GetCampaign() {
            return mapInfo.GetComponent<Campaign>();
        }

#endregion

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MapEditor))]
    public class ObjectBuilderEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            MapEditor myScript = (MapEditor)target;
            if (GUILayout.Button("End")) {
                myScript.End();
            }
        }
    }
#endif

}

