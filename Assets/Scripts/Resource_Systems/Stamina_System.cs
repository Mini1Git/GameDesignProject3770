using UnityEngine;

public class Stamina_System : MonoBehaviour
{
    //stamina
    [SerializeField] private float stamina; // this is the actual value
    public float staminaRegenRate = 3.5f;
    public float Stamina // this is the property that modifies the actual value.
    {   //getter and setter.
        get { return Mathf.Clamp(stamina,0, player.maxStamina); } 
        set { stamina = Mathf.Clamp(stamina, 0, player.maxStamina); }
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
        RegenerateStamina();
    }

    public void RegenerateStamina()
    {
        Stamina += staminaRegenRate * Time.deltaTime;
    }
    public void UpdateStamina(float stamina_value) { // use this function to increase or decrease stamina (use a negative number).
        Stamina += stamina_value;
    }
    
}
