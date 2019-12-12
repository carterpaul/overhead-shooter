using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float projectile_speed;
    public float cooldown;
    public Rigidbody2D projectile;
    private Rigidbody2D rb;
    private float last_shot;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        last_shot = -10f;
    }

    void Update()
    {
        // Fire if cooldown period has passed
        if (Input.GetButton("Fire1") & Time.time - last_shot > cooldown)
        {
            Shoot();
            last_shot = Time.time;
        }

        // Update object's rotation to face cursor
        Vector2 cursor = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float angle = Mathf.Atan2(cursor.y - rb.position.y, cursor.x - rb.position.x) * Mathf.Rad2Deg;
        rb.rotation = angle;
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);

        rb.MovePosition(movement * speed + rb.position);
    }

    void Shoot()
    {
        Rigidbody2D p = Instantiate(projectile, rb.transform.position, rb.transform.rotation);
        Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (target - rb.position).normalized;
        p.AddForce((direction * projectile_speed) + rb.velocity);
    }
}
