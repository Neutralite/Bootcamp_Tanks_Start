using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tanks
{
    public class TankMovement : MonoBehaviour
    {
        private const string MOVEMENT_AXIS_NAME = "Vertical";
        private const string TURN_AXIS_NAME = "Horizontal";

        public float speed = 12f;
        public float turnSpeed = 180f;
        public AudioSource movementAudio;
        public AudioClip engineIdling;
        public AudioClip engineDriving;
		public float pitchRange = 0.2f;

        private PhotonView photonView;

        private Rigidbody tankRigidbody;
        private float movementInputValue;
        private float turnInputValue;
        private float originalPitch;
        private ParticleSystem[] particleSystems;

        public void GotHit(float explosionForce, Vector3 explosionSource, float explosionRadius)
        {
            tankRigidbody.AddExplosionForce(explosionForce, explosionSource, explosionRadius);
        }

        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
            tankRigidbody = GetComponent<Rigidbody>();

            tankRigidbody.isKinematic = false;
        }

        private void OnEnable()
        {
            tankRigidbody.isKinematic = false;

            movementInputValue = 0f;
            turnInputValue = 0f;

            particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (var system in particleSystems) system.Play();
        }

        private void OnDisable()
        {
            tankRigidbody.isKinematic = true;

            foreach (var system in particleSystems) system.Stop();
        }

        private void Start()
        {
            originalPitch = movementAudio.pitch;
        }

        private void Update()
        {
            if (!photonView.IsMine)
            {
                return;
            }

            movementInputValue = Input.GetAxis (MOVEMENT_AXIS_NAME);
            turnInputValue = Input.GetAxis (TURN_AXIS_NAME);

            EngineAudio();
        }

        private void EngineAudio()
        {
            // If there is no input (the tank is stationary)...
            if (Mathf.Abs (movementInputValue) < 0.1f && Mathf.Abs (turnInputValue) < 0.1f)
            {
                // ... and if the audio source is currently playing the driving clip...
                if (movementAudio.clip == engineDriving)
                {
                    // ... change the clip to idling and play it.
                    movementAudio.clip = engineIdling;
                    movementAudio.pitch = Random.Range (originalPitch - pitchRange, originalPitch + pitchRange);
                    movementAudio.Play();
                }
            }
            else
            {
                // Otherwise if the tank is moving and if the idling clip is currently playing...
                if (movementAudio.clip == engineIdling)
                {
                    // ... change the clip to driving and play.
                    movementAudio.clip = engineDriving;
                    movementAudio.pitch = Random.Range(originalPitch - pitchRange, originalPitch + pitchRange);
                    movementAudio.Play();
                }
            }
        }

        private void FixedUpdate()
        {
            // Only allow owner of this tank to move it
            if (!photonView.IsMine)
            {
                return;
            }

            Move();
            Turn();
        }

        // TODO: Synchronize position and rotation across clients

        private void Move()
        {
            Vector3 movement = transform.forward * movementInputValue * speed * Time.deltaTime;
            tankRigidbody.MovePosition(tankRigidbody.position + movement);
        }

        private void Turn()
        {
            float turn = turnInputValue * turnSpeed * Time.deltaTime;
            Quaternion turnRotation = Quaternion.Euler (0f, turn, 0f);

            tankRigidbody.MoveRotation(tankRigidbody.rotation * turnRotation);
        }
    }
}