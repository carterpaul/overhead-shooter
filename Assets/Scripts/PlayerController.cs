using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float projectile_speed;
    public float shot_cooldown;
    public float charge_speed;
    public float slowdown_factor;
    public Slider charge_slider;
    public Rigidbody2D projectile;
    public Sprite right_sprite;
    public Sprite up_sprite;
    public Sprite left_sprite;
    public Sprite down_sprite;
    private Rigidbody2D rb;
    private float charge_level;
    private bool last_frame_pressed;
    private float movement_speed;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        charge_level = 0;
        last_frame_pressed = false;
        movement_speed = speed;

        charge_slider.value = 0;
    }

    void Update()
    {
        // Update object's rotation to face cursor
        float angle = getAngleToCursor();
        if (angle > -45 & angle <= 45)
            GetComponent<SpriteRenderer>().sprite = right_sprite;
        else if (angle > 45 & angle <= 135)
            GetComponent<SpriteRenderer>().sprite = up_sprite;
        else if (angle <= -45 & angle > -135)
            GetComponent<SpriteRenderer>().sprite = down_sprite;
        else
            GetComponent<SpriteRenderer>().sprite = left_sprite;

        // Fire if the button is released and cooldown has passed
        if  (charge_level > shot_cooldown & last_frame_pressed == true & !Input.GetButton("Fire1"))
        {
            Shoot();
            charge_level = 0;
        }

        // Keep track of if the fire button was held during this frame
        if (Input.GetButton("Fire1"))
        {
            if (charge_level + charge_speed < 1.0f)
                charge_level += charge_speed;
            else
                charge_level = 1.0f;

            movement_speed = speed * slowdown_factor;
            last_frame_pressed = true;
        }
        else
        {
            last_frame_pressed = false;
            movement_speed = speed;
            charge_level = 0;
        }

        charge_slider.value = charge_level;
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);

        rb.MovePosition(movement * movement_speed + rb.position);
    }

    void Shoot()
    {
        Rigidbody2D p = Instantiate(projectile, rb.transform.position,
            Quaternion.Euler(0, 0, getAngleToCursor()));
        Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (target - rb.position).normalized;
        p.AddForce(direction * (projectile_speed * charge_level) + rb.velocity);
    }

    float getAngleToCursor()
    {
        Vector2 cursor = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return Mathf.Atan2(cursor.y - rb.position.y, cursor.x - rb.position.x) * Mathf.Rad2Deg;
    }
}
