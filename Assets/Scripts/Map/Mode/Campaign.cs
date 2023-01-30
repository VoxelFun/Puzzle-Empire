using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {
    public abstract class Campaign : MapMode {

        [Header("Story")]
        public TextAsset storyText;

        [Header("Teams")]
        public Player[] players;

        [Header("Other")]
        public GameObject tutorialObject;

        public Mission mission;

        protected int loading;
        protected int missionId;

        private Story story;

        public override void OnBegin() {
            SetPlayers();
        }

        private void Start() {
            PrepareMissions();
        }

        public void SetPlayers() {
            int amount = this.players.Length;
            Global.Player[] players = new Global.Player[amount];
            for (int i = 0; i < amount; i++) {
                Player player = this.players[i];
                players[i] = new Global.Player(player.type, player.kingdom.race, player.team, player.kingdom.color, i);
            }
            global::Info.players = players;
        }

        #region Gold

        protected void SetGold(int id, int gold, int income) {
            SetGold(id, gold);
            SetIncome(id, income);
        }

        protected void SetGold(int id, int value) {
            if (loading != 0)
                return;
            Info.contenders[id].gold = value;
            if (id == Info.contenderId)
                Library.ui.UpdateGold(value);
        }

        protected void SetIncome(int id, int value) {
            Info.contenders[id].SetPermanentIncome(value);
        }

        #endregion

        #region Load

        public void LoadMission(int missionId) {
            this.missionId = missionId;
            loading = 1;
        }

        #endregion

        #region Story

        protected void CompleteMission() {
            if(++missionId < GetMissions().Length) {
                PrepareMission();
                return;
            }
            Progress.WinMission();

            global::Info.mapId++;
            if (Library.general.IsNextMissionExists())
                global::Info.map = Library.general.campaignMaps[global::Info.mapId].gameObject;

            Library.controller.Victory();
        }

        protected void PrepareMission() {
            mission = GetMissions()[missionId];

            bool load = loading != 0;
            if (load) loading = 0;
            else story.Begin();

            TriggerEvent(load);
        }

        protected void PrepareMissions() {
            if (tutorialObject)
                Instantiate(tutorialObject);
            OnStartCampaign();

            story = new Story(storyText);
            for (int i = 0; i < missionId + loading; i++)
                while (story.reader.Peek() >= 0 && story.reader.ReadLine() != "") ;
            
            PrepareMission();
        }

        protected abstract Mission[] GetMissions();
        protected virtual void OnStartCampaign() { }
        protected virtual void TriggerEvent(bool load) { }

        #region Missions

        public abstract class Mission {
            public abstract string ToText();

            public virtual void OnBeginTurn() { }
            public virtual void OnOccupyField(Field field) { }
            public virtual void OnRemoveContender() { }
            public virtual void OnSummarizeBattle() { }

            protected void Complete() {
                Library.mapInfo.GetCampaign().CompleteMission();
            }
        }

        public class DefeatEnemy : Mission {
            public override string ToText() {
                return "Defeat Enemy";
            }

            public override void OnRemoveContender() {
                if (Info.GetEnemyContenders(Info.contenders[0]).Count == 0)
                    Complete();
            }
        }

        public class DestroyEnemyArmy : Mission {
            public override string ToText() {
                return "Destroy all enemy's units.";
            }

            public override void OnSummarizeBattle() {
                List<Contender> contenders = Info.GetEnemyContenders(Info.contenders[0]);
                int result = contenders.Count;
                foreach (Contender contender in contenders)
                    if(contender.army.Count == 0)
                        result--;
                if (result == 0)
                    Complete();
            }
        }

        public class OccupyField : Mission {
            Field target;
            string placeName;

            public OccupyField(Field field, string placeName) {
                target = field;
                this.placeName = placeName;
            }

            public override string ToText() {
                return "Conquer " + placeName;
            }

            public override void OnOccupyField(Field field) {
                if (field == target)
                    Complete();
            }
        }

        public class Survive : Mission {
            int time;

            public Survive(int time) {
                this.time = time;
            }

            public override string ToText() {
                return "Survive for " + time + " turns";
            }

            public override void OnBeginTurn() {
                if (Library.controller.turn >= time)
                    Complete();
            }
        }

        #endregion

        #endregion

        #region Other

        public int GetMissionId() {
            return missionId;
        }

        public string GetMissionText() {
            return mission.ToText();
        }

        #endregion

        [System.Serializable]
        public class Player {
            public bool mainBuilding = true;
            public Kingdom kingdom;
            public int team;
            public PlayerType type;

            public Field[] ownedFields;
            public ArmyInfo[] units;
            public ArmyInfo[] buildings;
            public ArmyInfo[] heroes;

            [System.Serializable]
            public class ArmyInfo {
                public Field field;
                public byte id;

                public ArmyInfo(Field field, byte id) {
                    this.field = field;
                    this.id = id;
                }
            }
        }

    }
}
