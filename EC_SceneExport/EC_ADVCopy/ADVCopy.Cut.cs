namespace EC_ADVCopy
{
    public partial class ADVCopy
    {
        const int CUT_LIMIT = 50;
        private HEdit.ADVPart.Cut m_cut;

        private bool CopyCut()
        {
            if (ADVCreate.ADVPartUICtrl.Instance.cut == null) return false;

            m_cut = new HEdit.ADVPart.Cut(ADVCreate.ADVPartUICtrl.Instance.cut);

            // エンドカットにはしない
            m_cut.endCut = false;
            foreach (HEdit.ADVPart.SpeechBubbles speechBubbles in m_cut.speechBubbles)
            {
                // 選択肢は無効
                speechBubbles.option = false;
            }

            Logger.LogMessage("Copy cut");
            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
            return true;
        }

        private bool PasteCut()
        {
            if (m_cut == null) return false;
            if (ADVCreate.ADVPartUICtrl.Instance.cut == null) return false;

            // 貼り付け先のカットのキャラ人数が、コピー元と異なる場合は、貼り付けない
            if (this.m_cut.charStates.Count != ADVCreate.ADVPartUICtrl.Instance.cut.charStates.Count)
            {
                this.m_cut = null;
                return false;
            }

            // カット 上限チェック
            if (ADVCreate.ADVPartUICtrl.Instance.advPart.cuts.Count >= CUT_LIMIT) return false;

            // カット追加。
            // この際、ADVCreate.ADVPartUICtrl.Instance.cutには今のCutがコピーされた新しいCutインスタンスが入る
            this.m_listUICtrl.cutListUICtrl.Add();

            // 新しいインスタンスに上書き
            ADVCreate.ADVPartUICtrl.Instance.cut.Copy(this.m_cut);
            ADVCreate.ADVPartUICtrl.Instance.ReloadCut();

            Logger.LogMessage("Paste cut");
            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
            return true;
        }
    }
}
