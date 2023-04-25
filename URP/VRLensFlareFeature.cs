using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AleVerDes.VRLensFlares
{
    public class VRLensFlareFeature : ScriptableRendererFeature
    {
        private const string ShaderName = "Hidden/AleVerDes/VR Lens Flare";
        
        private VRLensFlarePass _pass;
        [HideInInspector, SerializeField] private Shader _lensFlareShader;
        [HideInInspector, SerializeField] private Material _lensFlareMaterial;
        [SerializeField] private LayerMask _occlusionLayerMask;
        
        public override void Create()
        {
            _pass = new VRLensFlarePass
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!_lensFlareShader)
            {
                _lensFlareShader = Shader.Find(ShaderName);
            }
            
            if (!_lensFlareMaterial)
            {
                _lensFlareMaterial = CoreUtils.CreateEngineMaterial(_lensFlareShader);
            }
            
            _pass.Setup(renderingData.cameraData.cameraTargetDescriptor, _lensFlareMaterial, _occlusionLayerMask);
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            if (_lensFlareMaterial)
            {
                CoreUtils.Destroy(_lensFlareMaterial);
            }
        }
    }
}