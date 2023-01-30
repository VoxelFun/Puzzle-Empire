using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Preference{

    public static Difficulty difficulty = Difficulty.Normal;

    public static void Load() {
        difficulty = (Difficulty)Disk.Get(ByteCell.Difficulty, (byte)Difficulty.Normal);
    }

    public static void SaveDifficulty(Difficulty value) {
        difficulty = value;
        Disk.Set(ByteCell.Difficulty, (byte)value);
    }

    public enum Difficulty {
        Easy, Normal
    }

}