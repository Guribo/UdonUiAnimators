#if !COMPILER_UDONSHARP && UNITY_EDITOR
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace TLP.UdonUiAnimators.Runtime
{
    [DefaultExecutionOrder(ExecutionOrder)]
    [TlpDefaultExecutionOrder(typeof(MockAnimator), ExecutionOrder)]
    public class MockAnimator : TlpAnimator
    {
        #region ExecutionOrder
        public override int ExecutionOrderReadOnly => ExecutionOrder;

        [PublicAPI]
        public new const int ExecutionOrder = TlpAnimator.ExecutionOrder + 1;
        #endregion

        [FormerlySerializedAs("animationValue")]
        [PublicAPI]
        public float AnimationValue;

        protected override void Animate(float value) {
            AnimationValue = value;
        }
    }
}
#endif