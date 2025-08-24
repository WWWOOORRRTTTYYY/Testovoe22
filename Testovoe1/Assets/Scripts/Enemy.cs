using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player; 
    public float moveSpeed = 2f;
    public float shootInterval = 2f; 
    public float shootingRange = 5f; 
    public GameObject arrowPrefab;    
    public Transform firePoint;       

    private Animator animator; 
    private float lastShootTime;

    private PlayerController PlayerController; 

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerController = playerObj.GetComponent<PlayerController>();
        }

        lastShootTime = -shootInterval; 
    }

    void Update()
    {
        if (player == null) return;

        Vector3 directionToPlayer = player.position - transform.position;
        float distance = directionToPlayer.magnitude;

        Vector3 moveDir = directionToPlayer.normalized;

        if (distance <= shootingRange && Time.time - lastShootTime >= shootInterval)
        {
            ShootAtPlayer(moveDir);
            lastShootTime = Time.time;
        }

        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);

        UpdateAnimation(moveDir);
    }

    void UpdateAnimation(Vector3 moveDir)
    {
        if (animator == null) return;

        float absX = Mathf.Abs(moveDir.x);
        float absY = Mathf.Abs(moveDir.y);

        if (absX > absY)
        {
            if (moveDir.x > 0)
            {
                animator.Play("WalkRight");
            }
            else
            {
                animator.Play("WalkLeft");
            }
        }
        else
        {
            if (moveDir.y > 0)
            {
                animator.Play("WalkUp");
            }
            else
            {
                animator.Play("WalkDown");
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            if (PlayerController != null)
            {
                PlayerController.EnemyDestroyed();
            }
            Destroy(gameObject);
        }
    }

    void ShootAtPlayer(Vector3 direction)
    {
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        Arrow arrowScript = arrow.GetComponent<Arrow>();
        arrowScript.Setup(direction);
    }
}
