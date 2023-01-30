using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class Fortification : Structure {

        [Header("Fortification")]
        public Repair repair;
        public Data data;

        public override void ActiveAction() {
            if(repair != Repair.Manual)
                base.ActiveAction();
        }

        public override bool IsNotDefenseless() {
            return ready;
        }

        public override void OnTurnBegin() {
            base.OnTurnBegin();

            if (ready) {
                field.owner.SetIncome(info.income);
                data.SetHealth(data.health + (int)(data.maxHealth * Engine.buildingHealValue));
                Heal();
            }
        }

        public override void OnOcuppy(Contender newOwner, Contender oldOwner) {
            base.OnOcuppy(newOwner, oldOwner);
            if(IsMainBuilding())
                Library.controller.RemoveContender(oldOwner);
        }

        public override Renderer GetRenderer() {
            return data.renderer;
        }

        public override void SetInformation(Race.Information information) {
            this.information = data.information = information;
        }

        public enum Repair {
            None, Manual, Auto
        }
    }

}
