using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Tanks
{
    public class SettingsController : MonoBehaviour
    {
        private const float VOLUME_MIN = -80;
        private const float VOLUME_MAX = 0.1f;
        private const string BGM_ID = "BGM";
        private const string SFX_ID = "SFX";

        [SerializeField] private AudioMixer mixer;

        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private Button closeButton;

        [SerializeField] private Button enabledBGMButton;
        [SerializeField] private Button disabledBGMButton;

        [SerializeField] private Button enabledSFXButton;
        [SerializeField] private Button disabledSFXButton;

        public void Setup()
        {
            var sfxEnabled = PlayerPrefs.GetInt(SFX_ID, 1) == 1;
            SetSound(SFX_ID, sfxEnabled, disabledSFXButton, enabledSFXButton);

            var bgmEnabled = PlayerPrefs.GetInt(BGM_ID, 1) == 1;
            SetSound(BGM_ID, bgmEnabled, disabledBGMButton, enabledBGMButton);
        }

        private void Start()
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);

            enabledBGMButton.onClick.AddListener(() => SetSound(BGM_ID, false, disabledBGMButton, enabledBGMButton));
            disabledBGMButton.onClick.AddListener(() => SetSound(BGM_ID, true, disabledBGMButton, enabledBGMButton));
            enabledSFXButton.onClick.AddListener(() => SetSound(SFX_ID, false, disabledSFXButton, enabledSFXButton));
            disabledSFXButton.onClick.AddListener(() => SetSound(SFX_ID, true, disabledSFXButton, enabledSFXButton));
        }

        private void OnEnable()
        {
            playerNameInput.text = PlayerPrefs.GetString("PlayerName");
            PhotonNetwork.LocalPlayer.NickName = playerNameInput.text;
            playerNameInput.Select();
            playerNameInput.ActivateInputField();
        }

        private void OnCloseButtonClicked()
        {
            if (string.IsNullOrEmpty(playerNameInput.text)) return;

            gameObject.SetActive(false);

            PlayerPrefs.SetString("PlayerName", playerNameInput.text);

            // TODO: Update photon local player nickname
            PhotonNetwork.LocalPlayer.NickName = playerNameInput.text;
        }

        private void SetSound(string id, bool newValue, Button disabledButton, Button enabledButton)
        {
            PlayerPrefs.SetInt(id, newValue ? 1 : 0);

            mixer.SetFloat(id, newValue ? VOLUME_MAX : VOLUME_MIN);

            disabledButton.gameObject.SetActive(!newValue);
            enabledButton.gameObject.SetActive(newValue);
        }
    }
}