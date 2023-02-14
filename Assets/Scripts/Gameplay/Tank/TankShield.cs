using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TankShield : MonoBehaviour, IPunObservable
{
    private const string ROTATE_SHILD_BUTTON = "RotateShield";

    [SerializeField]
    private float rotationSpeed = 150;

    private PhotonView photonView;

    private float CurrentRotation => transform.rotation.eulerAngles.y;

    
    void Start()
    {
        photonView = GetComponentInParent<PhotonView>();
    }

    private void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        var rotateInput = Input.GetAxis(ROTATE_SHILD_BUTTON);
        SetRotation(CurrentRotation + rotateInput * Time.deltaTime * rotationSpeed);
    }

    private void SetRotation(float newYRotation)
    {
        transform.rotation = Quaternion.Euler(0, newYRotation, 0);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(CurrentRotation);
        }
        else
        {
            var newYRotation = (float)stream.ReceiveNext();
            SetRotation(newYRotation);
        }
    }
}
