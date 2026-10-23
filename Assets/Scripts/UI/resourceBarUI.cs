using UnityEngine;

public class resourceBarUI : MonoBehaviour
{
    [SerializeField] private RectTransform healthBar;   // Health fill
    [SerializeField] private RectTransform staminaBar;  // Stamina fill
    [SerializeField] private RectTransform manaBar;     // Mana fill
    [SerializeField] private player playerScript;       // optional, found by tag if empty

    Vector2 healthFull, staminaFull, manaFull;          // full size of each fill

    void Awake()
    {
        // if the slot is empty, find the player by its "Player" tag
        if (playerScript == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) playerScript = p.GetComponent<player>();
        }

        // remember each fill's size at full (keep them at full size in the scene)
        healthFull  = healthBar.sizeDelta;
        staminaFull = staminaBar.sizeDelta;
        manaFull    = manaBar.sizeDelta;
    }

    void Update()
    {
        if (playerScript == null) return;

        Fill(healthBar,  playerScript.health,                 playerScript.maxHealth,  healthFull);
        Fill(staminaBar, playerScript.stamina_System.Stamina, playerScript.maxStamina, staminaFull);
        Fill(manaBar,    playerScript.mana_System.Mana,       playerScript.maxMana,    manaFull);
    }

    // sets the fill's width to value/max of its full width (clamped between empty and full)
    void Fill(RectTransform bar, float value, float max, Vector2 full)
    {
        float ratio = max > 0 ? Mathf.Clamp01(value / max) : 0;
        bar.sizeDelta = new Vector2(ratio * full.x, full.y);
    }
}