using UnityEngine;

public class Stamina_System : MonoBehaviour
{
    //stamina
    [SerializeField] private float stamina; // this is the actual value
    public float staminaRegenRate = 3.5f;
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
    {   if (Stamina != player.maxStamina)
        {
            RegenerateStamina();
        }
        
    }

    public void RegenerateStamina()
    {
        Stamina += staminaRegenRate * Time.deltaTime;
    }
    public void ConsumeStamina(float stamina_value) { // use this function to consume stamina with a value.
        Stamina -= stamina_value;
    }
    
}
