using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    private Vector2 direction;

    private Animator animator;

    private static readonly int ExplosionTrigger = Animator.StringToHash("Explode");

    public void Setup(Vector2 dir)
    {
        direction = dir.normalized;
        animator = GetComponent<Animator>();

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //animator.SetTrigger(ExplosionTrigger);
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            //animator.SetTrigger(ExplosionTrigger);
            Destroy(gameObject);
        }
    }
}
