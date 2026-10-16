using UnityEngine;

[DefaultExecutionOrder(100)]
public class Stamina_System : MonoBehaviour
{
    //stamina
    [SerializeField] private float stamina; // this is the actual value
    public float staminaRegenRate = 3.5f;
    
    //block stamina regen during the combat
    [SerializeField, Min(0f)] private float staminaRegenDelay = 1.5f;
    private bool combatBlocksRegen;
    private double regenAllowedAt = double.NegativeInfinity;  
    
    public float Stamina // this is the property that modifies the actual value.
    {   //getter and setter.
        get { return Mathf.Clamp(stamina,0, player.maxStamina); } 
        set { stamina = Mathf.Clamp(value, 0, player.maxStamina); }
    }
    private player player;
    public void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<player>(); // this gets the player gameobject by tag.
    }

    public void Start()
    {
        Stamina = player.stamina;
    }
    public void Update()
    {
        if (!combatBlocksRegen && Time.timeAsDouble >= regenAllowedAt && Stamina < player.maxStamina)
        {
            RegenerateStamina();
        }
        
    }

    public void RegenerateStamina()
    {
        RegenerateStaminaForFrame(Time.timeAsDouble, Time.deltaTime);
    }

    private void RegenerateStaminaForFrame(double now, float deltaTime)
    {
        if (combatBlocksRegen || now < regenAllowedAt) return;

        // only restore the part of this frame after the recovery delay
        float elapsed = Mathf.Min(deltaTime, (float)(now - regenAllowedAt));
        Stamina += staminaRegenRate * elapsed;
    }
    public void BeginCombatAction()
    {
        combatBlocksRegen = true;
    }

    public void EndCombatAction()
    {
        if (!combatBlocksRegen) return;
        combatBlocksRegen = false;
        regenAllowedAt = Time.timeAsDouble + Mathf.Max(0f, staminaRegenDelay);
    }
    public void ConsumeStamina(float stamina_value) { // use this function to consume stamina with a value.
        Stamina -= stamina_value;
    }
    
}
