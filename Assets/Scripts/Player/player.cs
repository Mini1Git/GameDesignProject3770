using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class player : MonoBehaviour
{
    
    public SpellCasting caster;
    public spell_SO activeSpell; // this is the scriptable object that has all of the data of the currrently active spell.
    Rigidbody2D rb;
    [Header("Player Settings")]
    public float speed = 10;
    public float health = 10;
    public float maxHealth = 10;
    public float stamina = 10;
    public float maxStamina = 10;
    public float mana = 10;
    public float maxMana = 10;

    public GameObject mainHand;
    public GameObject offHand;
    public bool canAct = true;

    public Stamina_System stamina_System;
    public Mana_System mana_System;

    PlayerInput playerInput;
    InputAction move;
    InputAction attackMain;
    InputAction attackOff;
    Vector2 moveDirection = Vector2.zero;
    PlayerMeleeAttack meleeAttack;
    bool inputFocused;
    bool inputPaused;

    private void Awake()
    {
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
    }

    private void OnDisable() // when this script is disabled.
    {
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

        
    }

    
    void FixedUpdate()
    {
        //note, this movement code uses new input system

        //makes the player face the mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (canAct) transform.rotation = Quaternion.Euler(0, 0, angle);

        //player WASD movement
        rb.linearVelocity = new Vector2(moveDirection.x * speed, moveDirection.y * speed);
    }

    void mainHandPressed(InputAction.CallbackContext ctx)
    {
        if (inputFocused && Application.isFocused && !inputPaused && canAct)
            meleeAttack?.BeginCharge(ctx.time);
    }

    void mainHandReleased(InputAction.CallbackContext ctx)
    {
        // only attack when the button is released, not when input is reset
        bool released = ctx.control is ButtonControl button && !button.isPressed
            && button.device.added && button.device.enabled;
        if (isActiveAndEnabled && inputFocused && Application.isFocused && !inputPaused
            && canAct && attackMain.enabled && released)
            meleeAttack?.ReleaseCharge(ctx.time);
        else
            meleeAttack?.CancelAttack();
    }

    void offHandAttack(InputAction.CallbackContext ctx)
    {
        if (!canAct || mana_System.Mana < activeSpell.manaCost)
        {
            Debug.Log("OUT OF Mana!");
            return;
        }

        Debug.Log("Attack with spell!");
        mana_System.ConsumeMana(activeSpell.manaCost);
        caster.Cast(activeSpell);
    }
}
