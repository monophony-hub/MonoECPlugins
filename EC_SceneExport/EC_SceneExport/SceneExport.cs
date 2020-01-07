// 新しいBepinEx 5用には、以下のdefineを有効
// #define USE_BEPINEX_50

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Harmony;
#if USE_BEPINEX_50
using HarmonyLib;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

using HEdit;
using YS_Node;

namespace EC_SceneExport
{
    [BepInPlugin(GUID, PluginName, Version)]
    public class SceneExport : BaseUnityPlugin
    {
        public const string PluginNameInternal = "EC_SceneExport";
        public const string GUID = "com.monophony.bepinex.sceneexport";
        public const string PluginName = "Scene Export";
        public const string Version = "0.4";
        public static readonly string ExportPath = Path.Combine(Paths.GameRootPath, @"UserData\SceneExport");

        // partファイル識別用
        private const string MAGIC = "ECP1";
#if USE_BEPINEX_50
        public static ConfigEntry<KeyboardShortcut> PartsExportHotkey { get; private set; }
        public static ConfigEntry<KeyboardShortcut> PartsImportHotkey { get; private set; }
        public static ConfigEntry<bool> EnableCharaRemove { get; private set; }
        public static ConfigEntry<bool> EnablePlugin { get; private set; }
#endif
        private const int PART_KIND_H = 0;
        private const int PART_KIND_ADV = 1;
        private const string FileExtension = "part";

        private const String SceneName_HEditScene = "HEditScene";

        private static bool bEnable = false;

        internal void Awake()
        {
            Logger.LogDebug("Awake");

            SceneManager.sceneUnloaded += (_scene) =>
            {
                Logger.LogDebug("unloaded:" + _scene.name);
                if (_scene.name == SceneName_HEditScene)
                {
                    bEnable = false;
                }
            };

            SceneManager.activeSceneChanged += (_s1, _s2) =>
            {
                Logger.LogDebug("activeSceneChanged:" + _s2.name);
                if (_s2.name == SceneName_HEditScene)
                {
                    bEnable = true;
                }
                else
                {
                    bEnable = false;
                }
            };

#if USE_BEPINEX_50
            // フック関数を処理します
            HarmonyWrapper.PatchAll(typeof(Hooks));
#endif
        }

        internal void Start()
        {
#if USE_BEPINEX_50
            EnablePlugin = Config.Bind("Config", "Enable chara remove", true, "");
            PartsExportHotkey = Config.Bind("Config", "Export Parts", new KeyboardShortcut(KeyCode.E, new KeyCode[] { KeyCode.LeftControl }), "Export all currently loaded parts in the game.");
            PartsImportHotkey = Config.Bind("Config", "Import Parts", new KeyboardShortcut(KeyCode.I, new KeyCode[] { KeyCode.LeftControl }), "Import all files in the exported folder.");
            EnableCharaRemove = Config.Bind("Config", "Enable chara remove (Experimental)", false, "If the importing ADV part over characters, delete the characters.");
#endif
        }

