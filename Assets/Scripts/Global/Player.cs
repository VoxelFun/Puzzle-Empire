using UnityEngine;

namespace Script.Global {
    public class Player {

        public PlayerType type;
        public ContenderRace race;
        public int team;
        public ContenderColor color;

        public int id;

        public Player(int i) {
            type = i == 0 ? PlayerType.Local : PlayerType.Computer;
            race = ContenderRace.Humans;
            team = i;
            color = (ContenderColor)i;

            id = i;
        }

        public Player(PlayerType type, ContenderRace race, int team, ContenderColor color, int id) {
            this.type = type;
            this.race = race;
            this.team = team;
            this.color = color;
            this.id = id;
        }

        public string GetTeamName() {
            return (team + 1).ToString();
        }

        #region Board

        public Board.Player CreateBoardPlayer(string name) {
            GameObject gameObject = new GameObject(name);
            if (type == PlayerType.Computer || type == PlayerType.Neutral)
                return gameObject.AddComponent<Board.AI>();
            return gameObject.AddComponent<Board.Player>();
        }

        #endregion

    }
}
