using Mono.Cecil;
using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
{
    public Rigidbody2D rb;
    public float speed = 10;
    public int health = 10;

    public GameObject mainHand;
    public GameObject offHand;
    public bool canAct = true;
    

    PlayerInput playerInput;
    InputAction move;
    InputAction attackMain;
    InputAction attackOff;
    Vector2 moveDirection = Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // get the rigidbody on the game Object.
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
        attackMain.performed += mainHandAttack; // so basically, when player clicks left mouse button or right clicks
        attackOff.performed += offHandAttack; 
    }

    private void OnDisable() // when this script is disabled.
    {
        playerInput.Disable();
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

    void mainHandAttack(InputAction.CallbackContext ctx)
    {
        if (!canAct) return;
        mainHand.GetComponent<Animator>().SetTrigger("Swing");
    }

    void offHandAttack(InputAction.CallbackContext ctx)
    {
        if (!canAct) return;
        Debug.Log("Attack with spell!");
    }
}
