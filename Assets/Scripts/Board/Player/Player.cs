using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script.Spell;

namespace Script.Board {

    public class Player : MonoBehaviour {

        [Header("State")]
        public float armor;
        public int armorDiffrence;
        public float damage;
        public float health;
        public float maxHealth;
        public List<bool> spellAvailability = new List<bool>();
        public int[] spellPriorities = new int[7];
        public float takenDamageReductor = 1;

        public float[] diamonds = new float[Controller.diamondsAmount];
        public float diamondsMultiplier = 1;
        public List<Spell.Spell> spells = new List<Spell.Spell>();
        public UI.PlayerStatus status;
        public List<Character> characters = new List<Character>();

        [System.NonSerialized] public bool building;
        [System.NonSerialized] public int playerId;
        [System.NonSerialized] public Data fortification;
        [System.NonSerialized] public Map.Place place;
        [System.NonSerialized] public int sideMultiplier;
        [System.NonSerialized] public float spellDamage;
        [System.NonSerialized] public Character currentCharacter;

        public Dictionary<EffectName, List<Effect>> effects = new Dictionary<EffectName, List<Effect>>();
        public List<Effect> effectsToAdd = new List<Effect>();
        public bool removingEffects;
        public bool hero;

        GameObject placeObject;
        Map.Contender contender;

        public void Begin(Global.MapPack pack, UI.PlayerStatus status, int id) {
            this.status = status;
            playerId = id;
            sideMultiplier = id * 2 - 1;

            if (pack.datas != null) {
                foreach (Data data in pack.datas)
                    AddCharacter(data);
                CreatePlace(pack.field, false);
                hero = pack.level >= 0;
                if (hero)
                    diamondsMultiplier = 1f + pack.level * Engine.Hero.diamondsMultiplier; 
            }
            else {
                building = true;
                AddCharacter(CreatePlace(pack.field, true));
            }

#if UNITY_EDITOR
            if (Scene.IsFirstLoaded()) {
                int value = 10;
                //if (id == 0)
                Main.AddArraysValue(diamonds, new int[] { value, value, value, value, value });
                //Main.AddArraysValue(diamonds, new int[] { 15, 0, 0, 0, 0 });
            }
            else
#endif
                contender = pack.field.owner;
            //Library.Board.controller.ui.DestroySpellObject(this);

            UpdateValues();
            SetSpellsPriority();
            OnBegin();
        }

        #region AI

        public virtual bool IsLocalPlayer() { return true; }

        public virtual void OnBegin() { }
        public virtual void OnEndTurn() { }

        #endregion

        #region Character

        void AddCharacter(Data data) {
            Main.AddArraysValue(diamonds, data.diamonds);
            CreateCharacter(data);
            CreateSpells(data.information.spells);
        }

        public void AddTemporaryCharacter(SummonedUnit unit, Effect effect) {
            currentCharacter.data.health = Mathf.RoundToInt(health);

            CreateCharacter(unit.data);
            characters.MoveIndex(characters.Count - 1, 0);

            UpdateValues();
            SetSpellsPriority();

#if UNITY_EDITOR
            if (Scene.IsFirstLoaded())
#endif
                currentCharacter.data.renderer.sharedMaterial = unit.materials[(int)contender.color]; ;
            currentCharacter.SetAsTemporary(effect);
        }

        void CreateCharacter(Data data) {
            Transform child = data.transform.GetChild(0);
            GameObject gameObject = Instantiate(child.gameObject);
            Transform transform = gameObject.transform;

            if(!building) {
                transform.rotation = Quaternion.Euler(0, 165 + playerId * 45 + transform.eulerAngles.y, 0);

                Animation animation = gameObject.GetComponent<Animation>();
                if (animation)
                    animation.Play("stand");
            }
            //gameObject.GetComponentInChildren<Renderer>().material = data.renderer.material;
            characters.Add(new Character(data, gameObject, transform, child.localPosition.y));
        }

        public bool KillCharacter() {
            return KillCharacter(currentCharacter, false);
        }

        public bool KillCharacter(Character old, bool endOfLifeTime) {
            Destroy(currentCharacter.model);
            characters.RemoveAt(0);

            if (!endOfLifeTime && old.temporary)
                Library.Board.controller.RemoveEffect(this, old.effect);

            if (characters.Count == 0) {
                if (!IsAggressor() && fortification && place != null)
                    PrepareFortification();
                else {
                    currentCharacter = null;
                    Library.Board.controller.EndGame(this);
                    return false;
                }
            }
            KillCharacterEnd(old);
            SetSpellsPriority();
            return true;
        }

