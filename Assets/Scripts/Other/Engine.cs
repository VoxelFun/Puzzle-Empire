using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Engine {

    public const int fps = 30;

    public const int fieldSize = 15;
    public const int limitUnitPerField = 3;

    public const int movementSpeed = 60;//40, a hp twierdzy to 200

    public const int amountOfTurns = 30;
    public const float damagePerEveryNextUnit = 0.3f;
    public const float diamondsMultiplierPerEveryNextUnit = 0.2f;

    public const float buildingHealValue = 0.05f;
    public const int buildingTime = 2;
    public const int fieldsPerTurn = 3;
    public const float healValue = 0.2f;

    //Skirmish exp
    public const int maxHeroLevel = 12;
    public const int requiredExpForLevel = 400;
    public const float expRequirementMultiplier = 1.2f;

    public static class Hero {
        public const float diamondsMultiplier = 0.6f / maxHeroLevel;
        public const float increment = 1f / maxHeroLevel;
    }
}
