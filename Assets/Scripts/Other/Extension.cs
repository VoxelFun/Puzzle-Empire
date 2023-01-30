using UnityEngine;
using System.Collections.Generic;

public static class Extension {

    #region Animations

    public static void Play(this Animation[] animations) {
        foreach (Animation animation in animations)
            animation.Play();
    }

    public static void Play(this Animation[] animations, string name) {
        foreach (Animation animation in animations)
            animation.Play(name);
    }

    public static void PlayQueued(this Animation[] animations, string name) {
        foreach (Animation animation in animations)
            animation.PlayQueued(name);
    }

    #endregion

    #region Array

    public static T GetRandom<T>(this T[] array){
        return array[Random.Range(0, array.Length)];
    }

    public static void Shuffle<T>(this T[] list) {
        int n = list.Length;
        System.Random random = new System.Random();
        while (n > 1) {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static T[] SubArray<T>(this T[] data, int index, int length) {
        T[] result = new T[length];
        System.Array.Copy(data, index, result, 0, length);
        return result;
    }

    #endregion

    #region Bit

    #region Byte

    public static byte GetBit(this byte num, int pos) {
        return (byte)((num >> pos) & 1);
    }

    public static byte GetBit(this byte num, int pos, int amount) {
        int result = 0;
        for (int i = 0; i < amount; i++)
            result |= num.GetBit(pos + i) << i;
        return (byte)result;
    }

    public static byte SetBit(this byte num, int pos, int bit) {
        return bit == 0 ? num.SetBit0(pos) : num.SetBit1(pos);
    }

    public static byte SetBit(this byte num, int pos, int bit, int amount) {
        for (int i = 0; i < amount; i++) {
            num = num.SetBit(pos + i, bit);
        }
        return num;
    }

    public static byte SetBit0(this byte num, int pos) {
        return (byte)(num & ~(1 << pos));
    }

    public static byte SetBit1(this byte num, int pos) {
        return (byte)(num | (1 << pos));
    }

    public static byte SetBitValue(this byte num, int value, int pos) {
        int condition = 1, i = 0;
        while (value >= condition) {
            num = num.SetBit(pos + i, value.GetBit(i));
            condition *= 2;
            i++;
        }
        return num;
    }

    public static byte SetFixedLengthBitValue(this byte num, int value, int length, int pos) {
        for (int i = 0; i < length; i++)
            num = num.SetBit(pos + i, value.GetBit(i));
        return num;
    }

    #endregion

    #region Int

    public static int GetBit(this int num, int pos) {
        return (num >> pos) & 1;
    }

    public static int GetBit(this int num, int pos, int amount) {
        int result = 0;
        for (int i = 0; i < amount; i++)
            result |= num.GetBit(pos + i) << i;
        return result;
    }

    public static int SetBit(this int num, int pos, int bit) {
        return bit == 0 ? num.SetBit0(pos) : num.SetBit1(pos);
    }

    public static int SetBit(this int num, int pos, int bit, int amount) {
        for (int i = 0; i < amount; i++)
            num = num.SetBit(pos + i, bit);
        return num;
    }

    public static int SetBit0(this int num, int pos) {
        return num & ~(1 << pos);
    }

    public static int SetBit1(this int num, int pos) {
        return num | (1 << pos);
    }

    public static int SetBitValue(this int num, int value, int pos) {
        int condition = 1, i = 0;
        while (value >= condition) {
            num = num.SetBit(pos + i, value.GetBit(i));
            condition *= 2;
            i++;
        }
        return num;
    }

    public static int SetFixedLengthBitValue(this int num, int value, int length, int pos) {
        for (int i = 0; i < length; i++)
            num = num.SetBit(pos + i, value.GetBit(i));
        return num;
    }

    #endregion

    #endregion

    #region Bool

    public static int ToInt(this bool value) {
        return value ? 1 : 0;
    }

    public static int ToSign(this bool value) {
        return (value ? 1 : 0) * 2 - 1;
    }

    #endregion

    #region Float

    public static float Abs(this float value) {
        return value < 0 ? -value : value;
    }

    #endregion

    #region IEnumerable

    public static void DestroyListElements<T>(this IEnumerable<T> list) where T : Component {
        foreach (T item in list)
            Object.Destroy(item.gameObject);
    }

    #endregion

    #region Int

    public static int Abs(this int value) {
        return value < 0 ? -value : value;
    }

    public static bool Contains(this int mask, int layer) {
        return mask == (mask | (1 << layer));
    }

    public static bool ToBool(this int value) {
        return value != 0;
    }

    #endregion

    #region List

    public static T GetLastElement<T>(this List<T> list) {
        return list[list.Count - 1];
    }

    public static void MoveIndex<T>(this List<T> list, int srcIdx, int destIdx) {
        if (srcIdx != destIdx) {
            list.Insert(destIdx, list[srcIdx]);
            list.RemoveAt(destIdx < srcIdx ? srcIdx + 1 : srcIdx);
        }
    }

    public static void RemoveCommonItems<T>(this List<T> l1, List<T> l2) {
        foreach (T item in l2)
            l1.Remove(item);
    }

    #endregion

    #region String

    public static string GetFirst(this string input) {
        return input.Substring(0, 1);
    }

    public static string ReplaceAt(this string input, int index, char newChar) {
        char[] chars = input.ToCharArray();
        chars[index] = newChar;
        return new string(chars);
    }

    #endregion

    #region T

    

    #endregion

    #region Transform

    public static void DestroyChildren(this Transform transform) {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Transform child = transform.GetChild(i);
            Object.Destroy(child.gameObject);
            child.SetParent(null);
        }
    }

    #endregion

    #region Vector2

    public static Vector2 Ceil(this Vector2 vector2) {
        return new Vector2(Mathf.CeilToInt(vector2.x), Mathf.CeilToInt(vector2.y));
    }

    public static Vector2 Floor(this Vector2 vector2) {
        return new Vector2(Mathf.FloorToInt(vector2.x), Mathf.FloorToInt(vector2.y));
    }

    public static Vector2 Round(this Vector2 vector2) {
        return new Vector2(Mathf.Round(vector2.x), Mathf.Round(vector2.y));
    }

    public static Vector2Int ToVector2Int(this Vector2 vector2) {
        return new Vector2Int(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.y));
    }

    public static Vector3 ToVector3(this Vector2 vector2) {
        return new Vector3(vector2.x, 0, vector2.y);
    }

    #endregion

    #region Vector2Int

    public static Vector2Int Abs(this Vector2Int vector2Int) {
        return new Vector2Int(vector2Int.x.Abs(), vector2Int.y.Abs());
    }

    public static Vector3 ToVector3(this Vector2Int vector2Int) {
        return new Vector3(vector2Int.x, 0, vector2Int.y);
    }

    #endregion

    #region Vector3

    public static Vector3 RemoveY(this Vector3 vector3) {
        vector3.y = 0;
        return vector3;
    }

    public static Vector3 RemoveZ(this Vector3 vector3) {
        vector3.z = 0;
        return vector3;
    }

    public static Vector3 Round(this Vector3 vector3) {
        return new Vector3(Mathf.Round(vector3.x), Mathf.Round(vector3.y), Mathf.Round(vector3.z));
    }

    public static Vector2 ToVector2(this Vector3 vector3) {
        return new Vector2(vector3.x, vector3.z);
    }

    public static Vector2Int ToVector2Int(this Vector3 vector3) {
        return new Vector2Int(Mathf.RoundToInt(vector3.x), Mathf.RoundToInt(vector3.z));
    }

    #endregion

}
