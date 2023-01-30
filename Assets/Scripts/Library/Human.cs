using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.Spell;

public class Human : Race {

    public static Human stock;

    public Hero garam;
    public Hero darek;
    public Hero trynda;

    protected override void Begin() {
        stock = this;

        mainBuilding.SetValues(
            new StrongWalls(), new Militia()
        );

        buildings[2].SetValues(
            new BurningBullet(), new PowerfulShot()
        );

        units[0].SetValues(     //Piechur
            new Swing(), new Guard()
        );

        units[1].SetValues(     //Strzelec
            new Headshot(), new QuickFingers()
        );

        units[2].SetValues(     //Czarodziejka
            new Heal(), new Silent()
        );

        units[3].SetValues(     //Kanonier
            new BurningBullets(), new ExplosionShot()
        );

        units[4].SetValues(     //Rycerz
            new Charge(), new BattleCry()
        );

        units[5].SetValues(     //Zyrokopter
            new GasBomb(), new AirStrike()
        );

        units[6].SetValues(     //Czolg
            new Reload(), new PiercingShell()
        );

        garam.SetValues(
            new DecisiveStrike(), new Swordonado(), new Defence(), new Steadfastness(), new NoMercy()
        );

        darek.SetValues(
            new DeadlyAxe(), new DeepCut(), new Sadism(), new GoBerserk(), new Guillotine()
        );

        trynda.SetValues(
            new ConclusiveStrike(), new LastSlashes(), new SurviveWill(), new Amok(), new FinalBreath()
        );
    }

    public override Information[] GetHeroes() {
        return new Information[] {
            garam, darek, trynda
        };
    }

}
