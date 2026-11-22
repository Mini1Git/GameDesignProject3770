using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


// keep the definitions of global types in here for easy access.
public enum ElementType
{
    Frost, Fire, Wind, Dark, Poison, Water, Nature, Summon
}
public enum PositionType
{
    Caster, Mouse
}


public class SpellSystem : MonoBehaviour
{
	public enum AttackType
	{
		No_Move,
		Projectile
	}
    public enum LimitType
    {
        Range, 
        Timer
    }
    public enum EndType // basically, this enum is for when the projectile has completed it's limit type cycle.
    {
        Explode,
        Disappear, 
        Split
        
    }
    [Header("Movement Settings")]
	public AttackType attack_Type;
    public float speedIfProjectile = 1f;
    
    [Header("Limit Settings")]
    [Tooltip("If the limit type is range, then modify the range variable, else modify the timer.")]
    public LimitType limit_Type;
    public float timeIfTimer = 5;
    public float rangeIfRange = 3;

    [Header("End Behaviour")]
    public EndType end_Type;
    [Header("Split Settings")]
    public GameObject splitPrefab;
    public int splitCount = 3;
    public float splitAngle = 30f;
    [Header("Explosion settings")]
    public ParticleSystem explosionParticleSystem;
    public float explosionRadius;


    [Header("Spell SO Info")]
    public spell_SO spellData;
    private float actualSpeed => attack_Type == AttackType.Projectile ? speedIfProjectile : 0f;
    
    private player player;

    private Coroutine limitCoroutine;
    private Vector2 startPos;
    bool hasEnded = false;
    HashSet<enemyBase> damagedEnemies = new HashSet<enemyBase>();
    
    public void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<player>();
        spellData = player.activeSpell; // get the SO data of the spell.
        startPos = transform.position;
        
        limitCoroutine = StartCoroutine(limitFunction());
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasEnded) return;

        Debug.Log("Spell have collided with " + collision.collider.name);
        if (collision.collider.CompareTag("Enemy"))
        {
            enemyBase enemy = collision.collider.GetComponent<enemyBase>();
            if (enemy != null)
            {
                damagedEnemies.Add(enemy);
                doSpellDamage(enemy, spellData.damage);
            }
            
        }
       
        endSpell(true);
        
    }
    public void Update()
    {
        if (actualSpeed != 0)
        {
            transform.position += transform.right * actualSpeed * Time.deltaTime;
        }
        
        
    }

    IEnumerator limitFunction()
    {
        if (limit_Type == LimitType.Timer)
        {
            yield return new WaitForSeconds(timeIfTimer);
            
        }
        else if (limit_Type == LimitType.Range)
        {
            //range limit code
            while (Vector2.Distance(startPos, transform.position) < rangeIfRange)
            {
                //Debug.Log($"{Vector2.Distance(startPos, transform.position)} vs {rangeIfRange}");
                yield return null;
            }
        }
        endSpell(false);

        limitCoroutine = null;
        yield return null;
    }

    void endSpell(bool onImpact) //particle effects and visuals.
    {
        if (hasEnded) return;

        hasEnded = true;

        if (limitCoroutine != null)
        {
            StopCoroutine(limitCoroutine);
            limitCoroutine = null;
        }
        if (onImpact)
        {
            Debug.Log("Hit something!");

            switch (end_Type)
            {
                case EndType.Explode:
                    // Explosion logic
                    explode();
                    break;

                case EndType.Disappear:
                    Destroy(gameObject);
                    break;

                case EndType.Split:
                    split();
                    break;
            }




            GameObject.Destroy(gameObject);

        }
        if (!onImpact) {
            Debug.Log("DID NOT HIT ANYTHING"); // so just disappear
            
            GameObject.Destroy(gameObject);
        }
        
    }
    void doSpellDamage(enemyBase enemy, int damage)
    {
        //first do damage
        enemy.takeDmg(damage);

        //debuffs, can use percentage % chance of happening on hit.
        switch (spellData.spellElement)
        {
            case ElementType.Fire:
                applyElement(enemy, typeof(Burn_Debuff));
                break;

        }
    }
    void applyElement(enemyBase enemy, Type debuffToApply)
    {

        Component existing = enemy.GetComponent(debuffToApply);

        if (existing == null)
        {
            enemy.gameObject.AddComponent(debuffToApply);
        }
        else if (existing is BaseDebuff debuff)
        {
            debuff.resetTimer();
        }
    }

    void split()
    {
        float angleStep = splitCount == 1 ? 0f: splitAngle * 2f / (splitCount - 1);

        for (int i = 0; i < splitCount; i++)
        {
            float angle = -splitAngle + (angleStep * i);

            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            Instantiate(gameObject,transform.position, transform.rotation * rotation);
        }

        Destroy(gameObject);
    }

    void explode()
    {
        Debug.Log(explosionRadius);
        Instantiate(explosionParticleSystem, transform.position,transform.rotation);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                enemyBase enemy = collider.GetComponent<enemyBase>();

                if (enemy != null && damagedEnemies.Add(enemy)) // using this means it will add an enemy if it's not already there and return true, otherwise false.
                {
                    doSpellDamage(enemy, spellData.damage);
                }
            }
        }
        Destroy(gameObject);
    }


    //visual aid
    private void OnDrawGizmosSelected()
    {
        if (end_Type != EndType.Explode)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }


}
