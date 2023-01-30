using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Script.Spell {

    public abstract class Spell {

        public Dictionary<Gem, int> cost;
        public bool endTurn = true;

        protected Board.Controller controller;

        public int duration;
        public int stacks;

        public bool board;
        public bool onlyOne;
        public byte cumulating;
        public bool chargingAction;

        public static bool onlyYou;
        public EffectName effectName;
        public Board.Player.Character character;

        public abstract void Use();

        public virtual void AssignToAI(Board.AI ai) { }
        public virtual bool IsAvailable() { return true; }
        public virtual void OnCharge() { }
        public virtual void OnRemove(Board.Effect effect) { }

        public virtual void OnBegin() {
            controller = Library.Board.controller;

            T = true;
        }

        public void Cumulate(byte value) { cumulating = value; onlyOne = true; }
        public void SetStacks(int value) { stacks = value; }

        #region Board

        protected delegate void BoardAction(int x, int y);
        protected delegate void Shape(int x, int y, BoardAction action, int size);

        protected void CreateGemsOnBoard(Gem gem, int amount) {
            if (T) {
                controller.EnableSpellExecution(this);
                controller.board.CreateGems(gem, amount);
            }
            else S("Creates", amount, gem, "gems on board");
        }

        protected void DestroyGem(int x, int y){
             controller.board.DestroyGemBySpell(x, y);
        }

        protected void DestroyParticularGems(Gem gem, bool getLoot) {
            if (T) {
                controller.EnableSpellExecution(this);

                controller.board.PrepareBoardForSpell();
                controller.board.DestroyParticularGems(gem);
                controller.board.UpdateBoardAfterSpell();
                if (!getLoot) controller.board.ResetLoot();
            }
            else S("Takes all", gem, "gems from board");
        }

        protected void ModifyBoard(BoardAction action, Shape shape, int size){
            board = true;
            if (!controller.IsSpellUsed(this)) {
                controller.PrepareSelectingGemBySpell(this, size);
                return;
            }

            controller.board.GemToPoint(controller.board.activeGem, out int x, out int y);

            controller.board.PrepareBoardForSpell();
            shape(x, y, action, size);
            controller.board.UpdateBoardAfterSpell();
        }

        protected void ReplaceGemsOnBoard(Gem oldGem, Gem newGem) {
            if (T) {
                controller.EnableSpellExecution(this);
                controller.board.ReplaceGems(oldGem, newGem);
            }
            else S("Changes all", oldGem, "gems for", newGem, "on board");
        }

        #region Shape

        protected void Column(int x, int y, BoardAction action, int size) {
            for (int i = -size; i <= size; i++)
                for (int j = 0; j < Board.Board.boardSize; j++)
                    action(x + i, j);
        }

        protected void Square(int x, int y, BoardAction action, int size){
            for (int i = -size; i <= size; i++)
                for (int j = -size; j <= size; j++)
                    action(x + i, y + j);
        }

        protected void Shape_X(int x, int y, BoardAction action, int size) {
            int actualHeigth = -size;
            for (int i = -size; i <= size; i++) {
                action(x + i, y + actualHeigth);
                if(actualHeigth != 0)
                    action(x + i, y - actualHeigth);
                actualHeigth++;
            }
        }

        #endregion

        #endregion

        #region Effects

        protected void AddGems(Gem gem, int value) {
            if (T) controller.AddDiamondBySpell(you, (int)gem, value); else S("Gives you", value, gem, "gems");
        }

        protected void ArmorBuff(int duration, int percent) {
            this.duration = duration;
            if (T) { AddEffect(you, EffectName.ArmorBuff); you.ReduceTakenDamage(percent); } else S("Decrease taken damage by", percent+"% during", duration, Turn());
        }

        protected void Bleed() {
            duration = 4; Cumulate(1);
            if (T) AddNonStandardEffect(opponent, EffectName.Bleeding, new Bleeding()); else S("Bleeding");
        }

        protected void Blind(int duration) {
            this.duration = duration;
            if (T) AddEffect(opponent, EffectName.Blind); else S("Blinds opponent for", duration, Turn());
        }

        protected void BlockAutoAttack(int duration) {
            this.duration = duration;
            if (T) AddEffect(you, EffectName.BlockOfAutoAttack); else S("Blocks auto attacks for", duration, Turn(), "(max:", stacks+")");
        }

        protected void Charge(int duration) {
            this.duration = duration;
            if (T) AddEffect(you, EffectName.Charging); else S("Charges", duration, Turn());
        }

        protected void CreateShield(float value, int duration) {
            this.duration = duration; onlyOne = true;
            if (T) AddNonStandardEffect(you, EffectName.Shield, new Shield(value)); else S("Gives shield that can absorb up to", value, "damage (" + duration, Turn() + ")");
        }

        protected void DealDamage(float value) {
            if (T) controller.SpellDamage(opponent, value); else S("Deals", value, "damage");
        }

        protected void DealLeftHealthDamage(float value) {
            if (T) controller.SpellDamage(opponent, opponent.LeftHealth(value)); else S("Deals", value + "%", "of left HP");
        }

        protected void DealMaxHealthDamage(float value) {
            if (T) controller.SpellDamage(opponent, opponent.MaxHealth(value)); else S("Deals", value + "%", "of maximum HP");
        }

        protected void DealMissingHealthDamage(float value) {
            if (T) controller.SpellDamage(opponent, opponent.MissingHealth(value)); else S("Deals", value + "%", "of missing HP");
        }

        protected void Delay(int duration) {
            this.duration = duration;
            if (T) AddEffect(you, EffectName.Delay); else S("Charges", duration, Turn());
        }

        protected void Freeze(int value, int duration) {
            this.duration = duration; onlyOne = true;
            if (T) AddNonStandardEffect(controller.opponent, EffectName.Freeze, new FreezeEffect(value)); else S("Freezes opponent for", duration, Turn());
        }

        protected void GetAdditionalMove(int duration) {
            this.duration = duration; onlyOne = true;
            if (T) { AddEffect(you, EffectName.AdditionalMove); } else S("Gives", duration, "additional", Turn());
        }

        protected void Heal(float value) {
            if (T) controller.SpellDamage(you, -value); else S("Heals", value, "HP");
        }

        protected void IncreaseAutoAttackByEffect(int stacks, Effect effect) {
            effect.spell = this;
            onlyOne = true; SetStacks(stacks);
            if (!T) S("Next auto attack additionaly:");
            AddNonStandardEffect(you, EffectName.AutoAttackBuff, new AutoAttackEffect(effect));
        }

        protected void IncreaseAutoAttackStrength(int damage, int leftHealthDamage, int maxHealthDamage, int missingHealthDamage) {
            onlyOne = true; SetStacks(1);
            if (!T) S("Next auto attack additionaly:");
            AddNonStandardEffect(you, EffectName.AutoAttackBuff, new AutoAttackModification(damage, leftHealthDamage, maxHealthDamage, missingHealthDamage));
        }

        protected void ReduceArmor(int value, int duration) {
            this.duration = duration;
            if (!T) S("Reduce opponent's armor by", value);
            AddNonStandardEffect(opponent, EffectName.ArmorReduction, new ArmorReduction());
        }

        protected int RemoveGems(Gem gem, int value) {
            if (T)
                return controller.RemoveDiamond(opponent, gem, value);
            S("Takes opponent's", value, gem, "gems");
            return -1;
        }

        protected void SetDuration(int duration) {
            this.duration = duration; chargingAction = true;
            if (T) { AddEffect(you, EffectName.Charging); } else S("Every", duration, "next", Turn());
        }

        protected void Silent(int duration) {
            this.duration = duration; onlyOne = true;
            if (T) { AddEffect(opponent, EffectName.Silent); opponent.CheckSpellsAvailability(true); } else { S("Silents opponent for", duration, Turn()); }
        }

        protected void Stun(int duration) {
            this.duration = duration; onlyOne = true;
            if (T) AddEffect(opponent, EffectName.Stun); else S("Stuns opponent for", duration, Turn());
        }

        protected void TakeHealth(float value) {
            if (T) controller.SpellDamage(you, value); else S("Takes", value, "HP");
        }

        #endregion

        #region Effects Per Turn

        protected void DealDamagePerTurn(float value, int duration) {
            this.duration = duration;
            if (T) AddNonStandardEffect(opponent, EffectName.DamagePerTurn, new Damage(value)); else S("Deals", value, "damage for", duration, Turn());
        }

        #endregion

        #region Effects - Summon

        protected void SendBackSummonedUnit() {
            if(T) you.KillCharacter(character, true);
        }

        protected void SummonUnit(Board.SummonedUnit unit, int duration) {
            if (T) {
                Delay(duration);
                you.AddTemporaryCharacter(unit, you.effects[EffectName.Delay].GetLastElement());
                character = you.currentCharacter;
            }
            else S("Summons unit for", duration, "turns");
        }

        #endregion

        #region Other

        public static string text;

        public static bool T { get; set; }
        public static void S(params object[] args) { foreach (object item in args) text += item.ToString() + ' '; if(args.Length > 0) text += "\n"; }

        public string Turn() { return "turn" + (duration > 1 ? "s" : "");  }

        protected Board.Player opponent { get { return onlyYou ? controller.you : controller.opponent; } }
        protected Board.Player you { get { return controller.you; } }

        private void AddEffect(Board.Player player, EffectName name) {
            effectName = name;
            if (T) { controller.TryToAddEffect(player, new Board.Effect(name, this, player == you)); /*controller.ui.CreateStatusChange(player, name);*/ }
        }

        private void AddNonStandardEffect(Board.Player player, EffectName name, Effect effect) {
            effectName = name;
            Board.Effect boardEffect = new Board.Effect(name, this, player == you, effect);
            effect.SetValues(boardEffect);

            if (T) { controller.TryToAddEffect(player, boardEffect); effect.OnBegin(); /*controller.ui.CreateStatusChange(player, name);*/ } else { effect.Use(); }
        }

        public string GetName() {
            return Main.AddSpacesToSentence(GetType().Name);
        }

        public bool IsCumulating() { return cumulating > 0; }

        private void ReduceOpponentArmor(int value) {
            if(T) opponent.SetArmorDiffrence(-value);
        }

        public void ShowInfo(string text) {
            if(you.IsLocalPlayer())
                Library.guiController.ShowInfo(text);
        }

        public string ToText(Board.Effect effect) {
            T = false;
            text = "";

            OnCharge();
            Use();

            if(effect != null) {
                text = "";

                S(Main.AddSpacesToSentence(effect.name.ToString()));
                if (effect.effect != null)
                    effect.effect.Use();

                if (effect.duration > 0)
                    S("Duration: ", effect.duration);
                if (effect.stacks > 0)
                    S("Stacks: ", effect.stacks);
                if (effect.amount > 0)
                    S("Amount: ", effect.amount);
            }
            else {
                if (!endTurn)
                    S("Doesn't end turn");
            }


            T = true;
            return text.Remove(text.Length - 1);
        }

        #endregion

        public abstract class Effect {
            public Spell spell;
            public Board.Effect effect;
            public bool usedPerTurn;

            public void SetValues(Board.Effect effect) {
                this.effect = effect;
                spell = effect.spell;
            }

            protected void Remove() {
                effect.duration = 1;
            }

            public abstract void Use();

            public virtual void OnBegin() { }
            public virtual void OnCumulating() { Use(); }
            public virtual void OnStackRemoved() { }

            public virtual void SetEffect() { }
        }

        public class AutoAttackModification : Effect {
            public int damage;
            public int leftHealthDamage;
            public int maxHealthDamage;
            public int missingHealthDamage;

            public AutoAttackModification(int damage, int leftHealthDamage, int maxHealthDamage, int missingHealthDamage) {
                this.damage = damage;
                this.leftHealthDamage = leftHealthDamage;
                this.maxHealthDamage = maxHealthDamage;
                this.missingHealthDamage = missingHealthDamage;
            }

            public override void Use() {
                if(damage > 0) spell.DealDamage(damage);
                if(missingHealthDamage > 0) spell.DealMissingHealthDamage(missingHealthDamage);
            }

            public override void OnStackRemoved() {
                Use();
            }
        }

        public class ArmorReduction : Effect {

            public override void Use() {
                spell.ReduceOpponentArmor(effect.amount);
            }

            public override void OnBegin() {
                Use();
            }
        }

        public class AutoAttackEffect : Effect {
            readonly Effect cargo;

            public AutoAttackEffect(Effect effect) {
                cargo = effect;
            }

            public override void Use() {
                spell.stacks = 0;
                cargo.SetEffect();
            }

            public override void OnStackRemoved() {
                Use();
            }
        }

        public class Bleeding : Effect {

            public Bleeding() {
                usedPerTurn = true;
            }

            public override void Use() {
                spell.DealDamage(effect.amount);
            }

            public override void SetEffect() {
                spell.Bleed();
            }
        }

        public class Damage : Effect {
            float damage;

            public Damage(float damage) {
                this.damage = damage;
                usedPerTurn = true;
            }

            public override void Use() {
                spell.DealDamage(damage);
            }
        }

        public class FreezeEffect : Effect {
            readonly int value;

            public FreezeEffect(int value) {
                usedPerTurn = true;
                this.value = value;
            }

            public override void Use() {
                if (T) {
                    if (spell.RemoveGems(Gem.Red, value) < 0) {
                        spell.Stun(2);
                        Remove();
                    }
                }
                else S("Removes", value, "red gems.\nStuns if there aren't any.");
            }
        }

        public class Shield : Effect {
            float value;

            public Shield(float value) {
                this.value = value;
            }

            public override void Use() {
                if (!T) S("Strength:", value);
            }

            public float AbsorbDamage(float damage) {
                value -= damage;
                if (value < 0)
                    return -value;
                return 0;
            }

            public float GetLeftStrength() { return value; }
        }

    }

    public abstract class HeroSpell : Spell {

    }

}
