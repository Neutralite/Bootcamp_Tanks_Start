using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks
{
    public class TankShooting : MonoBehaviour
    {
        private const string FIRE_BUTTON = "Fire1";
        private const string HOMING_MISSILE_BUTTON = "Fire2";
        private const string AIR_STRIKE_BUTTON = "Fire3";

        public Rigidbody shell;
        public Transform fireTransform;
        public Slider aimSlider;
        public AudioSource shootingAudio;
        public AudioClip chargingClip;
        public AudioClip fireClip;
        public float minLaunchForce = 15f;
        public float maxLaunchForce = 30f;
        public float maxChargeTime = 0.75f;

        public float homingMissileInstantiateOffset = 4f;
        public float airStrikeInstantiateOffset = 5f;

        private PhotonView photonView;

        private float currentLaunchForce;
        private float chargeSpeed;
        private bool fired;

        private void OnEnable()
        {
            currentLaunchForce = minLaunchForce;
            aimSlider.value = minLaunchForce;
        }

        private void Start()
        {
            photonView = GetComponent<PhotonView>();

            chargeSpeed = (maxLaunchForce - minLaunchForce) / maxChargeTime;
        }

        private void Update()
        {
            // Only allow owner of this tank to shoot
            if (!photonView.IsMine)
            {
                return;
            }

            aimSlider.value = minLaunchForce;

            if (currentLaunchForce >= maxLaunchForce && !fired)
            {
                currentLaunchForce = maxLaunchForce;
                Fire();
            }
            else if (Input.GetButtonDown(FIRE_BUTTON))
            {
                fired = false;
                currentLaunchForce = minLaunchForce;

                shootingAudio.clip = chargingClip;
                shootingAudio.Play();
            }
            else if (Input.GetButton(FIRE_BUTTON) && !fired)
            {
                currentLaunchForce += chargeSpeed * Time.deltaTime;

                aimSlider.value = currentLaunchForce;
            }
            else if (Input.GetButtonUp(FIRE_BUTTON) && !fired)
            {
                Fire();
            }
            if (Input.GetButtonDown(AIR_STRIKE_BUTTON))
            {
                AirStrike();
            }
        }

        private void TryFireHomingMissile()
        {
            if (!Input.GetButtonDown(HOMING_MISSILE_BUTTON))
            {
                return;
            }
            if(!GetClickPosition(out var clickPos))
            {
                return;
            }

            Collider[] colliders = Physics.OverlapSphere(clickPos, 5, LayerMask.GetMask("Players"));

            foreach(var tankCollider in colliders)
            {
                if(tankCollider.gameObject == gameObject)
                {
                    continue;
                }

                var direction = (tankCollider.transform.position - transform.position).normalized;

                var position = transform.position + direction * homingMissileInstantiateOffset + Vector3.up;
                object[] data =
                {
                    tankCollider.GetComponent<PhotonView>().ViewID
                };

                PhotonNetwork.Instantiate(
                    nameof(HomingMissile),
                    position,
                    Quaternion.LookRotation(transform.forward),
                    0,
                    data);
            }
        }

        private bool GetClickPosition(out Vector3 clickPos)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(!Physics.Raycast(ray, out var hit, 1000, LayerMask.GetMask("Boundaries")))
            {
                var gotHit = Physics.Raycast(ray, out var hit2, 1000, LayerMask.GetMask("Default"));

                clickPos = gotHit ? hit2.point : Vector3.zero;

                return gotHit;
            }
            clickPos = Vector3.zero;
            return false;
        }

        private void Fire()
        {
            fired = true;

            // Instantiate the projectile on all clients
            photonView.RPC
                (
                "Fire",
                RpcTarget.All,
                fireTransform.position,
                fireTransform.rotation,
                currentLaunchForce * fireTransform.forward
                );

            currentLaunchForce = minLaunchForce;
        }

        private void AirStrike()
        {
            Vector3 strikePoint;
            if (GetClickPosition(out strikePoint))
            {
                // Instantiate the projectile on all clients
                photonView.RPC
                    (
                    "Fire",
                    RpcTarget.All,
                    strikePoint + Vector3.up*airStrikeInstantiateOffset,
                    Quaternion.Euler(90,0,0),
                    Vector3.zero
                    );
            }
        }

        [PunRPC]
        private void Fire(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            Rigidbody shellInstance = Instantiate(shell, position, rotation);
            shellInstance.velocity = velocity;

            shootingAudio.clip = fireClip;
            shootingAudio.Play();
            
        }
    }
}