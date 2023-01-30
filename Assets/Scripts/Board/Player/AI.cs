using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Board {

    public class AI : Player {

        Board board;
        bool thinking;

        List<Spell.Spell> blockSpells;
        List<Spell.Spell> controlSpells;
        List<Spell.Spell> healSpells;
        List<Spell.Spell> otherSpells;
        List<Spell.Spell> protectionSpells;

        bool IsDump => Preference.difficulty == Preference.Difficulty.Easy;

        public override void OnBegin() {
            board = Library.Board.controller.board;

            SetSpells();
        }

        public override void OnEndTurn() {
            StartCoroutine(Think());
        }

        public void SetSpells() {
            blockSpells = new List<Spell.Spell>();
            controlSpells = new List<Spell.Spell>();
            healSpells = new List<Spell.Spell>();
            otherSpells = new List<Spell.Spell>();
            protectionSpells = new List<Spell.Spell>();

            foreach (Spell.Spell spell in spells)
                spell.AssignToAI(this);

            otherSpells = new List<Spell.Spell>();
            foreach (Spell.Spell spell in spells)
                if (!healSpells.Contains(spell))
                    otherSpells.Add(spell);
        }

        IEnumerator Think(){
            yield return new WaitWhile(() => Library.Board.controller.board.block);
            yield return new WaitForSeconds(0.4f);

            GetBestMove();
        }

        public void TryThink() {
            if (!Library.Board.controller.board.block)
                OnEndTurn();
        }

        #region Board

        private void GetBestMove() {
            int result = -1;
            int[] gemsPositions = null;

            int amount = board.loots.Count;
            for (int i = 0; i < amount; i++) {
                int sum = 0;
                for (int j = 0; j < (int)Gem.SkullPlus; j++)
                    sum += board.loots[i][j] * spellPriorities[j];
                if (IsDump)
                    sum = (int)(sum * Random.value + 0.5f);

                if(sum > result) {
                    result = sum;
                    gemsPositions = board.switchedGems[i];
                }
            }

            Spell.Spell spell = GetBestSpell(result);
            if (spell != null)
                Library.Board.controller.UseSpell(spell);
            else
                board.SwitchGem(gemsPositions);
        }

        #endregion

        #region Effect

        bool HasEffect(Player player, params EffectName[] effectNames){
            foreach (var effectName in effectNames) {
                if (player.effects.ContainsKey(effectName))
                    return true;
            }
            return false;
        }

        bool HasNotPositiveEffect(Player player, params EffectName[] effectNames) {
            foreach (var effectName in effectNames) {
                if (player.effects.ContainsKey(effectName)){
                    List<Effect> effects = player.effects[effectName];
                    foreach (var effect in effects) {
                        if (!effect.positive)
                            return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region Spell

        public Spell.Spell ChooseSpell(List<Spell.Spell> spells) {
            int amount = spells.Count;
            List<Spell.Spell> possibilities = new List<Spell.Spell>(); 
            for (int i = 0; i < amount; i++) {
                Spell.Spell spell = spells[i];
                if (spellAvailability[this.spells.IndexOf(spell)] && spell.IsAvailable())
                    possibilities.Add(spell);
            }
            amount = possibilities.Count;
            return amount == 0 ? null : possibilities[Random.Range(0, amount)];
        }

        //int a = 0;
        public Spell.Spell GetBestSpell(int gemPriority) {
            //if(a == 0) {
            //    Spell.Spell battle = new Spell.BattleCry();
            //    battle.OnBegin();
            //    a = 1;
            //    return battle;
            //}
            if (!IsDump) {
                if (HasEffect(opponent, EffectName.Charging)) {
                    return ChooseSpell(controlSpells);
                }
                bool autoAttackBuff = HasEffect(opponent, EffectName.AutoAttackBuff);
                if (autoAttackBuff) {
                    Spell.Spell spell = ChooseSpell(blockSpells);
                    if (spell != null) return spell;
                }
                if (autoAttackBuff || HasEffect(you, EffectName.DamagePerTurn) || HasNotPositiveEffect(you, EffectName.Delay)) {
                    Spell.Spell spell = ChooseSpell(protectionSpells);
                    if (spell != null) return spell;
                }
                if (gemPriority > 0.99f * 3 * Controller.priorityMultiplier) {
                    return null;
                }
            }
            if (Library.Board.controller.you.PercentOfHP() < 0.8f) {
                Spell.Spell spell = ChooseSpell(healSpells);
                if (spell != null) return spell;
            }
            if (gemPriority < (0.6f - Random.value * 0.3f) * 3 * Controller.priorityMultiplier)
                return ChooseSpell(otherSpells);
            return null;
        }

        #endregion

        #region Spell - Assigning

        public void ArmorBuff(Spell.Spell spell) { protectionSpells.Add(spell); }
        public void Blind(Spell.Spell spell) { blockSpells.Add(spell); }
        public void BlockOfAutoAttack(Spell.Spell spell) { blockSpells.Add(spell); }
        public void Heal(Spell.Spell spell) { healSpells.Add(spell); }
        public void Shield(Spell.Spell spell) { protectionSpells.Add(spell); }
        public void Silent(Spell.Spell spell) { controlSpells.Add(spell); }
        public void Stun(Spell.Spell spell) { controlSpells.Add(spell); }

        #endregion

        #region Other

        protected Player opponent { get { return Library.Board.controller.opponent; } }
        protected Player you { get { return Library.Board.controller.you; } }

        public override bool IsLocalPlayer() {
            return false;
        }

        #endregion

    }

}


