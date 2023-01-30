using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Script.Spell {

    #region Piechur

    public class Swing : Spell {
        public Swing() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 3 },
                { Gem.Yellow, 6 }
            };
        }

        public override void Use() {
            DealDamage(5);
            Stun(2);
        }

        public override void AssignToAI(Board.AI ai) { ai.Stun(this); }
    }

    public class Guard : Spell {
        public Guard() {
            cost = new Dictionary<Gem, int> {
                { Gem.Green, 7 },
            };
            SetStacks(1);
            endTurn = false;
        }

        public override void Use() {
            BlockAutoAttack(4);
        }

        public override void AssignToAI(Board.AI ai) { ai.BlockOfAutoAttack(this); }
    }

    #endregion

    #region Strzelec

    public class Headshot : Spell {
        public Headshot() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 10 },
            };
        }

        public override void Use() {
            DealDamage(12);
        }
    }

    public class QuickFingers : Spell {
        public QuickFingers() {
            cost = new Dictionary<Gem, int> {
                { Gem.Yellow, 6 },
            };
            endTurn = false;
        }

        public override void Use() {
            GetAdditionalMove(1);
        }
    }

    #endregion

    #region Czarodziejka

    public class Heal : Spell {
        public Heal() {
            cost = new Dictionary<Gem, int> {
                { Gem.Green, 10 },
            };
        }

        public override void Use() {
            Heal(8);
        }

        public override void AssignToAI(Board.AI ai) { ai.Heal(this); }
    }

    public class Silent : Spell {
        public Silent() {
            cost = new Dictionary<Gem, int> {
                { Gem.Blue, 4 },
                { Gem.Yellow, 6 },
            };
            endTurn = false;
        }

        public override void Use() {
            DealDamage(5);
            Silent(3);
        }

        public override void AssignToAI(Board.AI ai) { ai.Silent(this); }
    }

    #endregion

    #region Kanonier

    public class BurningBullets : Spell {
        public BurningBullets() {
            cost = new Dictionary<Gem, int> {
                { Gem.Blue, 12 },
            };
        }

        public override void Use() {
            DealDamagePerTurn(4, 5);
        }
    }

    public class ExplosionShot : Spell {
        public ExplosionShot() {
            cost = new Dictionary<Gem, int> {
                { Gem.Purple, 12 },
            };
        }

        public override void Use() {
            if (T) ModifyBoard(DestroyGem, Square, 1); else S("Destroys 3x3 gems");
        }
    }

    #endregion

    #region Rycerz

    public class Charge : Spell {
        public Charge() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 8 },
                { Gem.Yellow, 2 },
            };
        }

        public override void Use() {
            Charge(1);
        }

        public override void OnCharge() {
            DealDamage(14);
        }
    }

    public class BattleCry : Spell {
        public BattleCry() {
            cost = new Dictionary<Gem, int> {
                { Gem.Purple, 7 },
            };
            endTurn = false;
        }

        public override void Use() {
            ReplaceGemsOnBoard(Gem.White, Gem.Red);
        }
    }

    #endregion

    #region Zyrokopter

    public class GasBomb : Spell {
        public GasBomb() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 7 },
                { Gem.Blue, 5 },
            };
        }

        public override void Use() {
            DealDamage(10);
            DealDamagePerTurn(2, 3);
        }
    }

    public class AirStrike : Spell {
        public AirStrike() {
            cost = new Dictionary<Gem, int> {
                { Gem.Purple, 8 },
            };
        }

        public override void Use() {
            if (T) ModifyBoard(DestroyGem, Column, 0); else S("Destroys selected column");
        }
    }

    #endregion

    #region Czolg

    public class Reload : Spell {
        public Reload() {
            cost = new Dictionary<Gem, int> {
                { Gem.Green, 9 },
            };
        }

        public override void Use() {
            IncreaseAutoAttackStrength(6, 0, 0, 8);
        }
    }

    public class PiercingShell : Spell {
        public PiercingShell() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 8 },
                { Gem.Yellow, 6 },
            };
            Cumulate(1);
        }

        public override void Use() {
            DealDamage(12);
            ReduceArmor(cumulating, 0);
        }
    }

    #endregion

    #region Ghul

    public class Hunger : Spell {
        public Hunger() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 5 },
            };
        }

        public override void Use() {
            DealDamage(5);
            if (T) DealDamage(you.PercentOfMissingHP() * 5); else S("+1 for every missing 20% of your HP");
        }
    }

    public class Cannibalism : Spell {
        public Cannibalism() {
            cost = new Dictionary<Gem, int> {
                { Gem.Green, 8 },
            };
        }

        public override void Use() {
            TakeHealth(10);
            Delay(2);
        }

		public override void OnCharge() {
            Heal(20);
		}

        public override void AssignToAI(Board.AI ai) { ai.Heal(this); }
    }

    #endregion

    #region Bies

    public class SpiderWeb : Spell {
        public SpiderWeb() {
            cost = new Dictionary<Gem, int> {
                { Gem.Blue, 6 },
                { Gem.Yellow, 4 },
            };
        }

        public override void Use() {
            DealDamage(8);
            Blind(3);
        }

        public override void AssignToAI(Board.AI ai) { ai.Blind(this); }
    }

    public class Cocoon : Spell {
        public Cocoon() {
            cost = new Dictionary<Gem, int> {
                { Gem.Green, 5 },
                { Gem.Yellow, 5 },
            };
        }

        public override void Use() {
            CreateShield(14.9f, 4);
        }

        public override void AssignToAI(Board.AI ai) { ai.Shield(this); }
    }

    #endregion

    #region Nekromanta

    public class Darkness : Spell {
        public Darkness() {
            cost = new Dictionary<Gem, int> {
                { Gem.Purple, 6 },
            };
            endTurn = false;
        }

        public override void Use() {
            ReplaceGemsOnBoard(Gem.Yellow, Gem.Purple);
        }
    }

    public class Necromancy : Spell {
        public static Board.SummonedUnit skeleton;

        public Necromancy() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 5 },
                { Gem.Green, 7 },
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
            SummonUnit(skeleton, 10);
        }

        public override void OnCharge() {
            SendBackSummonedUnit();
        }

        public override void AssignToAI(Board.AI ai) { ai.Shield(this); }
    }

    #endregion

    #region WozMiesa

    public class DiseaseCloud : Spell {
        public DiseaseCloud() {
            cost = new Dictionary<Gem, int> {
                { Gem.Blue, 6 },
                { Gem.Yellow, 4 },
            };
        }

        public override void Use() {
            const int damage = 2;

            int diffrence = RemoveGems(Gem.Green, 7);
            if (diffrence < 0)
                if (T) DealDamage(damage * -diffrence); else S("Deals", damage, "damage for every missing gem");
        }
    }

    public class GatheringCorpses : Spell {
        public GatheringCorpses() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 6 },
                { Gem.Purple, 10 },
            };
        }

        public override void Use() {
            const int value = 2;
            if (T) {
                IncreaseAutoAttackStrength(controller.board.CountGems(Gem.Skull) * value, 0, 0, 0);
                DestroyParticularGems(Gem.Skull, false);
            }
            else S("Destroys all Skull gems on board. Next auto attack will be increased by", value, "damage per every destroyed gem");
        }
    }

    #endregion

    #region Banshee

    public class Curse : Spell {
        public Curse() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 6 },
                { Gem.Yellow, 2 },
            };
        }

        public override void Use() {
            DealDamage(8);
            Silent(1);
        }

        public override void AssignToAI(Board.AI ai) { ai.Silent(this); }
    }

    public class ChainBond : Spell {
        public ChainBond() {
            cost = new Dictionary<Gem, int> {
                { Gem.Green, 9 },
            };
        }

        public override void Use() {
            DealMaxHealthDamage(6);
            if (T) Heal(opponent.MaxHealth(3)); else S("Heals half of this value");
        }

        public override void AssignToAI(Board.AI ai) { ai.Heal(this); }
    }

    #endregion

    #region Plugastwo

    public class Butcher : Spell {
        public Butcher() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 8 },
                { Gem.Green, 4 },
            };
        }

        public override void Use() {
            DealDamage(12);
            Heal(3);
        }

        public override void AssignToAI(Board.AI ai) { ai.Heal(this); }
    }

    public class Surgeon : Spell {
        public Surgeon() {
            cost = new Dictionary<Gem, int> {
                { Gem.Purple, 6 },
            };
        }

        public override void Use() {
            if (T) ModifyBoard(DestroyGem, Shape_X, 1); else S("Destroys gems in X shape");
        }
    }

    #endregion

    #region Zmij

    public class FreezingBreath : Spell {
        public FreezingBreath() {
            cost = new Dictionary<Gem, int> {
                { Gem.Blue, 6 },
                { Gem.Yellow, 3 },
            };
        }

        public override void Use() {
            Freeze(2, 3);
        }
    }

    public class IceStrike : Spell {
        public IceStrike() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 10 },
                { Gem.Yellow, 4 },
            };
        }

        public override void Use() {
            DealDamage(14);
            if (T) {
                if (opponent.IsStunned())
                    DealDamage(4);
            }
            else S("+4 if opponent is stunned");
        }
    }

    #endregion

    #region BudynekGlowny

    public class StrongWalls : Spell {
        const int value = 6;

        public StrongWalls() {
            cost = new Dictionary<Gem, int> {
                { Gem.Purple, 10 },
            };
        }

        public override void Use() {
            if (T) controller.leftMoves -= value; else S("Reduces number of left moves by ", value);
        }
    }

    public class Militia : Spell {
        public Militia() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 10 },
                { Gem.Blue, 5 },
            };
        }

        public override void Use() {
            IncreaseAutoAttackStrength(20, 0, 0, 0);
        }
    }

    #endregion

    #region Wieza

    public class BurningBullet : Spell {
        public BurningBullet() {
            cost = new Dictionary<Gem, int> {
                { Gem.Blue, 11 },
            };
        }

        public override void Use() {
            DealDamagePerTurn(5, 3);
        }
    }

    public class PowerfulShot : Spell {
        public PowerfulShot() {
            cost = new Dictionary<Gem, int> {
                { Gem.Red, 9 },
                { Gem.Yellow, 5 },
            };
            endTurn = false;
        }

        public override void Use() {
            DealDamage(10);
            Stun(1);
        }
    }

    #endregion

}
