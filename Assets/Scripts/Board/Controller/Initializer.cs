using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Script.Board {

    public class Initializer : MonoBehaviour {

        public Controller controller;

        [Header("Summoned Units")]
        public SummonedUnit abomination;
        public SummonedUnit ghoul;
        public SummonedUnit skeleton;

#if UNITY_EDITOR
        [Header("Editor Only")]
        public GameObject racesObject;

        List<Data> aggressors = new List<Data>();
        List<Data> defenders = new List<Data>();
#endif

        IEnumerator Start() {
            yield return new WaitForEndOfFrame();
#if UNITY_EDITOR
            if(Scene.current == SceneName.Editor) {
                Instantiate(racesObject);

                string unit = "Garam";
                Race race = Undead.stock;
                CreateUnit(race, "Ghul", aggressors);
                CreateUnit(race, "Bies", aggressors);
                CreateUnit(race, "WozMiesa", aggressors);

                string unit2 = "Piechur";
                CreateUnit(Human.stock, "Piechur", defenders);
                CreateUnit(Human.stock, "Strzelec", defenders);
                //CreateUnit(Human.stock, "Kanonier", defenders);

                Info.defender = new Global.MapPack(null, defenders.ToArray());
                Info.aggressor = new Global.MapPack(null, aggressors.ToArray());
            }
#endif
            Spell.Necromancy.skeleton = skeleton;
            Spell.Abomination.abomination = abomination;
            Spell.Ghoul.ghoul = ghoul;

            Library.Board.controller = controller;
            Scene.SetBoardAsActiveScene();

            controller.board.Begin();
            controller.Begin();

            Main.SetCanvasScaling();
            //controller.EndGame(null);
        }

#if UNITY_EDITOR
        void CreateUnit(Race race, string name, List<Data> datas) {
            Race.Information[] informations = race.units.Where(o => o.gameObject.name == name).ToArray();
            if (informations.Length == 0)
                informations = race.GetHeroes().Where(o => o.gameObject.name == name).ToArray();
            Race.Information information = informations[0];

            GameObject gameObject = Instantiate(information.gameObject);
            Data data = gameObject.GetComponent<Data>();
            datas.Add(data);

            data.information = information;
            gameObject.SetActive(false);
        }
#endif

    }

    [System.Serializable]
    public class SummonedUnit {
        public Data data;
        public Material[] materials;
    }

}
