using System.Collections.Generic;
using UnityEngine;

namespace Script.Map {

    public static class Info {

        public static GameObject selectionObject;

        public static Contender[] contenders;
        public static int contendersAmount = 2;
        public static int contenderId;

        public static int startIncome;

        public static int movementSpeed = Engine.movementSpeed;
        internal static GameObject healthStatus;
        internal static Transform canvasWorld;
        internal static Material accessibleFieldMaterial;

        public static List<Contender> GetAllyContenders(Contender contender) {
            List<Contender> contenders = new List<Contender>();
            foreach (Contender item in Info.contenders) {
                if (contender != item && item.teamId == contender.teamId && !item.removed)
                    contenders.Add(item);
            }
            return contenders;
        }

        public static List<Contender> GetEnemyContenders(Contender contender) {
            List<Contender> contenders = new List<Contender>();
            foreach (Contender item in Info.contenders) {
                if (contender != item && item.teamId != contender.teamId && !item.removed)
                    contenders.Add(item);
            }
            return contenders;
        }

        //public static List<Field> GetEnemyMainBuildingsFields(Contender contender) {
        //    List<Field> fields = new List<Field>();
        //    foreach (Contender item in contenders) {
        //        if (item.teamId != contender.teamId)
        //            fields.Add(item.GetMainBuilding().field);
        //    }
        //    return fields;
        //}

    }

}
