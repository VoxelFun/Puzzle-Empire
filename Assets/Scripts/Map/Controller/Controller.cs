using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Script.Global;

namespace Script.Map {

    public class Controller : MonoBehaviour {

        public int turn;
        List<Field> fields = new List<Field>();

        #region Board

        public void PrepareBattle(Field aggressorField, Data[] aggressors, Field defenderField, Data[] defenders) {

            global::Info.aggressor = new MapPack(aggressorField, aggressors);
            global::Info.defender = new MapPack(defenderField, defenders);

            Scene.LoadAdditive(SceneName.Board);
        }

        public void SummarizeBattle(Board.Player[] players, Board.Player looser) {

            MapPack aggressorInfo = global::Info.aggressor;
            MapPack defenderInfo = global::Info.defender;

            Contender aggressor = aggressorInfo.field.owner;
            Contender defender = defenderInfo.field.owner;

            SummarizePlayer(players[0], aggressor, defender, aggressorInfo);
            SummarizePlayer(players[1], defender, aggressor, defenderInfo);

            Scene.RestoreMap();

            if (players[1] == looser)
                Control.contender.WinBattle();
            else {
                Control.contender.EndBattle();
                foreach (Field field in fields)
                    field.InformRest();
                fields.Clear();
            }

            Control.contender.player.OnResumeTurn();

            global::Info.aggressor = global::Info.defender = null;
            if (global::Info.campaign)
                Library.mapInfo.GetCampaign().mission.OnSummarizeBattle();
        }

        void SummarizePlayer(Board.Player player, Contender contender, Contender enemy, MapPack mapPack) {
            Data[] units = mapPack.datas;
            int amountOfUnits = units != null ? units.Length : 0;
            List<Data> datas = new List<Data>();
            List<Movement> selectedUnits = new List<Movement>();

            if (amountOfUnits > 0) {
                int amountOfLeftUnits = player.building ? 0 : player.characters.Count;

                for (int i = 0; i < amountOfUnits; i++) {
                    int id = amountOfLeftUnits - amountOfUnits + i;
                    if (id < 0) {
                        contender.KillUnit(units[i], enemy);
                    }
                    else if(amountOfLeftUnits > 0) {
                        datas.Add(units[i]);
                        if (player.IsAggressor())
                            selectedUnits.Add(units[i].GetComponent<Movement>());
                    }
                }

                if (amountOfLeftUnits > 0 && amountOfUnits > amountOfLeftUnits)
                    fields.Add(mapPack.field);
            }

            if (player.IsAggressor())
                contender.selectedUnits = selectedUnits;

            if (player.currentCharacter == null)
                return;

            if (datas.Count == 0) {
                datas.Add(player.fortification);
            }

            int count = datas.Count;
            foreach (Data data in datas) {
                for (int i = 0; i < (int)Gem.White; i++) {
                    data.diamonds[i] = player.GetDiamond(i) / count;
                }
            }
            player.currentCharacter.data.SetHealth(Mathf.CeilToInt(player.health));

        }

        #endregion

        #region Contender

        public Contender GetNonRemovedContender() {
            foreach (var contender in Info.contenders) {
                if (!contender.removed)
                    return contender;
            }
            return null;
        }

        public void RemoveContender(Contender contender) {
            contender.Remove();

            if (global::Info.campaign) {
                if (contender.IsLocalPlayer())
                    Defeat();
                Library.mapInfo.GetCampaign().mission.OnRemoveContender();

                return;
            }

            int team = GetNonRemovedContender().teamId;
            if (Info.contenders.All(o => o.teamId == team))
                EndGame();
        }

        #endregion

        #region Game

        public void BeginPause() {
            Control.SetBlock(true);
            Library.ui.ShowMenu();
        }

        public void EndPause() {
            Control.SetBlock(false);
        }

        public void Load() {
            global::Info.load = true;
            Restart();
        }

        public void Restart() {
            RestartSceneInfo();
            Scene.Load(SceneName.Map);
        }

        void RestartSceneInfo() {
            Control.amountOfBlocks = 0;
            Control.block = false;
            Destroy(Tutorial.gameObject);
        }

        public void Save() {
            if (Control.contender.attackPhase) {
                Library.guiController.ShowInfo("You must end your move first");
                return;
            }
            Progress.Save();
            Library.ui.Return();
            Library.guiController.ShowInfo("Game Saved!");
        }

        public void Quit() {
            RestartSceneInfo();
            Scene.Load(SceneName.Menu);
        }

        #endregion

        #region GameOver

        public void Defeat() {
            GameOver("DEFEAT", global::Info.campaign);
        }

        void EndGame() {
            Contender winner = Control.contender;
            foreach (Contender contender in Info.contenders) {
                if(contender.teamId == winner.teamId && contender.IsLocalPlayer()) {
                    Victory();
                    return;
                }
            }
            Defeat();
        }

        void GameOver(string message, bool reload) {
            Control.SetBlock(true);
            Library.ui.ShowGameOver(message);

            Invoke(reload ? "Restart" : "Quit", 2);
        }

        public void Victory() {
            GameOver("VICTORY", global::Info.campaign && Library.general.IsNextMissionExists());
        }

        #endregion

        #region Turn

        public void BeginPlayerTurn(bool action) {
            Control.SetLocalPlayer(Control.contender.IsLocalPlayer());
            Library.ui.ShowPlayerTools();

            if (IsFirstTurn() && !global::Info.load && !global::Info.campaign)
                Control.contender.player.GetSkirmishHero();
            Control.contender.OnBeginTurn(action);

            if (action && global::Info.campaign)
                Library.mapInfo.GetCampaign().mission.OnBeginTurn();
        }

        public void EndTurn() {
            Control.contender.OnEndTurn();
            SwitchContender();
        }

        public bool IsFirstTurn() {
            return turn == 0;
        }

        public void NexTurn() {
            turn++;
        }

        public void SwitchContender() {
            Info.contenderId = (Info.contenderId + 1) % Info.contendersAmount;
            Control.contender = Info.contenders[Info.contenderId];
            if (Control.contender.removed) {
                SwitchContender();
                return;
            }
            if (Info.contenderId == 0) NexTurn();
            BeginPlayerTurn(!IsFirstTurn());
        }

        #endregion

    }

}


