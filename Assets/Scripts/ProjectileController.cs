using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ProjectileController : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public float x_limit;
    public float y_limit;
    public int ID;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 p = gameObject.transform.position;
        if (p.y > x_limit || p.y < -x_limit || p.x > y_limit || p.x < -y_limit)
        PhotonNetwork.Destroy(this.gameObject);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("OnPhotonInstantiation");
        object[] instantiationData = info.photonView.InstantiationData;
        ID = (int) instantiationData[0];
        Debug.Log("ProjectileController ID: " + ID);
    }
}
