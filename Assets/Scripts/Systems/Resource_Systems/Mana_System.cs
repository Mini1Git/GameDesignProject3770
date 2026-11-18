using UnityEngine;

public class Mana_System : MonoBehaviour
{
    public static Mana_System Instance;
    public bool manaUnlock;
    //mana
    [SerializeField] private float mana; // this is the actual value
    public float manaRegenRate = 3.5f;
    public float manaRegenMult = 2f;
    public float Mana // this is the property that modifies the actual value.
    {   //getter and setter.
        get { return Mathf.Clamp(mana, 0, player.maxMana); }
        set { mana = Mathf.Clamp(value, 0, player.maxMana); }
    }
    private player player;
    public void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<player>(); // this gets the player gameobject by tag.
        // Check if an instance already exists
        if (Instance != null && Instance != this)
        {
            // Destroy this duplicate object immediately to enforce a single instance
            Destroy(gameObject);
            return;
        }

        // Set the active instance to this object
        Instance = this;
    }

    public void Start()
    {
        Mana = player.mana;
    }
    public void Update()
    {
        if (Mana != player.maxMana && manaUnlock)
        {
            RegenerateMana(manaRegenMult); // faster rate
        }
        else if (Mana != player.maxMana)
        {
            RegenerateMana(1f); // regen at normal rate
        }

    }

    public void RegenerateMana(float mult)
    {
        Mana += manaRegenRate * Time.deltaTime * mult;
    }
    public void ConsumeMana(float mana_value)
    { // use this function to consume mana with a value.
        Mana -= mana_value;
    }

}