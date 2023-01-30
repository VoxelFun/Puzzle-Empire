using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class Place : MonoBehaviour, IMapObject {

        [Header("Data")]
        public PlaceInfo info;

        [Header("State")]
        public Field field;

        public virtual void ActiveAction() {
            
        }

        public virtual bool IsMatterInBattle() {
            return info.defence != Defence.None;
        }

        public virtual bool IsNotDefenseless() {
            return false;
        }

        public virtual void OnTurnBegin() {
            field.owner.SetIncome(info.income);
            Heal();
        }

        public virtual void OnOcuppy(Contender newOwner, Contender oldOwner) {
            if (oldOwner) oldOwner.places.Remove(this);
            newOwner.places.Add(this);

            CallOnOcuppy(newOwner);
        }

        #region IMapObject

        public void Create(Race.Information information, Field field) {
            field.CreateStructure(information, Engine.buildingTime);
        }

        public virtual Race.Information GetInformation() {
            throw new System.NotImplementedException();
        }

        public bool IsCreatable(Race.Information information, Field field)
        {
            return !field.place;
        }

        public bool IsUnit() {
            return false;
        }

        #endregion


        #region Other

        protected void CallOnOcuppy(Contender newOwner) {
            foreach (Contender contender in Info.contenders)
                contender.player.OnOccupy(field, newOwner);
        }

        protected void Heal() {
            if(info.heal)
                foreach (Movement movement in field.army)
                    movement.data.SetHealth(movement.data.health + (int)(movement.data.maxHealth * Engine.healValue));
        }

        #endregion

        public enum Defence {
            None, Light = 2, Medium = 4, Heavy = 7
        }

    }

}
