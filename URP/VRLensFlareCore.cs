using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace AleVerDes.VRLensFlares
{
    public class VRLensFlareCore
    {
        public static VRLensFlareCore Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                
                lock (InstanceLock)
                {
                    _instance ??= new VRLensFlareCore();
                }

                return _instance;
            }
        }

        private static VRLensFlareCore _instance;
        private static readonly object InstanceLock = new object();

        private readonly HashSet<VRLensFlare> _lensFlares = new HashSet<VRLensFlare>();

        public bool IsEmpty => _lensFlares.Count == 0;

        public void AddLensFlare(VRLensFlare lensFlare)
        {
            Debug.Assert(Instance == this, "VRSimpleLensFlareEngine can have only one instance");
            _lensFlares.Add(lensFlare);
        }

        public void RemoveLensFlare(VRLensFlare lensFlare)
        {
            Debug.Assert(Instance == this, "VRSimpleLensFlareEngine can have only one instance");
            _lensFlares.Remove(lensFlare);
        }

        public HashSet<VRLensFlare> GetLensFlares()
        {
            return _lensFlares;
        }
        
        public static Vector3 GetEyePosition(Transform cameraTransform, Camera.MonoOrStereoscopicEye eye)
        {
            if (eye == Camera.MonoOrStereoscopicEye.Mono)
            {
                return cameraTransform.position;
            }
            
            if (!XRSettings.enabled)
            {
                return cameraTransform.position;
            }
            
            var headPosition = Vector3.zero;
            var head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (head.isValid)
            {
                head.TryGetFeatureValue(CommonUsages.devicePosition, out headPosition);
            }
                
            var device = InputDevices.GetDeviceAtXRNode(eye == Camera.MonoOrStereoscopicEye.Left ? XRNode.LeftEye : XRNode.RightEye);
            if (!device.isValid || !device.TryGetFeatureValue(eye == Camera.MonoOrStereoscopicEye.Left ? CommonUsages.leftEyePosition : CommonUsages.rightEyePosition, out var eyePosition))
            {
                return cameraTransform.position;
            }
            
            var eyeLocalPosition = eyePosition - headPosition;
            eyeLocalPosition = cameraTransform.rotation * eyeLocalPosition;
            return cameraTransform.position + eyeLocalPosition;
        }

        public static Vector2 WorldPointToViewportPoint(Camera camera, Camera.MonoOrStereoscopicEye eye, Vector3 worldPosition)
        {
            return camera.WorldToViewportPoint(worldPosition, !XRSettings.enabled ? Camera.MonoOrStereoscopicEye.Mono : eye);
        }
        
        public static Matrix4x4 GetMatrixFromEye(Camera camera, Camera.StereoscopicEye eye)
        {
            var gpuView = camera.GetStereoViewMatrix(eye);
            var gpuNonJitteredProj = GL.GetGPUProjectionMatrix(camera.GetStereoNonJitteredProjectionMatrix(eye), true);
            gpuView.SetColumn(3, new Vector4(0, 0, 0, 1));
            return gpuNonJitteredProj * gpuView;
        }

        public static Vector3 WorldToViewport(Camera.MonoOrStereoscopicEye eye, Camera camera, bool isLocalLight, bool isCameraRelative, Matrix4x4 viewProjMatrix, Vector3 worldPosition)
        {
            if (isLocalLight)
            {
                if (eye == Camera.MonoOrStereoscopicEye.Mono)
                {
                    return WorldToViewportLocal(isCameraRelative, viewProjMatrix, camera.transform.position, worldPosition);
                }

                var eyePos = GetEyePosition(camera.transform, eye);
                return WorldToViewportLocal(isCameraRelative, viewProjMatrix, eyePos, worldPosition);
            }

            return WorldToViewportDistance(eye, camera, worldPosition);
        }
        
        public static Vector2 GetScreenPosition(Camera.MonoOrStereoscopicEye eye, VRLensFlare comp, Camera cam, bool isCameraRelative, Matrix4x4 viewProjMatrix)
        {
            var light = comp.GetComponent<Light>();

            Vector3 worldPosition;

            var isDirLight = false;
            if (light && light.type == LightType.Directional)
            {
                worldPosition = -light.transform.forward * cam.farClipPlane;
                isDirLight = true;
            }
            else
            {
                worldPosition = comp.transform.position;
            }

            var viewportPosition = WorldToViewport(eye, cam, !isDirLight, isCameraRelative, viewProjMatrix, worldPosition);

            return new Vector2(2f * viewportPosition.x - 1f, 1f - 2f * viewportPosition.y);
        }
        
        public static Vector3 WorldToViewportLocal(bool isCameraRelative, Matrix4x4 viewProjMatrix, Vector3 cameraPosition, Vector3 worldPosition)
        {
            var localPosition = worldPosition;
            if (isCameraRelative)
            {
                localPosition -= cameraPosition;
            }

            var viewportPos4 = viewProjMatrix * localPosition;
            var viewportPos = new Vector3(viewportPos4.x, viewportPos4.y, 0f);
            viewportPos /= viewportPos4.w;
            viewportPos.x = viewportPos.x * 0.5f + 0.5f;
            viewportPos.y = viewportPos.y * 0.5f + 0.5f;
            viewportPos.y = 1.0f - viewportPos.y;
            viewportPos.z = viewportPos4.w;
            return viewportPos;
        }

        public static Vector3 WorldToViewportDistance(Camera.MonoOrStereoscopicEye eye, Camera cam, Vector3 positionWS)
        {
            if (eye == Camera.MonoOrStereoscopicEye.Mono)
            {
                var camPos = cam.worldToCameraMatrix * positionWS;
                var viewportPos4 = cam.projectionMatrix * camPos;
                var viewportPos = new Vector3(viewportPos4.x, viewportPos4.y, 0f);
                viewportPos /= viewportPos4.w;
                viewportPos.x = viewportPos.x * 0.5f + 0.5f;
                viewportPos.y = viewportPos.y * 0.5f + 0.5f;
                viewportPos.z = viewportPos4.w;
                return viewportPos;
            }
            else
            {
                var camPos = cam.GetStereoViewMatrix((Camera.StereoscopicEye)eye) * positionWS;
                var viewportPos4 = cam.GetStereoProjectionMatrix((Camera.StereoscopicEye)eye) * camPos;
                var viewportPos = new Vector3(viewportPos4.x, viewportPos4.y, 0f);
                viewportPos /= viewportPos4.w;
                viewportPos.x = viewportPos.x * 0.5f + 0.5f;
                viewportPos.y = viewportPos.y * 0.5f + 0.5f;
                viewportPos.z = viewportPos4.w;
                return viewportPos;
            }
        }
        
        public static float GetLensFlareLightAttenuation(Light light, Camera cam, Vector3 wo)
        {
            // Must always be true
            if (!light)
            {
                return 1.0f;
            }

            return light.type switch
            {
                LightType.Directional => LensFlareCommonSRP.ShapeAttenuationDirLight(light.transform.forward, wo),
                LightType.Point => LensFlareCommonSRP.ShapeAttenuationPointLight(),
                LightType.Spot => LensFlareCommonSRP.ShapeAttenuationSpotConeLight(light.transform.forward, wo, light.spotAngle, light.innerSpotAngle / 180.0f),
                _ => 1.0f
            };
        }
        
        public static Vector2 GetLensFlareRayOffset(Vector2 screenPos, float position, float globalCos0, float globalSin0)
        {
            var rayOff = -(screenPos + screenPos * (position - 1.0f));
            return new Vector2(globalCos0 * rayOff.x - globalSin0 * rayOff.y, globalSin0 * rayOff.x + globalCos0 * rayOff.y);
        }
        
        public static Vector4 GetFlareData(Vector2 screenPos, Vector2 translationScale, Vector2 rayOff0, Vector2 vLocalScreenRatio, float angleDeg, float position, float angularOffset, Vector2 positionOffset, bool autoRotate)
        {
            if (!SystemInfo.graphicsUVStartsAtTop)
            {
                angleDeg *= -1;
                positionOffset.y *= -1;
            }

            var globalCos0 = Mathf.Cos(-angularOffset * Mathf.Deg2Rad);
            var globalSin0 = Mathf.Sin(-angularOffset * Mathf.Deg2Rad);

            var rayOff = -translationScale * (screenPos + screenPos * (position - 1.0f));
            rayOff = new Vector2(globalCos0 * rayOff.x - globalSin0 * rayOff.y, globalSin0 * rayOff.x + globalCos0 * rayOff.y);

            var rotation = angleDeg;

            rotation += 180.0f;
            if (autoRotate)
            {
                var pos = (rayOff.normalized * vLocalScreenRatio) * translationScale;
                rotation += -Mathf.Rad2Deg * Mathf.Atan2(pos.y, pos.x);
            }

            rotation *= Mathf.Deg2Rad;
            var localCos0 = Mathf.Cos(-rotation);
            var localSin0 = Mathf.Sin(-rotation);

            return new Vector4(localCos0, localSin0, positionOffset.x + rayOff0.x * translationScale.x, -positionOffset.y + rayOff0.y * translationScale.y);
        }
    }
}