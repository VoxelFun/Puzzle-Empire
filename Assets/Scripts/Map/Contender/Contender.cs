using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Script.Map {

    public class Contender : MonoBehaviour {

        [Header("State")]
        [Header("Army")]
        public List<Movement> army;
        public List<Hero> heroes;
        public bool attackPhase;
        public List<Movement> lastUsedArmy;
        public List<Movement> selectedUnits;

        [Header("Gold")]
        public int gold;
        public int income;
        public bool permanentIncome;

        [Header("Other")]
        public ContenderColor color;
        public List<Field> path;
        public List<Place> places;
        public Race race;
        public ContenderRace raceName;
        public bool removed;
        public List<Structure> structures;
        public int teamId;

        public Global.Player info;

        [System.NonSerialized] public Player player;
        [System.NonSerialized] public Field fieldOfSelectedUnits;

        int amountOfSentUnit;
        Field lastUsedField;

        Data[] aggressors;
        Data[] defenders;
        MoveType moveType;

        public void Begin() {
            player = GetComponent<Player>();
            player.contender = this;
            player.OnBegin();
        }

        #region Army

        public void AddToArmy(Movement unit) {
            army.Add(unit);
            player.OnCreateUnit(unit);
        }

        public void KillUnit(Data data, Contender culprit) {
            culprit.SetExp(data);
            DestroyUnit(data);
        }

        public Data[] PrepareArmy(Field field) {
            Data[] datas = null;
            int count = field.amountOfUnits;
            if (count > 0) {
                datas = new Data[count];
                for (int i = 0; i < count; i++)
                    datas[i] = field.army[i].data;
            }

            return datas;
        }
        
        public void DestroyUnit(Data data) {
            Movement unit = data.GetComponent<Movement>();
            data.DestroyHealthStatus();

            army.Remove(unit);
            unit.field.RemoveUnit(unit);

            player.OnDestroyUnit(unit);
            Destroy(data.gameObject);
        }

        #endregion

        #region Battle

        public void EndBattle() {
            foreach (Movement unit in selectedUnits) {
                //unit.EndMove();
                unit.EndTurn();
            }
            DeselectAllUnits();
        }

        public void WinBattle() {
            Field target = global::Info.defender.field;
            path = new List<Field> { target };
            moveType = MoveType.WinBattle;
            fieldOfSelectedUnits = global::Info.aggressor.field;

            Move(target);
        }

        #endregion

        #region Color

        public Material GetColorMaterial() {
            return Library.general.contendersMaterials[(int)color];
        }

        #endregion

        #region Hero

        [System.Serializable]
        public class Hero {
            public Race.Hero info;
            public Movement unit;
            public int exp;
            public int level;



            public Hero(Race.Information information, int exp) {
                info = (Race.Hero)information;
                this.exp = exp;
            }

            public Hero(Movement unit, Race.Information information) {
                this.unit = unit;
                info = (Race.Hero)information;
            }

            public int GetLevel() {
                int i, exp = this.exp;
                float requirement = Engine.requiredExpForLevel;
                for (i = 0; i < Engine.maxHeroLevel && exp >= requirement; i++, exp -= (int)requirement, requirement *= Engine.expRequirementMultiplier);

                return i;
            }

            public bool HasMaxLevel() {
                return level + 1 == Engine.maxHeroLevel;
            }

            public bool IsDead() {
                return !unit;
            }

            public void Revive(Movement unit) {
                this.unit = unit;
                level = 0;
            }

            public void SetExp(int value) {
                exp += value;
                int level = global::Info.campaign ? global::Info.mapId / 2 : GetLevel();
                if (level == this.level)
                    return;
                unit.data.SetLevel(level, level - this.level);
                this.level = level;
            }

        }

        public void AddHero(Movement unit, Race.Information information) {
            int id, amount = GetAmountOfHeroes();
            Hero hero = GetHero(information, out id, amount);

            if (hero == null) {
                hero = new Hero(unit, information);
                heroes.Add(hero);
            }
            else
                hero.Revive(unit);

            unit.data.hero = hero;
            
            army.Remove(unit);
            army.Insert(id, unit);

            hero.SetExp(0);
        }

        public List<Hero> GetAliveHeroes() {
            List<Hero> informations = new List<Hero>();
            foreach (Hero hero in heroes)
                if (!hero.IsDead())
                    informations.Add(hero);
            return informations;
        }

        public int GetAmountOfDeadHeroes() {
            int result = 0;
            foreach (Hero hero in heroes)
                result += hero.IsDead().ToInt();
            return result;
        }

        public int GetAmountOfHeroes() {
            return heroes.Count;
        }

        public List<Race.Information> GetDeadHeroes() {
            List<Race.Information> informations = new List<Race.Information>();
            foreach (Hero hero in heroes)
                if (hero.IsDead()) {
                    hero.info.SetCost(hero.level);
                    informations.Add(hero.info);
                }

            return informations;
        }

        public Hero GetHero(Race.Information information, out int id, int amount) {
            for (id = 0; id < amount; id++) {
                Hero hero = heroes[id];
                if (hero.info == information)
                    return hero;
            }
            return null;
        }

        public bool HaveAnyHero() {
            return heroes.Count > 0;
        }

        public bool IsHeroSelected() {
            return fieldOfSelectedUnits.Hero();
        }

        public void SetExp(Data victim) {
            List<Hero> heroes = GetAliveHeroes();
            int amount = heroes.Count;
            if (amount == 0)
                return;

            int exp = victim.GetValue() / amount;
            foreach (Hero hero in heroes)
                hero.SetExp(exp);
        }

        #endregion

        #region MapController

        public void FindAllPaths(Field start) {
            if (Control.localPlayer)
                MapController.FindAllPaths(start);
        }

        public void ForgetPaths() {
            if (Control.localPlayer)
                MapController.ForgetPaths();
        }

        #endregion

        #region Order

        public void Attack(Field aggressor, Field defender) {
            lastUsedArmy.RemoveCommonItems(selectedUnits);

            DeselectAllUnits();
            Library.controller.PrepareBattle(aggressor, aggressors, defender, defenders);
        }

        public void EndMove(Movement unit) {
            if (--amountOfSentUnit > 0)
                return;
            Control.SetBlock(false);
            Library.ui.ShowGameLayer();
            Library.control.StopFollowingUnit();
            if(moveType != MoveType.WinBattle) {
                if (MapController.SetPotentionalTargets(unit.field, this))
                    PrepareToAttack();
                else
                    DeselectAllUnits();
            }
            else
                DeselectAllUnits();
        }

        public void EndTurnToLastUsedArmy() {
            attackPhase = false;
            foreach (Movement unit in lastUsedArmy)
                unit.EndTurn();
            lastUsedArmy.Clear();
        }

        public bool TryToAttack(Field field) {
            if (field.IsBlockade(this)) {
                defenders = PrepareArmy(field);
                aggressors = selectedUnits.Select(o => o.data).OrderBy(o => o.range).ThenByDescending(o => o.health).ToArray();
                if (defenders != null)
                    defenders = defenders.OrderBy(o => o.range).ThenByDescending(o => o.health).ToArray();
                return true;
            }
            return false;
        }

        public void TryToMoveControl(Field field) {
            if (fieldOfSelectedUnits == field || !MapController.IsFieldInPath(field)) {
                if (attackPhase)
                    DeselectAllUnits();
                else
                    player.ShowCommunique("Field isn't available");
                return;
            }
            TryToMove(field);
        }

        public void TryToMove(Field field) {


            moveType = MoveType.Map;
            SetLastUsedField(field);
            if (TryToAttack(field)) {
                Attack(fieldOfSelectedUnits, field);
                TryToMoveEnd(field);
                return;
            }

            if (!field.IsEnoughSpace(selectedUnits.Count, IsHeroSelected())) {
                player.ShowCommunique("Not enough space");
                return;
            }

            path = MapController.GetPath(field, this);
            path.RemoveAt(0);

            Move(field);
        }

        private void Move(Field field) {
            lastUsedArmy = new List<Movement>(selectedUnits);

            amountOfSentUnit = selectedUnits.Count;
            foreach (Movement unit in selectedUnits)
                unit.BeginMapMove(moveType);

            fieldOfSelectedUnits.InformRest();
            TryToMoveEnd(field);

            Control.SetBlock(true);
            Library.ui.HideGameLayer();
            Library.control.StartFollowingUnit(selectedUnits[0]);
        }

        private void PrepareToAttack() {
            attackPhase = true;
        }

        private void TryToMoveEnd(Field field) {
            ForgetPaths();
            fieldOfSelectedUnits = field;
        }

        #endregion

        #region Payment

        public bool Pay(int price) {
            if(price > gold) {
                player.ShowCommunique("Not enough gold");
                return false;
            }
            gold -= price;
            Library.ui.UpdateGold(gold);
            return true;
        }

        public void SetIncome(int value) {
            if(!permanentIncome)
                income += value;
        }

        public void SetPermanentIncome(int value) {
            income = value;
            permanentIncome = true;
        }

        #endregion

        #region SelectingUnits

        void DeselectAllUnits() {
            int count = selectedUnits.Count;
            if (count == 0)
                return;
            for (int i = selectedUnits.Count - 1; i >= 0; i--) {
                DeselectUnit(selectedUnits[i]);
            }
            EndTurnToLastUsedArmy();
            ForgetPaths();
        }

        public void DeselectUnit(Movement unit) {
            selectedUnits.Remove(unit);
            player.DeselectUnit(unit);
        }

        Movement GetSelectedUnit() {
            return selectedUnits[0];
        }

        public bool IsAnyUnitSelected() {
            return selectedUnits.Count > 0;
        }

        bool IsUnitSelected(Movement unit) {
            return selectedUnits.Contains(unit);
        }

        public void SelectUnit(Movement unit) {
            if (unit.endedTurn)
                return;
            if (IsUnitSelected(unit)) {
                DeselectUnit(unit);
                if(!IsAnyUnitSelected()) {
                    EndTurnToLastUsedArmy();
                    ForgetPaths();
                }

                return;
            }

            if (!IsAnyUnitSelected()) {
                fieldOfSelectedUnits = unit.field;
                FindAllPaths(unit.field);
            }
            else if (fieldOfSelectedUnits != unit.field) {
                DeselectAllUnits();
                fieldOfSelectedUnits = unit.field;

                FindAllPaths(unit.field);
            }

            selectedUnits.Add(unit);
            player.SelectUnit(unit);
        }

        #endregion

        #region Structure

        public void AddStructure(Structure structure) {
            structures.Add(structure);
            foreach (Contender contender in Info.contenders)
                contender.player.OnCreateStructure(structure);
        }

        public void RemoveStructure(Structure structure) {
            structures.Remove(structure);
            player.OnDestroyStructure(structure);
        }

        #endregion

        #region Turn

        public void OnBeginTurn(bool action) {
            FocusCamera();

            if (action) {
                if(!permanentIncome)
                    income = 0;
                foreach (Structure structure in structures)
                    structure.OnTurnBegin();
                foreach (Place place in places)
                    place.OnTurnBegin();

                gold += income;
            }
            Library.ui.UpdateGold(gold);
            
            player.OnBeginTurn();
        }

        public void OnEndTurn() {
            DeselectAllUnits();

            foreach (Movement unit in army)
                unit.ResetMove();

            player.OnEndTurn();
        }

        #endregion

        #region Other

        public void FocusCamera() {
            Library.control.FocusCameraOnField(lastUsedField);
        }

        public Structure GetMainBuilding() {
            return structures[0];
        }

        bool IsFieldsConnected(Field field) {
            if (!fieldOfSelectedUnits.IsConnected(field)) {
                player.ShowCommunique("Fields are not connected");
                return false;
            }
            return true;
        }

        public bool IsLocalPlayer() {
            return info.type == PlayerType.Local;
        }

        public void Remove() {
            removed = true;
        }

        public void SetLastUsedField(Field field) {
            lastUsedField = field;
        }

        #endregion

    }

}
