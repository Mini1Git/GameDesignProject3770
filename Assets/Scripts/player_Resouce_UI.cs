using UnityEngine;
using UnityEngine.InputSystem;

public class player_Resouce_UI : MonoBehaviour
{
    [SerializeField] 
    private player playerScript;
    [SerializeField] 
    private resourceBarUI healthBar;   // this creates the "Health Bar" slot
    [SerializeField] 
    private resourceBarUI staminaBar; 
    [SerializeField] 
    private resourceBarUI manaBar; 

    void Awake()
    {
        // if the slot is empty, look for the player script on this same object
        if (playerScript == null) playerScript = GetComponent<player>();
    }

    void Start()
    {
        healthBar.SetMaxHealth(playerScript.maxHealth);
        healthBar.SetHealth(playerScript.health);
        staminaBar.SetMaxStamina(playerScript.maxStamina);
        staminaBar.SetStamina(playerScript.stamina);
        manaBar.SetMaxMana(playerScript.maxMana);
        manaBar.SetMana(playerScript.mana);
    }

    void Update()
    {
        // test keys: M = lose health, H = heal
        if (Keyboard.current != null)
        {
            if (Keyboard.current.mKey.wasPressedThisFrame) ChangeHealth(-2);
            if (Keyboard.current.hKey.wasPressedThisFrame) ChangeHealth(2);
            if (Keyboard.current.oKey.wasPressedThisFrame) ChangeStamina(-2);
            if (Keyboard.current.pKey.wasPressedThisFrame) ChangeStamina(2);
            if (Keyboard.current.lKey.wasPressedThisFrame) ChangeMana(-2);
            if (Keyboard.current.iKey.wasPressedThisFrame) ChangeMana(2);
            
        }

        // keep the bar in sync with player.cs
        healthBar.SetMaxHealth(playerScript.maxHealth);
        healthBar.SetHealth(playerScript.health);
        staminaBar.SetMaxStamina(playerScript.maxStamina);
        staminaBar.SetStamina(playerScript.stamina);
        manaBar.SetMaxMana(playerScript.maxMana);
        manaBar.SetMana(playerScript.mana);
    }

    void ChangeHealth(int amount)
    {
        playerScript.health = Mathf.Clamp(playerScript.health + amount, 0, (int)playerScript.maxHealth);
    }
    
    void ChangeStamina(int amount)
    {
        playerScript.stamina = Mathf.Clamp(playerScript.stamina + amount, 0, (int)playerScript.maxStamina);
    }
    
    void ChangeMana(int amount)
    {
        playerScript.mana = Mathf.Clamp(playerScript.mana + amount, 0, (int)playerScript.maxMana);
    }
}