using UnityEngine;

namespace AleVerDes.VRLensFlares
{
    [CreateAssetMenu(menuName = "Lens Flare (VR)", fileName = "VR Lens Flare", order = 303)]
    public class VRLensFlareData : ScriptableObject
    {
        public VRLensFlareDataElement[] Elements;
    }
}