        private void KillCharacterEnd(Character old) {
            UpdateValues();
            RemoveSpells(currentCharacter, old);
        }

        public void RemoveTemporaryCharacters() {
            int i; bool temporary = false;
            for (i = characters.Count - 1; i >= 0; i--) {
                if (!characters[i].temporary)
                    break;
                temporary = true;
                characters.RemoveAt(i);
            }
            if (i < 0 || !temporary)
                return;
            currentCharacter = characters[i];
            health = currentCharacter.data.health;
            maxHealth = currentCharacter.data.maxHealth;
        }

        #endregion

        #region Effect

        public Effect GetEffect(EffectName name) {
            if (!effects.ContainsKey(name))
                return null;
            return effects[name][0];
        }

        public void RemoveEffects() {
            removingEffects = true;
            effectsToAdd.Clear();

            List<EffectName> trash = new List<EffectName>();
            foreach (EffectName name in effects.Keys) {
                List<Effect> effects = this.effects[name];
                int count = effects.Count;
                
                int sum = 0;
                for (int i = count - 1; i >= 0; i--) {
                    Effect effect = effects[i];
                    effect.TryToUse();
                    if (effect.TryToRemove()) {
                        effects.RemoveAt(i);
                        sum++;
                    }
                }

                if(count == sum) {
                    if (name == EffectName.Silent)
                        CheckSpellsAvailability(false);
                    trash.Add(name);
                }
            }
            foreach (EffectName name in trash)
                effects.Remove(name);
            foreach (Effect effect in effectsToAdd)
                Library.Board.controller.AddEffect(this, effect);

            Library.Board.controller.UseSpellDamage();
            removingEffects = false;
        }

        #endregion

        #region Effect - Checking

        public bool HasEffect(EffectName name) {
            if (!effects.ContainsKey(name))
                return false;
            if (effects[name][0].RemoveStack())
                effects.Remove(name);
            return true;
        }

        public bool IsStunned() {
            return effects.ContainsKey(EffectName.Stun);
        }

        #endregion

        #region Health

        public float LeftHealth(float percent) {
            return health * percent * 0.01f;
        }

        public float MaxHealth(float percent) {
            return maxHealth * percent * 0.01f;
        }

        public float MissingHealth(float percent){
            return (maxHealth - health) * percent * 0.01f;
        }

        public float PercentOfHP(){
            return health / maxHealth;
        }

        public float PercentOfMissingHP(){
            return 1 - PercentOfHP();
        }

        #endregion

        #region Place

        public Data CreatePlace(Map.Field field, bool onlyDefender) {
#if UNITY_EDITOR
            if (!field) return null;
#endif
            if (field.place == null || !field.place.IsMatterInBattle())
                return null;
            place = field.place;
            Data data = null;
            if(field.place is Map.Fortification)
                data = (field.place as Map.Fortification).data;

            if (!onlyDefender) {
                Renderer[] renderers = field.place.gameObject.GetComponentsInChildren<Renderer>();
                placeObject = new GameObject("Place");
                Transform transform = placeObject.transform;
                foreach (Renderer renderer in renderers)
                    Instantiate(renderer.gameObject, transform);

                transform.position = new Vector3(sideMultiplier * 9, 14.5f, 30);
                transform.localScale *= 0.8f;
            }
            else
                place = null;
            fortification = data;

            return data;
        }

        public void PrepareFortification() {
            building = true;

            AddCharacter(fortification);
            Destroy(placeObject);

            place = null;
        }

        #endregion

        #region Spell

        bool CheckSpellAvailability(Spell.Spell spell) {
            foreach (Gem gem in spell.cost.Keys)
                if (diamonds[(int)gem] < spell.cost[gem])
                    return false;
            return true;
        }

        public void CheckSpellsAvailability() {
            CheckSpellsAvailability(HasEffect(EffectName.Silent));
        }

        public void CheckSpellsAvailability(bool silent) {
            int amount = spells.Count;
            for (int i = 0; i < amount; i++) {
                bool availability = !silent && CheckSpellAvailability(spells[i]);
                if (availability == spellAvailability[i])
                    continue;

                Library.Board.controller.ui.UpdateSpellColor(this, i, availability);
                spellAvailability[i] = availability;
            }
            Library.Board.controller.ui.UpdateDiamentParent(this);
        }

        public void CreateSpells(Spell.Spell[] spells) {
            foreach (Spell.Spell spell in spells) {
                if (this.spells.Contains(spell))
                    continue;
                Library.Board.controller.ui.CreateSpell(this, spell);
                spellAvailability.Add(false);
                this.spells.Add(spell);
                spell.OnBegin();
            }
        }

