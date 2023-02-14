using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Tanks;
using UnityEngine;

public class HomingMissile : MonoBehaviour, IPunInstantiateMagicCallback
{
    [SerializeField]
    private Rigidbody missileBody;
    [SerializeField]
    private float speed = 12;
    [SerializeField]
    private PhotonView view;
    [SerializeField]
    private ShellExplosion shellExplosion;

    private Rigidbody target;
    private int targetViewID;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;

        target = PhotonView.Find(targetViewID).GetComponent<Rigidbody>();

        if (view.IsMine)
        {
            view.TransferOwnership(PhotonNetwork.MasterClient);
        }
    }

    void FixedUpdate()
    {
        if (!view.IsMine)
        {
            return;
        }
        var direction = (target.position - transform.position).normalized;
        direction.y = 0;
        transform.forward = direction;

        Vector3 movement = direction * speed * Time.deltaTime;
        missileBody.MovePosition(missileBody.position + movement);

    }
}
