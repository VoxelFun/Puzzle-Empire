using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using Script.UI;

namespace Script.Menu {

    public class Controller : MonoBehaviour {

        [Header("Requirement")]
        public Transform canvas;
        public Text headerText;
        public GameObject mainWindow;
        public GameObject returnObject;

        [Header("Maps")]
        public Transform mapParent;
        public GameObject mapTemplate;

        [Header("Mission")]
        public Transform missionsParent;
        public GameObject missionObject;

        [Header("Players")]
        public GameObject[] playerObjects;

        [Header("Color")]
        public Image[] colorImages;

        Global.Player chosenPlayer;
        int playerAmount;
        Global.Player[] players;
        Map.Skirmish skirmishMap;
        Map.Skirmish[] skirmishMaps;

        void Start() {
            //PlayerPrefs.SetInt("mission", 5);
            if (Scene.IsFirstLoaded()) {
#if UNITY_IOS
	        string path = Application.persistentDataPath + "/";
	        if (!Directory.Exists(path + "All"))
		        Directory.CreateDirectory(path + "All");
	        path += "All/";

            Progress.SetFilePath(path);
#elif UNITY_ANDROID
            Progress.SetFilePath(Application.persistentDataPath + "/");
#endif
#if UNITY_EDITOR
                Progress.SetFilePath("");
#endif

                Preference.Load();
            }

            SetActivity();
            Main.SetCanvasScaling();

            for (int i = 0; i < colorImages.Length; i++) {
                Image image = colorImages[i];
                image.color = Library.general.contendersMaterials[i].color;
            }
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.Escape))
                Return();
        }

        #region EventTrigger

        public void OnAmountOfPlayersButtonClick(int amount) {
            players = new Global.Player[amount];
            playerAmount = amount;
            ShowMaps();
        }

        public void OnCampaignButtonClick() {
            if (Play(true))
                WindowShow("Campaign");
            else
                OnSelectMissionButtonClick();
        }

        public void OnColorButtonClick(int i) {
            chosenPlayer.color = (ContenderColor)i;
            ChangePlayerValue();
        }

        public void OnContinueButtonClick() {
            Info.load = true;
            StartGame();
        }

        public void OnChoosePlayerInfoClick(Transform transform) {
            chosenPlayer = players[transform.parent.GetSiblingIndex() - 1];
            WindowShow(transform.name);
        }

        public void OnMapButtonClick(Transform transform) {
            int id = transform.GetSiblingIndex() - 1;
            skirmishMap = skirmishMaps[id];
            Info.map = skirmishMap.gameObject;
            Info.mapId = id;

            ShowPlayers();
        }

        public void OnMissionButtonClick(Transform transform) {
            int id = transform.GetSiblingIndex();

            Info.map = Library.general.campaignMaps[id].gameObject;
            Info.mapId = id;
            StartGame();
        }

        public void OnNewSkirmishButtonClick() {
            WindowShow("Amount Of Players");
        }

        public void OnPlayerTypeButtonClick(int playerType) {
            chosenPlayer.type = (PlayerType)playerType;
            ChangePlayerValue();
        }

        public void OnQuitButtonClick() {
            Application.Quit();
        }

        public void OnRaceButtonClick(int race) {
            chosenPlayer.race = (ContenderRace)race;
            ChangePlayerValue();
        }

        public void OnSelectMissionButtonClick() {
            if (missionObject) {
                int amount = Mathf.Min(Progress.GetMissionId() + 1, Library.general.campaignMaps.Length);
                for (int i = 0; i < amount; i++) {
                    GameObject gameObject = Instantiate(missionObject, missionsParent);

                    string text = i + "";
                    while (text.Length < 2) text = "0" + text;
                    gameObject.GetComponentInChildren<Text>().text = text;
                }
                Destroy(missionObject);
            }

            WindowShow("Missions");
        }

        public void OnSkirmishButtonClick() {
            WindowShow(Play(false) ? "Skirmish" : "Amount Of Players");
        }

        public void OnTeamButtonClick(int team) {
            chosenPlayer.team = team;
            ChangePlayerValue();
        }


        #endregion

        #region Game

        public void StartGame() {
            Scene.Load(SceneName.Map);
        }

        #endregion

        #region Maps

        public void ShowMaps() {
            skirmishMaps = Library.general.skirmishMaps.Where(o => o.playerAmount == playerAmount).ToArray();
            int amount = skirmishMaps.Length;

            for (int i = 1; i < mapParent.childCount; i++)
                Destroy(mapParent.GetChild(i).gameObject);

            for (int i = 0; i < amount; i++) {
                GameObject gameObject = Instantiate(mapTemplate, mapParent);
                gameObject.SetActive(true);

                Text text = gameObject.GetComponentInChildren<Text>();
                text.text = skirmishMaps[i].GetName();
            }

            WindowShow("Maps");
        }

        #endregion

        #region Play

        bool Play(bool campaign) {
            Info.campaign = campaign;
            return Progress.IsAnyMapSaved();
        }

        #endregion

        #region Player

        void ChangePlayerValue() {
            UpdatePlayer(chosenPlayer);
            Return();
        }

        void ShowPlayers() {
            for (int i = 0; i < playerAmount; i++) {
                GameObject playerObject = playerObjects[i];
                playerObject.SetActive(true);

                Global.Player player = players[i] ?? new Global.Player(i);

                UpdatePlayer(player);
                players[i] = player;
            }

            for (int i = playerAmount; i < 4; i++)
                playerObjects[i].SetActive(false);

            Info.players = players;
            Map.Info.contendersAmount = playerAmount;
            WindowShow("Players");
        }

        void UpdatePlayer(Global.Player player) {
            GameObject gameObject = playerObjects[player.id];
            Text[] texts = gameObject.GetComponentsInChildren<Text>();

            texts[0].text = player.type.ToString().GetFirst();
            texts[1].text = player.race.ToString().GetFirst();
            texts[2].text = player.GetTeamName();
            texts[3].text = player.color.ToString().GetFirst();

            Tile tile = gameObject.transform.GetChild(3).GetComponent<Tile>();
            Tile.Color color = new Tile.Color() {
                image = Library.general.contendersMaterials[(int)player.color].color,
                text = tile.Text.color
            };
            tile.LastColor = color;
            tile.SetColor(color);
        }

        #endregion

        #region Special Window

        internal Window specialWindow;

        public void CreateWindow(GameObject gameObject) {
            specialWindow = Instantiate(gameObject, canvas).GetComponent<Window>();
            specialWindow.name = gameObject.name;
            specialWindow.Begin(this);
            WindowShow(new Activity(true, specialWindow.gameObject, specialWindow));
        }

        public static void DestroyWindow(Window window) {
            window.OnEnd();
            Destroy(window.gameObject);
        }

        #endregion

        #region Windows

        Stack<Activity> activities;
        Activity oldActivity;

        private GameObject GetWindow(string name) {
            return canvas.Find(name).gameObject;
        }

        public void WindowShow(GameObject window) {
            WindowShow(new Activity(true, window));
        }

        public void WindowShow(Activity activity, bool @return = false) {
            if (!@return) {
                oldActivity.Pause();
                activities.Push(oldActivity);
            }
            else
                oldActivity.Stop();

            activity.Start();
            returnObject.SetActive(activity.isReturnable);
            oldActivity = activity;

            headerText.text = activity.window.name;
        }

        void WindowShow(int id) {
            WindowShow(canvas.GetChild(id).gameObject);
        }

        void WindowShow(string name) {
            WindowShow(GetWindow(name));
        }

        public void Return() {
            int ilosc = activities.Count;
            if (ilosc == 0)
                return;
            Activity activity = activities.Pop();
            WindowShow(activity, true);
        }

        void SetActivity() {
            oldActivity = new Activity(false, mainWindow);
            activities = new Stack<Activity>();
        }

        public class Activity {
            public readonly bool isReturnable;
            public readonly GameObject window;

            readonly Window special;

            public Activity(bool isReturnable, GameObject window) {
                this.isReturnable = isReturnable;
                this.window = window;
            }

            public Activity(bool isReturnable, GameObject window, Window special) : this(isReturnable, window) {
                this.special = special;
            }

            public void Pause() {
                window.SetActive(false);
            }

            public void Stop() {
                Pause();
                if (special)
                    DestroyWindow(special);
            }

            public void Start() {
                window.SetActive(true);

            }
        }

        #endregion

    }

}
