using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class Scene {

    public static SceneName current = SceneName.Editor;

    static UnityEngine.SceneManagement.Scene scene;

    public static bool IsFirstLoaded() {
        return current == SceneName.Editor;
    }

    public static void Load(SceneName sceneName) {
        LoadScene(sceneName, LoadSceneMode.Single);
    }

    public static void LoadAdditive(SceneName sceneName) {
        LoadScene(sceneName, LoadSceneMode.Additive);
        scene = SceneManager.GetActiveScene();

        EnableScene(false);
    }

    private static void EnableScene(bool enable) {
        foreach (GameObject item in scene.GetRootGameObjects())
            item.SetActive(enable);
    }

    static void LoadScene(SceneName sceneName, LoadSceneMode loadSceneMode) {
        current = sceneName;
        SceneManager.LoadScene((int)sceneName, loadSceneMode);
    }

    public static void RestoreMap() {
        current = SceneName.Map;
        SceneManager.UnloadSceneAsync((int)SceneName.Board);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex((int)SceneName.Map));
        EnableScene(true);
    }

    public static void SetBoardAsActiveScene() {
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex((int)SceneName.Board));
    }

}

public enum SceneName {
    Editor = -1, Menu, Map, Board
}
