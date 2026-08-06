using System;
using Bless;

namespace Shrine
{
    /// <summary>
    /// Deprecated wrapper.
    /// 기존 Shrine 전용 Bless Runtime은 공용 BlessRuntimeData.BlessEntry 구조로 통합되었다.
    /// 남아있는 참조 호환용 Wrapper.
    /// </summary>
    [Obsolete("Use BlessRuntimeData.BlessEntry instead.")]
    [Serializable]
    public class ShrineBlessingRuntime : BlessRuntimeData.BlessEntry
    {
        public ShrineBlessingRuntime()
            : base(null)
        {
        }

        public ShrineBlessingRuntime(
            BlessSO source,
            int slotIndex,
            string generatedFromPoolId = null)
            : base(
                source,
                generatedFromPoolId,
                slotIndex)
        {
        }
    }
}