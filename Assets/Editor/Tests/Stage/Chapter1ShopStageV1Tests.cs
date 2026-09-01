#if UNITY_EDITOR
using NUnit.Framework;
using Shop;
using Stage;
using UnityEngine;

namespace ProjectBS.EditorTests.Stage
{
    public sealed class Chapter1ShopStageV1Tests
    {
        [Test]
        public void Executor_PreservesLegacyAndRejectsPartialTypedIdentity()
        {
            var executor = new ShopChoiceExecutionExecutor();
            int opens = 0;
            var context = new ChoiceExecutionContext(openShop: _ => { opens++; return true; });

            Assert.IsTrue(executor.TryExecute(new ShopExecutionData(), context, out string legacyError), legacyError);
            Assert.AreEqual(1, opens);

            var partial = new ShopExecutionData { serviceId = "shop.service.chapter1.normal.v1" };
            Assert.IsFalse(executor.TryExecute(partial, context, out string partialError));
            StringAssert.StartsWith("SHOP_IDENTITY_PARTIAL", partialError);
            Assert.AreEqual(1, opens, "Partial identity must fail before any shop mutation/open.");
        }

        [Test]
        public void Executor_AcceptsCompleteTypedIdentity()
        {
            var data = CompleteData();
            bool opened = false;
            var context = new ChoiceExecutionContext(openShop: _ => opened = true);

            Assert.IsTrue(new ShopChoiceExecutionExecutor().TryExecute(data, context, out string error), error);
            Assert.IsTrue(opened);
        }

        [Test]
        public void Ownership_RestoresSameStockAndCompletesExactlyOnce()
        {
            var ownership = new StageShopRuntimeOwnership();
            var stock = new ShopRuntimeData("shop.service.chapter1.normal.v1");

            Assert.IsTrue(ownership.TryStoreStock("receipt|node", stock));
            Assert.IsTrue(ownership.TryGetStock("receipt|node", out ShopRuntimeData restored));
            Assert.AreSame(stock, restored);
            Assert.IsTrue(ownership.TryComplete("completion|node"));
            Assert.IsFalse(ownership.TryComplete("completion|node"));
            Assert.IsTrue(ownership.IsComplete("completion|node"));
        }

        [Test]
        public void RuntimeItemId_IsDeterministicForFixedStockSlot()
        {
            var product = ScriptableObject.CreateInstance<ShopProductSO>();
            try
            {
                product.productId = "product.chapter1.test";
                var first = new ShopRuntimeItem(product, 10, 2, "pool");
                var second = new ShopRuntimeItem(product, 10, 2, "pool");
                Assert.AreEqual(first.runtimeId, second.runtimeId);
                Assert.AreEqual("shop_item_2_product.chapter1.test", first.runtimeId);
            }
            finally
            {
                Object.DestroyImmediate(product);
            }
        }

        private static ShopExecutionData CompleteData() => new()
        {
            serviceId = "shop.service.chapter1.normal.v1",
            stockReservationId = "reservation.shop.chapter1.normal.v1.stock",
            stockReceiptId = "receipt.shop.chapter1.normal.v1.stock",
            nodeCompletionReceiptId = "receipt.shop.chapter1.normal.v1.node"
        };
    }
}
#endif
