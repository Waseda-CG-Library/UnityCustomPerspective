using UnityEngine;

namespace WCGL
{
    [ExecuteInEditMode]
    public class CustomPerspectiveMesh : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] CustomPerspectiveModel customPerspectiveModel;
#pragma warning restore 649

        public Renderer Renderer { get; private set; }
        MaterialCache materialChace;

        public void enableCustomMatrix(ref Matrix4x4 customProj, ref Matrix4x4 invVP, ref Vector3 viewDirectionCorrectWorld, Texture screenSpaceShadowMap, bool correctShadow)
        {
            if (Renderer == null) return;

            var mateials = materialChace.GetMaterials(true);
            foreach (var material in mateials)
            {
                material.EnableKeyword("CUSTOM_PERSPECTIVE_ON");
                material.SetMatrix("CUSTOM_MATRIX_P", customProj);
                material.SetMatrix("MATRIX_I_VP", invVP);
                if (correctShadow == true) material.SetTexture("_ShadowMapTexture", screenSpaceShadowMap);
                material.SetVector("ViewDirectionCorrectWorld", viewDirectionCorrectWorld);
            }
        }

        public void disableCustomMatrix(bool correctShadow)
        {
            if (Renderer == null) return;

            var mateials = materialChace.GetMaterials(false);
            foreach (var material in mateials)
            {
                material.DisableKeyword("CUSTOM_PERSPECTIVE_ON");
                if (correctShadow == true) material.SetTexture("_ShadowMapTexture", null);
            }
        }

        void Start() { } //for GUI

        void Awake()
        {
            Renderer = GetComponent<Renderer>();
            materialChace = new MaterialCache(Renderer);
            if (customPerspectiveModel != null) customPerspectiveModel.Meshes.Add(this);
        }

        void OnDestroy()
        {
            if (customPerspectiveModel != null) customPerspectiveModel.Meshes.Remove(this);
        }
    }
}
