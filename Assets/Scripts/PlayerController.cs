using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    public float speed;
    public float projectile_speed;
    public float shot_cooldown;
    public float charge_speed;
    public float slowdown_factor;
    public GameObject PlayerUiPrefab;
    public GameObject projectile;
    public Sprite right_sprite;
    public Sprite up_sprite;
    public Sprite left_sprite;
    public Sprite down_sprite;
    public static GameObject LocalPlayerInstance;
    public float charge_level;
    public float health = 1f;
    private bool last_frame_pressed;
    private float movement_speed;
    private int ID;

    // Start is called before the first frame update
    void Start()
    {
        System.Random rnd = new System.Random();
        ID = rnd.Next(65536);

        charge_level = 0;
        last_frame_pressed = false;
        movement_speed = speed;

        if (PlayerUiPrefab != null)
        {
            GameObject _uiGo =  Instantiate(PlayerUiPrefab);
            _uiGo.SendMessage ("SetTarget", this, SendMessageOptions.RequireReceiver);
        }
        else
        {
            Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
        }
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Awake()
    {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            PlayerController.LocalPlayerInstance = this.gameObject;
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        // Don't control characters that aren't my own
        if (!photonView.IsMine && PhotonNetwork.IsConnected){
            return;
        }

        if (health <= 0f)
        {
            GameManager.Instance.LeaveRoom();
        }


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

        // move the player
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, moveVertical, 0);

        LocalPlayerInstance.transform.position = movement * movement_speed * Time.deltaTime + LocalPlayerInstance.transform.position;
    }

    void Shoot()
    {
        object[] initData = {ID};
        print("initData: " + initData);
        GameObject p = PhotonNetwork.Instantiate(this.projectile.name, LocalPlayerInstance.transform.position,
                        Quaternion.Euler(0, 0, getAngleToCursor()), data: initData);
        Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        target = new Vector3(target.x, target.y, 0.0f); // get rid of weirdness in the z dimension before normalizing
        Vector3 direction = Vector3.Normalize(target - LocalPlayerInstance.transform.position);
        Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
        rb.AddForce(direction * (projectile_speed * charge_level));
    }

    float getAngleToCursor()
    {
        Vector2 cursor = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return Mathf.Atan2(cursor.y - LocalPlayerInstance.transform.position.y, cursor.x - LocalPlayerInstance.transform.position.x) * Mathf.Rad2Deg;
    }

    //PUN callbacks
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
    {
        GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
        _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
    }

    public void OnDisable()
    {
        // Always call the base to remove callbacks
        //base.OnDisable ();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        // We are only interested in Beamers
        // we should be using tags but for the sake of distribution, let's simply check by name.
        if (!other.name.Contains("Projectile"))
        {
            return;
        }
        int otherID = other.gameObject.GetComponent<ProjectileController>().ID;
        if (otherID != ID){
            health -= 0.1f;
        }
        Debug.Log("Trigger entered, health: " + health);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(charge_level);
            stream.SendNext(health);
        }
        else
        {
            // Network player, receive data
            this.charge_level = (float)stream.ReceiveNext();
            this.health = (float)stream.ReceiveNext();
        }
    }
}
