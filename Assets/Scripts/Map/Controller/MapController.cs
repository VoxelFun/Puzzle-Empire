using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Script.Map {

    public static class MapController {

        #region CanvasWorld

        public static void CreateHealthText(Data data) {
            data.healthStatus = Object.Instantiate(Info.healthStatus, Info.canvasWorld);
            data.UpdateHealthStatusPosition();
            data.UpdateHealthStatusText();
        }

        #endregion

        #region Field

        public static HashSet<Field> blockedFields;
        public static HashSet<Field> fieldsInPaths;
        public static HashSet<Field> fieldsOnBorders;

        static List<List<Field>> paths;
        static bool pathsForgoten = true;

        public static void FindAllPaths(Field start) {
            pathsForgoten = false;
            paths = new List<List<Field>>(32);

            fieldsInPaths = new HashSet<Field>();
            blockedFields = new HashSet<Field>();
            fieldsOnBorders = new HashSet<Field>();

            FindPaths(start, new List<Field>(), Engine.fieldsPerTurn);
            if(Control.localPlayer)
                SetPotentionalTargets(start, start.owner);

            SetFieldsMaterial();
        }

        static void FindPaths(Field start, List<Field> visited, int amount) {
            if (start.IsBlockade(Control.contender)) {
                blockedFields.Add(start);
                return;
            }

            visited.Add(start);
            fieldsInPaths.Add(start);
            paths.Add(visited);

            if (amount-- == 0) {
                fieldsOnBorders.Add(start);
                return;
            }

            foreach (Field.Connection connection in start.connections) {
                if(!visited.Contains(connection.field)) {
                    FindPaths(connection.field, new List<Field>(visited), amount);
                }
            }
        }

        //public static void FindShortestPath(Field start, Field end) {
        //    List<Field> neighbors = new List<Field>(32);
        //    List<float> distances = new List<float>(32);


        //}

        public static void ForgetPaths() {
            if (pathsForgoten)
                return;
            if(Control.localPlayer)
                foreach (Field field in fieldsInPaths)
                    field.RestoreMaterial();
            fieldsInPaths.Clear();
            pathsForgoten = true;
        }

        public static List<Field> GetPath(Field end, Contender contender) {
            int min, length = min = Engine.fieldsPerTurn + 2;
            List<Field> result = null;
            foreach (List<Field> path in paths)
                if (path.Last() == end) {
                    int value = 0;
                    int count = path.Count;
                    
                    foreach (Field field in path)
                        if (contender == field.owner)
                            value++;
                    if(count < length || count == length && (value < min || Random.value > 0.5f)) {
                        min = value;
                        length = count;
                        result = path;
                    }
                }

            return result;
        }

        public static bool IsFieldInPath(Field field) {
            return fieldsInPaths.Contains(field);
        }

        private static void SetFieldsMaterial() {
            if (!Control.localPlayer)
                return;
            foreach (Field field in fieldsInPaths)
                field.SetMaterial(Info.accessibleFieldMaterial);
        }

        public static bool SetPotentionalTargets(Field field, Contender contender) {
            bool result = false;
            foreach (Field.Connection connection in field.connections) {
                if (connection.field.IsBlockade(contender)) {
                    fieldsInPaths.Add(connection.field);
                    result = true;
                }
            }
            if(pathsForgoten) {
                pathsForgoten = false;
                SetFieldsMaterial();
            }
            return result;
        }

        #endregion

        #region Path

        //public static void GetPath(Field start, Field end, int limit) {

        //}

        #endregion

        #region Sprite

        public static void ArmySelectionSpriteNew(Movement movement, GameObject gameObject) {
            movement.selectionObject = Main.CreateAndAssign(gameObject, movement.transform);
        }

        #endregion

    }

}
