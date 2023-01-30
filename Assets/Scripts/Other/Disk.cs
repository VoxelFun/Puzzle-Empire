using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Reflection;
using System;

public static class Disk {

    static string path = "";

    public static void Prepare() {
#if UNITY_IOS
	    path = Application.persistentDataPath + "/";
	    if (!Directory.Exists(path + "All"))
		    Directory.CreateDirectory(path + "All");
	    path += "All/";
#elif UNITY_ANDROID
        path = Application.persistentDataPath + "/";
#endif
#if UNITY_EDITOR
        path = "";
#endif
    }

    #region File

    public static string GetFileContent(string name) {
        StreamReader reader = File.OpenText(path + name + ".txt");
        string content = reader.ReadToEnd();
        reader.Close();
        return content;
    }

    public static Parser LoadFile(string name) {
        return new Parser(GetFileContent(name));
    }

    public static void SaveFile(string name, string content) {
        StreamWriter stream = File.CreateText(path + name + ".txt");
        stream.Write(content);
        stream.Close();
    }

    public static void SaveFile(string name, StringWriter content) {
        SaveFile(name, content.ToString());
    }

    #endregion

    #region PlayerPrefs

    #region Expect

    public static bool ExpectValue(BoolCell cell, bool value) {
        if (Get(cell) == value)
            return true;
        Set(cell, value);
        return false;
    }

    public static bool ExpectValue(StringCell cell, string value) {
        if (Get(cell) == value)
            return true;
        Set(cell, value);
        return false;
    }

    #endregion

    #region Get

    static int Get(Cell cell, int id) {
        int product = id * cell.size;
        int num = product / 32;
        int pos = product % 32;

        return PlayerPrefs.GetInt(cell.prefix + num).GetBit(pos, cell.size);
    }

    public static bool Get(BoolCell cell) => Get(@bool, (int)cell).ToBool();
    public static bool Get(BoolCell cell, bool defaultValue) => IsSet(cell) ? Get(cell) : defaultValue;

    public static byte Get(ByteCell cell) => (byte)Get(@byte, (int)cell);
    public static byte Get(ByteCell cell, byte defaultValue) => IsSet(cell) ? Get(cell) : defaultValue;
    public static T Get<T>(ByteCell cell, T defaultValue) => IsSet(cell) ? (T)(object)Get(cell) : defaultValue;
    //public static T Get<T>(ByteCell cell, T defaultValue) where T : unmanaged => IsSet(cell) ? (T)Get(cell) : defaultValue;

    public static int Get(IntCell cell) => PlayerPrefs.GetInt(@int + (int)cell);
    public static int Get(IntCell cell, int defaultValue) => PlayerPrefs.GetInt(@int + (int)cell, defaultValue);

    public static string Get(StringCell cell) => PlayerPrefs.GetString(@string + (int)cell);
    public static string Get(StringCell cell, string defaultValue) => PlayerPrefs.GetString(@string + (int)cell, defaultValue);

    #endregion

    #region IsSet

    private static int GetIsSetValue(string name) => PlayerPrefs.GetInt(@isset + name);

    private static bool HasKey(string key) {
        return PlayerPrefs.HasKey(key);
    }

    private static bool IsSet(Cell cell, int id) {
        int product = id;
        int num = product / 32;
        int pos = product % 32;

        return GetIsSetValue(cell.prefix + num).GetBit(pos).ToBool();
    }

    private static void SetAsSet(string name, int pos) {
        PlayerPrefs.SetInt(@isset + name, GetIsSetValue(name).SetBit1(pos));
    }

    private static bool IsSet(BoolCell cell) {
        return IsSet(@bool, (int)cell);
    }

    private static bool IsSet(ByteCell cell) {
        return IsSet(@byte, (int)cell);
    }

    private static bool IsSet(IntCell cell) {
        return HasKey(@int + (int)cell);
    }

    private static bool IsSet(StringCell cell) {
        return HasKey(@string + (int)cell);
    }

    #endregion

    #region Set

    static void Set(Cell cell, int value, int id) {
        int product = id * cell.size;
        int num = product / 32;
        int pos = product % 32;

        string name = cell.prefix + num;
        int data = PlayerPrefs.GetInt(name);

        PlayerPrefs.SetInt(name, data.SetFixedLengthBitValue(value, cell.size, pos));
        SetAsSet(name, id);
    }

    public static void Set(BoolCell cell, bool value) {
        Set(@bool, value.ToInt(), (int)cell);
    }

    public static void Set(ByteCell cell, byte value) {
        Set(@byte, value, (int)cell);
    }

    public static void Set(IntCell cell, int value) {
        PlayerPrefs.SetInt(@int + (int)cell, value);
    }

    public static void Set(StringCell cell, string value) {
        PlayerPrefs.SetString(@string + (int)cell, value);
    }

    #endregion

    #region Cell

    static readonly Cell @bool = new Cell("b", 1);
    static readonly Cell @byte = new Cell("y", 8);

    const string @int = "i";
    const string @string = "s";
    const string @isset = "i";

    struct Cell {
        public readonly string prefix;
        public readonly int size;

        public Cell(string prefix, int size) {
            this.prefix = prefix;
            this.size = size;
        }
    }

    #endregion

    #endregion

    #region Serialization

    public static T DeserializeFile<T>(string name) {
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(path + name + ".bin", FileMode.Open, FileAccess.Read, FileShare.Read);
        T obj = (T)formatter.Deserialize(stream);
        stream.Close();

        return obj;
    }

    public static void SerializeToFile(string name, object obj) {
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(path + name + ".bin", FileMode.Create, FileAccess.Write, FileShare.None);
        formatter.Serialize(stream, obj);
        stream.Close();

        //byte[] buffer = ConvertFileToBytesArray(name);
        //StreamWriter stringWriter = File.CreateText(path + name + ".txt");
        //stringWriter.Write(ByteArrayToString(buffer));
        //stringWriter.Close();
    }

    public static string ByteArrayToString(byte[] ba) {
        System.Text.StringBuilder hex = new System.Text.StringBuilder(ba.Length * 2);
        foreach (byte b in ba)
            hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    }

    #endregion

    #region Other

    public static byte[] ConvertFileToBytesArray(string name) {
        return File.ReadAllBytes(path + name + ".bin");
    }

    #endregion

}

public enum BoolCell {
    
}

public enum ByteCell {
    Difficulty
}

public enum IntCell {
    
}

public enum StringCell {
    
}