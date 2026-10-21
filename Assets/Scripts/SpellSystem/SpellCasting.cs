using UnityEngine;
using UnityEngine.InputSystem;

public class SpellCasting : MonoBehaviour
{
    
    public void Cast(spell_SO spell) {
        // mouse dir setup
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = -Camera.main.transform.position.z;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos); // gets mouse position.
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (spell.positionType == PositionType.Mouse)
        {
            

            Instantiate(spell.spellPrefab, mouseWorldPos, Quaternion.Euler(0, 0, angle)); // spawns the spell prefab, at the mouse's position in the world,
                                                                                          // with current rotation.
        }
        else if (spell.positionType == PositionType.Caster) {
            Instantiate(spell.spellPrefab, transform.position, Quaternion.Euler(0, 0, angle)); // spawns prefab, at caster's location, default rotation.
        }
        

    }


}
