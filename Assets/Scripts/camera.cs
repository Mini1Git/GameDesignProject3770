using UnityEngine;

public class camera : MonoBehaviour
{
    public GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //camera's Z value has to be -10 to see the player
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10);
    }
}