        internal void Update()
        {
#if USE_BEPINEX_50
            if (EnablePlugin.Value == false) return;
#endif
            if (bEnable == false) return;

#if USE_BEPINEX_50
            if (PartsExportHotkey.Value.IsDown())
                this.SafeAction(ExportParts);
            if (PartsImportHotkey.Value.IsDown())
                this.SafeAction(ImportParts);
#else
            if (!Input.GetKey(KeyCode.LeftControl)) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                this.SafeAction(ExportParts);
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                this.SafeAction(ImportParts);
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

        /// <summary>
        /// Exports all currently loaded characters. Probably wont export characters that have not been loaded yet, like characters in a different classroom.
        /// </summary>
        private void ExportParts()
        {
            Logger.LogDebug("Start export");

            if (!Directory.Exists(ExportPath))
            {
                Directory.CreateDirectory(ExportPath);
            }

            // ADVPartのデータをセーブ
            int partCount = 0;
            foreach (KeyValuePair<string, BasePart> keyValuePair in HEditData.Instance.nodes)
            {
                if ((keyValuePair.Value.kind != PART_KIND_H) && (keyValuePair.Value.kind != PART_KIND_ADV))
                {
                    continue;
                }

                // パートの名前を取得
                NodeUI _nodeUI = HEdit.HEditGlobal.Instance.nodeControl.dictNode[keyValuePair.Value.uuId];
                string partName = _nodeUI.nodeBase.name;

                // Kindを文字列にする
                string strKind = "H";
                if (keyValuePair.Value.kind == PART_KIND_ADV)
                {
                    strKind = "ADV";
                }

                string fileName = HEditData.Instance.info.title + "_" + partCount + "_" + strKind + "_" + partName + "." + FileExtension;
                string fullPath = Path.Combine(ExportPath, fileName);

                Logger.LogDebug(fileName);

                partCount++;

                using (FileStream fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                    {
                        //マジックコード
                        binaryWriter.Write(MAGIC);
                        // 名前
                        binaryWriter.Write(partName);

                        // ここから【EroMakeHScene】の一部と同じ
                        // ADV == 1 or H == 0
                        binaryWriter.Write(keyValuePair.Value.kind);
                        // キーを書き込み
                        binaryWriter.Write(keyValuePair.Key);

                        keyValuePair.Value.Save(binaryWriter);
                        Logger.Log(BepInEx.Logging.LogLevel.Message, "Exported " + fullPath);
                    }
                }
            }
            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.ok_s);
        }

        /// <summary>
        /// パートをインポートします。
        /// </summary>
        private void ImportParts()
        {
            Logger.LogDebug("Start import");

            if (!Directory.Exists(ExportPath))
            {
                Logger.LogDebug("No files");
                // フォルダなし
                return;
            }

            DirectoryInfo di = new System.IO.DirectoryInfo(ExportPath);
            FileInfo[] files =
                di.GetFiles("*." + FileExtension, System.IO.SearchOption.TopDirectoryOnly);

            foreach (System.IO.FileInfo f in files)
            {
                BasePart aPart;
                string partTitle;

                // ADVPartにデータをロード
                using (FileStream fileStream = new FileStream(f.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        if (this.ReadPart(binaryReader, f, out aPart, out partTitle) == false)
                        {
                            continue;
                        }
                    }
                }

                // ノードを作成
                NodeBase aNode = HEdit.HEditGlobal.Instance.nodeControl.Create((NodeKind)aPart.kind, 0);

                // パートのIDをノード（画面上の箱）のIDに一致させる
                aPart.uuId = aNode.uid;

                aNode.name = partTitle;
                // パートのテーブルに追加
                HEdit.HEditData.Instance.nodes.Add(aNode.uid, aPart);

                // NodeUIを取得してタイトルを更新
                HEdit.HEditGlobal.Instance.nodeControl.dictNode[aPart.uuId].UpdateTitle();

                Logger.Log(BepInEx.Logging.LogLevel.Message, "Imported " + f.FullName);
            }

            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.ok_s);
        }
    
        private void CheckMap(HEdit.BasePart part)
        {
            Logger.LogDebug("Part mapID:" + part.useMapID);

            //マップID
            if (part.useMapID >= HEditData.Instance.maps.Count)
            {
                Logger.LogDebug("useMapID out of range. Set map id to 0");
                part.useMapID = 0;
            }
        }

        /// <summary>
        /// ADVパートのキャラ数を調整
        /// </summary>
        /// <param name="part"></param>
        private bool CheckADVPart(HEdit.ADVPart part, string partName)
        {
            Logger.LogDebug("check start");

            CheckMap(part);

            int charaNum = 0;

            // キャラチェック
            foreach (HEdit.ADVPart.Cut c in part.cuts)
            {
                int diff = HEditData.Instance.charas.Count - c.charStates.Count;

                if (diff == 0) continue;

                charaNum = c.charStates.Count;

                Logger.LogDebug("Chara num:" + charaNum);

                if (diff > 0)
                {
                    //キャラが不足しているので追加
                    for (int i = 0; i < diff; i++)
                    {
                        //非表示に設定して追加
                        HEdit.ADVPart.CharState cs = new HEdit.ADVPart.CharState();
                        cs.visible = false;
                        c.charStates.Add(cs);
                    }
                }
                else if (diff < 0)
                {
#if USE_BEPINEX_50
                    //キャラが多いので削除
                    if (EnableCharaRemove.Value)
                    {
                        c.charStates.RemoveRange(c.charStates.Count + diff, -diff);
                    }
                    else
#endif
                    {
                        //キャラが多い場合はエラーで抜ける
                        Logger.Log(BepInEx.Logging.LogLevel.Message, "Error: The number of charas (" + c.charStates.Count + ") is over in ADV part." + partName);
                        return false;
                    }
                }
            }

            if (charaNum != 0)
            {
                Logger.Log(BepInEx.Logging.LogLevel.Message, charaNum + " charactors in ADV part:" + partName);
            }

            return true;
        }

#if false
        private bool checkCharaInfo(HEdit.HPart.PartCharaInfo ci)
        {
            if (ci.useCharaID < 0) return true;

            if (ci.useCharaID >= HEditData.Instance.charas.Count) return true;

            return false;
        }

