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

            TeamConflictResolution();
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
            // if the room is full, then you can't change teams
            if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                return;
            }

            // if the player is the only one in the room, just pick the next team color
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                PlayerTeam = (PlayerTeam + 1) % PhotonNetwork.CurrentRoom.MaxPlayers;
                return;
            }

            FindNextAvailableTeam();
        }

        private void FindNextAvailableTeam()
        {
            bool unique = false;

            int increment = 1;

            // if it is true that the next times increment team has no player
            while (!unique)
            {
                // check each non-local player in the current room
                foreach (Player item in PhotonNetwork.CurrentRoom.Players.Values)
                {
                    if (!item.IsLocal)
                    {
                        // if even one of the non-local players is a part of the next times increment team,
                        // check to see if any of the non-local players are a part of the next times (increment+1) team
                        unique = ((PlayerTeam + increment) % PhotonNetwork.CurrentRoom.MaxPlayers) != (int)item.CustomProperties["Team"];

                        if (!unique)
                        {
                            increment++;
                            // already found one of the non-local players is a part of the next times increment team,
                            // no need to see if the rest are or aren't
                            break;
                        }
                    }
                }
            }
            // Change player team to the next times increment team
            PlayerTeam = (PlayerTeam + increment) % PhotonNetwork.CurrentRoom.MaxPlayers;
        }

        private void OnReadyButtonClick(bool isReady)
        {
            waitingButton.gameObject.SetActive(!isReady);
            waitingText.SetActive(!isReady);
            readyButton.gameObject.SetActive(isReady);
            readyText.SetActive(isReady);

            IsPlayerReady = isReady;
        }

        private void TeamConflictResolution()
        {
            // in a team conflict, the player who joins last between two needs to change to the next available team

            // first player in the room doesn't ever have to change teams if there is a team conflict
            // wait for a player's initial team to be determined before handling conflicts 
            if (player.ActorNumber > 1 && player.CustomProperties.ContainsKey("Team"))
            {
                // check all players that joint earlier than the current plater
                foreach (Player item in PhotonNetwork.CurrentRoom.Players.Values)
                {
                    if (item.ActorNumber < player.ActorNumber)
                    {
                        // if a player that joined earlier is currently on the same team as the current player,
                        // then find and join the next available team that the current player can be in
                        if ((int)PhotonNetwork.CurrentRoom.GetPlayer(player.ActorNumber).CustomProperties["Team"] == (int)PhotonNetwork.CurrentRoom.GetPlayer(item.ActorNumber).CustomProperties["Team"])
                        {
                            FindNextAvailableTeam();
                        }
                    }
                }
            }
        }
    }
}