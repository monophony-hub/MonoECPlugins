// 新しいBepinEx 5用には、以下のdefineを有効
//#define USE_BEPINEX_50

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
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
        public const string Version = "0.2";

#if USE_BEPINEX_50
        public static ConfigEntry<KeyboardShortcut> m_CopyKey { get; private set; }
        public static ConfigEntry<KeyboardShortcut> m_PasteKey { get; private set; }
        public static ConfigEntry<KeyboardShortcut> m_SwapKey { get; private set; }
#endif
        private const String SceneName_HEditScene = "HEditScene";
        private bool bEnable = false;

        private ADVPart.Manipulate.CharaUICtrl m_chUI;
        private ADVPart.Manipulate.EffectUICtrl m_effectUICtrl;
        private ADVPart.Manipulate.TextUICtrl m_textUICtrl;
        private ADVPart.List.ListUICtrl m_listUICtrl;

        const int INIT = -1;

        internal void Awake()
        {
            Logger.LogDebug("Awake");

            SceneManager.sceneLoaded += (_scene, _module) =>
            {
                if (_scene.name == SceneName_HEditScene)
                {
                    bEnable = true;
                    m_chUI = GameObject.FindObjectOfType<ADVPart.Manipulate.CharaUICtrl>();
                    m_effectUICtrl = GameObject.FindObjectOfType<ADVPart.Manipulate.EffectUICtrl>();
                    m_textUICtrl = GameObject.FindObjectOfType<ADVPart.Manipulate.TextUICtrl>();
                    m_listUICtrl = GameObject.FindObjectOfType<ADVPart.List.ListUICtrl>();
                }
            };

            SceneManager.sceneUnloaded += (_scene) =>
            {
                if (_scene.name == SceneName_HEditScene)
                {
                    bEnable = false;
                }
            };
        }

        internal void Start()
        {
#if USE_BEPINEX_50
            m_CopyKey = Config.Bind("Keyboard Shortcuts", "Copy", new KeyboardShortcut(KeyCode.C, new KeyCode[] { KeyCode.LeftAlt }), "Copy chara info.");
            m_PasteKey = Config.Bind("Keyboard Shortcuts", "Paste", new KeyboardShortcut(KeyCode.V, new KeyCode[] { KeyCode.LeftAlt }), "Paste chara info.");
            m_SwapKey = Config.Bind("Keyboard Shortcuts", "Swap", new KeyboardShortcut(KeyCode.S, new KeyCode[] { KeyCode.LeftAlt }), "Swwap chara info.");
#endif
        }

        internal void Update()
        {
            if (bEnable == false) return;
            //            if (m_nodeSettingCanvas.CgNode.interactable == false) return;

#if USE_BEPINEX_50
            if (m_CopyKey.Value.IsDown())
                SafeAction(Copy);

            if (m_SwapKey.Value.IsDown())
                SafeAction(Swap);

            if (m_PasteKey.Value.IsDown())
                SafeAction(Paste);

#else
            if (!Input.GetKey(KeyCode.LeftAlt)) return;

            if (Input.GetKeyDown(KeyCode.C))
            {
                this.SafeAction(Copy);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                this.SafeAction(Swap);
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                this.SafeAction(Paste);
            }
#endif
        }

        private void SafeAction(Action _action)
        {
            try
            {
                _action();
            }
            catch (Exception ex)
            {
                Logger.Log(BepInEx.Logging.LogLevel.Message, "Error: " + ex.Message);
                Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.cancel);
                throw;
            }
        }

        private void Copy()
        {
            if (ADVCreate.ADVPartUICtrl.Instance.pause == true) return;     // ADV編集モードが停止中

            if (CopyEffect()) return;
            if (CopyChara()) return;
            if (CopyCut()) return;
        }

        private void Paste()
        {
            if (ADVCreate.ADVPartUICtrl.Instance.pause == true) return;     // ADV編集モードが停止中

            if (PasteEffect()) return;
            if (PasteChara()) return;
            if (PasteCut()) return;
        }

        private void Swap()
        {
            if (ADVCreate.ADVPartUICtrl.Instance.pause == true) return;     // ADV編集モードが停止中

            if (SwapChara()) return;
        }

    }
}