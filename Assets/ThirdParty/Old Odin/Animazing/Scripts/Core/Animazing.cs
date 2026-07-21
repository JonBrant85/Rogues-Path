using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace OldOdin
{
    /// <summary>
    /// Add this component to your avatar in the same place as the Animator component. Then manipulate your animations using only the methods of this class.
    /// </summary>
    public class Animazing : MonoBehaviour
    {
        public enum TiebreakerCriteria {Olders, Newest};

        Dictionary<int, AnimationLayer> layers = new Dictionary<int, AnimationLayer>();

        /// <summary>
        /// Defines the default animation that will be played on that layer when no other animation is playing. This animation will have the lowest priority of all: - Infinity.
        /// </summary>
        /// <param name="layer">The layer where the animation will be played. The animation clip must be on the corresponding layer in the Animator Controller. The base layer = 0</param>
        /// <param name="defaultClip">default animation clip to be played Generally, an idle animation.</param>
        /// <param name="seconds">Time seconds for new animation.</param>
        public void SetLayerDefaultAnimation(int layer, AnimationClip defaultClip, float seconds = 0.2f)
        {
            GetLayer(layer).SetDefault(defaultClip, seconds);
        }

        /// <summary>
        /// Defines the default state that will be played on that layer when no other animation is playing. This state will have the lowest priority of all: - Infinity.
        /// </summary>
        /// <param name="layer">The layer where the state will be played. The state must be on the corresponding layer in the Animator Controller. The base layer = 0</param>
        /// <param name="stateName">state  to be played Generally, an empty state</param>
        /// <param name="seconds">The normalized transition time for new state.  It ranges from 0 to 1.</param>
        public void SetLayerDefaultState(int layer, string stateName, float seconds = 0.2f)
        {
            AnimationClip clip = new AnimationClip();
            clip.name = stateName;
            clip.wrapMode = WrapMode.Loop;
            SetLayerDefaultAnimation(layer, clip, seconds);
        }

        /// <summary>
        /// Plays the animation on the base layer while no higher priority animation is playing. Call this function at Start or Update. The winning animation will start playing on LateUpdate.
        /// </summary>
        /// <param name="clip">Animation clip to be played.</param>
        /// <param name="priority">Animation priority. If two animations are played at the same time, the animation with the highest priority will be displayed.</param>
        /// <param name="seconds">Time seconds for new animation.</param>
        /// <param name="minDuration">Minimum duration of the animation. Useful for avoiding interrupted gestures too early and for extending a looping animation.For death animations, use minDuration = Mathf.Infiniy.</param>
        /// <param name="layer">The layer where the animation will be played. The animation clip must be on the corresponding layer in the Animator Controller. The base layer = 0</param>
        public void Play(AnimationClip clip, float priority, float seconds = 0.2f, float minDuration = 0)
        {
            PlayLayer(0, clip, priority, seconds, minDuration);
        }

        /// <summary>
        /// Plays the animation on the specified layer while no higher priority animation is playing. Call this function at Start or Update. The winning animation will start playing on LateUpdate.
        /// </summary>
        /// <param name="clip">Animation clip to be played.</param>
        /// <param name="priority">Animation priority. If two animations are played at the same time, the animation with the highest priority will be displayed.</param>
        /// <param name="seconds">Time seconds for new animation.</param>
        /// <param name="minDuration">Minimum duration of the animation. Useful for avoiding interrupted gestures too early and for extending a looping animation.For death animations, use minDuration = Mathf.Infiniy.</param>
        /// <param name="layer">The layer where the animation will be played. The animation clip must be on the corresponding layer in the Animator Controller. The base layer = 0</param>
        public void PlayLayer(int layer, AnimationClip clip, float priority, float seconds = 0.2f, float minDuration = 0)
        {
            if (!CanPlayPreviousLayers(priority, layer))
                return;
            GetLayer(layer).Play(clip, priority, seconds, minDuration);
        }

        /// <summary>
        /// Check if the animation clip is currently being played on the layer. It only works for animations that were played using Animazing.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public bool IsPlaying(AnimationClip clip, int layer = 0)
        {
            return GetLayer(layer).IsPlaying(clip);
        }

        /// <summary>
        /// Check if the animation clip is starting to play on the layer. It only works for animations that were played using Animazing.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public bool IsStartingPlaying(AnimationClip clip, int layer = 0)
        {
            return GetLayer(layer).IsStartingPlaying(clip);
        }

        /// <summary>
        /// Returns true if there is any animation being played on the layer with the given priority. Useful to know if a member of a set of animations with the same priority (such as attacks) is playing.
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public bool IsPlaying(float priority, int layer = 0)
        {
            return GetLayer(layer).IsPlayingPriority(priority);
        }

        /// <summary>
        /// Stops the animation currently being played on the layer. Useful to interrupt an action, for example. It only works for animations that were played using Animazing.
        /// </summary>
        /// <param name="layer"></param>
        public void Stop(int layer = 0)
        {
            GetLayer(layer).Stop();
        }

        /// <summary>
        /// Stops the specified animation if it is currently being played on the layer. Useful to interrupt an action, for example.It only works for animations that were played using Animazing.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="layer"></param>
        public void Stop(AnimationClip clip, int layer = 0)
        {
            GetLayer(layer).Stop(clip);
        }

        /// <summary>
        /// Returns true if the animation currently being played on the layer has a lower priority than the specified priority. Useful for performing certain behaviors only when the situation allows.
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public bool CanPlay(float priority, int layer = 0)
        {
            if (!CanPlayPreviousLayers(priority, layer))
                return false;
            return GetLayer(layer).CanPlay(priority);
        }

        /// <summary>
        /// Sets the tiebreaker for animations with the same priority. The default value favors older animations over newer ones.
        /// </summary>
        /// <param name="priority">The priority that will be affected.</param>
        /// <param name="tiebreakerCriteria">Tiebreaker criteria: in favor of the oldest animations, or in favor of the most recent animations.</param>
        /// <param name="layer">The layer that will be affected.</param>
        public void SetTiebreaker(float priority, TiebreakerCriteria tiebreakerCriteria, int layer = 0)
        {
            if(tiebreakerCriteria == TiebreakerCriteria.Olders)
            {
                GetLayer(layer).DisableTimePriority(priority); 
            }
            else
            {
                GetLayer(layer).EnableTimePriority(priority);
            }
        }

        /// <summary>
        /// returns the current priority of the base layer or the specified layer
        /// </summary>
        /// <param name="layer"></param>
        /// <returns>the current priority of the base layer or the specified layer</returns>
        public float GetPriority(int layer = 0)
        {
            return  GetLayer(layer).GetPriority();
        }

        /// <summary>
        /// Adds a listener for when a new animation starts
        /// </summary>
        /// <param name="onStart">callback function</param>
        /// <param name="layer">the listener layer</param>
        public void AddListener(Action<AnimationClip> onStart, int layer = 0)
        {
            GetLayer(layer).OnStart += onStart;
        }

        /// <summary>
        /// Remove a listener for when a new animation starts
        /// </summary>
        /// <param name="onStart">callback function</param>
        /// <param name="layer">the listener layer</param>
        public void RemoveListener(Action<AnimationClip> onStart, int layer = 0)
        {
            GetLayer(layer).OnStart -= onStart;
        }

        bool CanPlayPreviousLayers(float priority, int layer)
        {
            for(int i = 0; i < layer; i++)
            {
                if (!GetLayer(i).CanPlay(priority))
                    return false;
            }
            return true;
        }

        AnimationLayer GetLayer(int index)
        {
            if (layers.ContainsKey(index))
            {
                return layers[index];
            }
            else
            {
                AnimationLayer layer = gameObject.AddComponent<AnimationLayer>();
                layer.layer = index;
                layers.Add(index, layer);
                return layer;
            }
        }

    }
}