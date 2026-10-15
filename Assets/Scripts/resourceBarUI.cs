using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class resourceBarUI : MonoBehaviour
{
    public float Health, Maxhealth, Stamina, MaxStamina, Mana, Maxmana, Width, Height;
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
        Maxhealth = maxHealth;
    }

    public void SetHealth(float health)
    {
        Health = health;
        float newWidth = Maxhealth > 0 ? (Health / Maxhealth) * Width : 0;

        healthBar.sizeDelta = new Vector2(newWidth, Height);
    }

    public void SetMaxMana(float maxMana)
    {
        Maxmana = maxMana;
    }

    public void SetMana(float mana)
    {
        Mana = mana;
        float newWidth = Maxmana > 0 ? (Mana / Maxmana) * Width : 0;
        
        Debug.Log("Mana width: " + newWidth);
        
        manaBar.sizeDelta = new Vector2(newWidth, Height);
    }

    public void SetMaxStamina(float maxStamina)
    {
        MaxStamina = maxStamina;
    }

    public void SetStamina(float stamina)
    {
        Stamina = stamina;
        float newWidth = MaxStamina > 0 ? (Stamina / MaxStamina) * Width : 0;
        
        staminaBar.sizeDelta = new Vector2(newWidth, Height);
    }
}