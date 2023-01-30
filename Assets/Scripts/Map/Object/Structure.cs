using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public class Structure : Place {

        [Header("Data - Structure")]
        public bool factory;

        [Header("State - Structure")]
        public int leftBuildingTime;
        public GameObject scaffolding;

        protected Race.Information information;
        protected bool ready = true;

        public override void ActiveAction() {
            if (factory)
                Library.shopController.Show(Control.contender.race.units);
        }

        public override bool IsMatterInBattle() {
            return base.IsMatterInBattle() && ready;
        }

        public override void OnTurnBegin() {
            if (ready || --leftBuildingTime > 0)
                return;
            Destroy(scaffolding);
            ready = true;
        }

        public override void OnOcuppy(Contender newOwner, Contender oldOwner) {
            CallOnOcuppy(newOwner);

            Destroy(gameObject);
            field.place = null;
            oldOwner.RemoveStructure(this);
        }

		#region IMapObject

		public override Race.Information GetInformation() {
			return information;
		}

		#endregion

		#region Structure

		public virtual Renderer GetRenderer() {
            return GetComponentInChildren<Renderer>();
        }

        public void OnBegin(Race.Information information, Field field, int buildingTime) {
            this.field = field;
            SetInformation(information);

            if (IsMainBuilding() || buildingTime == 0)
                return;
            leftBuildingTime = buildingTime;
            scaffolding = Main.CreateAndAssign(field.owner.race.scaffolding, transform);
            ready = false;
        }

        public virtual void SetInformation(Race.Information information) {
            this.information = information;
        }

        #endregion

        public bool IsMainBuilding() {
            return information.IsEmpty();
        }

        public void SetDefaultMaterial() {
            Material[] materials = IsMainBuilding() ? field.owner.race.mainBuilding.materials : information.materials;
            GetRenderer().material = materials[(int)field.owner.color];
        }



    }

}