        /// <summary>
        /// Hパートのキャラ数を調整
        /// </summary>
        /// <param name="part"></param>
        private void FixHPart(HEdit.HPart part, string partName)
        {
            int charaNum = 0;

            foreach (HEdit.HPart.Group g in part.groups)
            {

                foreach (var cs in g.infoCharas)
                {
                    Logger.Log(BepInEx.Logging.LogLevel.Debug, "CharaInfo:" + cs.useCharaID);
                    if (charaNum < cs.useCharaID) charaNum = cs.useCharaID;
                }

                g.infoCharas.RemoveAll(checkCharaInfo);
            }

            if (charaNum >= HEditData.Instance.charas.Count)
            {
                Logger.Log(BepInEx.Logging.LogLevel.Message, charaNum + 1+ " charactors in H part:" + partName);
            }
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <param name="partName"></param>
        /// <returns>パートに問題があるときはfalse</returns>
        private bool CheckHPart(HEdit.HPart part, string partName)
        {
            CheckMap(part);

            foreach (HEdit.HPart.Group g in part.groups)
            {
                foreach (var cs in g.infoCharas)
                {
                    if (cs.useCharaID >= HEditData.Instance.charas.Count)
                    {
                        Logger.Log(BepInEx.Logging.LogLevel.Message, "Error: Invalid charaID (" + cs.useCharaID + ") in H part." + partName);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="br"></param>
        /// <param name="f"></param>
        /// <param name="aPart"></param>
        /// <param name="partTitle"></param>
        /// <returns></returns>
        private bool ReadPart(BinaryReader br, FileInfo f, out BasePart aPart, out string partTitle)
        {
            Logger.LogDebug("ReadPart " + f.FullName);

            aPart = null;
            partTitle = null;

            // マジック
            string tmpMagic = br.ReadString();
            if (tmpMagic != SceneExport.MAGIC)
            {
                Logger.Log(BepInEx.Logging.LogLevel.Message, "Error: Invalid file format:" + f.FullName);
                return false;
            }
            // 名前
            partTitle = br.ReadString();

            // Kind
            int kind = br.ReadInt32();

            //キー
            string sb_key = br.ReadString();

            if (kind == 0)
            {
                //H パート
                aPart = new HEdit.HPart();
                aPart.Load(br, HEditData.Instance.dataVersion);
                if (this.CheckHPart((HEdit.HPart)aPart, f.Name) == false)
                {
                    //読み込めないデータ
                    return false;
                }
            }
            else
            {
                //ADV パート
                aPart = new HEdit.ADVPart(0);
                aPart.Load(br, HEditData.Instance.dataVersion);

                if (this.CheckADVPart((HEdit.ADVPart)aPart, f.Name) == false)
                {
                    //読み込めないデータ
                    return false;
                }
            }

            return true;
        }

#if USE_BEPINEX_50
        private static class Hooks
        {
            //[HarmonyPatch(typeof(ADVPart.Manipulate.CharaUICtrl), "SelectChara", typeof(int))]
            [HarmonyPostfix]
            [HarmonyPatch(typeof(YS_Node.NodeControl), "UpdateNodeConditions", typeof(string))]
            private static void UpdateNodeConditionsHook(string uid)
            {
                System.Console.WriteLine("Scene Exposrt:UpdateNode");
                bEnable = true;
            }
        }
#endif
    }
}
