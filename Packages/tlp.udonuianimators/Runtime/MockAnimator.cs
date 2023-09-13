#if !COMPILER_UDONSHARP && UNITY_EDITOR
using JetBrains.Annotations;

namespace TLP.UdonUiAnimators.Runtime
{
    public class MockAnimator : TlpAnimator
    {
        [PublicAPI] public float animationValue;

        protected override void Animate(float value)
        {
            animationValue = value;
        }
    }
}
#endif