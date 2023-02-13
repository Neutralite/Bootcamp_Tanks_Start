using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Tanks
{
    public class MainMenuController : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button lobbyButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsController settingsPopup;

        private Action pendingAction;

        private void Start()
        {
            // Connect to photon server
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                PhotonNetwork.ConnectUsingSettings();
            }


            playButton.onClick.AddListener(() => OnConnectionDependantActionClicked(JoinRandomRoom));
            lobbyButton.onClick.AddListener(() => OnConnectionDependantActionClicked(GoToLobbyList));
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);

            settingsPopup.gameObject.SetActive(false);
            settingsPopup.Setup();

            if (!PlayerPrefs.HasKey("PlayerName"))
                PlayerPrefs.SetString("PlayerName", "Player #" + Random.Range(0, 9999));
        }

        private void OnSettingsButtonClicked()
        {
            settingsPopup.gameObject.SetActive(true);
        }

        public void JoinRandomRoom()
        {
            // Connect to a random room
            RoomOptions roomOptions = new RoomOptions 
            { 
                IsOpen = true, 
                MaxPlayers = 4 
            };

            PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: roomOptions);

        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("connected to master");
            PhotonNetwork.AutomaticallySyncScene = false;

            pendingAction?.Invoke();
        }

        public override void OnJoinedRoom()
        {
            SceneManager.LoadScene("RoomLobby");
        }

        private void OnConnectionDependantActionClicked(Action action)
        {
            if(pendingAction != null)
            {
                return;
            }

            pendingAction = action;

            LoadingGraphics.Enable();

            if (PhotonNetwork.IsConnectedAndReady)
            {
                action();
            }
        }

        private void GoToLobbyList()
        {
            SceneManager.LoadSceneAsync("LobbyList");
        }
    }
}