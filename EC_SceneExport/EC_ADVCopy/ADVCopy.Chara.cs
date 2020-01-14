using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EC_ADVCopy
{
    public partial class ADVCopy
    {
        private HEdit.ADVPart.CharState m_tmpCharState = new HEdit.ADVPart.CharState();
        private int m_tmpCopyIndex = INIT;

        private bool CopyChara()
        {
            Logger.LogDebug("Copy");

            ChaControl ctrl = ADVCreate.ADVPartUICtrl.Instance.chaControl;
            if (ctrl == null) return false;

            CopyCharState(ADVCreate.ADVPartUICtrl.Instance.cut.charStates[ctrl.chaID], m_tmpCharState);
            m_tmpCopyIndex = ctrl.chaID;

            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
            Logger.LogMessage("Copy " + GetCharaName(m_tmpCopyIndex));

            return true;
        }

        private bool PasteChara()
        {
            Logger.LogDebug("Paste");

            ChaControl ctrl = ADVCreate.ADVPartUICtrl.Instance.chaControl;
            if (ctrl == null) return false;

            if (m_tmpCopyIndex == INIT) return false;

            CopyCharState(m_tmpCharState, ctrl.chaID);

            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
            Logger.LogMessage("Paste " + GetCharaName(m_tmpCopyIndex) + " into " + GetCharaName(ctrl.chaID));

            m_chUI.Adapt(); //CharStateのデータをキャラに反映
            //ADVCreate.ADVPartUICtrl.Instance.ReloadCut();
            return true;
        }

        private bool SwapChara()
        {
            Logger.LogDebug("Swap");

            ChaControl ctrl = ADVCreate.ADVPartUICtrl.Instance.chaControl;
            if (ctrl == null) return false;

            if (m_tmpCopyIndex < 0) return false;
            if (m_tmpCopyIndex == ctrl.chaID) return false;

            var tmpState = new HEdit.ADVPart.CharState();

            CopyCharState(ADVCreate.ADVPartUICtrl.Instance.cut.charStates[ctrl.chaID], tmpState);
            CopyCharState(m_tmpCopyIndex, ctrl.chaID);
            CopyCharState(tmpState, m_tmpCopyIndex);

            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
            Logger.LogMessage("Swap " + GetCharaName(m_tmpCopyIndex) + " and " + GetCharaName(ctrl.chaID));

            m_chUI.Adapt(); //CharStateのデータをキャラに反映

            return true;
        }

        private static string GetCharaName(int i)
        {
            return HEdit.HEditData.Instance.charaFiles[i].parameter.fullname;
        }

        private static void CopyCharState(int idx_src, int idx_dst)
        {
            CopyCharState(ADVCreate.ADVPartUICtrl.Instance.cut.charStates[idx_src],
                ADVCreate.ADVPartUICtrl.Instance.cut.charStates[idx_dst]);
        }

        private static void CopyCharState(HEdit.ADVPart.CharState cs_src, int idx)
        {
            CopyCharState(cs_src, ADVCreate.ADVPartUICtrl.Instance.cut.charStates[idx]);
        }

        private static void CopyCharState(HEdit.ADVPart.CharState cs_src, HEdit.ADVPart.CharState cs_dest)
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
