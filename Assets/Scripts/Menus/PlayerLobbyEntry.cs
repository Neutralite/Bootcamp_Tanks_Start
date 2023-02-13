using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks
{
    public class PlayerLobbyEntry : MonoBehaviour
    {
        [SerializeField] private Button readyButton;
        [SerializeField] private GameObject readyText;
        [SerializeField] private Button waitingButton;
        [SerializeField] private GameObject waitingText;

        [SerializeField] private TMP_Text playerName;
        [SerializeField] private Button changeTeamButton;
        [SerializeField] private Image teamHolder;
        [SerializeField] private List<Sprite> teamBackgrounds;

        private Player player;

        public int PlayerTeam
        {
            //Update player team to other clients
            get => player.CustomProperties.ContainsKey("Team") ? (int)player.CustomProperties["Team"] : 0;
            set
            {
                Hashtable hash = new Hashtable { { "Team", value } };
                player.SetCustomProperties(hash);
            }
        }

        public bool IsPlayerReady { get; set; } // TODO: Update player ready status to other clients

        private bool IsLocalPlayer => Equals(player, PhotonNetwork.LocalPlayer); // TODO: Get if this entry belongs to the local player

        public void Setup()
        {
            // TODO: Store and update player information

            if (!IsLocalPlayer)
                Destroy(changeTeamButton);

            UpdateVisuals();
        }

        public void Setup(Player entryPlayer)
        {
            player = entryPlayer;

            if (IsLocalPlayer)
            {
                PlayerTeam = (player.ActorNumber - 1) % PhotonNetwork.CurrentRoom.MaxPlayers;
            }

            playerName.text = player.NickName;

            if (!IsLocalPlayer)
            {
                Destroy(changeTeamButton);
            }

            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            teamHolder.sprite = teamBackgrounds[PlayerTeam];

            waitingText.SetActive(!IsPlayerReady);
            readyText.SetActive(IsPlayerReady);
        }

        private void Start()
        {
            waitingButton.onClick.AddListener(() => OnReadyButtonClick(true));
            readyButton.onClick.AddListener(() => OnReadyButtonClick(false));
            changeTeamButton.onClick.AddListener(OnChangeTeamButtonClicked);

            waitingButton.gameObject.SetActive(IsLocalPlayer);
            readyButton.gameObject.SetActive(false);
        }

        private void OnChangeTeamButtonClicked()
        {
            // Change player team
            PlayerTeam = (PlayerTeam + 1) % PhotonNetwork.CurrentRoom.MaxPlayers;
        }

        private void OnReadyButtonClick(bool isReady)
        {
            waitingButton.gameObject.SetActive(!isReady);
            waitingText.SetActive(!isReady);
            readyButton.gameObject.SetActive(isReady);
            readyText.SetActive(isReady);

            IsPlayerReady = isReady;
        }
    }
}