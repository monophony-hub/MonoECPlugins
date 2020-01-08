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
    public class ADVCopy : BaseUnityPlugin
    {
        //private DropDownUI m_drop = new DropDownUI();

        public const string PluginNameInternal = "EC_ADVCopy";
        public const string GUID = "com.monophony.bepinex.advcopy";
        public const string PluginName = "ADV Copy";
        public const string Version = "0.1";

#if USE_BEPINEX_50
        public static ConfigEntry<KeyboardShortcut> m_CopyKey { get; private set; }
        public static ConfigEntry<KeyboardShortcut> m_PasteKey { get; private set; }
        public static ConfigEntry<KeyboardShortcut> m_SwapKey { get; private set; }
#endif
        private const String SceneName_HEditScene = "HEditScene";
        private bool bEnable = false;
        private HEdit.ADVPart.CharState m_tmpCharState = new HEdit.ADVPart.CharState();
        private int m_tmpCopyIndex = -1;
        private ADVPart.Manipulate.CharaUICtrl m_chUI;

        internal void Awake()
        {
            Logger.LogDebug("Awake");

            SceneManager.sceneLoaded += (_scene, _module) =>
            {
                if (_scene.name == SceneName_HEditScene)
                {
                    bEnable = true;
                    m_chUI = (ADVPart.Manipulate.CharaUICtrl)GameObject.FindObjectOfType(typeof(ADVPart.Manipulate.CharaUICtrl));
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
            Logger.LogDebug("Copy");

            if (ADVCreate.ADVPartUICtrl.Instance.pause == true) return;     // ADV編集モードが停止中
            ChaControl ctrl = ADVCreate.ADVPartUICtrl.Instance.chaControl;
            if (ctrl == null) return;

            CopyCharState(ADVCreate.ADVPartUICtrl.Instance.cut.charStates[ctrl.chaID], m_tmpCharState);
            m_tmpCopyIndex = ctrl.chaID;

            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
            Logger.LogMessage("Copy " + GetCharaName(m_tmpCopyIndex));
        }

        private void Swap()
        {
            Logger.LogDebug("Swap");

            if (ADVCreate.ADVPartUICtrl.Instance.pause == true) return;     // ADV編集モードが停止中
            ChaControl ctrl = ADVCreate.ADVPartUICtrl.Instance.chaControl;
            if (ctrl == null) return;

            if (m_tmpCopyIndex < 0) return;
            if (m_tmpCopyIndex == ctrl.chaID) return;

            var tmpState = new HEdit.ADVPart.CharState();

            CopyCharState(ADVCreate.ADVPartUICtrl.Instance.cut.charStates[ctrl.chaID], tmpState);
            CopyCharState(m_tmpCopyIndex, ctrl.chaID);
            CopyCharState(tmpState, m_tmpCopyIndex);

            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
            Logger.LogMessage("Swap " + GetCharaName(m_tmpCopyIndex) + " and " + GetCharaName(ctrl.chaID));

            m_chUI.Adapt(); //CharStateのデータをキャラに反映
            //ADVCreate.ADVPartUICtrl.Instance.ReloadCut();
        }

        private void Paste()
        {
            Logger.LogDebug("Paste");

            if (ADVCreate.ADVPartUICtrl.Instance.pause == true) return;     // ADV編集モードが停止中
            ChaControl ctrl = ADVCreate.ADVPartUICtrl.Instance.chaControl;
            if (ctrl == null) return;

            if (m_tmpCopyIndex < 0) return;

            CopyCharState(m_tmpCharState, ctrl.chaID);

            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
            Logger.LogMessage("Paste " + GetCharaName(m_tmpCopyIndex) + " into " + GetCharaName(ctrl.chaID));

            m_chUI.Adapt(); //CharStateのデータをキャラに反映
            //ADVCreate.ADVPartUICtrl.Instance.ReloadCut();
        }

        public static string GetCharaName(int i)
        {
            return HEdit.HEditData.Instance.charaFiles[i].parameter.fullname;
        }

        public static void CopyCharState(int idx_src, int idx_dst)
        {
            CopyCharState(ADVCreate.ADVPartUICtrl.Instance.cut.charStates[idx_src],
                ADVCreate.ADVPartUICtrl.Instance.cut.charStates[idx_dst]);
        }

        public static void CopyCharState(HEdit.ADVPart.CharState cs_src, int idx)
        {
            CopyCharState(cs_src, ADVCreate.ADVPartUICtrl.Instance.cut.charStates[idx]);
        }

        public static void CopyCharState(HEdit.ADVPart.CharState cs_src, HEdit.ADVPart.CharState cs_dest)
        {
            //cs_dest.id = cs_src.id;
            cs_dest.visible = cs_src.visible;
            cs_dest.posAndRot.pos = cs_src.posAndRot.pos;
            cs_dest.posAndRot.rot = cs_src.posAndRot.rot;
            cs_dest.pose.Copy(cs_src.pose);
            cs_dest.face.Copy(cs_src.face);
            cs_dest.neckAdd = cs_src.neckAdd;
            cs_dest.coordinate.Copy(cs_src.coordinate);
            for (int i = 0; i < cs_dest.clothes.Length; i++)
            {
                cs_dest.clothes[i] = cs_src.clothes[i];
            }
            for (int j = 0; j < cs_dest.accessory.Length; j++)
            {
                cs_dest.accessory[j] = cs_src.accessory[j];
            }
            for (int k = 0; k < cs_dest.liquid.Length; k++)
            {
                cs_dest.liquid[k] = cs_src.liquid[k];
            }
            cs_dest.visibleSun = cs_src.visibleSun;
            cs_dest.voice.Copy(cs_src.voice);
        }

    }
}