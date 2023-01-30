using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public abstract class Player : MonoBehaviour {

        [System.NonSerialized] public Contender contender;

        public virtual void OnBegin() { }


        public virtual void DeselectUnit(Movement unit) { }
        public virtual void SelectUnit(Movement unit) { }

        #region Communique

        public virtual void ShowCommunique(string message) { }

        #endregion

        #region Creating

        public virtual void OnCreateStructure(Structure structure) { }
        public virtual void OnCreateUnit(Movement unit) { }
        public virtual void OnDestroyStructure(Structure structure) { }
        public virtual void OnDestroyUnit(Movement unit) { }
        public virtual void OnOccupy(Field field, Contender newOwner) { }

        #endregion

        #region Hero

        public abstract void GetSkirmishHero();

        #endregion

        #region Turn

        public virtual void OnBeginTurn() { }
        public virtual void OnEndTurn() { }
        public virtual void OnResumeTurn() { }

        #endregion

    }

}
