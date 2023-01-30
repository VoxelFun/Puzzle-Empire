using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Script.Map {

    public class Computer : Player {

        [Header("State")]
        public List<Field> factoryFields = new List<Field>();
        public List<Field> occupiedFields;
        public HashSet<Field> targetFields = new HashSet<Field>();

        int amountOfSelectedUnits;
        int cheapestUnit;
        int occupiedFieldId;
        Field preparationToAttack;
        bool recruitUnitsFromField;

        public static readonly WaitWhile wait = new WaitWhile(() => Control.block);

        public override void OnBegin() {
            cheapestUnit = contender.race.units[0].cost;
            targetFields = new HashSet<Field>(Library.mapInfo.villages.Concat(Library.mapInfo.goldMines).Select(o => o.field));
        }

        public IEnumerator Think() {
            yield return new WaitForSeconds(0.1f);  //Delay required for turn resume
            yield return wait;

            do {
                int count = occupiedFields.Count;
                //Debug.Log("Count: " + count);
                for (occupiedFieldId = count - 1; occupiedFieldId >= 0; occupiedFieldId--) {
                    //Debug.Log(i+"\t\t"+occupiedFields[i].GetId());
                    Move(occupiedFields[occupiedFieldId]);
                    occupiedFields.RemoveAt(occupiedFieldId);
                    yield return wait;
                    //Debug.Log(contender.attackPhase+"\t\t"+preparationToAttack);
                    if (contender.attackPhase) {
                        Move(contender.fieldOfSelectedUnits);
                        yield break;
                    }
                    if (preparationToAttack)
                        yield break;
                }
            } while (TrainArmy());

            Library.controller.EndTurn();
        }

        #region Army

        private void SelectAdditionalUnits(Field field) {
            foreach (Movement unit in field.army) {
                if(!contender.selectedUnits.Contains(unit)) {
                    contender.SelectUnit(unit);
                    amountOfSelectedUnits++;
                }
            }
            RemoveOccupiedField(field);
        }

        private void SelectArmy(Field field) {
            foreach (Movement unit in field.army) {
                contender.SelectUnit(unit);
            }
            amountOfSelectedUnits = contender.selectedUnits.Count;
        }

        private void RemoveOccupiedField(Field field) {
            occupiedFields.Remove(field);
            occupiedFieldId--;
        }

        #endregion

        #region Creating

        public override void OnCreateStructure(Structure structure) {
            if (structure.field.owner == contender) {
                if (structure.factory)
                    factoryFields.Add(structure.field);
            }
            else if (structure.field.owner.teamId != contender.teamId && structure.info.isTarget)
                targetFields.Add(structure.field);
        }

        public override void OnDestroyStructure(Structure structure) {
            if (structure.factory)
                factoryFields.Remove(structure.field);
        }

        public override void OnOccupy(Field field, Contender newOwner) {
            if(field.place.info.isTarget)
                if (newOwner.teamId != contender.teamId)
                    targetFields.Add(field);
                else
                    targetFields.Remove(field);
        }

        #endregion

        #region Hero

        public override void GetSkirmishHero() {
            Race.Information[] informations = contender.race.GetHeroes();
            Race.Information information = informations[Random.Range(0, informations.Length)];
            information.GetMapObject().Create(information, contender.GetMainBuilding().field);
        }

        #endregion

        #region Order

        private void Move(Field start) {
            if (preparationToAttack) {
                TryToRecruitUnitsFromField(start);
                contender.TryToMove(preparationToAttack);
                return;
            }

            Field field = GetConnectedFieldWithWeakestEnemyArmy(start);
            if (contender.attackPhase) {
                TryToRecruitUnitsFromField(start);
                contender.TryToMove(field);
                return;
            }

            SelectArmy(start);
            MapController.FindAllPaths(start);
            if (field) {
                if(!TryToRecruitUnitsFromNearArea(start, field))
                    contender.TryToMove(field);
                return;
            }

            field = GetTargetInMovementRange(start);
            if (field) {
                contender.TryToMove(field);
                return;
            }

            Field connectedField;
            field = GetBlockedFieldInMovementRange(out connectedField);
            if (field) {
                preparationToAttack = field;
                contender.TryToMove(connectedField);
                return;
            }

            field = GetFieldNearestAnyTarget(MapController.fieldsOnBorders);
            if(!field) field = GetFieldNearestAnyTarget(MapController.fieldsInPaths);
            if (field) {
                if (IsFieldConnectedWithEnemyArmy(field, out field)) {
                    Field newField = GetMostFortificatedFieldInMovementRange(field);
                    if (newField) field = newField;
                }
                contender.TryToMove(field);
            }
        }

        private void TryToRecruitUnitsFromField(Field start) {
            if (recruitUnitsFromField || occupiedFields.Contains(start)) {
                SelectAdditionalUnits(start);
                recruitUnitsFromField = false;
            }
        }

        private bool TryToRecruitUnitsFromNearArea(Field start, Field target) {
            List<Field> fields = new List<Field>();
            foreach (Field field in occupiedFields) {
                if (field != start && MapController.IsFieldInPath(field) && IsEnoughSpace(field))
                    fields.Add(field);
            }
            int count = fields.Count;
            if (count == 0)
                return false;
            //List<Movement> selectedUnits = new List<Movement>(start.army);

            Field occupiedField = fields[Random.Range(0, count)];
            MapController.FindAllPaths(occupiedField);
            SelectArmy(occupiedField);
            preparationToAttack = target;
            recruitUnitsFromField = true;
            //foreach (var unit in selectedUnits) {
            //    contender.DeselectUnit(unit);
            //}
            RemoveOccupiedField(occupiedField);

            //Debug.Log(contender.selectedUnits.Count);
            contender.TryToMove(start);
            //foreach (var unit in selectedUnits) {
            //    contender.SelectUnit(unit);
            //}
            return true;
        }

        private Field GetBlockedFieldInMovementRange(out Field connected) {
            Field result = connected = null;
            bool resultPriority = false;
            int min = int.MaxValue;
            foreach (Field field in MapController.blockedFields) {
                Field connectedField = GetMostFortificatedFieldInMovementRange(field);
                if (!connectedField)
                    continue;
                bool priority = targetFields.Contains(field);
                if (resultPriority && !priority)
                    continue;
                int value = int.MaxValue;
                if (resultPriority == priority) {
                    value = field.GetArmyValue();
                    if (value > min)
                        continue;
                }

                result = field;
                connected = connectedField;
                resultPriority = priority;
                min = value;
            }
            return result;
        }

        private Field GetConnectedFieldWithWeakestEnemyArmy(Field field) {
            Field result = null;
            int min = int.MaxValue;
            foreach (Field.Connection connection in field.connections) {
                if (connection.field.IsBlockade(contender)) {
                    int value = connection.field.GetArmyValue();
                    if(value < min) {
                        min = value;
                        result = connection.field;
                    }
                }
            }
            return result;
        }

        private Field GetFieldNearestAnyTarget(HashSet<Field> fields) {
            Field result = null;
            float min = float.MaxValue;
            foreach (Field field in fields) {
                if (IsNotEnoughSpace(field))
                    continue;
                foreach (Field target in targetFields) {
                    if (IsNotEnoughSpace(target))
                        continue;
                    float distance = Vector2Int.Distance(field.position, target.position);
                    if (distance < min) {
                        min = distance;
                        result = field;
                    }
                }
            }

            return result;
        }

        private Field GetMostFortificatedFieldInMovementRange(Field field) {
            Place.Defence maxDefence = Place.Defence.None;
            Field result = null;
            foreach (Field.Connection connection in field.connections) {
                if (IsNotEnoughSpace(connection.field) || !MapController.IsFieldInPath(connection.field))
                    continue;
                Place.Defence defence = connection.field.GetDefence();
                if (defence >= maxDefence) {
                    maxDefence = defence;
                    result = connection.field;
                }
            }
            return result;
        }

        private Field GetTargetInMovementRange(Field start) {
            Field result = null;
            foreach (Field field in targetFields) {
                if (IsNotEnoughSpace(field))
                    continue;
                if ((!result || Random.value > 0.5f) && MapController.IsFieldInPath(field))
                    result = field;
            }
            return result;
        }

        private bool IsFieldConnectedWithEnemyArmy(Field field, out Field result) {
            result = field;
            foreach (Field.Connection connection in field.connections) {
                if (connection.field.IsBlockade(contender)) {
                    result = connection.field;
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Turn

        public override void OnBeginTurn() {
            occupiedFields = new List<Field>();
            foreach (Movement unit in contender.army)
                if(!occupiedFields.Contains(unit.field))
                    occupiedFields.Add(unit.field);

            StartCoroutine(Think());
        }

        public override void OnResumeTurn() {
            preparationToAttack = null;
            StartCoroutine(Think());
        }

        #endregion

        #region Training

        private bool TrainArmy() {
            bool result = false;
            Field field = factoryFields[Random.Range(0, factoryFields.Count)];
            do {
                if (!field.IsAnyArmySlotEmpty(false))
                    break;
                if (!TrainUnit(field))
                    break;
                result = true;
            } while (true);

            if (result) {
                occupiedFields.Add(field);
                return true;
            }
            return false;
        }

        private bool TrainUnit(Field field) {
            int minValue = cheapestUnit + (int)((contender.income - Info.startIncome) * (Random.value * 0.6f + 0.2f));
            if (minValue > contender.gold)
                return false;

            int maxValue = contender.gold;

            const int value = 4;
            int amount = contender.army.Count;
            if(amount < value){
                int diffrence = maxValue - minValue;
                maxValue = minValue + diffrence * (amount + 2) / value;
            }

            List<Race.Information> units = GetUnitWithCost(minValue, maxValue);
            int count = units.Count;
            if (count == 0)
                return false;

            int random = Random.Range(0, count);
            Race.Information unit = units[random];
            field.CreateUnit(unit, contender);
            contender.Pay(unit.cost);

            return true;
        }

        #endregion

        #region Other

        private List<Race.Information> GetUnitWithCost(int min, int max) {
            List<Race.Information> informations = new List<Race.Information>(4);
            foreach (Race.Information information in contender.race.units) {
                if (information.cost >= min && information.cost <= max)
                    informations.Add(information);
            }
            return informations;
        }

        private bool IsEnoughSpace(Field field) {
            return field.IsEnoughSpace(amountOfSelectedUnits, contender.fieldOfSelectedUnits.Hero());
        }

        private bool IsNotEnoughSpace(Field field) {
            return !field.IsEnoughSpace(amountOfSelectedUnits, contender.fieldOfSelectedUnits.Hero());
        }

        #endregion

    }

}
