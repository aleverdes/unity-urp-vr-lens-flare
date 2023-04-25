using System;
using UnityEngine;

namespace AleVerDes.VRLensFlares
{
    [Serializable]
    public sealed class VRLensFlareDataElement
    {
        /// <summary>
        /// Initialize default values
        /// </summary>
        public VRLensFlareDataElement()
        {
            Visible = true;
            LocalIntensity = 1.0f;
            Position = 0.0f;
            PositionOffset = new Vector2(0.0f, 0.0f);
            AngularOffset = 0.0f;
            TranslationScale = new Vector2(1.0f, 1.0f);
            LensFlareTexture = null;
            UniformScale = 1.0f;
            SizeXY = Vector2.one;
            Rotation = 0.0f;
            Tint = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            AutoRotate = false;
            IsFoldOpened = true;

            // Distortion
            EnableRadialDistortion = false;
            TargetSizeDistortion = Vector2.one;
            DistortionCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f, 1.0f, 1.0f), new Keyframe(1.0f, 1.0f, 1.0f, -1.0f));
            DistortionRelativeToCenter = false;
        }

        /// <summary>
        /// Visible
        /// </summary>
        public bool Visible;

        /// <summary>
        /// Position
        /// </summary>
        public float Position;

        /// <summary>
        /// Position offset
        /// </summary>
        public Vector2 PositionOffset;

        /// <summary>
        /// Angular offset
        /// </summary>
        public float AngularOffset;

        /// <summary>
        /// Translation Scale
        /// </summary>
        public Vector2 TranslationScale;

        [Min(0), SerializeField]
        private float _localIntensity;

        /// <summary>
        /// Intensity of this element
        /// </summary>
        public float LocalIntensity
        {
            get => _localIntensity;
            set => _localIntensity = Mathf.Max(0, value);
        }

        /// <summary>
        /// Texture used to for this Lens Flare Element
        /// </summary>
        public Texture LensFlareTexture;

        /// <summary>
        /// Uniform scale applied
        /// </summary>
        public float UniformScale;

        /// <summary>
        /// Scale size on each dimension
        /// </summary>
        public Vector2 SizeXY;
        
        /// <summary>
        /// Preserve Aspect Ratio
        /// </summary>
        public bool PreserveAspectRatio;

        /// <summary>
        /// Local rotation of the texture
        /// </summary>
        public float Rotation;

        /// <summary>
        /// Tint of the texture can be modulated by the light we are attached to
        /// </summary>
        public Color Tint;

        /// <summary>
        /// Rotate the texture relative to the angle on the screen (the rotation will be added to the parameter 'rotation')
        /// </summary>
        public bool AutoRotate;

        /// <summary>
        /// Modulate by light color if the asset is used in a 'SRP Lens Flare Source Override'
        /// </summary>
        public bool ModulateByLightColor;

        /// <summary>
        /// True to use or not the radial distortion.
        /// </summary>
        public bool EnableRadialDistortion;

        /// <summary>
        /// Target size used on the edge of the screen.
        /// </summary>
        public Vector2 TargetSizeDistortion;

        /// <summary>
        /// Curve blending from screen center to the edges of the screen.
        /// </summary>
        public AnimationCurve DistortionCurve;

        /// <summary>
        /// If true the distortion is relative to center of the screen otherwise relative to lensFlare source screen position.
        /// </summary>
        public bool DistortionRelativeToCenter;

#pragma warning disable 0414 // never used (editor state)
        /// <summary>
        /// Internal value use to store the state of minimized or maximized LensFlareElement
        /// </summary>
        [SerializeField] private bool IsFoldOpened;
#pragma warning restore 0414
    }
}