

namespace Shop
{
    /// <summary>
    /// 상점 아이템 희귀도.
    /// 생성 확률 및 가격 보정 등에 사용.
    /// </summary>
    public enum ShopItemRarity
    {
        Common = 0,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// 상점 아이템 카테고리.
    /// 현재는 상점 생성 및 UI 분류 목적.
    /// </summary>
    public enum ShopItemCategory
    {
        Generic = 0,
        Consumable,
        Relic,
        SkillUpgrade,
        Resource,
        Special
    }

    /// <summary>
    /// 구매 후 아이템 사용 흐름.
    /// 실제 효과 시스템 연결 전까지의 임시 플로우 제어용.
    /// </summary>
    public enum ShopItemUseMode
    {
        None = 0,
        Immediate,
        AddToInventory,
        Unlock,
        Upgrade
    }

    /// <summary>
    /// 상점 슬롯 상태.
    /// </summary>
    public enum ShopItemState
    {
        None = 0,
        Available,
        SoldOut,
        Locked
    }

    /// <summary>
    /// 상점 타입.
    /// 이후 바이옴/이벤트/특수 상점 분기에 사용.
    /// </summary>
    public enum ShopType
    {
        Normal = 0,
        Rare,
        Curse,
        Secret,
        Event
    }
}