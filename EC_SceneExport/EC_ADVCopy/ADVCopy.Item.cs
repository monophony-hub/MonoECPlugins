namespace EC_ADVCopy
{
    public partial class ADVCopy
    {
        private HEdit.ADVPart.ItemState m_itemState;

        private bool CopyItem()
        {
            if (this.m_itemUI.selectIndex == INIT) return false;
            if (this.m_itemUI.itemState == null) return false;

            this.m_itemState = new HEdit.ADVPart.ItemState(this.m_itemUI.itemState);

            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
            Logger.LogMessage("Copy item");

            return true;
        }

        private bool PasteItem()
        {
            if (this.m_itemUI.selectIndex == INIT) return false;
            if (this.m_itemUI.itemState == null) return false;
            if (this.m_itemState == null) return false;

            var itemStateTmp = new HEdit.ADVPart.ItemState(this.m_itemState);

            this.m_itemUI.itemState.oiItem = itemStateTmp.oiItem;
            this.m_itemUI.itemState.parentChara = itemStateTmp.parentChara;
            this.m_itemUI.itemState.parentKind = itemStateTmp.parentKind;

            this.m_itemUI.Adapt();
            this.m_itemUI.UpdateUI();

            Illusion.Game.Utils.Sound.Play(Illusion.Game.SystemSE.sel);
            Logger.LogMessage("Paste item");

            return true;
        }
    }
}