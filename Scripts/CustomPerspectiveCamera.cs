using UnityEngine;

namespace WCGL
{
    [ExecuteInEditMode]
    public class CustomPerspectiveCamera : MonoBehaviour
    {
        new Camera camera;
        ScreenspaceShadowMap screenspaceShadowMap;

        void Awake()
        {
            camera = GetComponent<Camera>();
            screenspaceShadowMap = new ScreenspaceShadowMap(camera);
        }

        void OnPreRender()
        {
            foreach (var cpm in CustomPerspectiveModel.GetActiveInstances())
            {
                cpm.UpdateMatrix(camera);
            }

            if (screenspaceShadowMap == null) Awake();
            var viewPosTexture = screenspaceShadowMap.updateBuffer(camera);
            Shader.SetGlobalTexture("_CustomPerspective_ViewPosTexture", viewPosTexture);
            Shader.EnableKeyword("CUSTOM_PERSPECTIVE_SHADOW_ON");

            foreach (var cpm in CustomPerspectiveModel.GetActiveInstances())
            {
                cpm.EnableMatrix(camera);
            }
        }

        void OnPostRender()
        {
            Shader.SetGlobalTexture("_CustomPerspective_ViewPosTexture", null);
            Shader.DisableKeyword("CUSTOM_PERSPECTIVE_SHADOW_ON");

            foreach (var cpm in CustomPerspectiveModel.GetActiveInstances())
            {
                cpm.DisableMatrix();
            }
        }

        void OnEnable()
        {
            screenspaceShadowMap?.enableCommandBuffer(camera);
        }

        void OnDisable()
        {
            screenspaceShadowMap?.disableCommandBuffer(camera);
        }
    }
}