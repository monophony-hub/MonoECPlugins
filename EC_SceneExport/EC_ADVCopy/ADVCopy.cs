// 新しいBepinEx 5用には、以下のdefineを有効
// USE_BEPINEX_50

using System;
using BepInEx;

#if USE_BEPINEX_50
using BepInEx.Configuration;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;

namespace EC_ADVCopy
{
    [BepInPlugin(GUID, PluginName, Version)]
    public partial class ADVCopy : BaseUnityPlugin
    {
        public const string PluginNameInternal = "EC_ADVCopy";
        public const string GUID = "com.monophony.bepinex.advcopy";
        public const string PluginName = "ADV Copy";
        public const string Version = "1.0";

#if USE_BEPINEX_50
        public static ConfigEntry<KeyboardShortcut> m_CopyKey { get; private set; }
        public static ConfigEntry<KeyboardShortcut> m_PasteKey { get; private set; }
        public static ConfigEntry<KeyboardShortcut> m_SwapKey { get; private set; }
#endif
        private const String SceneName_HEditScene = "HEditScene";

        private ADVPart.Manipulate.CharaUICtrl m_chUI;
        private ADVPart.Manipulate.ItemUICtrl m_itemUI;
        private ADVPart.Manipulate.EffectUICtrl m_effectUICtrl;
        private ADVPart.Manipulate.TextUICtrl m_textUICtrl;
        private ADVPart.List.ListUICtrl m_listUICtrl;

        private UnityEngine.UI.Toggle m_charaToggle;
        private UnityEngine.UI.Toggle m_itemToggle;
        private UnityEngine.UI.Toggle m_cutToggle;
        private UnityEngine.UI.Toggle m_textToggle;

        private const int INIT = -1;

        // Used by the Unity Engine Scripting API
        internal void Awake()
        {
            Logger.LogDebug("Awake");

            SceneManager.sceneLoaded += (_scene, _module) =>
            {
                if (_scene.name == SceneName_HEditScene)
                {
                    // プラグイン有効
                    this.enabled = true;

                    m_chUI = GameObject.FindObjectOfType<ADVPart.Manipulate.CharaUICtrl>();
                    m_effectUICtrl = GameObject.FindObjectOfType<ADVPart.Manipulate.EffectUICtrl>();
                    m_textUICtrl = GameObject.FindObjectOfType<ADVPart.Manipulate.TextUICtrl>();
                    m_listUICtrl = GameObject.FindObjectOfType<ADVPart.List.ListUICtrl>();
                    m_itemUI = GameObject.FindObjectOfType<ADVPart.Manipulate.ItemUICtrl>();

                    var chBtn = GameObject.Find("ADVPart/Canvas ADVPart/Manipulate/Button Root/Button Chara");
                    m_charaToggle = chBtn.GetComponent<UnityEngine.UI.Toggle>();

                    var itemBtn   = GameObject.Find("ADVPart/Canvas ADVPart/Manipulate/Button Root/Button Item");
                    m_itemToggle  = itemBtn.GetComponent<UnityEngine.UI.Toggle>();

                    var cutBtn    = GameObject.Find("ADVPart/Canvas ADVPart/List/List Tab/Button Cut");
                    m_cutToggle   = cutBtn.GetComponent<UnityEngine.UI.Toggle>();

                    var textBtn   = GameObject.Find("ADVPart/Canvas ADVPart/List/List Tab/Button Text");
                    m_textToggle  = textBtn.GetComponent<UnityEngine.UI.Toggle>();
                }
            };

            SceneManager.sceneUnloaded += (_scene) =>
            {
                if (_scene.name == SceneName_HEditScene)
                {
                    // プラグイン無効
                    this.enabled = false;
                }
            };
        }

        // Used by the Unity Engine Scripting API
        internal void Start()
        {
#if USE_BEPINEX_50
            m_CopyKey = Config.Bind("Keyboard Shortcuts", "Copy", new KeyboardShortcut(KeyCode.C, new KeyCode[] { KeyCode.LeftAlt }), "Copy");
            m_PasteKey = Config.Bind("Keyboard Shortcuts", "Paste", new KeyboardShortcut(KeyCode.V, new KeyCode[] { KeyCode.LeftAlt }), "Paste");
            m_SwapKey = Config.Bind("Keyboard Shortcuts", "Swap", new KeyboardShortcut(KeyCode.S, new KeyCode[] { KeyCode.LeftAlt }), "Swwap");
#endif
            this.enabled = false;
        }

        // Used by the Unity Engine Scripting API
        internal void Update()
        {

#if USE_BEPINEX_50
            if (m_CopyKey.Value.IsDown())
            {
                if (SafeAction(Copy)) return;
            }

            if (m_SwapKey.Value.IsDown())
            {
                if (SafeAction(SwapChara)) return;
            }

            if (m_PasteKey.Value.IsDown())
            {
                if (SafeAction(Paste)) return;
            }

#else
            if (!Input.GetKey(KeyCode.LeftAlt)) return;

            if (Input.GetKeyDown(KeyCode.C))
            {
                this.SafeAction(Copy);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                this.SafeAction(SwapChara);
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                this.SafeAction(Paste);
            }
#endif
        }

        private bool SafeAction(Func<bool> _action)
        {
            try
            {
                if (ADVCreate.ADVPartUICtrl.Instance.pause == true) return false;     // ADV編集モードが停止中
                return _action();
            }
            catch (Exception ex)
            {
                Logger.Log(BepInEx.Logging.LogLevel.Message, "Error: " + ex.Message);
                Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.cancel);
                throw;
            }
        }

        private bool Copy()
        {
            if (this.m_textToggle.isOn)
            {
                CopyEffect();
                return true;
            }

            if (this.m_itemToggle.isOn)
            {
                CopyItem();
                return true;
            }

            if (this.m_charaToggle.isOn)
            {
                CopyChara();
                return true;
            }

            if (this.m_cutToggle.isOn)
            {
                CopyCut();
                return true;
            }

            return false;
        }

        private bool Paste()
        {
            if (this.m_textToggle.isOn)
            {
                PasteEffect();
                return true;
            }

            if (this.m_itemToggle.isOn)
            {
                PasteItem();
                return true;
            }

            if (this.m_charaToggle.isOn)
            {
                PasteChara();
                return true;
            }

            if (this.m_cutToggle.isOn)
            {
                PasteCut();
                return true;
            }

            return false;
        }

    }
}