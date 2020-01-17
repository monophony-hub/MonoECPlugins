namespace EC_ADVCopy
{
    public partial class ADVCopy
    {
        private int m_copyKind = INIT;

        const int EFFECT = 1;
        const int BUBBLE = 0;

        private HEdit.ADVPart.ScreenEffect m_effect = new HEdit.ADVPart.ScreenEffect();
        private HEdit.ADVPart.SpeechBubbles m_bubble = new HEdit.ADVPart.SpeechBubbles();

        private bool CopyEffect()
        {
            var so = ADVCreate.ADVPartUICtrl.Instance.sortOrder;
            if (so == null) return false;

            if (so.kind == EFFECT)
            {
                Logger.LogDebug("Copy Effect");
                m_effect.Copy((HEdit.ADVPart.ScreenEffect)so);
            }
            else
            {
                Logger.LogDebug("Copy Speech Bubble");
                m_bubble.Copy((HEdit.ADVPart.SpeechBubbles)so);
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
                var effect = new HEdit.ADVPart.ScreenEffect(this.m_effect);
                m_effectUICtrl.AddEffect(effect, false);

                ADVCreate.ADVPartUICtrl.Instance.sortOrder = effect;
                m_effectUICtrl.effectListUICtrl.UpdateUI();
            }
            else
            {
                Logger.LogDebug("Paste SpeechBubble");
                var bubble = new HEdit.ADVPart.SpeechBubbles(this.m_bubble);
                m_textUICtrl.AddText(bubble, false);

                ADVCreate.ADVPartUICtrl.Instance.sortOrder = bubble;
                m_textUICtrl.effectListUICtrl.UpdateUI();
            }

            Logger.LogMessage("Paste effect");
            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
            return true;
        }

    }
}
