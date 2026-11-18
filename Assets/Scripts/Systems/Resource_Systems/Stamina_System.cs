using UnityEngine;

public class Stamina_System : MonoBehaviour
{
    public static Stamina_System Instance;
    //stamina
    [SerializeField] private float stamina; // this is the actual value
    public float staminaRegenRate = 3.5f;
    public float staminaRegenMult = 2f;
    public bool staminaUnlock;
    public float Stamina // this is the property that modifies the actual value.
    {   //getter and setter.
        get { return Mathf.Clamp(stamina,0, player.maxStamina); } 
        set { stamina = Mathf.Clamp(value, 0, player.maxStamina); }
    }
    private player player;
    public void Awake()
    {
        // Check if an instance already exists
        if (Instance != null && Instance != this)
        {
            // Destroy this duplicate object immediately to enforce a single instance
            Destroy(gameObject);
            return;
        }

        // Set the active instance to this object
        Instance = this;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<player>(); // this gets the player gameobject by tag.
    }

    public void Start()
    {
        Stamina = player.stamina;
    }
    public void Update()
    {   if (Stamina != player.maxStamina && staminaUnlock)
        {
            RegenerateStamina(staminaRegenMult);
        }
        else if (Stamina != player.maxStamina)
        {
            RegenerateStamina(1f); // regen at normal rate.
        }
        
    }

    public void RegenerateStamina(float mult)
    {
        Stamina += staminaRegenRate * Time.deltaTime * mult;
    }
    public void ConsumeStamina(float stamina_value) { // use this function to consume stamina with a value.
        Stamina -= stamina_value;
    }
    
}
