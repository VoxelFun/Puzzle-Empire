using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class Field : MonoBehaviour {

        [Header("Game State")]
        public int amountOfUnits;
        public List<Movement> army;
        public Contender owner;
        public Place place;

        [Header("State")]
        public Vector2Int position;
        public Connection[] connections;

        [Header("Requirement")]
        public new Renderer renderer;

        Material materialCopy;

        [System.Serializable]
        public class Connection {
            public Field field;
            public Renderer line;

            public Connection(Field field) {
                this.field = field;
            }

            public Connection(Field field, Renderer line) {
                this.field = field;
                this.line = line;
            }
        }

        #region Army

        public void AddUnit(Movement unit) {
            unit.field = this;
            army.Add(unit);
            amountOfUnits++;
        }

        public Movement CreateUnit(Race.Information unit, Contender contender) {
            Transform transform = Instantiate(unit.gameObject).transform;
            transform.name = unit.gameObject.name;

            Movement movement = transform.GetComponent<Movement>();
            movement.SetPositionImmediately(GetAvailablePosition());
            movement.Begin(contender);
            AddUnit(movement);

            movement.information = movement.data.information = unit;

            movement.SetDefaultMaterial();

            contender.AddToArmy(movement);
            movement.SetRotation(GetRotation());
            if (unit.IsHero())
                contender.AddHero(movement, unit);

            return movement;
        }

        public int GetArmyLevel() {
            if (Hero())
                return army[0].data.hero.level;
            return -1;
        }

        public int GetArmyValue() {
            int value = 0;
            foreach (Movement unit in army) {
                value += (int)(unit.data.GetValue() * unit.data.PercentOfHP());
            }
            return (int)(value * (1 + (place ? (int)place.info.defence * 0.1f : 0)));
        }

        public Vector3 GetAvailablePosition() {
            return GetAvailablePosition(amountOfUnits);
        }

        public Vector3 GetAvailablePosition(int id) {
            Vector3 vector3 = Vector3.zero;
            switch (id) {
                case 0:
                    vector3.Set(0, 0, -6);
                    break;
                case 1:
                    vector3.Set(5.196f, 0, 3);
                    break;
                case 2:
                    vector3.Set(-5.196f, 0, 3);
                    break;
            }
            return position.ToVector3() + vector3;
        }

        public float GetRotation() {
            return GetRotation(amountOfUnits - 1);
        }

        public float GetRotation(int id) {
            return new float[] { 165, 150, 195 }[id];
        }

        public int GetUnitArmyPosition(Movement movement) {
            return army.IndexOf(movement);
        }

        public void InformRest() {
            int id = 0;
            foreach (Movement movement in army)
                movement.BeginFieldMove(id++);
        }

        public bool IsEnoughSpace(int amount, bool hero) {
            int limit = Engine.limitUnitPerField;
            if (hero)
                limit = 1;
            return amountOfUnits + amount <= limit && !Hero();
        }

        public bool IsAnyArmySlotEmpty(bool hero) {
            return hero && amountOfUnits == 0 || amountOfUnits < Engine.limitUnitPerField && !Hero();
        }

        public void RemoveUnit(Movement unit) {
            army.Remove(unit);
            amountOfUnits--;
        }

        #endregion

        #region Connection

        public bool IsConnected(Field other) {
            foreach (Connection connection in connections) {
                if (connection.field == other)
                    return true;
            }
            return false;
        }

        #endregion

        #region Material

        public void RestoreMaterial() {
            renderer.sharedMaterial = materialCopy;
        }

        public void SetMaterial(Material material) {
            materialCopy = renderer.sharedMaterial;
            renderer.sharedMaterial = Info.accessibleFieldMaterial;
        }

        #endregion

        #region Occupation

        public void Occupy(Contender contender) {
            Material material = contender.GetColorMaterial();

            if (place)
                place.OnOcuppy(contender, owner);

            foreach (Connection connection in connections)
                if (connection.field.owner == contender)
                    connection.line.sharedMaterial = material;
            renderer.sharedMaterial = material;
            owner = contender;

            if (global::Info.campaign && Library.mapInfo.GetCampaign().mission != null)
                Library.mapInfo.GetCampaign().mission.OnOccupyField(this);
        }

        public void TryToOccupy(Contender contender) {
            if (!owner || owner.teamId != contender.teamId)
                Occupy(contender);
        }

        #endregion

        #region Place

        public Structure CreateStructure(Race.Information place, int buildingTime = 0) {
            Transform transform = Instantiate(place.gameObject).transform;
            transform.position = position.ToVector3();

            Structure structure = transform.GetComponent<Structure>();
            this.place = structure;
            structure.field = this;

            owner.AddStructure(structure);
            structure.OnBegin(place, this, buildingTime);
            structure.SetDefaultMaterial();

            return structure;
        }

        #endregion

        public Place.Defence GetDefence() {
            return place ? place.info.defence : Place.Defence.None;
        }

        public int GetId() {
            return transform.GetSiblingIndex();
        }

        public Vector3 GetPosition() {
            return position.ToVector3();
        }

        public bool Hero() {
            return amountOfUnits == 1 && army[0].information.IsHero();
        }

        public bool IsBlockade(Contender contender) {
            if (owner == null || contender.teamId == owner.teamId)
                return false;
            return amountOfUnits > 0 || place != null && place.IsNotDefenseless();
        }

        public bool IsNotDefenseless() {
            return place != null && place.IsNotDefenseless();
        }

    }

}
