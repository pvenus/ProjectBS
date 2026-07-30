using System;
using System.Collections.Generic;
using Shop;
using UnityEngine;

namespace Stage
{
    [Serializable]
    public sealed class ShopExecutionData : ChoiceExecutionData
    {
        public ShopType shopType = ShopType.Normal;

        public List<ShopItemPoolSO> pools = new();

        [Min(1)]
        public int itemCount = 6;
    }
}
