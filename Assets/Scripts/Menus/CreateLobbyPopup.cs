using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

namespace Tanks
{
    public class CreateLobbyPopup : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TMP_InputField lobbyNameInput;

        [SerializeField] private Button enablePrivateLobbyButton;
        [SerializeField] private Button disablePrivateLobbyButton;
        [SerializeField] private Button createButton;
        [SerializeField] private Button closeButton;

        private bool IsPrivate => disablePrivateLobbyButton.gameObject.activeSelf;

        private void OnCreateButtonClicked()
        {
            if (string.IsNullOrEmpty(lobbyNameInput.text)) return;

            // Create room
            RoomOptions roomOptions = new RoomOptions
            {
                IsOpen = true,
                MaxPlayers = 4
            };
            PhotonNetwork.CreateRoom(lobbyNameInput.text, roomOptions, TypedLobby.Default);
        }

        private void OnCloseButtonClicked()
        {
            gameObject.SetActive(false);
        }

        private void Start()
        {
            createButton.onClick.AddListener(OnCreateButtonClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            enablePrivateLobbyButton.onClick.AddListener(() => SetPasswordFields(true));
            disablePrivateLobbyButton.onClick.AddListener(() => SetPasswordFields(false));
        }

        public override void OnEnable()
        {
            lobbyNameInput.text = string.Empty;
            lobbyNameInput.Select();
            lobbyNameInput.ActivateInputField();

            SetPasswordFields(false);

            base.OnEnable();
        }

        private void SetPasswordFields(bool isPrivate)
        {
            enablePrivateLobbyButton.gameObject.SetActive(!isPrivate);
            disablePrivateLobbyButton.gameObject.SetActive(isPrivate);
        }
    }
}