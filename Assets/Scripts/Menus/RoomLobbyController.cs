using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tanks
{
    public class RoomLobbyController : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button closeButton;

        [SerializeField] private PlayerLobbyEntry playerLobbyEntryPrefab;
        [SerializeField] private RectTransform entriesHolder;

        // Create and Delete player entries
        private Dictionary<Player, PlayerLobbyEntry> lobbyEntries;

        private bool IsEveryPlayerReady => lobbyEntries.Values.ToList().TrueForAll(entry => entry.IsPlayerReady);

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            AddLobbyEntry(newPlayer);
            UpdateStartButton();
        }

        private void AddLobbyEntry(Player player)
        {
            var entry = Instantiate(playerLobbyEntryPrefab, entriesHolder);
            entry.Setup();

            // track created player lobby entries
            lobbyEntries.Add(player, entry);

        }

        private void Start()
        {
            LoadingGraphics.Disable();
            PhotonNetwork.AutomaticallySyncScene = true;
            DestroyHolderChildren();

            closeButton.onClick.AddListener(OnCloseButtonClicked);
            startButton.onClick.AddListener(OnStartButtonClicked);
            startButton.gameObject.SetActive(false);

            lobbyEntries = new Dictionary<Player, PlayerLobbyEntry>(PhotonNetwork.CurrentRoom.MaxPlayers);

            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                AddLobbyEntry(player);
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Destroy(lobbyEntries[otherPlayer].gameObject);
            lobbyEntries.Remove(otherPlayer);

            UpdateStartButton();
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            lobbyEntries[targetPlayer].UpdateVisuals();

            UpdateStartButton();
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            UpdateStartButton();
        }

        private void UpdateStartButton()
        {
            // Show start button only to the master client and when all players are ready
            startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && IsEveryPlayerReady);
        }

        private void OnStartButtonClicked()
        {
            // Load gameplay level for all clients
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("trying to start game while not masterclient");
                return;
            }

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel("Gameplay");
        }

        private void OnCloseButtonClicked()
        {
            // TODO: Leave room
            SceneManager.LoadScene("MainMenu");
        }

        private void DestroyHolderChildren()
        {
            for (var i = entriesHolder.childCount - 1; i >= 0; i--)
            {
                Destroy(entriesHolder.GetChild(i).gameObject);
            }
        }
    }
}