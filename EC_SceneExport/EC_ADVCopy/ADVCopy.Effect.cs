using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EC_ADVCopy
{
    public partial class ADVCopy
    {
        private int m_copyKind = INIT;

        const int EFFECT = 1;
        const int BUBBLE = 0;

        private HEdit.ADVPart.ScreenEffect m_sf = new HEdit.ADVPart.ScreenEffect();
        private HEdit.ADVPart.SpeechBubbles m_sb = new HEdit.ADVPart.SpeechBubbles();

        private bool CopyEffect()
        {
            var so = ADVCreate.ADVPartUICtrl.Instance.sortOrder;
            if (so == null) return false;

            if (so.kind == EFFECT)
            {
                Logger.LogDebug("Copy Effect");
                m_sf.Copy((HEdit.ADVPart.ScreenEffect)so);
            }
            else
            {
                Logger.LogDebug("Copy Speech Bubble");
                m_sb.Copy((HEdit.ADVPart.SpeechBubbles)so);
            }
            m_copyKind = so.kind;

            Logger.LogMessage("Copy effect");
            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
            return true;
        }

        private bool PasteEffect()
        {
            var so = ADVCreate.ADVPartUICtrl.Instance.sortOrder;

            if (so == null) return false;
            if (m_copyKind == INIT) return false;

            if (m_copyKind == EFFECT)
            {
                Logger.LogDebug("Paste Effect");
                var sf = new HEdit.ADVPart.ScreenEffect(this.m_sf);
                m_effectUICtrl.AddEffect(sf, false);

                ADVCreate.ADVPartUICtrl.Instance.sortOrder = sf;
                m_effectUICtrl.effectListUICtrl.UpdateUI();
            }
            else
            {
                Logger.LogDebug("Paste SpeechBubble");
                var sb = new HEdit.ADVPart.SpeechBubbles(this.m_sb);
                m_textUICtrl.AddText(sb, false);

                ADVCreate.ADVPartUICtrl.Instance.sortOrder = sb;
                m_textUICtrl.effectListUICtrl.UpdateUI();
            }

            Logger.LogMessage("Paste effect");
            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
            return true;
        }

    }
}
