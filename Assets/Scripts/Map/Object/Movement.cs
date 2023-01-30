using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {
    public class Movement : MonoBehaviour, IAnimationTrigger, IMapObject {

        [Header("State")]
        public bool endedTurn;
        public Field field;
        public Contender contender;

        [Header("Requirement")]
        public new Animation animation;
        public Data data;
        public new Transform transform;

        [System.NonSerialized] public Race.Information information;
        [System.NonSerialized] public Material material;
        [System.NonSerialized] public GameObject selectionObject;

        MoveType type;
        int movementProgress;

        public void Begin(Contender contender) {
            this.contender = contender;
        }

        #region Animation

        public void Play(string animationClip) {
            animation.clip = animation.GetClip(animationClip);
            animation.Play();
        } 

        #endregion

        #region IAnimationTrigger

        public void OnAnimationBegin() {
            
        }

        public void OnAnimationEnd() {
            if (type == MoveType.Field)
                EndMove();
            else
                TryGoToNextField();
        }

        #endregion

        #region IMapObject

        public void Create(Race.Information information, Field field) {
            field.CreateUnit(information, Control.contender);
        }

        public Race.Information GetInformation() {
            throw new System.NotImplementedException();
        }

        public bool IsCreatable(Race.Information information, Field field)
        {
            return field.IsAnyArmySlotEmpty(information.IsHero());
        }

        public bool IsUnit() {
            return true;
        }

        #endregion

        #region Movement

        public void BeginMove() {
            Play("walk");

            data.DisableHealthStatus();
        }

        public void BeginFieldMove(int id) {
            BeginMove();
            
            type = MoveType.Field;
            SetPosition(field.GetAvailablePosition(id));
        }

        public void BeginMapMove(MoveType type) {
            BeginMove();
            
            this.type = type;
            field.RemoveUnit(this);
            movementProgress = 0;
            TryGoToNextField();
        }

        public void EndMove() {
            Play("stand");
            SetRotation(field.GetRotation(field.GetUnitArmyPosition(this)));

            data.EnableHealthStatus();
        }

        void EndMapMove() {
            EndMove();

            contender.EndMove(this);
        }

        public void EndTurn() {
            endedTurn = true;
            data.renderer.material = information.greyMaterial;
        }

        void Move(Field field) {
            this.field = field;

            Vector3 position;
            if (movementProgress == Control.contender.path.Count) {
                position = field.GetAvailablePosition();
                field.AddUnit(this);
            }
            else
                position = field.GetPosition();
            SetPosition(position);
        }

        public void TryGoToNextField() {
            field.TryToOccupy(contender);
            if (movementProgress < Control.contender.path.Count)
                Move(Control.contender.path[movementProgress++]);
            else
                EndMapMove();
        }

        public void ResetMove() {
            endedTurn = false;
            SetDefaultMaterial();
        }

        #endregion

        #region Position

        public void SetPosition(Vector3 position) {
            AnimationController.Start(new AnimationType.MoveTowards(this, transform, position, Info.movementSpeed));
            transform.LookAt(position, Vector3.up);
        }

        public void SetPositionImmediately(Vector3 position) {
            transform.position = position;
        }

        #endregion

        #region Renderer

        public void SetDefaultMaterial() {
            data.renderer.material = information.materials[(int)contender.color];
        }

        #endregion

        #region Rotation

        public void SetRotation(float y) {
            transform.rotation = Quaternion.Euler(0, y, 0);
        }

        #endregion

    }
}
