using UnityEngine;

namespace WCGL
{
    [ExecuteInEditMode]
    public class CustomPerspectiveMesh : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] CustomPerspectiveModel customPerspectiveModel;
#pragma warning restore 649

        new Renderer renderer;
        MaterialCache materialChace = new MaterialCache();

        public void enableCustomMatrix(ref Matrix4x4 customProj, ref Matrix4x4 invVP)
        {
            if (renderer == null) return;

            var mateials = materialChace.GetMaterials(renderer, true);
            foreach (var material in mateials)
            {
                material.EnableKeyword("CUSTOM_PERSPECTIVE_ON");
                material.SetMatrix("CUSTOM_MATRIX_P", customProj);
                material.SetMatrix("MATRIX_I_VP", invVP);
            }
        }

        public void disableCustomMatrix()
        {
            if (renderer == null) return;

            var mateials = materialChace.GetMaterials(renderer, false);
            foreach (var material in mateials)
            {
                material.DisableKeyword("CUSTOM_PERSPECTIVE_ON");
            }
        }

        void Reset()
        {
            renderer = GetComponent<Renderer>();
            if (customPerspectiveModel != null) customPerspectiveModel.Meshes.Add(this);
        }

        void Start()
        {
            renderer = GetComponent<Renderer>();
            if (customPerspectiveModel != null) customPerspectiveModel.Meshes.Add(this);
        }

        void OnDestroy()
        {
            if (customPerspectiveModel != null) customPerspectiveModel.Meshes.Remove(this);
        }
    }
}
