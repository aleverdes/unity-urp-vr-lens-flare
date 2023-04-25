using UnityEditor;
using UnityEngine;

namespace AleVerDes.VRLensFlares
{
    [CustomPropertyDrawer(typeof(VRLensFlareDataElement))]
    public class VRLensFlareDataElementPropertyDrawer : PropertyDrawer
    {
        private const float PreviewSize = 80;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var singleLineHeight = EditorGUIUtility.singleLineHeight * 1.1f;
            
            var title = "<Empty>";
            var textureObject = property.FindPropertyRelative("LensFlareTexture").objectReferenceValue;
            if (textureObject)
            {
                title = $"{textureObject.name}";
            }

            // Header
            var headerRect = position;
            headerRect.height = singleLineHeight;

            var foldoutRect = headerRect;
            foldoutRect.width = foldoutRect.height;
            foldoutRect.x += 5;
            property.FindPropertyRelative("IsFoldOpened").boolValue = EditorGUI.Foldout(foldoutRect, property.FindPropertyRelative("IsFoldOpened").boolValue, "");

            var visibleRect = headerRect;
            visibleRect.x += foldoutRect.width - 10;
            visibleRect.width -= foldoutRect.width - 10;
            property.FindPropertyRelative("Visible").boolValue = EditorGUI.ToggleLeft(visibleRect, title, property.FindPropertyRelative("Visible").boolValue);
            
            var contentRect = headerRect;
            contentRect.x -= 15;
            contentRect.width += 15;
            contentRect.y += singleLineHeight * 1.5f;

            if (!property.FindPropertyRelative("IsFoldOpened").boolValue)
            {
                if (textureObject)
                {
                    var previewRect = contentRect;
                    previewRect.width = PreviewSize;
                    previewRect.height = PreviewSize;
                    var style = new GUIStyle
                    {
                        normal =
                        {
                            background = AssetPreview.GetAssetPreview(textureObject)
                        }
                    };
                    EditorGUI.LabelField(previewRect, GUIContent.none, style);
                }

                var previewLensFlareTextureRect = contentRect;
                previewLensFlareTextureRect.x += PreviewSize + 5;
                previewLensFlareTextureRect.width -= PreviewSize + 5;
                previewLensFlareTextureRect.height = singleLineHeight;
                EditorGUI.PropertyField(previewLensFlareTextureRect, property.FindPropertyRelative("LensFlareTexture"), Styles.LensFlareTexture);

                var previewTintRect = previewLensFlareTextureRect;
                previewTintRect.y += singleLineHeight;
                EditorGUI.PropertyField(previewTintRect, property.FindPropertyRelative("Tint"), Styles.Tint);

                var previewLocalIntensityRect = previewTintRect;
                previewLocalIntensityRect.y += singleLineHeight;
                EditorGUI.PropertyField(previewLocalIntensityRect, property.FindPropertyRelative("_localIntensity"), Styles.LocalIntensity);

                var previewUniformScaleRect = previewLocalIntensityRect;
                previewUniformScaleRect.y += singleLineHeight;
                EditorGUI.PropertyField(previewUniformScaleRect, property.FindPropertyRelative("UniformScale"), Styles.UniformScale);
                
                return;
            }
            
            // Image
            var imageLabelRect = contentRect;
            EditorGUI.LabelField(imageLabelRect, "Image", EditorStyles.boldLabel);

            var lensFlareTexturePropertyRect = imageLabelRect;
            lensFlareTexturePropertyRect.y += singleLineHeight;
            EditorGUI.PropertyField(lensFlareTexturePropertyRect, property.FindPropertyRelative("LensFlareTexture"), Styles.LensFlareTexture);

            var preserveAspectRatioPropertyRect = lensFlareTexturePropertyRect;
            preserveAspectRatioPropertyRect.y += singleLineHeight;
            EditorGUI.PropertyField(preserveAspectRatioPropertyRect, property.FindPropertyRelative("PreserveAspectRatio"), Styles.PreserveAspectRatio);
            
