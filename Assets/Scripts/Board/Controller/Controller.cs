using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Board {

    public class Controller : MonoBehaviour {

        [Header("State")]
        public Player[] players;
        public int leftMoves;

        [Header("Requirement")]
        public Board board;
        public Control control;
        public UI ui;

        [System.NonSerialized] public Player you;
        [System.NonSerialized] public Player opponent;
        [System.NonSerialized] public bool spellExecution;
        [System.NonSerialized] public Spell.Spell gemSelectingSpell;

        int playerId;
        int reservedMoves;

        Player choosenPlayerSpells;
        Transform choosenSpellParent;


        public const int diamondsAmount = (int)Gem.White;
        public const int priorityMultiplier = 1000;

        public void Begin() {
#if UNITY_EDITOR
            if (Scene.current == SceneName.Editor)
                players = new Player[] {
                    Info.players[0].CreateBoardPlayer("LeftPlayer"),
                    Info.players[1].CreateBoardPlayer("RightPlayer")
                };
            else
#endif
            players = new Player[] {
                Info.aggressor.field.owner.info.CreateBoardPlayer("LeftPlayer"),
                Info.defender.field.owner.info.CreateBoardPlayer("RightPlayer")
            };

            players[0].Begin(Info.aggressor, ui.playerStatuses[0], 0);
            players[1].Begin(Info.defender, ui.playerStatuses[1], 1);

            for (int i = 0; i < 2; i++) {
                players[i].CheckSpellsAvailability();
                ui.UpdateDiamondsAmount(players[i]);
            }

            SetBeginningLeftMoves();

            SetSuitablePlayer();
        }

        private void Update() {
            ui.UpdateRotationOfTurnAffiliation();

#if UNITY_EDITOR
            //if (Input.GetKeyDown(KeyCode.Space))
            //    OnEndOfBoardUpdate(new int[] { 0, 0, 0, 0, 0, 0, 10 });
            //if (Input.GetKeyDown(KeyCode.Return))
            //    OnEndOfBoardUpdate(new int[] { 0, 0, 0, 0, 0, 0, 0 });
            if (Input.GetKeyDown(KeyCode.Q))
                EndTurn();
#endif
        }

        #region AnimationTrigger

        class MovingSpell : IAnimationTrigger {

            Player player;

            public MovingSpell(Player player) {
                this.player = player;
            }

            public void OnAnimationBegin() {
                Library.Board.controller.ui.EnableSpells(player, false);
            }

            public void OnAnimationEnd() {
                Library.Board.controller.ui.EnableSpells(player, true);
            }

        }

        #endregion

        #region Board

        public void ConvertWhiteGemsIntoRandomOnes(int[] loot) {
            int i, sum = 0;
            float random = Random.value * priorityMultiplier;
            for (i = 0; i < diamondsAmount; i++) {
                sum += you.spellPriorities[i];
                if (random < sum)
                    break;
            }

            loot[i] += loot[(int)Gem.White];
        }

        public void OnEndOfBoardUpdate(int[] loot) {
            ConvertWhiteGemsIntoRandomOnes(loot);
            AddDiamonds(you, loot);
            AttackOpponent(loot[(int)Gem.Skull] * you.damage);

            if (!spellExecution)
                EndTurn();
            else
                you.OnEndTurn();
        }

        #endregion

        #region Effect

        public void AddEffect(Player player, Effect effect) {
            if (effect.name == EffectName.Stun || effect.name == EffectName.Silent)
                RemoveEffect(player, EffectName.Charging);

            if (player.effects.ContainsKey(effect.name)) {
                if (effect.spell.onlyOne) {
                    player.effects[effect.name][0].Copy(effect);
                    return;
                }
                player.effects[effect.name].Add(effect);
            }
            else
                player.effects.Add(effect.name, new List<Effect> { effect });
            ui.CreateEffect(player, effect);
        }

        public void TryToAddEffect(Player player, Effect effect) {
            if (player.removingEffects)
                player.effectsToAdd.Add(effect);
            else
                AddEffect(player, effect);
        }

        public void RemoveEffect(Player player, Effect effect) {
            effect.OnRemove(true);
            player.effects[effect.name].Remove(effect);
            if (player.effects[effect.name].Count == 0)
                player.effects.Remove(effect.name);
        }

        public void RemoveEffect(Player player, EffectName name) {
            if (!player.effects.ContainsKey(name))
                return;
            foreach (Effect effect in player.effects[name])
                effect.OnRemove(true);
            player.effects.Remove(name);
        }

        public void ShowEffectInfo(EffectInfo effectInfo){
            Library.guiController.ShowInfo(effectInfo.effect.spell.ToText(effectInfo.effect));
        }

        #endregion

        #region Game

        public void EndGame(Player looser) {
            you.RemoveTemporaryCharacters();
            opponent.RemoveTemporaryCharacters();

            AnimationController.Stop();
            Library.controller.SummarizeBattle(players, looser);
        }

        #endregion

        #region Player

        void AddDiamond(Player player, int id, float value) {
            player.diamonds[id] += value;
            ui.UpdateDiamondAmount(player, id, value);
        }

        void AddDiamond(Player player, Gem gem, float value) {
            player.diamonds[(int)gem] += value;
            ui.UpdateDiamondAmount(player, (int)gem);
        }

        public void AddDiamondBySpell(Player player, int id, float value) {
            AddDiamond(player, id, value);
            player.CheckSpellsAvailability();
        }

        void AddDiamonds(Player player, int[] loot) {
            for (int i = 0; i < diamondsAmount; i++) {
                float value = loot[i] * player.diamondsMultiplier;
                if(value != 0)
                    AddDiamond(player, i, value);
            }
            player.CheckSpellsAvailability();
        }

        void AttackOpponent(float value) {
            if (value == 0)
                return;

            if (you.HasEffect(EffectName.AutoAttackBuff)) {
                value += opponent.spellDamage;
                ResetSpellDamage();
            }

            if (opponent.HasEffect(EffectName.BlockOfAutoAttack) || you.HasEffect(EffectName.Blind))
                return;

            value *= opponent.armor;
            SetHealth(opponent, value);
        }

        public Player GetOpponet(Player player) {
            return player.playerId == you.playerId ? opponent : you;
        }

        public int RemoveDiamond(Player player, Gem gem, int value) {
            int diffrence = player.GetDiamond((int)gem) - value;
            if (diffrence < 0)
                value = value + diffrence;
            AddDiamondBySpell(player, (int)gem, -value);

            return diffrence;
        }

        public void SetHealth(Player player, float value) {
            if (value == 0)
                return;

            value *= player.takenDamageReductor;

            if (player.HasEffect(EffectName.Shield)) {
                Effect effect = player.effects[EffectName.Shield][0];
                value = (effect.effect as Spell.Spell.Shield).AbsorbDamage(value);
                if (value > 0)
                    RemoveEffect(player, effect);
                else
                    return;
            }

            player.health -= value;

            if (player.health <= 0)
                player.KillCharacter();
            else if (player.health > player.maxHealth)
                player.health = player.maxHealth;

            ui.UpdateHealth(player, -value);
        }

        #endregion

        #region Spell

        void HideSpells(Transform spellParent) {
            MoveSpells(spellParent, 0);
        }

        void HideSpellsAuto() {
            HideSpells(choosenSpellParent);
            choosenSpellParent = null;
        }

        public bool IsSpellUsed(Spell.Spell spell) {
            return gemSelectingSpell == spell;
        }

        void MoveSpells(Transform spellParent, int multiplier) {
            Player player = players[spellParent.GetSiblingIndex()];
            Transform transform = player.status.spellParent;
            AnimationController.Start(new AnimationType.MoveTowards(new MovingSpell(player), transform, new Vector3(0, -transform.childCount * 96 * multiplier), Board.gemSpeed));
        }

        void PayForSpell(Spell.Spell spell) {
            foreach (Gem gem in spell.cost.Keys)
                AddDiamond(you, gem, -spell.cost[gem]);
            you.CheckSpellsAvailability();
        }

        public void ShowSpellInfo(Transform transform) {
            int id = transform.GetSiblingIndex();
            Library.guiController.ShowInfo(choosenPlayerSpells.spells[id].ToText(null));
        }

        void ShowSpells(Transform spellParent) {
            choosenPlayerSpells = players[spellParent.GetSiblingIndex()];
            choosenSpellParent = spellParent;
            MoveSpells(spellParent, 1);
        }

        public void TryToShowSpells(Transform spellParent) {
            if(choosenSpellParent == null)
                ShowSpells(spellParent);
            else {
                HideSpells(choosenSpellParent);
                if(choosenSpellParent == spellParent)
                    choosenSpellParent = null;
                else
                    ShowSpells(spellParent);
            }
        }

        public void TryToUseSpell(Transform transform) {
            if (choosenPlayerSpells == opponent)
                return;
            int id = transform.GetSiblingIndex();
            Spell.Spell spell = choosenPlayerSpells.spells[id];
            if (!choosenPlayerSpells.spellAvailability[id] || !spell.IsAvailable())
                return;
            HideSpellsAuto();

            UseSpell(spell);
        }

        public void UseSpell(Spell.Spell spell) {
            PayForSpell(spell);
            spell.Use();
            UseSpellDamage();

            if (gemSelectingSpell == null || you is AI)
                ui.CreateCommunique(spell.GetName());

            if (spell.endTurn && !spell.board)
                EndTurn();
            else if (!spell.board)
                you.OnEndTurn();
            else if (you is AI)
                you.Invoke("TryThink", 0.01f);
        }

        #endregion

        #region Spell - Selecting Gem

        public void EnableSpellExecution(Spell.Spell spell) {
            spell.board = true;
            spellExecution = !spell.endTurn;
        }

        public void PrepareSelectingGemBySpell(Spell.Spell spell, int size) {
            gemSelectingSpell = spell;

            if (!Control.localPlayer)
                board.SelectGemByAI(size);
            else {
                control.SetControl(new Control.Spell());
                spell.ShowInfo("Choose gem");
            }
        }

        public void SelectGemBySpell(Transform transform) {
            board.activeGem = transform;

            gemSelectingSpell.Use();
            control.RestoreControl();
            EnableSpellExecution(gemSelectingSpell);

            gemSelectingSpell = null;
        }



        #endregion

        #region SpellDamage

        public void SpellDamage(Player player, float value) {
            player.spellDamage += value;
        }

        public void UseSpellDamage() {
            SetHealth(opponent, opponent.spellDamage);
            SetHealth(you, you.spellDamage);
            ResetSpellDamage();
        }

        private void ResetSpellDamage() {
            opponent.spellDamage = you.spellDamage = 0;
        }

        #endregion

        #region Turn

        public void EndTurn() {
            if (you && you.HasEffect(EffectName.AdditionalMove)) {
                you.RemoveEffects();
                you.OnEndTurn();
                return;
            }

            SetLeftMoves(-1);
            if(reservedMoves > 0) {
                SetReservedMoves(-1);
                you.OnEndTurn();
                return;
            }
            SetSuitablePlayer();
        }

        private void SetBeginningLeftMoves() {
            int amountOfTurns = Engine.amountOfTurns;
            float turns1 = players[0].GetTurnsFromRange();
            float turns2 = players[1].GetTurnsFromRange();

            if (turns1 != turns2) {
                if (turns2 > turns1)
                    playerId = 1;

                SetReservedMoves((int)(turns1 - turns2).Abs() - playerId);
                amountOfTurns += reservedMoves;
            }

            SetLeftMoves(amountOfTurns);
        }

        void SetReservedMoves(int value) {
            reservedMoves += value;
        }

        void SetSuitablePlayer() {
            you = players[playerId];
            playerId = (playerId + 1) % 2;
            opponent = players[playerId];

            if (you.IsStunned()) {
                you.RemoveEffects();
                SetSuitablePlayer();
                return;
            }
            you.RemoveEffects();
            if(you.HasEffect(EffectName.Charging) || you.IsStunned()) {
                SetSuitablePlayer();
                return;
            }

            ui.UpdatePositionOfTurnAffiliation(you);
            you.OnEndTurn();
            Control.localPlayer = you.IsLocalPlayer();
        }

        void SetLeftMoves(int value) {
            leftMoves += value;
            if (leftMoves <= 0)
                EndGame(null);
            ui.UpdateLeftMoves(leftMoves);
        }

        #endregion

    }

}
