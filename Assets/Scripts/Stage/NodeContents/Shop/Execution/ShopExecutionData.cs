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

        public string serviceId;
        public string stockReservationId;
        public string stockReceiptId;
        public string nodeCompletionReceiptId;

        public bool HasAnyIdentity =>
            !string.IsNullOrWhiteSpace(serviceId)
            || !string.IsNullOrWhiteSpace(stockReservationId)
            || !string.IsNullOrWhiteSpace(stockReceiptId)
            || !string.IsNullOrWhiteSpace(nodeCompletionReceiptId);

        public bool HasCompleteIdentity =>
            !string.IsNullOrWhiteSpace(serviceId)
            && !string.IsNullOrWhiteSpace(stockReservationId)
            && !string.IsNullOrWhiteSpace(stockReceiptId)
            && !string.IsNullOrWhiteSpace(nodeCompletionReceiptId);
    }
}