            // Color
            var colorLabelRect = preserveAspectRatioPropertyRect;
            colorLabelRect.y += singleLineHeight * 1.5f;
            EditorGUI.LabelField(colorLabelRect, "Color", EditorStyles.boldLabel);

            var tintPropertyRect = colorLabelRect;
            tintPropertyRect.y += singleLineHeight;
            EditorGUI.PropertyField(tintPropertyRect, property.FindPropertyRelative("Tint"), Styles.Tint);

            var modulateByLightColorPropertyRect = tintPropertyRect;
            modulateByLightColorPropertyRect.y += singleLineHeight;
            EditorGUI.PropertyField(modulateByLightColorPropertyRect, property.FindPropertyRelative("ModulateByLightColor"));

            var localIntensityPropertyRect = modulateByLightColorPropertyRect;
            localIntensityPropertyRect.y += singleLineHeight;
            EditorGUI.PropertyField(localIntensityPropertyRect, property.FindPropertyRelative("_localIntensity"), Styles.LocalIntensity);
            
            // Transform
            var transformLabelRect = localIntensityPropertyRect;
            transformLabelRect.y += singleLineHeight * 1.5f;
            EditorGUI.LabelField(transformLabelRect, "Transform", EditorStyles.boldLabel);

            var positionOffsetPropertyRect = transformLabelRect;
            positionOffsetPropertyRect.y += singleLineHeight;
            EditorGUI.PropertyField(positionOffsetPropertyRect, property.FindPropertyRelative("PositionOffset"), Styles.PositionOffset);

            var autoRotatePropertyRect = positionOffsetPropertyRect;
            autoRotatePropertyRect.y += singleLineHeight;
            EditorGUI.PropertyField(autoRotatePropertyRect, property.FindPropertyRelative("AutoRotate"), Styles.AutoRotate);

            var rotationPropertyRect = autoRotatePropertyRect;
            rotationPropertyRect.y += singleLineHeight;
            EditorGUI.PropertyField(rotationPropertyRect, property.FindPropertyRelative("Rotation"), Styles.Rotation);

            var sizeXyPropertyRect = rotationPropertyRect;
            sizeXyPropertyRect.y += singleLineHeight;
            EditorGUI.PropertyField(sizeXyPropertyRect, property.FindPropertyRelative("SizeXY"), Styles.SizeXY);

            var uniformScalePropertyRect = sizeXyPropertyRect;
            uniformScalePropertyRect.y += singleLineHeight;
            EditorGUI.PropertyField(uniformScalePropertyRect, property.FindPropertyRelative("UniformScale"), Styles.UniformScale);
            
            // Axis Transform
            var axisTransformLabelRect = uniformScalePropertyRect;
            axisTransformLabelRect.y += singleLineHeight * 1.5f;
            EditorGUI.LabelField(axisTransformLabelRect, "Axis Transform", EditorStyles.boldLabel);

            var startingPositionPropertyRect = axisTransformLabelRect;
            startingPositionPropertyRect.y += singleLineHeight;
            EditorGUI.PropertyField(startingPositionPropertyRect, property.FindPropertyRelative("Position"), Styles.Position);

            var angularOffsetPropertyRect = startingPositionPropertyRect;
            angularOffsetPropertyRect.y += singleLineHeight;
            EditorGUI.PropertyField(angularOffsetPropertyRect, property.FindPropertyRelative("AngularOffset"), Styles.AngularOffset);

            var translationScalePropertyRect = angularOffsetPropertyRect;
            translationScalePropertyRect.y += singleLineHeight;
            EditorGUI.PropertyField(translationScalePropertyRect, property.FindPropertyRelative("TranslationScale"), Styles.TranslationScale);
            
            // Radial Distortion
            var radialDistortionLabelRect = translationScalePropertyRect;
            radialDistortionLabelRect.y += singleLineHeight * 1.5f;
            EditorGUI.LabelField(radialDistortionLabelRect, "Radial Distortion", EditorStyles.boldLabel);

