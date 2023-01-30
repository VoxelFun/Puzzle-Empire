using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Script.Map;
using System.IO;

public static class Progress {

	static Field[] fields;
    static StreamReader reader;
    static string filePath;

    const int amountOfDataInformation = Script.Board.Controller.diamondsAmount + 1;

    const string mission = "mission";

    #region Load

    public static int GetMissionId() {
        return PlayerPrefs.GetInt(mission);
    }

    #endregion

    #region Load - Map

    public static void LoadGlobalVariables(){
        reader = File.OpenText(GetFileName());

        LoadMap();

        int[] vs = Read();
        Script.Map.Info.contendersAmount = vs[0];
        Script.Map.Info.contenderId = vs[1];
        Library.controller.turn = vs[2];
        LoadPlayers();
    }

    public static void Load(){
        fields = Library.mapInfo.GetFields();
        LoadContenders();
        LoadMissionInfo();

        reader.Close();
        fields = null;
    }

    private static void LoadContender(Contender contender) {
        LoadFields(contender);

        int[] vs = Read();
        if (vs[0] == 1)
            contender.Remove();
        contender.gold = vs[1];

        LoadUnits(contender, LoadHeroes(contender));
        LoadStructures(contender);
    }

    private static void LoadContenders() {
        for (int i = 0; i < Script.Map.Info.contendersAmount; i++) {
            LoadContender(Script.Map.Info.contenders[i]);
        }
    }

    private static void LoadData(Data data, int[] vs) {
        data.SetHealth(vs[0]);
        for (int i = 0; i < Script.Board.Controller.diamondsAmount; i++)
            data.diamonds[i] = vs[i + 1];
    }

    private static void LoadFields(Contender contender){
        int[] vs = Read();
        int amount = vs.Length;
        for (int i = 0; i < amount; i++)
            GetField(vs[i]).TryToOccupy(contender);
    }

    private static int LoadHeroes(Contender contender) {
        int[] vs = Read();

        for (int i = 0; i < vs[1]; i++) {
            int[] temp = Read();
            contender.heroes.Add(new Contender.Hero(contender.race.GetHeroes()[temp[0]], temp[1]));
        }

        return vs[0];
    }

    private static void LoadMap(){
        int[] vs = Read();

        //Info.campaign = vs[0].ToBool();
        Info.mapId = vs[0];
    }

    private static void LoadMissionInfo() {
        if (!Info.campaign)
            return;
        int[] vs = Read();

        Library.mapInfo.GetCampaign().LoadMission(vs[0]);
    }

    private static void LoadPlayer(Script.Global.Player player){
        int[] vs = Read();
        player.color = (ContenderColor)vs[0];
        player.race = (ContenderRace)vs[1];
        player.team = vs[2];
        player.type = (PlayerType)vs[3];
    }

    private static void LoadPlayers(){
        Info.players = new Script.Global.Player[Script.Map.Info.contendersAmount];
        for (int i = 0; i < Script.Map.Info.contendersAmount; i++){
            Info.players[i] = new Script.Global.Player(i);
            LoadPlayer(Info.players[i]);
        }
    }

    private static void LoadStructures(Contender contender) {
        int amount = ReadOne();
        int amountOfStructures = contender.structures.Count;
        for (int i = 0; i < amount; i++) {
            int[] vs = Read();

            bool fortification = amountOfDataInformation < vs.Length;
            int id = fortification ? amountOfDataInformation : 0;

            Structure structure = i < amountOfStructures ? contender.structures[i] : GetField(vs[id + 1]).CreateStructure(contender.race.buildings[vs[id]], vs[id + 2]);
            if (fortification)
                LoadData((structure as Fortification).data, vs);
        }
    }

    private static void LoadUnits(Contender contender, int amountOfHeroes) {
        int amount = ReadOne();
        for (int i = 0; i < amount; i++) {
            int[] vs = Read();

            int id = amountOfDataInformation;
            bool isHero = i < amountOfHeroes;
            Race.Information[] informations = isHero ? contender.race.GetHeroes() : contender.race.units;
            Movement unit = GetField(vs[id + 1]).CreateUnit(informations[vs[id]], contender);
            if (isHero)
                unit.data.hero.SetExp(0);
            LoadData(unit.data, vs);
            if (vs[id + 2].ToBool())
                unit.EndTurn();
        }
    }

    #endregion

    #region Save

    public static void WinMission() {
        int missionId = GetMissionId();
        if (missionId > Info.mapId)
            return;
        PlayerPrefs.SetInt(mission, missionId + 1);
    }

