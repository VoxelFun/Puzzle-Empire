using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class Main {

    #region Array

    public static void AddArraysValue(int[] a1, int[] a2) {
        int length = a1.Length;
        for (int i = 0; i < length; i++)
            a1[i] += a2[i];
    }

    public static void AddArraysValue(float[] a1, int[] a2) {
        int length = a1.Length;
        for (int i = 0; i < length; i++)
            a1[i] += a2[i];
    }

    public static void AddToArray<T>(ref T[] array, T value) {
        List<T> list = array.ToList();
        list.Add(value);
        array = list.ToArray();
    }

    public static void AddToArray<T>(ref T[] array, List<T> list) {
        list.AddRange(list);
        array = list.ToArray();
    }

    public static int[] FloatArrayToIntArray(float[] array) {
        int length = array.Length;
        int[] result = new int[length];
        for (int i = 0; i < length; i++)
            result[i] = Mathf.RoundToInt(array[i]);
        return result;
    }

    public static void RemoveFromArray<T>(ref T[] array, int id) {
        List<T> list = array.ToList();
        list.RemoveAt(id);
        array = list.ToArray();
    }

    public static void RemoveFromArray<T>(ref T[] array, T item) {
        List<T> list = array.ToList();
        list.Remove(item);
        array = list.ToArray();
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

    #region Canvas

    public static Transform CanvasChildNew(Transform parent, GameObject child) {
        Transform transform = Object.Instantiate(child, parent).transform;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        return transform;
    }

    public static Transform CanvasChildNew(Transform parent, GameObject child, Vector2 position) {
        Transform transform = CanvasChildNew(parent, child);
        transform.localPosition = position;
        return transform;
    }

    public static void SetCanvasScaling() {
        if(Camera.main.aspect <= 0.5f)
            Object.FindObjectOfType<UnityEngine.UI.CanvasScaler>().matchWidthOrHeight = 0;
    }

    public static Vector3 WorldToCanvasPosition(Vector3 position) {
        position = Camera.main.WorldToViewportPoint(position) - new Vector3(0.5f, 0.5f);
        position.Set(position.x * 1920, position.y * 1080, 0);
        return position;
    }

    #endregion

    #region Cursor

    public static void ShowCursor(bool value) {
#if !UNITY_ANDROID && !UNITY_IOS
        Cursor.visible = value;
#endif
    }

    #endregion

    #region Instantiate

    public static GameObject CreateAndAssign(GameObject child, Transform parent) {
        GameObject gameObject = Object.Instantiate(child, parent.position, child.transform.rotation, parent);
        gameObject.name = child.name;

        return gameObject;
    }

    #endregion

    #region Physics

    public static RaycastHit GetHit(int layer = ~0) {
        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layer);
        return hit;
    }

    public static Transform GetPointedObject() {
        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            return null;
        return hit.transform;
    }

    #endregion

    #region Random

    public static bool RandomBool() {
        return Random.value > 0.5f;
    }

    public static Color RandomColor() {
        return new Color(Random.value, Random.value, Random.value);
    }

    public static int RandomSign() {
        return Random.value > 0.5f ? -1 : 1;
    }

    public static Vector2 RandomNormalizedVector2() {
        float value = Random.value;
        return new Vector2(value * RandomSign(), (1f - value) * RandomSign());
    }

    public static Vector3 RandomNormalizedVector3() {
        float value = Random.value;
        return new Vector3(value * RandomSign(), 0, (1f - value) * RandomSign());
    }

    public static Quaternion RandomRotation() {
        Vector3 vector3 = new Vector3(Random.value, Random.value, Random.value) * 360;
        return Quaternion.Euler(vector3);
    }

    public static Vector3 RandomVector3(int range) {
        return new Vector3(Random.Range(-range, range + 1), 0, Random.Range(-range, range + 1));
    }

    public static Vector3 RandomVector3(int x, int y) {
        return new Vector3(Random.Range(-x, x + 1), 0, Random.Range(-y, y + 1));
    }

    public static Vector3 RandomVector3(float x, float y) {
        return new Vector3(Random.Range(-x, x), 0, Random.Range(-y, y));
    }

    public static Vector3 RandomVector3InRadius(float radius) {
        return RandomNormalizedVector3() * Random.value * radius;
    }

    #endregion

    #region Rotation

    public static float CalculateRotation(Vector2 direction) {
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
    }

    #endregion

    #region String

    public static string AddSpacesToSentence(string text) {
        System.Text.StringBuilder newText = new System.Text.StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++) {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                newText.Append(' ');
            newText.Append(text[i]);
        }
        return newText.ToString();
    }

    #endregion

    #region String - Array

    public static int[] StringArrayToIntArray(string[] array){
        int amount = array.Length;
        int[] result = new int[amount];
        for (int i = 0; i < amount; i++)
            result[i] = int.Parse(array[i]);
        return result;
    }

    #endregion

    #region Transform

    public static Transform[] GetAllChildren(Transform transform) {
        return GetChildren(transform, transform.childCount);
    }

    public static T[] GetAllChildrenWithTag<T>(Transform parent, string tag) {
        List<T> result = new List<T>();
        foreach (Transform child in parent) {
            if (child.tag == tag)
                result.Add(child.GetComponent<T>());
        }
        return result.ToArray();
    }

    public static Transform[] GetChildren(Transform transform, int amount) {
        Transform[] children = new Transform[amount];
        for (int i = 0; i < amount; i++) {
            children[i] = transform.GetChild(i);
        }
        return children;
    }

    #endregion

    #region Vector2

    public static Vector2 GetCenter(Vector2 a, Vector2 b) {
        return (a + b) * 0.5f;
    }

    public static Vector2 GetClosestPoint(Vector2[] array, Vector2 target) {
        Vector2 vector2 = Vector2.zero;
        float min = float.MaxValue;
        foreach (Vector2 item in array) {
            float distance = Vector2.Distance(item, target);
            if (distance < min) {
                min = distance;
                vector2 = item;
            }
        }
        return vector2;
    }

    public static Vector2 GetDiffrence(Vector2 start, Vector2 target) {
        return (target - start);
    }

    public static Vector2 GetDiffrenceAbs(Vector2 start, Vector2 target) {
        Vector2 vector = GetDiffrence(start, target);
        return new Vector2(vector.x.Abs(), vector.y.Abs());
    }

    #endregion

    #region Vector3

    public static Vector3 GetCenter(Vector3 a, Vector3 b) {
        return (a + b) * 0.5f;
    }

    public static Vector3 GetDiffrence(Vector3 start, Vector3 target) {
        return (target - start);
    }

    public static Vector3 GetMouseWorldPosition() {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mousePosition.z = 0;

        return mousePosition;
    }

    public static Vector3 GetNormalizedDiffrence(Vector3 a, Vector3 b) {
        return (a - b).normalized;
    }

    #endregion

    #region Other

    public static void Print(params object[] vs) {
        string message = "";
        foreach (object o in vs) {
            message += o + "\t\t";
        }
        Debug.Log(message);
    }

    #endregion

}
