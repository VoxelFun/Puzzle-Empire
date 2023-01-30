using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.Spell;

public class Undead : Race {

    public static Undead stock;
    public Hero piesek;
    public Hero liszu;

    protected override void Begin() {
        stock = this;

        mainBuilding.SetValues(
            new StrongWalls(), new Militia()
        );

        buildings[2].SetValues(
            new BurningBullet(), new PowerfulShot()
        );

        units[0].SetValues(     //Ghul
            new Hunger(), new Cannibalism()
        );

        units[1].SetValues(     //Bies
            new SpiderWeb(), new Cocoon()
        );

        units[2].SetValues(     //Nekromanta
            new Darkness(), new Necromancy()
        );

        units[3].SetValues(     //WozMiesa
            new DiseaseCloud(), new GatheringCorpses()
        );

        units[4].SetValues(     //Banshee
            new Curse(), new ChainBond()
        );

        units[5].SetValues(     //Plugastwo
            new Butcher(), new Surgeon()
        );

        units[6].SetValues(     //Zmij
            new FreezingBreath(), new IceStrike()
        );

        piesek.SetValues(
            new Ghoul(), new RitualBlade(), new BreathOfDeath(), new BlackFog(), new Abomination()
        );

        liszu.SetValues(
            new FrostNova(), new Necrosis(), new IceShield(), new DarkRitual(), new Decay()
        );
    }

    public override Information[] GetHeroes() {
        return new Information[] {
            piesek, liszu
        };
    }

}