    #endregion

    #region Save - Map

    static StringBuilder save;
    static string dataToWrite = "";

    private static void PrepareSaving() {
        save = new StringBuilder();
    }

    public static void Save(){
        PrepareSaving();
        SaveMapInfo();

        Write(Script.Map.Info.contendersAmount, Script.Map.Info.contenderId, Library.controller.turn);
        foreach (var player in Info.players)
            SavePlayer(player);
        SaveContenders();
        SaveMissionInfo();

        StreamWriter writer = File.CreateText(GetFileName());
        writer.Write(save);
        writer.Close();

        PrepareSaving();
    }

    private static void SaveContender(Contender contender) {
        Write(contender.removed.ToInt(), contender.gold);

        SaveHeroes(contender);
        SaveUnits(contender);
        SaveStructures(contender);
    }

    private static void SaveContenders() {
        MapInfo mapInfo = Library.mapInfo;
        Field[] fields = mapInfo.GetFields();
        int amount = Script.Map.Info.contendersAmount;

        StringBuilder[] stringBuilders = SaveFields(amount, fields);
        for (int i = 0; i < amount; i++) {
            Write(stringBuilders[i]);
			SaveContender(Script.Map.Info.contenders[i]);
        }
    }

    private static void SaveData(Data data){
        PrepareWrite(data.health);
        foreach (var item in data.diamonds)
            PrepareWrite(item);
    }

    private static StringBuilder[] SaveFields(int amount, Field[] fields) {
        StringBuilder[] result = new StringBuilder[amount];
        for (int i = 0; i < amount; i++)
            result[i] = new StringBuilder();

        for (int i = 0, fieldsLength = fields.Length; i < fieldsLength; i++) {
            Field field = fields[i];
            if (field.owner)
                result[field.owner.info.id].Append(field.GetId() + "\t");
        }

        for (int i = 0; i < amount; i++)
            result[i] = result[i].Remove(result[i].Length - 1, 1);

        return result;
    }

    private static void SaveHeroes(Contender contender) {
        int amount = contender.GetAmountOfDeadHeroes();
        int amountOfHeroes = contender.GetAmountOfHeroes();

        Write(amountOfHeroes - amount, amountOfHeroes);
        for (int i = 0; i < amountOfHeroes; i++) {
            Contender.Hero hero = contender.heroes[i];
            Write(hero.info.id, hero.exp);
        }
    }

    private static void SaveMapInfo(){
        Write(/*Info.campaign.ToInt(), */Info.mapId);
    }

    private static void SaveMissionInfo() {
        if (Info.campaign)
            Write(Library.mapInfo.GetCampaign().GetMissionId());
    }

    private static void SavePlayer(Script.Global.Player player) {
        Write((int)player.color, (int)player.race, player.team, (int)player.type);
    }

    private static void SaveStructure(Structure structure) {
		if (structure is Fortification)
			SaveData((structure as Fortification).data);
        Write(structure.GetInformation().id, structure.field.GetId(), structure.leftBuildingTime);
    }

    private static void SaveStructures(Contender contender) {
        Write(contender.structures.Count);
        foreach (var structure in contender.structures)
            SaveStructure(structure);
    }

    private static void SaveUnit(Movement unit) {
        SaveData(unit.data);
        Write(unit.information.id, unit.field.GetId(), unit.endedTurn.ToInt());
    }

    private static void SaveUnits(Contender contender){
        Write(contender.army.Count);
        foreach (var unit in contender.army)
            SaveUnit(unit);
    }

    #endregion

    #region Other

    static Field GetField(int id){
        return fields[id];
    }

    static string GetFileName() {
        return filePath + (Info.campaign ? "C" : "S") + ".txt";
    }

    public static bool IsAnyMapSaved() {
        return File.Exists(GetFileName());
    }

    static void PrepareWrite(params object[] args){
        foreach (var arg in args)
            dataToWrite += arg.ToString() + '\t';
    }

    static int[] Read(){
        return Main.StringArrayToIntArray(reader.ReadLine().Split('\t'));
    }

    static int ReadOne(){
        return int.Parse(reader.ReadLine());
    }

    public static void SetFilePath(string path) {
        filePath = path;
    }

    static void Write(){
        save.AppendLine(dataToWrite.Remove(dataToWrite.Length - 1));
        dataToWrite = "";
    }

    static void Write(params object[] args) {
        PrepareWrite(args);
        Write();
    }

    static void WriteOne(object arg) {
        save.AppendLine(arg.ToString());
    }

    #endregion

}