        void RemoveSpells(Character current, Character old) {
            if (old.data.information == current.data.information || old.data.information == null)
                return;
            int amount = old.data.information.spells.Length;
            for (int i = 0; i < amount; i++) {
                spells.RemoveAt(0);
                spellAvailability.RemoveAt(0);
                Library.Board.controller.ui.DestroySpell(this);
            }

            if (this is AI)
                (this as AI).SetSpells();
        }

        #endregion

        #region Spell - Effect

        public void ReduceTakenDamage(int percent) {
            takenDamageReductor *= 1 - (percent * 0.01f);
        }

        public void RestoreTakenDamage() {
            takenDamageReductor = 1;
        }

        #endregion

        #region Other

        public int GetDiamond(int id) {
            return Mathf.FloorToInt(diamonds[id]);
        }

        void UpdateValues() {
            int amount = characters.Count;
            if(!hero)
                diamondsMultiplier = 1 + Engine.diamondsMultiplierPerEveryNextUnit * (amount - 1);

            Vector3[] positions = GetPositions(amount);
            Vector3 scale = Vector3.one * 0.35f;
            if (building)
                scale *= 0.8f;

            float damage = 0; float damageMultiplier = 1;
            for (int i = 0; i < amount; i++) {
                Character character = characters[i];
                float basicSize = 1;
                if(character.data.information != null) {
                    Race.Information information = character.data.information;
                    basicSize = information.size;

                    if (information.animationSpeed != 1)
                        character.model.GetComponent<Animation>()["stand"].speed = information.animationSpeed;
                }

                Vector3 position = positions[i] * 6;
                character.transform.position = new Vector3(position.x * sideMultiplier, position.y + character.height * scale.x, position.z);
                character.transform.localScale = scale * basicSize * 3;
                

                damage += character.data.damage * damageMultiplier;


                scale = Vector3.one * 0.28f;
                if(!hero)
                    damageMultiplier = Engine.damagePerEveryNextUnit;
            }

            currentCharacter = characters[0];
            health = currentCharacter.data.health;
            maxHealth = currentCharacter.data.maxHealth;

            SetArmor();
            this.damage = damage / Board.gemsInRow;
            Library.Board.controller.ui.UpdateHealth(this);
        }

        public void SetArmor() {
            float armor = (currentCharacter.data.armor + (place != null ? (int)place.info.defence : 0) + armorDiffrence) * 6;
            this.armor = 1f - (armor / (armor + 100));
        }

        public void SetArmorDiffrence(int diffrence) {
            armorDiffrence += diffrence;
            SetArmor();
        }

        private void SetSpellsPriority() {
            float[] vs = new float[Controller.diamondsAmount];
            int sum = 0;
            for (int q = 0; q < spells.Count; q++) {
                Dictionary<Gem, int> cost = spells[q].cost;
                foreach (Gem gem in cost.Keys) {
                    int value = cost[gem];
                    vs[(int)gem] += value;
                    sum += value;
                }
            }
            for (int q = 0; q < (int)Gem.White; q++)
                spellPriorities[q] = Mathf.FloorToInt(vs[q] / sum * Controller.priorityMultiplier);
            
            spellPriorities[(int)Gem.White] = (int)(0.4f * Controller.priorityMultiplier);
            spellPriorities[(int)Gem.Skull] = Controller.priorityMultiplier;

            Library.Board.controller.ui.UpdateDiamentParent(this);
        }

        Vector3[] GetPositions(int amount) {
            Vector3[] positions;
            if (amount == 1)
                positions = new Vector3[] { new Vector3(1.45f, 2.4f) };
            else if (amount == 2)
                positions = new Vector3[] { new Vector3(1.23f, 2.4f), new Vector3(1.97f, 2.6f, 2) };
            else
                positions = new Vector3[] { new Vector3(1.23f, 2.4f), new Vector3(0.75f, 2.6f, 2), new Vector3(1.97f, 2.6f, 2) };
            return positions;
        }

        public float GetTurnsFromRange() {
            int amount = characters.Count;
            float result = 0;
            foreach (Character character in characters)
                result += (int)character.data.range;

            return result / amount;
        }

        public bool IsAggressor() {
            return playerId == 0;
        }

        #endregion

        [System.Serializable]
        public class Character {
            public Data data;
            public GameObject model;
            public Transform transform;

            public float height;

            public Effect effect;
            public bool temporary;

            public Character(Data data, GameObject model, Transform transform, float height) {
                this.data = data;
                this.model = model;
                this.transform = transform;

                this.height = height;
            }

            public void SetAsTemporary(Effect effect) {
                temporary = true;
                this.effect = effect;
            }
        }

    }

}


