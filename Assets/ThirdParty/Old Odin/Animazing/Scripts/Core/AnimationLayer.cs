using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace OldOdin
{

    public class AnimationLayer : MonoBehaviour
    {
        public Action onLateUpdate;
        Animator animator;
        new Animation animation;
        AnimationClip lastClip;
        float lastPriority = 0;
        public float GetPriority() { return lastPriority; }
        float _endTime;
        float endTime { get { return _endTime; } set { _endTime = value; /*Debug.Log("ent time = " + value); */} }
        bool latePlay = false;
        float lastTransitionTime;
        bool hasAnimator;
        float speed {
            get
            {
                if(hasAnimator)
                    return animator.speed;
                return 1;
            }
        }
        [SerializeField]
        public int layer;
        List<AnimationClip> loopingClips0 = new List<AnimationClip>();
        List<AnimationClip> loopingClips = new List<AnimationClip>();
        List<AnimationClip> clips = new List<AnimationClip>();
        AnimationClip defaultClip;
        float defaultTransitionTime = 0.2f;
        List<float> timePriority = new List<float>();
        float lastPlayTime;
        float minDuration;
        public Action<AnimationClip> OnStart;
        bool started = false;

        public void SetDefault(AnimationClip defaultClip, float transitionTime = 0.2f)
        {
            defaultTransitionTime = transitionTime;
            this.defaultClip = defaultClip;
        }

        public void EnableTimePriority(float priority)
        {
            if (!timePriority.Contains(priority))
            {
                timePriority.Add(priority);
            }
        }

        public void DisableTimePriority(float priority)
        {
            if (timePriority.Contains(priority))
            {
                timePriority.Remove(priority);
            }
        }

        void OnEnable()
        {
            lastClip = null;
            lastPriority = 0;
            endTime = 0;
            lastTransitionTime = 0;
        }

        void Awake()
        {
            animator = GetComponent<Animator>();
            animation = GetComponent<Animation>();
            hasAnimator = animator != null;
        }

        public void Play(AnimationClip clip, float priority = 0, float transitionTime = 0.2f, float minDuration = 0)
        {
            if(clip == null)
            {
                Debug.LogError("The animation clip cannot be null.");
                return;
            }
            SaveClip(clip, priority);
            float dt = 0;
            if (!hasAnimator)
                dt = 0.7f;
            if (!IsPlaying(dt))
            {
                if (clip == lastClip || lastClip == null)
                    transitionTime = 0;
                PlayNow(clip, priority, transitionTime, minDuration);
            }
            else if (clip != lastClip)
            {
                if (priority > lastPriority || (priority == lastPriority && !clip.isLooping && timePriority.Contains(priority)))
                {
                    PlayNow(clip, priority, transitionTime, minDuration);
                }
            }else if (clip.isLooping)
            {
                endTime = Time.time + 0.1f;// + extraDuration;
            }
        }

        void SaveClip(AnimationClip clip, float priority)
        {
            if(clip.isLooping)
            {
                if (priority <= 0)
                {
                    if (!loopingClips0.Contains(clip))
                    {
                        loopingClips0.Add(clip);
                    }
                }else if (!loopingClips.Contains(clip))
                {
                    loopingClips.Add(clip);
                }
            }else if (!clips.Contains(clip))
            {
                clips.Add(clip);
            }
        }

        public bool IsPlaying(AnimationClip clip)
        {
            return IsPlaying() && clip == lastClip;
        }

        public bool IsStartingPlaying(AnimationClip clip)
        {
            return IsPlaying(clip) && !started;
        }

        public void Stop()
        {
            endTime = 0;
        }

        public void Stop(AnimationClip clip)
        {
            if (IsPlaying() && clip == lastClip)
            {
                endTime = 0;
            }
        }

        public bool CanPlay(float priority)
        {
            float dt = 0;
            if (!hasAnimator)
                dt = 0.7f;
            if (!IsPlaying(dt))
            {
                return true;
            }
            if (priority > lastPriority)
                return true;
            if (priority == lastPriority && timePriority.Contains(priority))
                return true;
            return false;
        }

        void PlayNow(AnimationClip clip, float priority, float transitionTime, float minDuration)
        {
            //Debug.Log("Play Now " + clip.name + " " + priority);
            if (hasAnimator)
            {
                if(!clip.isLooping )
                {
                   if(defaultClip != null)
                   {
                       animator.CrossFade(defaultClip.name, 0.99f, layer);
                   }
                   else
                   {
                       AnimationClip c = GetIdleClip(clip);
                       if (c != null)
                       {
                           animator.CrossFade(c.name, 0.2f, layer);
                       }
                   }
                }
            }
            lastClip = clip;
            lastTransitionTime = transitionTime;
            latePlay = true;
            if (clip.isLooping)
            {
                endTime = Time.time + 0.1f;// + extraDuration;
            }
            else
            {
                endTime = GetEndTime();// + extraDuration;
            }
            this.minDuration = minDuration;
            lastPriority = priority;
        }

        AnimationClip GetIdleClip(AnimationClip clip)
        {
            for(int i = 0; i < loopingClips0.Count; i++)
            {
                if(loopingClips0[i] != clip)
                {
                    return loopingClips0[i];
                }
            }
            for (int i = 0; i < loopingClips.Count; i++)
            {
                if (loopingClips[i] != clip)
                {
                    return loopingClips[i];
                }
            }
            for (int i = 0; i < clips.Count; i++)
            {
                if (clips[i] != clip)
                {
                    return clips[i];
                }
            }
            return null;
        }

        float GetEndTime()
        {
            return Time.time + GetDuration(lastClip);
        }

        float GetDuration(AnimationClip clip)
        {
            if (clip == null)
                return 0;
            return clip.length / speed;
        }

        void Update()
        {
            if (defaultClip != null)
            {
                Play(defaultClip, -Mathf.Infinity, defaultTransitionTime);
            }
        }

        void LateUpdate()
        {
            if (latePlay)
            {
                if (hasAnimator)
                {
                    if (lastTransitionTime > 0)
                    {
                        animator.CrossFadeInFixedTime(lastClip.name, lastTransitionTime, layer);
                    }
                    else
                    {
                        animator.Play(lastClip.name, layer);
                    }
                }
                else
                {
                    if (lastTransitionTime > 0)
                    {
                        animation.CrossFade(lastClip.name, lastTransitionTime);
                    }
                    else
                    {
                        animation.Play(lastClip.name);
                    }
                }
                latePlay = false;
                if (OnStart != null)
                {
                    OnStart(lastClip);
                }
                lastPlayTime = Time.time;
                started = false;
            }
            else started = true;
            if (onLateUpdate != null)
            {
                onLateUpdate();
            }
        }

        public bool IsPlayingPriority(float priority)
        {
            return this.lastPriority == priority && IsPlaying();
        }

        bool IsPlaying(float dt = 0)
        {
            float t = endTime - (dt * speed);
            if (lastClip != null)
            {
                t = lastPlayTime +  Mathf.Max(minDuration, t - lastPlayTime);
            }
            return Time.time <= t;
        }

    }
}