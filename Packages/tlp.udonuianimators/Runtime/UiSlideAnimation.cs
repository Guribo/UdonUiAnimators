using JetBrains.Annotations;
using UnityEngine;
using VRC.SDKBase;

namespace TLP.UdonUiAnimators.Runtime
{
    [DefaultExecutionOrder(ExecutionOrder)]
    [TlpDefaultExecutionOrder(typeof(UiSlideAnimation), ExecutionOrder)]
    public class UiSlideAnimation : TlpAnimator
    {
        #region ExecutionOrder
        public override int ExecutionOrderReadOnly => ExecutionOrder;

        [PublicAPI]
        public new const int ExecutionOrder = ImageColorBlendAnimation.ExecutionOrder + 1;
        #endregion

        public RectTransform TargetTransform;
        public Vector3 StartPosition;
        public Vector3 EndPosition;

        protected override bool SetupAndValidate() {
            if (!base.SetupAndValidate()) {
                return false;
            }

            if (!Utilities.IsValid(TargetTransform)) {
                Error($"{nameof(TargetTransform)} not set");
                return false;
            }

            return true;
        }

        protected override void Animate(float value) {
            TargetTransform.anchoredPosition = Vector3.LerpUnclamped(StartPosition, EndPosition, value);
        }
    }
}