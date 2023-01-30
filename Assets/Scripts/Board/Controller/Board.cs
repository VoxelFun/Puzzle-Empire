using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Script.Board {

    public class Board : MonoBehaviour {

        [Header("State")]
        public bool block;
        public int[] loot = new int[7];
        public bool match;
        public int movingGems;

        [Header("Requirement")]
        public Controller controller;
        public Transform polaMiejsce;
        public Sprite[] pola;
        public GameObject pustePole;
        public GameObject pustaInformacja;
        public Transform informacjaRodzic;

        public List<int[]> loots;
        public List<int[]> switchedGems;

        bool czyszczenie;
        Transform[,] plansza = new Transform[boardSize, boardSize];
        public int[] planszaIlosc;

        public Gem[,] planszaId = new Gem[boardSize, boardSize];

        static Board board;

        bool cleaning;
        bool firstBoardUpdate;
        
        MovingGem movingGem;

        public const int boardSize = 8;
        const int gemSize = 128;
        public const int gemsInRow = 3;
        public const float gemSpeed = gemSize * 5;
        public const float gemGravitySpeed = gemSize * 8;
        const int skullBonus = 2;

        public void Begin() {
            movingGem = new MovingGem();
            board = this;

            Prepare(true);
        }

        private void Awake() {
            gameObject.AddComponent<AnimationController>();
        }
#if UNITY_EDITOR

        private void LateUpdate() {
            if (Input.GetKeyDown(KeyCode.Space))
                Add();
            if (Input.GetKeyDown(KeyCode.Z))
                Library.Board.controller.SetHealth(Library.Board.controller.you, 20);
            if (Input.GetKeyDown(KeyCode.X))
                Library.Board.controller.opponent.KillCharacter();
        }

        void Add() {
            for (int i = 0; i < boardSize; i++) {
                for (int j = 0; j < boardSize; j++) {
                    Cos cos = plansza[i, j].GetComponent<Cos>();
                    if (!cos) cos = plansza[i, j].gameObject.AddComponent<Cos>();
                    cos.gem = planszaId[i, j];
                }
            }
            Debug.Break();

        }
#endif

        #region AnimationTrigger

        public class MovingGem : IAnimationTrigger {

            public void OnAnimationBegin() {
                board.movingGems++;
                board.Block();
            }

            public void OnAnimationEnd() {
                if (--board.movingGems == 0) {
                    board.UpdateBoard(false);
                    board.Block();
                }
            }

        }

        #endregion

        #region Board

        void Fill(List<Hit> hits, bool check) {
            int[,] removedGems = new int[boardSize, boardSize];
            HashSet<Transform> transforms = new HashSet<Transform>();
            List<Vector2Int> points = new List<Vector2Int>(5);

            foreach (Hit hit in hits) {

                loot[(int)hit.gem] += hit.GetLoot();

                if(!check)
                    foreach (Vector2Int point in hit.points) {
                        if (transforms.Add(GetGem(point))) {
                            RemoveGem(point.x, point.y);
                            points.Add(point);
                            for (int y = point.y; y < boardSize; y++)
                                removedGems[point.x, y]++;
                        }
                    }
            }

            if (check)
                return;

            for (int x = 0; x < boardSize; x++) {
                for (int y = 0; y < boardSize; y++) {
                    Transform transform = plansza[x, y];
                    if (transforms.Contains(transform) || removedGems[x, y] == 0)
                        continue;
                    int i = y - removedGems[x, y];
                    AddGem(x, i, planszaId[x, y], transform, transform.localPosition);
                }
            }

            int id = 0;
            int[] addedGems = new int[boardSize];
            foreach (Transform transform in transforms) {
                Vector2Int point = points[id++];

                int value = removedGems[point.x, boardSize - 1]--;

                AddGem(point.x, boardSize - value, Gem.Random, transform, new Vector3(point.x, boardSize + addedGems[point.x]++) * gemSize);
            }
        }

        bool GetMovementAvailability() {
            loots = new List<int[]>(16);
            switchedGems = new List<int[]>(16);

            int[] temp = loot;
            ResetLoot();

            for (int x = 0; x < boardSize; x++) {
                for (int y = 0; y < boardSize; y++) {
                    if(x + 1 < boardSize)
                        MoveArraysGems(x, y, x + 1, y);
                    if (y + 1 < boardSize)
                        MoveArraysGems(x, y, x, y + 1);
                }
            }
            loot = temp;
            return loots.Count > 0;
        }

        void PrepareUpdate() {
            ResetLoot();
            firstBoardUpdate = true;
        }

        bool UpdateBoard(bool check) {
            List<Hit> hits = new List<Hit>();
            List<Vector2Int> points = new List<Vector2Int>();

            for (int y = 0; y < boardSize; y++) {

                Hit hit = new Hit(planszaId[0, y]);
                points.Clear();
                points.Add(new Vector2Int(0, y));

                for (int x = 1; x < boardSize; x++) {
                    hit = TryGetHit(hits, hit, points, x, y);
                }
                TryToSaveHit(hit, points, hits);

            }

            for (int x = 0; x < boardSize; x++) {

                Hit hit = new Hit(planszaId[x, 0]);
                points.Clear();
                points.Add(new Vector2Int(x, 0));

                for (int y = 1; y < boardSize; y++) {
                    hit = TryGetHit(hits, hit, points, x, y);
                }
                TryToSaveHit(hit, points, hits);

            }

            bool hitted = hits.Count > 0;

            if(!check && firstBoardUpdate)
                match = hitted;

            if (hitted)
                Fill(hits, check);

            if (check)
                return hitted;

            if (!hitted) {
                bool movementAvailability = !cleaning && !match || GetMovementAvailability();

                if (!cleaning) {
                    if(match)
                        controller.OnEndOfBoardUpdate(loot);
                    controller.spellExecution = false;
                }

                if (!movementAvailability) {
                    controller.ui.CreateCommunique("No moves available");
                    Prepare(false);
                }
            }
            
            firstBoardUpdate = false;
            return hitted;
        }

        private Hit TryGetHit(List<Hit> hits, Hit hit, List<Vector2Int> points, int x, int y) {
            Gem gem = planszaId[x, y];
            Vector2Int point = new Vector2Int(x, y);

            if (gem == hit.gem) {

            }
            else if (hit.isSkull && IsSkull(gem)) {
                hit.additionalLoot += skullBonus;
            }
            else if (hit.isNature && gem == Gem.Multiplier || IsNatureGem(gem) && hit.gem == Gem.Multiplier) {
                if (hit.gem == Gem.Multiplier) hit.SetNatureGem(gem);
                hit.multiplier = 2;
            }
            else {
                TryToSaveHit(hit, points, hits);
                hit = new Hit(gem) {
                    gemsAmount = 0
                };
            }
            points.Add(point);
            hit.gemsAmount++;
            return hit;
        }

        private static void TryToSaveHit(Hit hit, List<Vector2Int> points, List<Hit> hits) {
            if (hit.gemsAmount >= gemsInRow) {
                hit.SetValues(points);
                hits.Add(hit);
            }
            points.Clear();
        }

        void Prepare(bool first) {
            planszaIlosc = new int[7];


            for (int x = 0; x < boardSize; x++)
                for (int y = 0; y < boardSize; y++) {
                    if(!first)
                        DestroyGem(x, y);
                    CreateGem(x, y, Gem.Random);
                }

            cleaning = true;
            while (UpdateBoard(false));
            cleaning = false;
        }

        #endregion

        #region Control

        public Transform activeGem;

        public void Block() {
            block = movingGems > 0;
        }

        public void SelectGem(Transform gem) {
            if (block || activeGem == gem) {
                activeGem = null;
                return;
            }

            int x1, y1, x2, y2;
            GemToPoint(activeGem, out x1, out y1);
            GemToPoint(gem, out x2, out y2);
            if (x1 == x2) {
                int y = y2 - y1;
                y = y / y.Abs();
                gem = plansza[x1, y1 + y];
            }
            else if (y1 == y2) {
                int x = x2 - x1;
                x = x / x.Abs();
                gem = plansza[x1 + x, y1];
            }
            else
                return;
            StartCoroutine(SwitchGems(new SwitchingGem(activeGem), new SwitchingGem(gem)));
        }

        #endregion

        #region Gem

        void AddGem(int x, int y, Gem gem, Transform transform, Vector3 start) {
            Vector3 target = new Vector3(x, y) * gemSize;
            if (!cleaning) {
                transform.localPosition = start;
                AnimationController.Start(new AnimationType.MoveTowards(movingGem, transform, target, gemGravitySpeed));
            }
            else
                transform.localPosition = target;
            NewGem(x, y, gem, transform);
        }

        void CreateGem(int x, int y, Gem gem) {
            var transform = Instantiate(pustePole, polaMiejsce).transform;
            SetGemPosition(transform, x, y);

            NewGem(x, y, gem, transform);
        }

        void DestroyGem(int x, int y) {
            Destroy(plansza[x, y].gameObject);
        }

        public void GemToPoint(Transform gem, out int x, out int y) {
            x = Mathf.RoundToInt(gem.localPosition.x / gemSize);
            y = Mathf.RoundToInt(gem.localPosition.y / gemSize);
        }

        Transform GetGem(Vector2Int point) {
            return plansza[point.x, point.y];
        }

        private void NewGem(int x, int y, Gem gem, Transform transform) {
            if (gem == Gem.Random) {
                int random;
                do {
                    random = Random.Range(0, (int)Gem.SkullPlus);
                } while (Random.Range(0, 28) < planszaIlosc[random]);
                planszaIlosc[random]++;
                if (random == 6 && Random.value > 0.97)
                    random = 7;

                gem = (Gem)random;
            }

            transform.GetComponent<Image>().sprite = pola[(int)gem];
            SetGem(x, y, gem, transform);
        }

        void RemoveGem(int x, int y) {
            Gem gem = planszaId[x, y];
            if (gem == Gem.Multiplier)
                return;
            int id = (int)gem;
            if (gem == Gem.SkullPlus)
                id = (int)Gem.Skull;
            planszaIlosc[id]--;
        }

        void ReplaceGem(int x, int y, Gem newGem) {
            RemoveGem(x, y);
            NewGem(x, y, newGem, plansza[x, y]);
            planszaIlosc[(int)newGem]++;
        }

        private void SetGem(int x, int y, Gem gem, Transform transform) {
            plansza[x, y] = transform;
            planszaId[x, y] = gem;
        }

        public void SwitchGem(int[] positions) {
            StartCoroutine(SwitchGems(new SwitchingGem(plansza[positions[0], positions[1]]), new SwitchingGem(plansza[positions[2], positions[3]])));
        }

        void SwitchGem(SwitchingGem firstGem, SwitchingGem secondGem, MovingGem movingGem) {
            AnimationController.Start(new AnimationType.MoveTowards(movingGem, firstGem.transform, secondGem.position, gemSpeed));
            SetGem(firstGem.x, firstGem.y, secondGem.gem, secondGem.transform);
        }

        IEnumerator SwitchGems(SwitchingGem firstGem, SwitchingGem secondGem) {
            PrepareUpdate();
            firstGem.transform.SetAsLastSibling();
            SwitchGem(firstGem, secondGem, movingGem);
            SwitchGem(secondGem, firstGem, movingGem);

            yield return new WaitWhile(() => movingGems > 0);

            if (!match) {
                SwitchGem(secondGem, secondGem, null);
                SwitchGem(firstGem, firstGem, null);
            }
        }

        class SwitchingGem {

            public Gem gem;
            public Transform transform;
            public Vector3 position;
            public int x;
            public int y;

            public SwitchingGem(Transform transform) {
                this.transform = transform;

                position = transform.localPosition;
                x = Mathf.RoundToInt(position.x / gemSize);
                y = Mathf.RoundToInt(position.y / gemSize);

                gem = board.planszaId[x, y];
            }

        }

        #endregion

        #region Gem - Array

        void MoveArraysGems(int x1, int y1, int x2, int y2) {
            SwitchArraysGems(x1, y1, x2, y2);

            if (UpdateBoard(true)) {
                loots.Add(loot);
                switchedGems.Add(new []{ x1, y1, x2, y2});
                ResetLoot();
            }

            SwitchArraysGems(x2, y2, x1, y1);
        }

        void SwitchArraysGems(int x1, int y1, int x2, int y2) {
            Gem gem = planszaId[x1, y1];
            planszaId[x1, y1] = planszaId[x2, y2];
            planszaId[x2, y2] = gem;
        }

        #endregion

        #region Spell

        public int CountGems(Gem gem) {
            int sum = 0;
            for (int x = 0; x < boardSize; x++)
                for (int y = 0; y < boardSize; y++)
                    if (planszaId[x, y] == gem)
                        sum++;

            return sum;
        }

        public bool CreateGems(Gem gem, int amount) {
            PrepareUpdate();

            int count = 0;
            do {
                int x, y;
                do {
                    x = Random.Range(0, boardSize);
                    y = Random.Range(0, boardSize);
                } while (planszaId[x, y] == gem);
                ReplaceGem(x, y, gem);
            } while (++count < amount);

            return UpdateBoard(false);
        }

        List<Hit> spellHits;

        public void DestroyGemBySpell(int x, int y){
            if (x >= boardSize || x < 0 || y >= boardSize || y < 0)
                return;
            Gem gem = planszaId[x, y];
            if (gem == Gem.Multiplier)
                return;
            spellHits.Add(new Hit(gem, x, y));
        }

        public void DestroyParticularGems(Gem gem) {
            for (int x = 0; x < boardSize; x++)
                for (int y = 0; y < boardSize; y++)
                    if (planszaId[x, y] == gem)
                        spellHits.Add(new Hit(gem, x, y));
        }

        public void PrepareBoardForSpell() {
            spellHits = new List<Hit>();
        }

        public void ReplaceGems(Gem oldGem, Gem newGem) {
            bool endTurn = controller.spellExecution;

            PrepareUpdate();
            for (int x = 0; x < boardSize; x++)
                for (int y = 0; y < boardSize; y++)
                    if (planszaId[x, y] == oldGem)
                        ReplaceGem(x, y, newGem);
            UpdateBoard(false);

            if (!match && !endTurn)
                controller.EndTurn();
        }

        public void UpdateBoardAfterSpell(){
            PrepareUpdate();
            firstBoardUpdate = false;

            if(spellHits.Count > 0)
                Fill(spellHits, false);
            match = true;
        }

        #endregion

        #region Spell - AI

        public void SelectGemByAI(int size) {
            int x = Random.Range(size, boardSize - size);
            int y = Random.Range(size, boardSize - size);
            controller.SelectGemBySpell(plansza[x, y]);
        }

        #endregion

        #region Other

        void SetGemPosition(Transform gem, int x, int y) {
            gem.localPosition = new Vector3(x * gemSize, y * gemSize);
        }

        static bool IsNatureGem(Gem gem) {
            return gem < Gem.White;
        }

        static bool IsSkull(Gem gem) {
            return gem == Gem.Skull || gem == Gem.SkullPlus;
        }

        public void ResetLoot() {
            loot = new int[7];
        }

        #endregion

        class Hit {

            public Gem gem;
            public bool isNature;
            public bool isSkull;
            public List<Vector2Int> points;

            public int multiplier = 1;
            public int additionalLoot;
            public int gemsAmount = 1;

            public Hit(Gem gem) {
                this.gem = gem;
                isNature = IsNatureGem(gem);

                if (gem == Gem.SkullPlus) {
                    additionalLoot += skullBonus;
                    this.gem = Gem.Skull;
                    isSkull = true;
                }
                else
                    isSkull = gem == Gem.Skull;
            }

            public Hit(Gem gem, int x, int y) : this(gem){
                points = new List<Vector2Int> { new Vector2Int(x, y) };
            }

            public int GetLoot() {
                return GetAmount() * multiplier + additionalLoot;
            }

            private int GetAmount() {
                switch (gemsAmount) {
                    case 5:
                        return 8;
                    case 4:
                        return 5;
                    default:
                        return 3;
                }
            }

            public void SetNatureGem(Gem gem) {
                this.gem = gem;
                isNature = true;
            }

            public void SetValues(List<Vector2Int> points) {
                this.points = new List<Vector2Int>(points);
            }

        }

    }

}