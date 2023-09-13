using System;
using JetBrains.Annotations;
using TLP.UdonUtils;
using TLP.UdonUtils.Extensions;
using UdonSharp;
using UnityEngine;
using VRC.Udon;

namespace TLP.UdonUiAnimators.Runtime
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(ExecutionOrder)]
    public abstract class TlpAnimator : TlpBaseBehaviour
    {
        protected override int ExecutionOrderReadOnly => ExecutionOrder;

        [PublicAPI]
        public new const int ExecutionOrder = TlpExecutionOrder.UiEnd;

        #region Monobehaviour

        private void OnEnable()
        {
#if TLP_DEBUG
            DebugLog(nameof(OnEnable));
#endif

            if (playOnEnable)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        #endregion

        #region Public API

        [PublicAPI]
        public bool loop;

        [PublicAPI]
        public bool playOnEnable = true;

        [PublicAPI]
        public bool activateGameObjectOnPlay;

        [PublicAPI]
        public bool deactivateGameObjectOnPause;

        [PublicAPI]
        public bool deactivateGameObjectOnStop;

        [SerializeField]
        internal AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [PublicAPI]
        public float NormalizedTime
        {
            get => Mathf.Clamp01(UdonMath.Remap(startTime, endTime, 0, 1, m_CurrentTime));
            set =>
                m_CurrentTime = Mathf.Clamp(
                    UdonMath.Remap(0, 1, startTime, endTime, value),
                    startTime,
                    endTime
                );
        }


        [PublicAPI]
        public virtual void Play()
        {
#if TLP_DEBUG
            DebugLog(nameof(Play));
#endif
            animationCurve.preWrapMode = WrapMode.ClampForever;
            animationCurve.postWrapMode = WrapMode.ClampForever;

#if !COMPILER_UDONSHARP
            UpdateInternalTime();
#endif

            enabled = true;
            if (activateGameObjectOnPlay)
            {
                gameObject.SetActive(true);
            }

            UpdateAnimation(0);
        }

        [PublicAPI]
        public void Pause()
        {
#if TLP_DEBUG
            DebugLog(nameof(Pause));
#endif
            enabled = false;
            if (deactivateGameObjectOnPause)
            {
                gameObject.SetActive(false);
            }
        }


        [PublicAPI]
        public void Restart()
        {
#if TLP_DEBUG
            DebugLog(nameof(Restart));
#endif
            m_CurrentTime = startTime;
            Play();
        }

        [PublicAPI]
        public virtual void Stop()
        {
#if TLP_DEBUG
            DebugLog(nameof(Stop));
#endif
            m_CurrentTime = startTime;
            enabled = false;

            if (deactivateGameObjectOnStop)
            {
                gameObject.SetActive(false);
            }
        }

        #endregion

        #region UdonSharpBehaviour

        public override void PostLateUpdate()
        {
#if TLP_DEBUG
            DebugLog(nameof(PostLateUpdate));
#endif
            UpdateAnimation(Time.deltaTime);
        }

        private void UpdateAnimation(float deltaTime)
        {
            _lastUpdate = Time.frameCount;

            m_CurrentTime = Mathf.Max(m_CurrentTime, startTime) + deltaTime;

            if (m_CurrentTime < endTime)
            {
                Animate(animationCurve.Evaluate(m_CurrentTime));
            }
            else if (loop)
            {
                m_CurrentTime = startTime + (m_CurrentTime - endTime);
                Animate(animationCurve.Evaluate(m_CurrentTime));
            }
            else
            {
                AnimationEnd();
            }
        }

        private void AnimationEnd()
        {
            Animate(animationCurve.Evaluate(endTime));
            Stop();
        }

        #endregion

        #region Hooks

        protected abstract void Animate(float value);

        #endregion

        #region Internal

        public override void OnEvent(string eventName)
        {
            switch (eventName)
            {
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

        [SerializeField]
        internal float startTime;

        [SerializeField]
        internal float endTime = 1;

        private float m_CurrentTime;
        private int _lastUpdate;


#if !COMPILER_UDONSHARP
        // TODO [] on AnimationCurves is not yet supported by U#
        private void UpdateStartAndEnd()
        {
            if (animationCurve.length > 0)
            {
                startTime = animationCurve[0].time;
                endTime = animationCurve[animationCurve.length - 1].time;
            }
            else
            {
                startTime = 0;
                endTime = 0;
            }
        }

        private void UpdateInternalTime()
        {
            float currentTime = NormalizedTime;
            UpdateStartAndEnd();
            NormalizedTime = currentTime;
        }
#endif

        #endregion
    }
}