using System.Collections;
using System.Collections.Generic;
using Script.Board;
using UnityEngine;

namespace Script.Spell {

    #region Garam

    public class DecisiveStrike : HeroSpell {
        public DecisiveStrike() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 4 },
                { Gem.Yellow, 4 },
            };
        }

        public override void Use() {
            DealDamage(8);
            Silent(2);
        }

        public override void AssignToAI(AI ai) {
            ai.Silent(this);
        }
    }

    public class Swordonado : HeroSpell {
        public Swordonado() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 8 },
            };
        }

        public override void Use() {
            SetDuration(3);
        }

        public override void OnCharge() {
            DealDamage(5);
        }
    }

    public class Defence : HeroSpell {
        public Defence() {
            cost = new Dictionary<Gem, int> {
                { Gem.Green, 4 },
                { Gem.Yellow, 3 },
            };
            endTurn = false;
        }

        public override void Use() {
            ArmorBuff(3, 40);
        }

        public override void OnRemove(Board.Effect effect) {
            you.RestoreTakenDamage();
        }

        public override void AssignToAI(AI ai) {
            ai.ArmorBuff(this);
        }
    }

    public class Steadfastness : HeroSpell {
        const float value = 3;

        public Steadfastness() {
            cost = new Dictionary<Gem, int> {
                { Gem.Green, 7 },
                { Gem.Purple, 3 },
            };
        }

        public override void Use() {
            if (T) Heal(value + value * controller.board.CountGems(Gem.Green) * 0.2f); else S("Heals", value, "HP\n+20% for every green gem on the board");
        }

        public override void AssignToAI(AI ai) {
            ai.Heal(this);
        }
    }

    public class NoMercy : HeroSpell {
        const float value = 3;

        public NoMercy() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 13 },
                { Gem.Green, 5 },
            };
        }

        public override void Use() {
            DealDamage(15);
            DealMissingHealthDamage(10);
        }
    }

    #endregion

    #region Darek

    public class DeadlyAxe : HeroSpell {
        public DeadlyAxe() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 11 },
            };
        }

        public override void Use() {
            DealDamage(9);
            Bleed();
        }
    }

    public class DeepCut : HeroSpell {
        public DeepCut() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 4 },
            };
            endTurn = false;
        }

        public override void Use() {
            IncreaseAutoAttackByEffect(1, new Bleeding());
        }
    }

    public class Sadism : HeroSpell {
        public Sadism() {
            cost = new Dictionary<Gem, int> {
                { Gem.Green, 7 },
            };
            endTurn = false;
        }

        public override void Use() {
            int value = 2;

            Board.Effect effect = opponent.GetEffect(EffectName.Bleeding);
            if (effect != null)
                value += value * effect.amount;
            if (T) Heal(value); else S("Heals", value, "HP\n+100% for every bleeding stack");
        }

        public override void AssignToAI(AI ai) {
            ai.Heal(this);
        }
    }

    public class GoBerserk : HeroSpell {
        public GoBerserk() {
            cost = new Dictionary<Gem, int> {
                { Gem.Blue, 5 },
                { Gem.Purple, 5 },
            };
        }

        public override void Use() {
            AddGems(Gem.Red, 7);
        }
    }

    public class Guillotine : HeroSpell {

        public Guillotine() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 14 },
            };
        }

        public override void Use() {
            float value = 12;

            Board.Effect effect = opponent.GetEffect(EffectName.Bleeding);
            if (effect != null)
                value += value * effect.amount * 0.75f;

            Library.Board.controller.RemoveEffect(opponent, EffectName.Bleeding);
            if (T) DealDamage(value); else S("Deals", value, "damage\n+75% for every bleeding stack");
        }
    }

    #endregion

    #region Trynda

    public class ConclusiveStrike : HeroSpell {
        public ConclusiveStrike() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 5 },
                { Gem.Yellow, 5 },
            };
        }

        public override void Use() {
            DealDamage(10);
            if (T) { if(you.PercentOfMissingHP() > opponent.PercentOfMissingHP()) Stun(1); } else S("Stuns for 1 turn if opponent's percent of missing health is lower than yours");
        }

        public override void AssignToAI(AI ai) {
            ai.Stun(this);
        }
    }

    public class LastSlashes : HeroSpell {

        public LastSlashes() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 6 },
                { Gem.Green, 6 },
            };
        }

        public override void Use() {
            const int value = 9;
            if (T) DealDamage(value * (1 + you.PercentOfMissingHP() * 4 / 3)); else S("Deals", value, "damage\n+20% for every 15% of missing HP");
        }
    }

    public class SurviveWill : HeroSpell {

        public SurviveWill() {
            cost = new Dictionary<Gem, int> {
                { Gem.Green, 10 },
            };
        }

        public override void Use() {
            const int value = 5;
            if (T) Heal(value * (1 + you.PercentOfMissingHP() * 1.5f)); else S("Heals", value, "HP\n+20% for every 10% of missing HP");
        }

        public override void AssignToAI(AI ai) {
            ai.Heal(this);
        }
    }

    public class Amok : HeroSpell {

        public Amok() {
            cost = new Dictionary<Gem, int> {
                { Gem.Purple, 12 },
            };
        }

        public override void Use() {
            ReplaceGemsOnBoard(Gem.Red, Gem.Skull);
        }
    }

    public class FinalBreath : HeroSpell {

        public FinalBreath() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 5 },
                { Gem.Green, 12 },
                { Gem.Yellow, 5 },
            };
        }

        public override void Use() {
            ArmorBuff(3, 100);
            if (!T) S("After that, heals 10% of missing HP");
        }

        public override void OnRemove(Board.Effect effect) {
            you.RestoreTakenDamage();
            Heal(you.MissingHealth(10));
        }

        public override void AssignToAI(AI ai) {
            ai.ArmorBuff(this);
        }
    }

    #endregion

    #region Piesek

    public class Ghoul : Spell {
        public static SummonedUnit ghoul;

        public Ghoul() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 6 },
                { Gem.Green, 5 },
                { Gem.Purple, 4 },
            };
            endTurn = false;
        }

        public override bool IsAvailable() {
            if (you.characters.Count < Engine.limitUnitPerField)
                return true;
            ShowInfo("Not enough free space");
            return false;
        }

        public override void Use() {
            SummonUnit(ghoul, 8);
        }

        public override void OnCharge() {
            SendBackSummonedUnit();
        }

        public override void AssignToAI(AI ai) { ai.Shield(this); }
    }

    public class RitualBlade : Spell {
        public RitualBlade() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 3 },
                { Gem.Blue, 5 },
                { Gem.Green, 4 },
            };
        }

        public override void Use() {
            DealDamage(9);
            DealLeftHealthDamage(5);
        }
    }

    public class BreathOfDeath : Spell {
        public BreathOfDeath() {
            cost = new Dictionary<Gem, int> {
                { Gem.Purple, 10 },
            };
        }

        public override void Use() {
            CreateGemsOnBoard(Gem.Skull, 4);
        }
    }

    public class BlackFog : Spell {
        public BlackFog() {
            cost = new Dictionary<Gem, int> {
                { Gem.Purple, 8 },
            };
        }

        public override void Use() {
            const int value = 6;
            if (T) controller.leftMoves += value; else S("Increase number of left moves by ", value);
        }
    }

    public class Abomination : Spell {
        public static SummonedUnit abomination;

        public Abomination() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 8 },
                { Gem.Green, 18 },
                { Gem.Purple, 5 },
            };
        }

        public override bool IsAvailable() {
            if (you.characters.Count < Engine.limitUnitPerField)
                return true;
            ShowInfo("Not enough free space");
            return false;
        }

        public override void Use() {
            SummonUnit(abomination, 20);
        }

        public override void OnCharge() {
            SendBackSummonedUnit();
        }

        public override void AssignToAI(AI ai) { ai.Shield(this); }
    }

    #endregion

    #region Liszu

    public class FrostNova : HeroSpell {
        public FrostNova() {
            cost = new Dictionary<Gem, int> {
                { Gem.Blue, 12 },
                { Gem.Yellow, 6 },
            };
            endTurn = false;
        }

        public override void Use() {
            DealDamage(20);
            Stun(1);
        }

        public override void AssignToAI(AI ai) {
            ai.Stun(this);
        }
    }

    public class Necrosis : HeroSpell {
        public Necrosis() {
            cost = new Dictionary<Gem, int> {
                { Gem.Blue, 5 },
            };
        }

        public override void Use() {
            const int value = 6;
            if (T) DealDamage(value * (1 + opponent.IsStunned().ToInt())); else S("Deals", value, "damage\n+100% if opponent is stunned");
        }
    }

    public class IceShield : HeroSpell {
        public IceShield() {
            cost = new Dictionary<Gem, int> {
                { Gem.Green, 10 },
                { Gem.Yellow, 4 },
            };
            endTurn = false;
        }

        public override void Use() {
            CreateShield(20, 3);
            if (!T) S("After that, heals for 50% of left strength");
        }

        public override void OnRemove(Board.Effect effect) {
            Heal((effect.effect as Shield).GetLeftStrength() * 0.5f);
        }

        public override void AssignToAI(AI ai) {
            ai.Heal(this);
        }
    }

    public class DarkRitual : HeroSpell {
        const int value = 6;

        public DarkRitual() {
            cost = new Dictionary<Gem, int> {
                { Gem.Green, 3 },
                { Gem.Purple, 8 },
            };
            endTurn = false;
        }

        public override bool IsAvailable() {
            if (you.health > value)
                return true;
            ShowInfo("Not enough HP");
            return false;
        }

        public override void Use() {
            const int bonus = 10;

            if (T) AddGems(Gem.Blue, (int)(bonus * (1 + controller.board.CountGems(Gem.Skull) * 0.1f))); else S("Gives", bonus, "Blue gems\n+10% for every Skull on board");
            if (T) controller.SpellDamage(you, you.MaxHealth(value)); else S("Takes", value + " HP");
        }
    }

    public class Decay : HeroSpell {
        public Decay() {
            cost = new Dictionary<Gem, int> {
                { Gem.Blue, 15 },
                { Gem.Green, 8 },
            };
        }

        public override void Use() {
            SetDuration(3);
        }

        public override void OnCharge() {
            DealMaxHealthDamage(12);
        }
    }

    #endregion

}