            var radialDistortionPropertyRect = radialDistortionLabelRect;
            radialDistortionPropertyRect.y += singleLineHeight;
            EditorGUI.PropertyField(radialDistortionPropertyRect, property.FindPropertyRelative("EnableRadialDistortion"), Styles.EnableRadialDistortion);

            var radialDistortionContentRect = radialDistortionPropertyRect;
            
            if (property.FindPropertyRelative("EnableRadialDistortion").boolValue)
            {
                var radialEdgeSizePropertyRect = radialDistortionContentRect;
                radialEdgeSizePropertyRect.y += singleLineHeight;
                EditorGUI.PropertyField(radialEdgeSizePropertyRect, property.FindPropertyRelative("TargetSizeDistortion"), Styles.TargetSizeDistortion);

                var radialEdgeCurvePropertyRect = radialEdgeSizePropertyRect;
                radialEdgeCurvePropertyRect.y += singleLineHeight;
                EditorGUI.PropertyField(radialEdgeCurvePropertyRect, property.FindPropertyRelative("DistortionCurve"), Styles.DistortionCurve);

                var relativeToCenterPropertyRect = radialEdgeCurvePropertyRect;
                relativeToCenterPropertyRect.y += singleLineHeight;
                EditorGUI.PropertyField(relativeToCenterPropertyRect, property.FindPropertyRelative("DistortionRelativeToCenter"), Styles.DistortionRelativeToCenter);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.FindPropertyRelative("IsFoldOpened").boolValue)
            {
                return 120f;
            }
            
            return property.FindPropertyRelative("EnableRadialDistortion").boolValue ? 512 : 450;
        }

        private static class Styles
        {
            public static readonly GUIContent LensFlareTexture 
                = EditorGUIUtility.TrTextContent("Lens Flare Texture", "Texture used to for this Lens Flare Element");
            
            public static readonly GUIContent Tint 
                = EditorGUIUtility.TrTextContent("Tint", "Tint of the texture can be modulated by the light we are attached to");
            
            public static readonly GUIContent PreserveAspectRatio 
                = EditorGUIUtility.TrTextContent("Use Aspect Ration", "Use Aspect Ration");
            
            public static readonly GUIContent LocalIntensity
                = EditorGUIUtility.TrTextContent("Intensity", "Intensity of this element");
            
            public static readonly GUIContent UniformScale
                = EditorGUIUtility.TrTextContent("Uniform Scale", "Uniform scale applied");
            
            public static readonly GUIContent SizeXY
                = EditorGUIUtility.TrTextContent("Scale", "Scale size on each dimension");
            
            public static readonly GUIContent PositionOffset
                = EditorGUIUtility.TrTextContent("Position Offset", "Position Offset");
            
            public static readonly GUIContent AutoRotate
                = EditorGUIUtility.TrTextContent("Auto Rotate", "Rotate the texture relative to the angle on the screen");
            
            public static readonly GUIContent Rotation
                = EditorGUIUtility.TrTextContent("Rotation", "Local rotation of the texture");
            
            public static readonly GUIContent Position
                = EditorGUIUtility.TrTextContent("Position", "Position");
            
            public static readonly GUIContent AngularOffset
                = EditorGUIUtility.TrTextContent("Angular Offset", "Angular Offset");
            
            public static readonly GUIContent TranslationScale
                = EditorGUIUtility.TrTextContent("Translation Scale", "Translation Scale");
            
            public static readonly GUIContent EnableRadialDistortion
                = EditorGUIUtility.TrTextContent("Enable", "True to use or not the radial distortion");
            
            public static readonly GUIContent TargetSizeDistortion
                = EditorGUIUtility.TrTextContent("Radial Edge Size", "Target size used on the edge of the screen");
            
            public static readonly GUIContent DistortionCurve
                = EditorGUIUtility.TrTextContent("Radial Edge Curve", "Curve blending from screen center to the edges of the screen");
            
            public static readonly GUIContent DistortionRelativeToCenter
                = EditorGUIUtility.TrTextContent("Relative To Center", "If true the distortion is relative to center of the screen otherwise relative to lensFlare source screen position");
        }
    }
}