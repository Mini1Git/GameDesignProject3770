using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class resourceBarUI : MonoBehaviour
{
    public float health, maxHealth, Stamina, MaxStamina, mana, maxMana, width, height;
    [SerializeField]
    private RectTransform healthBar;
    [SerializeField]
    private RectTransform staminaBar;
    [SerializeField]
    private RectTransform manaBar;
    [SerializeField]
    private player playerScript;  //get the resources from player.cs script

    void Awake()
    {
        // if the slot is empty, find the player by its "Player" tag
        if (playerScript == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) playerScript = p.GetComponent<player>();
        }
    }

    void Update()
    {
        if (playerScript == null) return;

        SetMaxHealth(playerScript.maxHealth);
        SetHealth(playerScript.health);
        SetMaxStamina(playerScript.maxStamina);
        SetStamina(playerScript.stamina);
        SetMaxMana(playerScript.maxMana);
        SetMana(playerScript.mana);
    }

    public void SetMaxHealth(float maxHealth)
    {
        this.maxHealth = maxHealth;
    }

    public void SetHealth(float health)
    {
        this.health = health;
        float newWidth = this.maxHealth > 0 ? (health / maxHealth) * width : 0;

        healthBar.sizeDelta = new Vector2(newWidth, height);
    }

    public void SetMaxMana(float maxMana)
    {
        this.maxMana = maxMana;
    }

    public void SetMana(float mana)
    {
        this.mana = mana;
        float newWidth = maxMana > 0 ? (this.mana / maxMana) * width : 0;
        
        
        
        manaBar.sizeDelta = new Vector2(newWidth, height);
    }

    public void SetMaxStamina(float maxStamina)
    {
        MaxStamina = maxStamina;
    }

    public void SetStamina(float stamina)
    {
        Stamina = stamina;
        float newWidth = MaxStamina > 0 ? (Stamina / MaxStamina) * width : 0;
        
        staminaBar.sizeDelta = new Vector2(newWidth, height);
    }
}