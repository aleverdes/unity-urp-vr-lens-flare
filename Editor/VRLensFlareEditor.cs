using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace AleVerDes.VRLensFlares
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VRLensFlare))]
    public class VRLensFlareEditor : Editor
    {
        private SerializedProperty _lensFlareData;
        private SerializedProperty _intensity;
        private SerializedProperty _scale;
        private SerializedProperty _maxAttenuationDistance;
        private SerializedProperty _maxAttenuationScale;
        private SerializedProperty _distanceAttenuationCurve;
        private SerializedProperty _scaleByDistanceCurve;
        private SerializedProperty _attenuationByLightShape;
        private SerializedProperty _radialScreenAttenuationCurve;
        private SerializedProperty _useOcclusion;
        private SerializedProperty _occlusionSpeed;
        private SerializedProperty _allowOffScreen;
        private SerializedProperty _offScreenSpeed;

        private void OnEnable()
        {
            var entryPoint = new PropertyFetcher<VRLensFlare>(serializedObject);
            _lensFlareData = entryPoint.Find("LensFlareData");
            _intensity = entryPoint.Find(x => x.Intensity);
            _scale = entryPoint.Find(x => x.Scale);
            _maxAttenuationDistance = entryPoint.Find(x => x.MaxAttenuationDistance);
            _distanceAttenuationCurve = entryPoint.Find(x => x.DistanceAttenuationCurve);
            _maxAttenuationScale = entryPoint.Find(x => x.MaxAttenuationScale);
            _scaleByDistanceCurve = entryPoint.Find(x => x.ScaleByDistanceCurve);
            _attenuationByLightShape = entryPoint.Find(x => x.AttenuationByLightShape);
            _radialScreenAttenuationCurve = entryPoint.Find(x => x.RadialScreenAttenuationCurve);
            _useOcclusion = entryPoint.Find(x => x.UseOcclusion);
            _occlusionSpeed = entryPoint.Find(x => x.OcclusionSpeed);
            _allowOffScreen = entryPoint.Find(x => x.AllowOffScreen);
            _offScreenSpeed = entryPoint.Find(x => x.OffScreenSpeed);
        }

        /// <summary>
        /// Implement this function to make a custom inspector
        /// </summary>
        public override void OnInspectorGUI()
        {
            var lensFlare = _intensity.serializedObject.targetObject as VRLensFlare;

            if (!lensFlare)
            {
                return;
            }
            
            var attachedToLight = false;
            var lightIsDirLight = false;
            if (lensFlare.TryGetComponent(out Light light))
            {
                attachedToLight = true;
                lightIsDirLight = light.type == LightType.Directional;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(Styles.GeneralData.text, EditorStyles.boldLabel);
            {
                EditorGUILayout.PropertyField(_lensFlareData, Styles.LensFlareData);
                EditorGUILayout.PropertyField(_intensity, Styles.Intensity);
                EditorGUILayout.PropertyField(_scale, Styles.Scale);
                if (!lightIsDirLight)
                {
                    if (attachedToLight)
                    {
                        EditorGUILayout.PropertyField(_attenuationByLightShape, Styles.AttenuationByLightShape);
                    }

                    EditorGUILayout.PropertyField(_maxAttenuationDistance, Styles.MaxAttenuationDistance);
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(_distanceAttenuationCurve, Styles.DistanceAttenuationCurve);
                    --EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(_maxAttenuationScale, Styles.MaxAttenuationScale);
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(_scaleByDistanceCurve, Styles.ScaleByDistanceCurve);
                    --EditorGUI.indentLevel;
                }
                EditorGUILayout.PropertyField(_radialScreenAttenuationCurve, Styles.RadialScreenAttenuationCurve);
            }
            EditorGUILayout.LabelField(Styles.OcclusionData.text, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_useOcclusion, Styles.EnableOcclusion);
            if (_useOcclusion.boolValue)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(_occlusionSpeed, Styles.OcclusionSpeed);
                --EditorGUI.indentLevel;
            }
            EditorGUILayout.PropertyField(_allowOffScreen, Styles.AllowOffScreen);
            if (_allowOffScreen.boolValue)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(_offScreenSpeed, Styles.OffScreenSpeed);
                --EditorGUI.indentLevel;
            }

            if (EditorGUI.EndChangeCheck())
            {
                _lensFlareData.serializedObject.ApplyModifiedProperties();
            }
        }

        private static class Styles
        {
            public static readonly GUIContent GeneralData = EditorGUIUtility.TrTextContent("General");
            public static readonly GUIContent OcclusionData = EditorGUIUtility.TrTextContent("Occlusion");

            public static readonly GUIContent LensFlareData = EditorGUIUtility.TrTextContent("Lens Flare Data", "Specifies the SRP Lens Flare Data asset this component uses.");
            public static readonly GUIContent Intensity = EditorGUIUtility.TrTextContent("Intensity", "Sets the intensity of the lens flare.");
            public static readonly GUIContent Scale = EditorGUIUtility.TrTextContent("Scale", "Sets the scale of the lens flare.");
            public static readonly GUIContent MaxAttenuationDistance = EditorGUIUtility.TrTextContent("Attenuation Distance", "Sets the distance, in meters, between the start and the end of the Distance Attenuation Curve.");
            public static readonly GUIContent DistanceAttenuationCurve = EditorGUIUtility.TrTextContent("Attenuation Distance Curve", "Specifies the curve that reduces the effect of the lens flare  based on the distance between the GameObject this asset is attached to and the Camera.");
            public static readonly GUIContent MaxAttenuationScale = EditorGUIUtility.TrTextContent("Scale Distance", "Sets the distance, in meters, between the start and the end of the Scale Attenuation Curve.");
            public static readonly GUIContent ScaleByDistanceCurve = EditorGUIUtility.TrTextContent("Scale Distance Curve", "Specifies the curve used to calculate the size of the lens flare based on the distance between the GameObject this asset is attached to, and the Camera.");
            public static readonly GUIContent AttenuationByLightShape = EditorGUIUtility.TrTextContent("Attenuation By Light Shape", "When enabled, if the component is attached to a light, automatically reduces the effect of the lens flare based on the type and shape of the light.");
            public static readonly GUIContent RadialScreenAttenuationCurve = EditorGUIUtility.TrTextContent("Screen Attenuation Curve", "Specifies the curve that modifies the intensity of the lens flare based on its distance from the edge of the screen.");
            public static readonly GUIContent EnableOcclusion = EditorGUIUtility.TrTextContent("Enable", "When enabled, the renderer uses the depth buffer to occlude (partially or completely) the lens flare. Partial occlusion also occurs when the lens flare is partially offscreen.");
            public static readonly GUIContent AllowOffScreen = EditorGUIUtility.TrTextContent("Allow Off Screen", "When enabled, allows the lens flare to affect the scene even when it is outside the Camera's field of view.");
            public static readonly GUIContent OcclusionSpeed = EditorGUIUtility.TrTextContent("Occlusion Speed", "Speed of showing and hidding lens flare effect when object its occluded.");
            public static readonly GUIContent OffScreenSpeed = EditorGUIUtility.TrTextContent("Off Screen Speed", "Speed of showing and hidding lens flare effect when object its off screen.");
        }
    }
}