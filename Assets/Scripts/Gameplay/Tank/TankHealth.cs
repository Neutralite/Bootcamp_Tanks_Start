using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;

namespace Tanks
{
    public class TankHealth : MonoBehaviour, IPunObservable, IOnEventCallback
    {
        public const int TANK_DIED_PHOTON_EVENT = 0;

        public float startingHealth = 100f;
        public Slider slider;
        public Image fillImage;
        public Color fullHealthColor = Color.green;
        public Color zeroHealthColor = Color.red;
        public GameObject explosionPrefab;

        private PhotonView photonView;

        private AudioSource explosionAudio;
        private ParticleSystem explosionParticles;
        private float currentHealth;
        private bool dead;

        private void Awake()
        {
            explosionParticles = Instantiate(explosionPrefab).GetComponent<ParticleSystem>();
            explosionAudio = explosionParticles.GetComponent<AudioSource>();
            photonView = GetComponent<PhotonView>();

            explosionParticles.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
            currentHealth = startingHealth;
            dead = false;

            SetHealthUI();
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            SetHealthUI();

            if (currentHealth <= 0f && !dead && photonView.IsMine)
                OnDeath();
        }

        private void SetHealthUI()
        {
            slider.value = currentHealth;

            fillImage.color = Color.Lerp(zeroHealthColor, fullHealthColor, currentHealth / startingHealth);
        }

        /// <summary>
        /// Notify server that this tank died
        /// </summary>
        private void OnDeath()
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            };

            PhotonNetwork.RaiseEvent(TANK_DIED_PHOTON_EVENT, photonView.Owner, raiseEventOptions, SendOptions.SendReliable);

            
        }

        /// <summary>
        /// Synchronize health across clients
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(currentHealth);
            }
            else
            {
                var newHealth = (float)stream.ReceiveNext();
                TakeDamage(currentHealth - newHealth);
            }
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code != TANK_DIED_PHOTON_EVENT)
                return;

            var player = (Player)photonEvent.CustomData;
            if (!Equals(photonView.Owner, player))
                return;

            explosionParticles.transform.position = transform.position;
            explosionParticles.gameObject.SetActive(true);
            explosionParticles.Play();
            explosionAudio.Play();

            dead = true;
            gameObject.SetActive(false);
        }
    }
}