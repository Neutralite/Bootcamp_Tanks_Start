using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks
{
    public class LobbyListEntry : MonoBehaviour
    {
        [SerializeField] private Button enterButton;
        [SerializeField] private TMP_Text lobbyNameText;
        [SerializeField] private TMP_Text lobbyPlayerCountText;

        private RoomInfo roomInfo;

        private void OnEnterButtonClick()
        {
            LoadingGraphics.Enable();

            // Join target room
            PhotonNetwork.JoinRoom(roomInfo.Name);
        }

        public void Setup(RoomInfo info)
        {
            // Store and update room information
            roomInfo = info;

            lobbyNameText.text = info.Name;
            lobbyPlayerCountText.text = $"{info.PlayerCount}/{info.MaxPlayers}";
        }

        private void Start()
        {
            enterButton.onClick.AddListener(OnEnterButtonClick);
        }
    }
}