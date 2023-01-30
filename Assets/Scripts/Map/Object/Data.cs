using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Script.Map;


public class Data : MonoBehaviour {

    [Header("Data")]
    public int armor;
    public int damage;
    public int maxHealth;
    public Range range;

    [Header("State")]
    public int health;
    public int[] diamonds = new int[(int)Gem.White];

    [Header("Requirement")]
    public new Renderer renderer;
    public new Transform transform;

    [System.NonSerialized] public GameObject healthStatus;
    [System.NonSerialized] public Contender.Hero hero;
    [System.NonSerialized] public Race.Information information;

    public enum Range {
        Melee, Medium = 2, Long = 5
    }

    private void Awake() {
        health = maxHealth;
    }

    public int GetValue() {
        if (information.IsHero())
            hero.info.SetCost(hero.level);
        return information.cost;
    }

    public void SetHealth(int value) {
        health = value;
        if (health >= maxHealth) {
            health = maxHealth;
            DestroyHealthStatus();
        }
        else if (!healthStatus)
            MapController.CreateHealthText(this);
        else
            UpdateHealthStatusText();
    }

    #region HealthStatus

    public void DestroyHealthStatus() {
        if (!healthStatus)
            return;
        Destroy(healthStatus);
        healthStatus = null;
    }

    public void DisableHealthStatus() {
        if (healthStatus)
            healthStatus.SetActive(false);
    }

    public void EnableHealthStatus() {
        if (!healthStatus)
            return;
        healthStatus.SetActive(true);
        UpdateHealthStatusPosition();
    }

    public float PercentOfHP() {
        return (float)health / maxHealth;
    }

    public void UpdateHealthStatusPosition() {
        healthStatus.transform.position = transform.position - Script.Map.Info.canvasWorld.forward * 20 - Script.Map.Info.canvasWorld.up * 3;
    }

    public void UpdateHealthStatusText() {
        healthStatus.GetComponent<Text>().text = Mathf.Floor(health * 100f / maxHealth).ToString();
    }

    #endregion

    #region Hero

    public void SetLevel(int level, int diffrence) {
        Data data = information.GetData();

        float multiplier = 1 + Engine.Hero.increment * level;
        maxHealth = Mathf.RoundToInt(data.maxHealth * multiplier);
        armor = (int)(data.armor * multiplier);
        damage = (int)(data.damage * multiplier);

        SetHealth(health + Mathf.RoundToInt(data.maxHealth * Engine.Hero.increment * diffrence));
    }

    #endregion

}

