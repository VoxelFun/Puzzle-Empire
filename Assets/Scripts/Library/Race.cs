using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.Spell;
using Script.Map;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class Race : MonoBehaviour {

    public Information mainBuilding;
    public GameObject scaffolding;
    public Information[] buildings;
    public Information[] units;

    private void Awake() {
        Begin();

        //foreach (Information information in buildings) {
        //    Script.Map.Structure structure = information.gameObject.GetComponent<Script.Map.Structure>();
        //    structure.information = information;
        //    if (structure is Script.Map.Fortification)
        //        (structure as Script.Map.Fortification).data.information = information;
        //}
        //foreach (Information information in units) {
        //    Script.Map.Movement movement = information.gameObject.GetComponent<Script.Map.Movement>();
        //    movement.information = movement.data.information = information;
        //}
    }

    protected virtual void Begin() { }

    public abstract Information[] GetHeroes();

    public static Race Get(ContenderRace race) {
        return race == ContenderRace.Humans ? (Race)Human.stock : Undead.stock;
    }

    public void SetId() {
        for (int i = 0; i < buildings.Length; i++) {
            buildings[i].id = i;
        }
        for (int i = 0; i < units.Length; i++) {
            units[i].id = i;
        }
    }

    [System.Serializable]
    public class Information {

        [Header("Information")]
        public string Name;
        [TextArea] public string description;
        public int cost;

        [Header("Model")]
        public GameObject gameObject;
        public Material[] materials;
        public Material greyMaterial;
        public Sprite icon;
        public Sprite sprite;

        [Header("Board")]
        public float size = 1;
        public float animationSpeed = 1;

        [Header("Other")]
        public int id;

        public Spell[] spells;

        public Data GetData() {
            return gameObject.GetComponent<Data>();
        }

        public IMapObject GetMapObject() {
            return gameObject.GetComponent<IMapObject>();
        }

        public int GetValue() {
            return cost;
        }

        public bool IsEmpty() {
            return Name == "";
        }

        public void SetValues(params Spell[] spells) {
            this.spells = spells;
        }

        public virtual bool IsHero() { return false; }
    }

    [System.Serializable]
    public class Hero : Information {

        public void SetCost(int level) {
            cost = 200 + level * 80;
        }

        public override bool IsHero() {
            return true;
        }
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(Undead))]
public class ObjectBuilderEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        Race myScript = (Race)target;
        if (GUILayout.Button("Build Object")) {
            myScript.SetId();
        }
    }
}
#endif