#if !COMPILER_UDONSHARP && UNITY_EDITOR
using JetBrains.Annotations;
using UnityEngine.Serialization;

namespace TLP.UdonUiAnimators.Runtime
{
    public class MockAnimator : TlpAnimator
    {
        [FormerlySerializedAs("animationValue")]
        [PublicAPI]
        public float AnimationValue;

        protected override void Animate(float value) {
            AnimationValue = value;
        }
    }
}
#endif