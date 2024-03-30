using JetBrains.Annotations;
using TLP.UdonUtils;
using TLP.UdonUtils.Sources;
using UdonSharp;
using UnityEngine;

namespace TLP.UdonUiAnimators.Runtime
{
    /// <summary>
    /// Waits until either a certain time is exceeded or a certain number of frames got rendered faster then a
    /// given threshold. Every Time the threshold is exceeded again the number of counted frames is reset again.
    ///
    /// This makes it so that the animation is triggered only when the game is running smoothly.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(ExecutionOrder)]
    public class PlayOnIdle : TlpBaseBehaviour
    {
        protected override int ExecutionOrderReadOnly => ExecutionOrder;

        [PublicAPI]
        private new const int ExecutionOrder = TlpExecutionOrder.UiStart;

        [SerializeField]
        [Range(0f, 30f)]
        private float MaxWaitDuration = 20f;

        [SerializeField]
        [Range(10f, 144f)]
        private float TargetFrameRate = 30f;

        [SerializeField]
        [Range(1, 1000)]
        private int MinFramesAboveTarget = 30;

        [SerializeField]
        private TlpAnimator TlpAnimator;

        [SerializeField]
        private TimeSource TimeSource;

        private float _startTime;
        private int _framesAboveTarget;

        private void Start() {
            #region TLP_DEBUG
#if TLP_DEBUG
            DebugLog(nameof(Start));
#endif
            #endregion

            if (!TlpAnimator) {
                ErrorAndDisableGameObject($"{nameof(TlpAnimator)} not set");
                return;
            }

            _startTime = TimeSource.Time();

            // ensure that the animation has played the first frame and remains paused until started
            TlpAnimator.Restart();
            TlpAnimator.Pause();
        }

        private void Update() {
            if (TimeSource.Time() - _startTime > MaxWaitDuration) {
                TlpAnimator.Play();
                enabled = false;
                return;
            }

            if (TimeSource.DeltaTime() > 1f / TargetFrameRate) {
                _framesAboveTarget = 0;
                return;
            }

            ++_framesAboveTarget;
            if (_framesAboveTarget < MinFramesAboveTarget) {
                return;
            }

            TlpAnimator.Play();
            enabled = false;
        }
    }
}