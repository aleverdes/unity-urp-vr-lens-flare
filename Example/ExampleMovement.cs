using UnityEngine;

namespace AleVerDes.VRLensFlares
{
    public class ExampleMovement : MonoBehaviour
    {
        public Vector3 Amplitude = new Vector3(5f, 3f, 1f);
        public Vector3 Duration = new Vector3(10f, 7f, 3f);

        private void Update()
        {
            var position = Vector3.zero;
            position.x = Amplitude.x * Mathf.Sin(Time.time / Duration.x);
            position.y = Amplitude.y * Mathf.Sin(Time.time / Duration.y);
            position.z = Amplitude.z * Mathf.Sin(Time.time / Duration.z);
            transform.position = position;
        }
    }
}