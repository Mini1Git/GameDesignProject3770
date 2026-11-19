using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using System.Linq;
public class player : MonoBehaviour
{
    
    public SpellCasting caster;
    public spell_SO activeSpell; // this is the scriptable object that has all of the data of the currrently active spell.
    Rigidbody2D rb;
    float originalSpeed;
    [Header("Player Settings")]
    public float speed = 10;
    public bool isSprinting;
    public float sprintMultiplier;
    public float sprintStaminaCost;
    public float health = 10;
    public float maxHealth = 10;
    public float stamina = 10;
    public float maxStamina = 10;
    public float mana = 10;
    public float maxMana = 10;

    public GameObject mainHand;
    public GameObject offHand;
    public bool canAct = true;
    public bool isDashing = false;
    public int dashSpeed;
    public int dashStaminaCost = 2;

    PlayerInput playerInput;
    InputAction move;
    InputAction attackMain;
    InputAction attackOff;
    InputAction dash;
    InputAction sprint;
    Vector2 moveDirection = Vector2.zero;
    PlayerMeleeAttack meleeAttack;
    bool inputFocused;
    bool inputPaused;

    public bool regenHealth = false;
    public float healthRegenAmount = 1;

    private void Awake()
    {
        originalSpeed = speed;
        caster = GetComponent<SpellCasting>();
        rb = GetComponent<Rigidbody2D>(); // get the rigidbody on the game Object.
        meleeAttack = GetComponent<PlayerMeleeAttack>();
        inputFocused = Application.isFocused;
        playerInput = new PlayerInput();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    
    private void OnEnable() // when this script is enabled.
    {
        playerInput.Enable();
        move = playerInput.PlayerGame.Movement;
        
        attackMain = playerInput.PlayerGame.Attack_MainHand;
        attackOff = playerInput.PlayerGame.Attack_OffHand;

        //triggers
        attackMain.started += mainHandPressed;
        attackMain.canceled += mainHandReleased;
        attackOff.performed += offHandAttack; 

        //skills
        dash = playerInput.PlayerGame.Dash;
        sprint = playerInput.PlayerGame.Sprint;

    }

    private void OnDisable() // when this script is disabled.
    {
        //skills
        dash.performed -= Dash;
        sprint.started -= heldSprint;
        sprint.canceled -= releaseSprint;
        // disabling an action also sends canceled, so stop listening before disabling it
        meleeAttack?.CancelAttack();
        attackMain.started -= mainHandPressed;
        attackMain.canceled -= mainHandReleased;
        attackOff.performed -= offHandAttack;
        playerInput.Disable();
    }

    private void OnDestroy()
    {
        playerInput?.Dispose();
    }

    private void OnApplicationFocus(bool focused)
    {
        inputFocused = focused;
        if (!focused) meleeAttack?.CancelAttack();
    }

    private void OnApplicationPause(bool paused)
    {
        inputPaused = paused;
        if (paused) meleeAttack?.CancelAttack();
    }


    // Update is called once per frame
    void Update()
    {
        moveDirection = move.ReadValue<Vector2>();

        //sprinting logic 
        if (isSprinting && 0 < Stamina_System.Instance.Stamina)
        {
            Stamina_System.Instance.ConsumeStamina(sprintStaminaCost * Time.deltaTime);

            speed = originalSpeed * sprintMultiplier;
        }
        else
        {
            isSprinting = false;
            speed = originalSpeed;
        }

        if (regenHealth)
        {
            heal(healthRegenAmount * Time.deltaTime);
        }
    }

    
    void FixedUpdate()
    {
        //note, this movement code uses new input system

        //makes the player face the mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (canAct) transform.rotation = Quaternion.Euler(0, 0, angle);

        if (isDashing)
        {
            Stamina_System.Instance.ConsumeStamina(dashStaminaCost);
            rb.linearVelocity = new Vector2(moveDirection.x * speed * dashSpeed, moveDirection.y * speed * dashSpeed);
            isDashing = false;
            return; // this will prevent wasd movement from overwriting.
        }


        //player WASD movement
        rb.linearVelocity = new Vector2(moveDirection.x * speed, moveDirection.y * speed);
    }


    public void updatePlayerStats()
    {
        Debug.Log("UPDATE PLAYER STATS");
        maxHealth = LevelingSystem.Instance.getPlayerStat(Stats.Vitality) + 1;
        maxMana = LevelingSystem.Instance.getPlayerStat(Stats.Magic) + 1;
        maxStamina = LevelingSystem.Instance.getPlayerStat(Stats.Endurance) + 1;
        originalSpeed = LevelingSystem.Instance.getPlayerStat(Stats.Agility) + 1;

        // Damage scaling has already been implemented within weaponData.cs
        //clamp the current 
        health = Mathf.Min(health, maxHealth);
        mana = Mathf.Min(mana, maxMana);
        stamina = Mathf.Min(stamina, maxStamina);
    }

    void mainHandPressed(InputAction.CallbackContext ctx)
    {
        if (inputFocused && Application.isFocused && !inputPaused && canAct)
            meleeAttack?.BeginCharge();
    }

    void mainHandReleased(InputAction.CallbackContext ctx)
    {
        // only attack when the button is released, not when input is reset
        bool released = ctx.control is ButtonControl button && !button.isPressed
            && button.device.added && button.device.enabled;
        if (isActiveAndEnabled && inputFocused && Application.isFocused && !inputPaused
            && canAct && attackMain.enabled && released)
            meleeAttack?.ReleaseCharge();
        else
            meleeAttack?.CancelAttack();
    }

    void offHandAttack(InputAction.CallbackContext ctx)
    {
        if (activeSpell == null) return; // no spell in the active spell slot

        if (!canAct || Mana_System.Instance.Mana < activeSpell.manaCost)
        {
            Debug.Log("OUT OF Mana!");
            return;
        }

        Debug.Log("Attack with spell!");
        Mana_System.Instance.ConsumeMana(activeSpell.manaCost);
        caster.Cast(activeSpell);
    }

    public void takeDmg(int dmg)
    {
        health -= dmg;
        if(health <= 0)
        {
            die();
        }
    }

    public void heal(float value)
    {
        health += value;
        if(health >= maxHealth)
        {
            health = maxHealth;
        }
    }

    void die()
    {
        //dying resets the scene
        //we can implement saving and loading player data later
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void activateSkills() // this function checks if the player has skills unlocked (hard coded), and if they do, can later implement the skills.
    {
        // the dictionary is responsible to see what skills are available to the player.
        foreach (KeyValuePair<skill_SO, bool> skill in SkillSystem.Instance.skillsDictionary)
        {
            if (skill.Value == true)
            { // check if the player currently has it unlocked.
                string skillName = skill.Key.name.ToLower();
                switch (skillName) // using switch case here for easier management
                {
                    case "dash":
                        Debug.Log("DASHING ABILITY");
                        //assign dashing button to Dash function.
                        dash.performed += Dash;
                        break;
                    case "staminaregen":
                        Stamina_System.Instance.staminaUnlock = true;
                        break;
                    case "manaregen":
                        Mana_System.Instance.manaUnlock = true;
                        break;
                    case "sprint":
                        sprint.started += heldSprint;
                        sprint.canceled += releaseSprint;
                        break;
                    case "hpregen":
                        
                        regenHealth = true;
                        
                        break;
                }
                
            }

        }
    }
    public void Dash(InputAction.CallbackContext ctx)
    {
        if (dashStaminaCost <= Stamina_System.Instance.Stamina)
        {
            isDashing = true;
        }
       
    }

    public void heldSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            isSprinting = true;
    }

    public void releaseSprint(InputAction.CallbackContext ctx)
    {
        isSprinting = false;
        speed = originalSpeed;
    }
}
