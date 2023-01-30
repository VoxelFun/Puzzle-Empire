using UnityEngine;

namespace Script.Board {

    public class Effect {

        public byte amount;
        public EffectName name;
        public int duration;
        public GameObject gameObject;
        public Spell.Spell spell;
        public int stacks;
        public bool positive;

        public Spell.Spell.Effect effect;

        private readonly bool charging;
        private readonly bool hasEffect;

        public Effect(EffectName name, Spell.Spell spell, bool positive) {
            this.name = name;
            duration = spell.duration;
			this.positive = positive;
            this.spell = spell;
            stacks = spell.stacks;
            amount = spell.cumulating;

            charging = name == EffectName.Charging || name == EffectName.Delay;
        }

        public Effect(EffectName name, Spell.Spell spell, bool positive, Spell.Spell.Effect effect) : this(name, spell, positive) {
            hasEffect = true;
            this.effect = effect;
        }

        public void Copy(Effect newEffect) {
            duration = newEffect.duration;
            if(spell.IsCumulating()) {
                amount += spell.cumulating;
                if (hasEffect)
                    effect.OnCumulating();
            }
        }

        public void OnRemove(bool cancel) {
            Object.Destroy(gameObject);
            if (cancel)
                return;
            spell.OnRemove(this);
            if (charging && !spell.chargingAction)
                spell.OnCharge();
        }

        public bool RemoveStack() {
            if (hasEffect)
                effect.OnStackRemoved();
            if (stacks == 0 || --stacks > 0)
                return false;
            OnRemove(false);
            return true;
        }

        public void SetGameObject(GameObject gameObject) {
            this.gameObject = gameObject;
        }

        public bool TryToRemove() {
            if (duration == 0 || --duration > 0)
                return false;
            OnRemove(false);
            return true;
        }

        public void TryToUse() {
            if (!hasEffect || !effect.usedPerTurn) {
                if (charging && spell.chargingAction)
                    spell.OnCharge();
                return;
            }

            Spell.Spell.onlyYou = !charging;
            effect.Use();
            Spell.Spell.onlyYou = false;
        }

    }

}
