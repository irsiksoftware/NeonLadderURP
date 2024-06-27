using Cinemachine;
using NeonLadder.Effects.Text;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace NeonLadder.Managers
{
    public class DialogueManager : MonoBehaviour
    {
        public Dictionary<string, (Canvas canvas, CinemachineVirtualCamera vCam, TextMeshProUGUI speechText)> characterSpeeches;
        public Canvas protagonistSpeechBubbleCanvas;
        public TextMeshProUGUI protagonistSpeechText;
        public CinemachineVirtualCamera protagonistVCam;

        public Canvas wrathSpeechBubbleCanvas;
        public TextMeshProUGUI wrathSpeechText;
        public CinemachineVirtualCamera wrathVCam;

        public Canvas prideSpeechBubbleCanvas;
        public TextMeshProUGUI prideSpeechText;
        public CinemachineVirtualCamera prideVCam;

        public Canvas envySpeechBubbleCanvas;
        public TextMeshProUGUI envySpeechText;
        public CinemachineVirtualCamera envyVCam;

        public Canvas lustSpeechBubbleCanvas;
        public TextMeshProUGUI lustSpeechText;
        public CinemachineVirtualCamera lustVCam;

        public Canvas gluttonySpeechBubbleCanvas;
        public TextMeshProUGUI gluttonySpeechText;
        public CinemachineVirtualCamera gluttonyVCam;

        public Canvas greedSpeechBubbleCanvas;
        public TextMeshProUGUI greedSpeechText;
        public CinemachineVirtualCamera greedVCam;

        public Canvas slothSpeechBubbleCanvas;
        public TextMeshProUGUI slothSpeechText;
        public CinemachineVirtualCamera slothVCam;

        public Canvas finalBossSpeechBubbleCanvas;
        public TextMeshProUGUI finalBossSpeechText;
        public CinemachineVirtualCamera finalBossVCam;

        public Canvas merchantSpeechBubbleCanvas;
        public TextMeshProUGUI merchantSpeechText;
        public CinemachineVirtualCamera merchantVCam;

        private Typewriter typewriterEffect;

        public StringTable currentStringTable { get; set; }

        void Start()
        {
            typewriterEffect = GetComponent<Typewriter>();
            InitializeLanguage(Application.systemLanguage);

            characterSpeeches = new Dictionary<string, (Canvas, CinemachineVirtualCamera, TextMeshProUGUI)>();
            if (protagonistSpeechBubbleCanvas != null)
            {
                characterSpeeches.Add("protagonist", (protagonistSpeechBubbleCanvas, protagonistVCam, protagonistSpeechText));
            }
            if (wrathSpeechBubbleCanvas != null)
            {
                characterSpeeches.Add("wrath", (wrathSpeechBubbleCanvas, wrathVCam, wrathSpeechText));
            }
            if (prideSpeechBubbleCanvas != null)
            {
                characterSpeeches.Add("pride", (prideSpeechBubbleCanvas, prideVCam, prideSpeechText));
            }
            if (envySpeechBubbleCanvas != null)
            {
                characterSpeeches.Add("envy", (envySpeechBubbleCanvas, envyVCam, envySpeechText));
            }
            if (lustSpeechBubbleCanvas != null)
            {
                characterSpeeches.Add("lust", (lustSpeechBubbleCanvas, lustVCam, lustSpeechText));
            }
            if (gluttonySpeechBubbleCanvas != null)
            {
                characterSpeeches.Add("gluttony", (gluttonySpeechBubbleCanvas, gluttonyVCam, gluttonySpeechText));
            }
            if (greedSpeechBubbleCanvas != null)
            {
                characterSpeeches.Add("greed", (greedSpeechBubbleCanvas, greedVCam, greedSpeechText));
            }
            if (slothSpeechBubbleCanvas != null)
            {
                characterSpeeches.Add("sloth", (slothSpeechBubbleCanvas, slothVCam, slothSpeechText));
            }
            if (finalBossSpeechBubbleCanvas != null)
            {
                characterSpeeches.Add("finalboss", (finalBossSpeechBubbleCanvas, finalBossVCam, finalBossSpeechText));
            }
            if (merchantSpeechBubbleCanvas != null)
            {
                characterSpeeches.Add("merchant", (merchantSpeechBubbleCanvas, merchantVCam, merchantSpeechText));
            }
            HideDialogues();
            
        }

        void Awake()
        {
            enabled = false;
        }

        public void Show(string key)
        {
            var target = key.Substring(0, key.IndexOf("-"));

            //get the canvas of with the vcam of hightest priority
            var model = characterSpeeches[target];
            model.canvas.gameObject.SetActive(true);
            string message = GetLocalizedString(key);
            typewriterEffect.DisplayText(message, model.speechText);
        }

        public void HideDialogues()
        {
            foreach (var model in characterSpeeches.Values)
            {
                model.canvas.gameObject.SetActive(false);
            }
        }

        public void InitializeLanguage(SystemLanguage language)
        {
            string stringTableAssetName = GetStringTableAssetNameBySystemLanguage(language);
            LoadStringTable(stringTableAssetName);
        }

        private string GetStringTableAssetNameBySystemLanguage(SystemLanguage language)
        {
            // You may need to adjust the case and naming to match your actual assets
            switch (language)
            {
                case SystemLanguage.ChineseSimplified:
                    return "DialogueStringTable_zh-Hans"; // Adjust the name as per your asset
                case SystemLanguage.Chinese:
                    return "DialogueStringTable_zh-Hans"; // ughhhh
                case SystemLanguage.English:
                    return "DialogueStringTable_en";
                case SystemLanguage.Spanish:
                    return "DialogueStringTable_es";
                case SystemLanguage.Romanian:
                    return "DialogueStringTable_ro";
                case SystemLanguage.Russian:
                    return "DialogueStringTable_ru";
                // Add more cases as needed
                default:
                    return "DialogueStringTable_en"; // Default to English
            }
        }

        private void LoadStringTable(string stringTableAssetName)
        {
            string stringTablePath = $"Localization/{stringTableAssetName}";
            currentStringTable = Resources.Load<StringTable>(stringTablePath);

            if (currentStringTable != null)
            {
                Debug.Log($"Loaded string table: {stringTableAssetName}");
            }
            else
            {
                Debug.LogError($"Failed to load string table at path: {stringTablePath}");
            }
        }


        public string GetLocalizedString(string key)
        {
            if (currentStringTable == null)
            {
                Debug.LogError("String table is not loaded.");
                return "String table not loaded";
            }

            var entry = currentStringTable.GetEntry(key);
            if (entry == null)
            {
                Debug.LogError($"The key '{key}' was not found in the string table.");
                return $"Key '{key}' not found";
            }

            return entry.GetLocalizedString();
        }
    }
}