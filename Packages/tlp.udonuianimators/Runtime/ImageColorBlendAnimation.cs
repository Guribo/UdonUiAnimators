using JetBrains.Annotations;
using TLP.UdonUtils.Runtime;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRC.SDKBase;

namespace TLP.UdonUiAnimators.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(ExecutionOrder)]
    public class ImageColorBlendAnimation : TlpAnimator
    {
        protected override int ExecutionOrderReadOnly => ExecutionOrder;

        [PublicAPI]
        public new const int ExecutionOrder = TlpExecutionOrder.UiEnd;

        [FormerlySerializedAs("image")]
        [SerializeField]
        private Image Image;

        [FormerlySerializedAs("minColor")]
        [SerializeField]
        private Color MinColor;

        [FormerlySerializedAs("maxColor")]
        [SerializeField]
        private Color MaxColor;

        protected override void Animate(float value) {
            if (!Utilities.IsValid(Image)) {
                Error($"{nameof(Image)} invalid");
                Pause();
                return;
            }

            Image.color = Color.Lerp(MinColor, MaxColor, value);
        }
    }
}