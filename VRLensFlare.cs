using System;
using UnityEngine;

namespace AleVerDes.VRLensFlares
{
    [ExecuteAlways]
    [AddComponentMenu("Rendering/Lens Flare (VR)")]
    public class VRLensFlare : MonoBehaviour
    {
        /// <summary>
        /// Lens flare asset used on this component
        /// </summary>
        public VRLensFlareData LensFlareData;

        /// <summary>
        /// Intensity
        /// </summary>
        [Min(0.0f)] public float Intensity = 1.0f;

        /// <summary>
        /// Distance used to scale the Distance Attenuation Curve
        /// </summary>
        [Min(1e-5f)] public float MaxAttenuationDistance = 100.0f;

        /// <summary>
        /// Distance used to scale the Scale Attenuation Curve
        /// </summary>
        [Min(1e-5f)] public float MaxAttenuationScale = 100.0f;

        /// <summary>
        /// Attenuation by distance
        /// </summary>
        public AnimationCurve DistanceAttenuationCurve = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f));

        /// <summary>
        /// Scale by distance, use the same distance as distanceAttenuationCurve
        /// </summary>
        public AnimationCurve ScaleByDistanceCurve = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f));

        /// <summary>
        /// Attenuation used radially, which allow for instance to enable flare only on the edge of the screen
        /// </summary>
        public AnimationCurve RadialScreenAttenuationCurve = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f));

        /// <summary>
        /// If component attached to a light, attenuation the lens flare per light type
        /// </summary>
        public bool AttenuationByLightShape = true;

        /// <summary>
        /// Enable Occlusion feature
        /// </summary>
        public bool UseOcclusion;

        /// <summary>
        /// Speed of showing and hidding lens flare effect when object its occluded
        /// </summary>
        [Min(float.MinValue)] public float OcclusionSpeed = 5f;

        /// <summary>
        /// Global Scale
        /// </summary>
        [Min(0.0f)] public float Scale = 1.0f;
        
        /// <summary>
        /// If allowOffScreen is true then If the lens flare is outside the screen we still emit the flare on screen
        /// </summary>
        public bool AllowOffScreen;

        /// <summary>
        /// Speed of showing and hidding lens flare effect when object its off screen
        /// </summary>
        [Min(float.MinValue)] public float OffScreenSpeed = 5f;

        [NonSerialized] public float OcclusionProgress;
        [NonSerialized] public float OffScreenProgress;

        private void OnEnable()
        {
            VRLensFlareCore.Instance.AddLensFlare(this);
        }

        private void OnDisable()
        {
            VRLensFlareCore.Instance.RemoveLensFlare(this);
        }

        private void OnValidate()
        {
            if (isActiveAndEnabled && LensFlareData)
            {
                VRLensFlareCore.Instance.AddLensFlare(this);
            }
            else
            {
                VRLensFlareCore.Instance.RemoveLensFlare(this);
            }
        }
    }
}