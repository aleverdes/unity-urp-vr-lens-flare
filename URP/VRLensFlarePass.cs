using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AleVerDes.VRLensFlares
{
    public class VRLensFlarePass : ScriptableRenderPass
    {
        private const string RenderPostProcessingTag = "VRLensFlareRenderPostProcessingEffects";

        private static readonly ProfilingSampler ProfilingRenderPostProcessing = new ProfilingSampler(RenderPostProcessingTag);
        private static readonly MaterialPropertyBlock MaterialPropertyBlock = new MaterialPropertyBlock();

        private RenderTextureDescriptor _cameraTargetDescriptor;
        private Material _lensFlareMaterial;
        private LayerMask _occlusionLayerMask;

        private CommandBuffer _cmd;
        private ScriptableRenderContext _context;

        private Camera _camera;

        private Matrix4x4 _cameraMatrix;
        private Matrix4x4 _leftEyeMatrix;
        private Matrix4x4 _rightEyeMatrix;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            _cmd = CommandBufferPool.Get();
            _context = context;

            using (new ProfilingScope(_cmd, ProfilingRenderPostProcessing))
            {
                Render(ref renderingData);
            }

            _context.ExecuteCommandBuffer(_cmd);
            CommandBufferPool.Release(_cmd);
        }

        public void Setup(RenderTextureDescriptor cameraTargetDescriptor, Material lensFlareMaterial, LayerMask occlusionLayerMask)
        {
            _cameraTargetDescriptor = cameraTargetDescriptor;
            _lensFlareMaterial = lensFlareMaterial;
            _occlusionLayerMask = occlusionLayerMask;
        }

        private void Render(ref RenderingData renderingData)
        {
            if (VRLensFlareCore.Instance.IsEmpty)
            {
                return;
            }

            ref var cameraData = ref renderingData.cameraData;
            _camera = cameraData.camera;

            var screenSize = new Vector2(_cameraTargetDescriptor.width, _cameraTargetDescriptor.height);
            var screenRatio = screenSize.x / screenSize.y;
            var vScreenRatio = new Vector2(screenRatio, 1.0f);

            _cameraMatrix = GL.GetGPUProjectionMatrix(_camera.projectionMatrix, true) * _camera.worldToCameraMatrix;
            _leftEyeMatrix = VRLensFlareCore.GetMatrixFromEye(_camera, Camera.StereoscopicEye.Left);
            _rightEyeMatrix = VRLensFlareCore.GetMatrixFromEye(_camera, Camera.StereoscopicEye.Right);

            if (!Application.isEditor)
            {
                _cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            }

            foreach (var lensFlare in VRLensFlareCore.Instance.GetLensFlares())
            {
                if (!lensFlare.enabled || !lensFlare.LensFlareData || lensFlare.Scale < float.Epsilon || lensFlare.Intensity < float.Epsilon)
                {
                    continue;
                }

                var light = lensFlare.GetComponent<Light>();

                var worldPosition = lensFlare.transform.position;
                var isDirLight = false;
                if (light && light.type == LightType.Directional)
                {
                    worldPosition = -light.transform.forward * _camera.farClipPlane;
                    isDirLight = true;
                }

                CalculateRayVisibility(lensFlare, isDirLight);
                if (lensFlare.OcclusionProgress >= 1f - float.Epsilon)
                {
                    continue;
                }

                var viewportPosition = VRLensFlareCore.WorldToViewport(Camera.MonoOrStereoscopicEye.Mono, _camera, !isDirLight, true, _cameraMatrix, worldPosition);

                if (viewportPosition.z < 0.0f)
                {
                    continue;
                }

                if (!lensFlare.AllowOffScreen)
                {
                    if (viewportPosition.x is < 0.0f or > 1.0f || viewportPosition.y is < 0.0f or > 1.0f)
                    {
                        continue;
                    }
                }

                var diffToObject = worldPosition - _camera.transform.position;
                if (Vector3.Dot(_camera.transform.forward, diffToObject) < 0.0f)
                {
                    continue;
                }

                var distanceToObject = diffToObject.magnitude;
                var distanceFactor = distanceToObject / lensFlare.MaxAttenuationDistance;
                var scaleFactor = distanceToObject / lensFlare.MaxAttenuationScale;

                var distanceAttenuation =
                    !isDirLight && lensFlare.DistanceAttenuationCurve.length > 0 ? lensFlare.DistanceAttenuationCurve.Evaluate(distanceFactor) : 1.0f;

                var scaleByDistance =
                    !isDirLight && lensFlare.ScaleByDistanceCurve.length >= 1 ? lensFlare.ScaleByDistanceCurve.Evaluate(scaleFactor) : 1.0f;

                var globalColorModulation = Color.white;

                if (light && lensFlare.AttenuationByLightShape)
                {
                    globalColorModulation *= VRLensFlareCore.GetLensFlareLightAttenuation(light, _camera, -diffToObject.normalized);
                }

                globalColorModulation *= distanceAttenuation;

                var dir = (_camera.transform.position - lensFlare.transform.position).normalized;
                var screenPosZ = VRLensFlareCore.WorldToViewport(Camera.MonoOrStereoscopicEye.Mono, _camera, !isDirLight, true, _cameraMatrix, worldPosition + dir * 0.05f);

                foreach (var element in lensFlare.LensFlareData.Elements)
                {
                    if (!element.Visible || !element.LensFlareTexture || element.LocalIntensity < float.Epsilon || element.SizeXY.magnitude < float.Epsilon)
                    {
                        continue;
                    }

                    var colorModulation = globalColorModulation;
                    if (light && element.ModulateByLightColor)
                    {
                        if (light.useColorTemperature)
                        {
                            colorModulation *= light.color * Mathf.CorrelatedColorTemperatureToRGB(light.colorTemperature);
                        }
                        else
                        {
                            colorModulation *= light.color;
                        }
                    }

                    var curColor = colorModulation;
                    var screenPos = new Vector2(2.0f * viewportPosition.x - 1.0f, 1.0f - 2.0f * viewportPosition.y);
                    var radPos = new Vector2(Mathf.Abs(screenPos.x), Mathf.Abs(screenPos.y));
                    var radius = Mathf.Max(radPos.x, radPos.y); // l1 norm (instead of l2 norm)
                    var radialsScaleRadius = lensFlare.RadialScreenAttenuationCurve.length > 0 ? lensFlare.RadialScreenAttenuationCurve.Evaluate(radius) : 1.0f;

                    var currentIntensity = lensFlare.Intensity * element.LocalIntensity * radialsScaleRadius * distanceAttenuation;

                    if (currentIntensity <= 0.0f)
                    {
                        continue;
                    }

                    var texture = element.LensFlareTexture;
                    var usedAspectRatio = element.PreserveAspectRatio ? texture.height / (float)texture.width : 1.0f;

                    var rotation = element.Rotation;

                    Vector2 elementSize;
                    if (element.PreserveAspectRatio)
                    {
                        if (usedAspectRatio >= 1.0f)
                        {
                            elementSize = new Vector2(element.SizeXY.x / usedAspectRatio, element.SizeXY.y);
                        }
                        else
                        {
                            elementSize = new Vector2(element.SizeXY.x, element.SizeXY.y * usedAspectRatio);
                        }
                    }
                    else
                    {
                        elementSize = new Vector2(element.SizeXY.x, element.SizeXY.y);
                    }

                    var combinedScale = scaleByDistance * 0.1f * element.UniformScale * lensFlare.Scale;
                    var localSize = elementSize * combinedScale;

                    curColor *= element.Tint;
                    curColor *= currentIntensity;
                    curColor *= 1f - lensFlare.OcclusionProgress;

                    var angularOffset = SystemInfo.graphicsUVStartsAtTop ? element.AngularOffset : -element.AngularOffset;
                    var globalCos0 = Mathf.Cos(-angularOffset * Mathf.Deg2Rad);
                    var globalSin0 = Mathf.Sin(-angularOffset * Mathf.Deg2Rad);

                    var position = 2.0f * element.Position;

                    var leftEyePosition = VRLensFlareCore.GetScreenPosition(Camera.MonoOrStereoscopicEye.Left, lensFlare, _camera, true, _leftEyeMatrix);
                    var rightEyePosition = VRLensFlareCore.GetScreenPosition(Camera.MonoOrStereoscopicEye.Right, lensFlare, _camera, true, _rightEyeMatrix);

                    var rayOff = VRLensFlareCore.GetLensFlareRayOffset(screenPos, position, globalCos0, globalSin0);
                    if (element.EnableRadialDistortion)
                    {
                        var rayOff0 = VRLensFlareCore.GetLensFlareRayOffset(screenPos, 0.0f, globalCos0, globalSin0);
                        localSize = ComputeLocalSize(rayOff, rayOff0, localSize, element.DistortionCurve);
                    }

                    var flareData0 =
                        VRLensFlareCore.GetFlareData(screenPos, element.TranslationScale, rayOff, vScreenRatio, rotation, position, angularOffset, element.PositionOffset, element.AutoRotate);

                    MaterialPropertyBlock.SetTexture("_BaseMap", element.LensFlareTexture);
                    MaterialPropertyBlock.SetColor("_BaseColor", curColor);
                    MaterialPropertyBlock.SetVector("_EyePositions", new Vector4(leftEyePosition.x, leftEyePosition.y, rightEyePosition.x, rightEyePosition.y));
                    MaterialPropertyBlock.SetFloat("_Intensity", currentIntensity);
                    MaterialPropertyBlock.SetVector("_FlareData0", flareData0);
                    MaterialPropertyBlock.SetVector("_FlareData1", new Vector4(screenPos.x, screenPos.y, localSize.x, localSize.y));
                    _cmd.DrawProcedural(Matrix4x4.identity, _lensFlareMaterial, 0, MeshTopology.Quads, 4, 1, MaterialPropertyBlock);

                    Vector2 ComputeLocalSize(Vector2 rayOff, Vector2 rayOff0, Vector2 curSize, AnimationCurve distortionCurve)
                    {
                        // var rayOffZ = VRSimpleLensFlareCore.GetLensFlareRayOffset(screenPos, position, globalCos0, globalSin0);
                        Vector2 localRadPos;
                        float localRadius;
                        if (!element.DistortionRelativeToCenter)
                        {
                            localRadPos = (rayOff - rayOff0) * 0.5f;
                            localRadius = Mathf.Clamp01(Mathf.Max(Mathf.Abs(localRadPos.x), Mathf.Abs(localRadPos.y))); // l1 norm (instead of l2 norm)
                        }
                        else
                        {
                            localRadPos = screenPos + (rayOff + new Vector2(element.PositionOffset.x, -element.PositionOffset.y)) * element.TranslationScale;
                            localRadius = Mathf.Clamp01(localRadPos.magnitude); // l2 norm (instead of l1 norm)
                        }

                        var localLerpValue = Mathf.Clamp01(distortionCurve.Evaluate(localRadius));
                        return new Vector2(
                            Mathf.Lerp(curSize.x, element.TargetSizeDistortion.x * combinedScale / usedAspectRatio, localLerpValue),
                            Mathf.Lerp(curSize.y, element.TargetSizeDistortion.y * combinedScale, localLerpValue));
                    }
                }
            }
        }

        private void CalculateRayVisibility(VRLensFlare lensFlare, bool isDirLight)
        {
            if (lensFlare.UseOcclusion)
            {
                var delta = isDirLight ? Quaternion.Inverse(lensFlare.transform.rotation) * Vector3.forward : lensFlare.transform.position - _camera.transform.position;
                var distance = isDirLight ? _camera.farClipPlane : delta.magnitude;
                var ray = new Ray(_camera.transform.position, delta);

                if (Physics.Raycast(ray, out var hit, distance, _occlusionLayerMask))
                {
                    lensFlare.OcclusionProgress = Mathf.Clamp01(lensFlare.OcclusionProgress + lensFlare.OcclusionSpeed * Time.deltaTime);
                }
                else
                {
                    lensFlare.OcclusionProgress = Mathf.Clamp01(lensFlare.OcclusionProgress - lensFlare.OcclusionSpeed * Time.deltaTime);
                }
            }
            else
            {
                lensFlare.OcclusionProgress = 0;
            }
        }
    }
}