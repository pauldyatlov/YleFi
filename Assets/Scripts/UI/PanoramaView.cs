using UnityEngine;

namespace Yle.Fi
{
    internal sealed class PanoramaView : MonoBehaviour
    {
        [SerializeField] private Camera _panoramaCamera = default;
        [SerializeField] private float _intensity = 0.2f;

        private void Update()
        {
            var xPosition = Mathf.Clamp01(Input.mousePosition.x / Screen.width) * _intensity - 1f;
            var yPosition = Mathf.Clamp01(Input.mousePosition.y / Screen.height) * _intensity - 1f;

            _panoramaCamera.transform.position = new Vector3(xPosition, yPosition, 0);
        }
    }
}