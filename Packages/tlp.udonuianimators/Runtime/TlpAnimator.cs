using JetBrains.Annotations;
using TLP.UdonUtils.Runtime;
using TLP.UdonUtils.Runtime.Extensions;
using TLP.UdonUtils.Runtime.Sources;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;

namespace TLP.UdonUiAnimators.Runtime
{
    /// <summary>
    /// Custom Animator that runs entirely in U# and only executes when needed.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(ExecutionOrder)]
    [TlpDefaultExecutionOrder(typeof(TlpAnimator), ExecutionOrder)]
    public abstract class TlpAnimator : TlpBaseBehaviour
    {
        #region ExecutionOrder
        public override int ExecutionOrderReadOnly => ExecutionOrder;

        [PublicAPI]
        public new const int ExecutionOrder = TlpExecutionOrder.UiStart + 900;
        #endregion

        #region Monobehaviour
        private void OnEnable() {
#if TLP_DEBUG
            DebugLog(nameof(OnEnable));
#endif
            if (!HasStartedOk) {
                ErrorAndDisableGameObject($"{nameof(OnEnable)}: Not initialized");
                return;
            }

            if (PlayOnEnable) {
                Play();
            }
        }
        #endregion

        protected override bool SetupAndValidate() {
            if (!base.SetupAndValidate()) {
                return false;
            }

            if (!Utilities.IsValid(TimeSource)) {
                Error($"{nameof(SetupAndValidate)}: {nameof(TimeSource)} is not set");
                return false;
            }

            return true;
        }


        #region Public API
        [FormerlySerializedAs("loop")]
        [PublicAPI]
        public bool Loop;

        [FormerlySerializedAs("playOnEnable")]
        [PublicAPI]
        public bool PlayOnEnable = true;

        [FormerlySerializedAs("activateGameObjectOnPlay")]
        [PublicAPI]
        public bool ActivateGameObjectOnPlay;

        [FormerlySerializedAs("deactivateGameObjectOnPause")]
        [PublicAPI]
        public bool DeactivateGameObjectOnPause;

        [FormerlySerializedAs("deactivateGameObjectOnStop")]
        [PublicAPI]
        public bool DeactivateGameObjectOnStop;

        [FormerlySerializedAs("animationCurve")]
        [SerializeField]
        internal AnimationCurve AnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [SerializeField]
        protected internal TimeSource TimeSource;

        [PublicAPI]
        public float NormalizedTime
        {
            get => Mathf.Clamp01(UdonMath.Remap(StartTime, EndTime, 0, 1, m_CurrentTime));
            set
            {
                m_CurrentTime = Mathf.Clamp(
                        UdonMath.Remap(0, 1, StartTime, EndTime, value),
                        StartTime,
                        EndTime
                );
                m_CurrentTime = Mathf.Lerp(StartTime, EndTime, value);

                if (enabled) {
                    UpdateAnimation(0);
                }
            }
        }


        /// <summary>
        /// Continues playing from current position
        /// </summary>
        [PublicAPI]
        public virtual void Play() {
#if TLP_DEBUG
            DebugLog(nameof(Play));
#endif
            AnimationCurve.preWrapMode = WrapMode.ClampForever;
            AnimationCurve.postWrapMode = WrapMode.ClampForever;

#if !COMPILER_UDONSHARP
            UpdateInternalTime();
#endif

            enabled = true;
            if (ActivateGameObjectOnPlay) {
                gameObject.SetActive(true);
            }

            UpdateAnimation(0);
        }

        [PublicAPI]
        public void Pause() {
#if TLP_DEBUG
            DebugLog(nameof(Pause));
#endif
            enabled = false;
            if (DeactivateGameObjectOnPause) {
                gameObject.SetActive(false);
            }
        }


        [PublicAPI]
        public void Restart() {
#if TLP_DEBUG
            DebugLog(nameof(Restart));
#endif
            m_CurrentTime = StartTime;
            Play();
        }

        [PublicAPI]
        public virtual void Stop() {
#if TLP_DEBUG
            DebugLog(nameof(Stop));
#endif
            m_CurrentTime = StartTime;
            enabled = false;

            if (DeactivateGameObjectOnStop) {
                gameObject.SetActive(false);
            }
        }
        #endregion

        #region UdonSharpBehaviour
        public override void PostLateUpdate() {
#if TLP_DEBUG
            DebugLog(nameof(PostLateUpdate));
#endif
            UpdateAnimation(TimeSource.DeltaTime());
        }

        private void UpdateAnimation(float deltaTime) {
            m_CurrentTime = Mathf.Max(m_CurrentTime, StartTime) + deltaTime;

            if (m_CurrentTime < EndTime) {
                Animate(AnimationCurve.Evaluate(m_CurrentTime));
            } else if (Loop) {
                m_CurrentTime = StartTime + (m_CurrentTime - EndTime);
                Animate(AnimationCurve.Evaluate(m_CurrentTime));
            } else {
                AnimationEnd();
            }
        }

        private void AnimationEnd() {
            Animate(AnimationCurve.Evaluate(EndTime));
            Stop();
        }
        #endregion

        #region Hooks
        /// <param name="value">value from the animation curve at the current time</param>
        protected abstract void Animate(float value);
        #endregion

        #region Internal
        public override void OnEvent(string eventName) {
            switch (eventName) {
                case nameof(Play):
                    Play();
                    break;
                case nameof(Pause):
                    Pause();
                    break;
                case nameof(Restart):
                    Restart();
                    break;
                case nameof(Stop):
                    Stop();
                    break;
                default:
                    base.OnEvent(eventName);
                    break;
            }
        }

        [FormerlySerializedAs("startTime")]
        [SerializeField]
        internal float StartTime;

        [FormerlySerializedAs("endTime")]
        [SerializeField]
        internal float EndTime = 1;

        private float m_CurrentTime;

#if !COMPILER_UDONSHARP
        // TODO [] on AnimationCurves is not yet supported by U#
        private void UpdateStartAndEnd() {
            if (AnimationCurve.length > 0) {
                StartTime = AnimationCurve[0].time;
                EndTime = AnimationCurve[AnimationCurve.length - 1].time;
            } else {
                StartTime = 0;
                EndTime = 0;
            }
        }

        private void UpdateInternalTime() {
            float currentTime = NormalizedTime;
            UpdateStartAndEnd();
            NormalizedTime = currentTime;
        }
#endif
        #endregion
    }
}