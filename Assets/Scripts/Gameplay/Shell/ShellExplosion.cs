using Photon.Pun;
using UnityEngine;

namespace Tanks
{
    public class ShellExplosion : MonoBehaviour
    {
        public LayerMask tankMask;
        public ParticleSystem explosionParticles;
        public AudioSource explosionAudio;
        public float maxDamage = 100f;
        public float explosionForce = 1000f;
        public float maxLifeTime = 2f;
        public float explosionRadius = 5f;

        private void Start()
        {
            Destroy(gameObject, maxLifeTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            explosionParticles.transform.parent = null;
            explosionParticles.Play();
            explosionAudio.Play();

            ParticleSystem.MainModule mainModule = explosionParticles.main;
            Destroy(explosionParticles.gameObject, mainModule.duration);
            Destroy(gameObject);

            TryDamageTanks();
        }

        private void TryDamageTanks()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, tankMask);

            for (int i = 0; i < colliders.Length; i++)
            {
                var photonView = colliders[i].GetComponent<PhotonView>();
                if(photonView == null)
                {
                    continue;
                }

                Rigidbody targetRigidbody = photonView.GetComponent<Rigidbody>();
                //tankManager.OnHit(explosionForce, transform.position, explosionRadius,
                //CalculateDamage(targetRigidbody.position));

                photonView.RPC(
                    "OnHit",
                    photonView.Owner,
                    explosionForce,
                    transform.position,
                    explosionRadius,
                    CalculateDamage(targetRigidbody.position));
            }
        }

        private float CalculateDamage(Vector3 targetPosition)
        {
            Vector3 explosionToTarget = targetPosition - transform.position;

            float explosionDistance = explosionToTarget.magnitude;
            float relativeDistance = (explosionRadius - explosionDistance) / explosionRadius;
            float damage = relativeDistance * maxDamage;

            damage = Mathf.Max(0f, damage);

            return damage;
        }
    }
}