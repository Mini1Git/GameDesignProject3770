using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
{
    public Rigidbody2D rb;
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
    [SerializeField] private PlayerCombat combat;
    private bool inputFocused = true;

    PlayerInput playerInput;
    InputAction move;
    InputAction attackMain;
    InputAction attackOff;
    Vector2 moveDirection = Vector2.zero;

    private void Awake()
    {
        
        rb = GetComponent<Rigidbody2D>(); // get the rigidbody on the game object
        if (combat == null) combat = GetComponent<PlayerCombat>();
        playerInput = new PlayerInput();
        move = playerInput.PlayerGame.Movement;
        attackMain = playerInput.PlayerGame.Attack_MainHand;
        attackOff = playerInput.PlayerGame.Attack_OffHand;
    }

    private void OnEnable() // when this script is enabled.
    {
        attackMain.started += mainHandPressed;
        attackMain.canceled += mainHandReleased;
        attackOff.performed += offHandAttack;
        playerInput.Enable();
    }

    private void OnDisable() // when this script is disabled.
    {
        // unsubscribe before disabling actions to avoid treating cancellation as release
        attackMain.started -= mainHandPressed;
        attackMain.canceled -= mainHandReleased;
        attackOff.performed -= offHandAttack;
        combat?.CancelCombat();
        playerInput.Disable();
    }

    private void OnDestroy()
    {
        playerInput?.Dispose();
    }

    private void OnApplicationFocus(bool focused)
    {
        inputFocused = focused;
        if (!focused) combat?.CancelCombat();
    }


    // read movement once per frame
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

        // player wasd movement
        rb.linearVelocity = new Vector2(moveDirection.x * speed, moveDirection.y * speed);
    }

    void mainHandPressed(InputAction.CallbackContext ctx)
    {
        if (inputFocused && Application.isFocused) combat?.OnAttackPressed();
    }

    void mainHandReleased(InputAction.CallbackContext ctx)
    {
        if (inputFocused && Application.isFocused && attackMain.enabled)
            combat?.OnAttackReleased();
        else
            combat?.CancelCombat();
    }

    void offHandAttack(InputAction.CallbackContext ctx)
    {
        if (!canAct || mana_System == null || mana_System.Mana < 5)
        {
            Debug.Log("OUT OF Mana!");
            return;
        }

        Debug.Log("Attack with spell!");
        mana_System.ConsumeMana(5);
    }
}
