using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Studio : MonoBehaviour {
#if UNITY_EDITOR
    public Transform[] unitTypes;

    IEnumerator Start() {
        string poczatek = "Assets/Models/";

        foreach (Transform unitType in unitTypes) {
            string srodek = unitType.name + "/";

            foreach (Transform child in unitType) {
                string sciezka = poczatek + srodek + "Sprites/" + child.name + ".png";
                child.gameObject.SetActive(true);

                yield return new WaitForEndOfFrame();

                ScreenCapture.CaptureScreenshot(sciezka);
                yield return new WaitForEndOfFrame();
                while (!File.Exists(sciezka))
                    yield return new WaitForEndOfFrame();
                AssetDatabase.ImportAsset(sciezka);
                TextureImporter importer;
                do {
                    yield return new WaitForEndOfFrame();
                    importer = AssetImporter.GetAtPath(sciezka) as TextureImporter;
                } while (!importer);

                importer.textureType = TextureImporterType.Sprite;
                AssetDatabase.WriteImportSettingsIfDirty(sciezka);
                child.gameObject.SetActive(false);
            }

        }

        UnityEditor.EditorApplication.isPlaying = false;
    }
#endif
}

