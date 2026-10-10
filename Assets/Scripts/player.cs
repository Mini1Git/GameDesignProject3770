using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
{
    public Rigidbody2D rb;
    public float speed = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    void FixedUpdate()
    {   
        //note, this movement code uses legacy Input Management Systems (this project uses both old and new)

        //makes the player face the mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        //player WASD movement
        float x = Input.GetAxisRaw("Horizontal") * speed;
        float y = Input.GetAxisRaw("Vertical") * speed;
        rb.linearVelocity = new Vector2(x, y);
    }
}
