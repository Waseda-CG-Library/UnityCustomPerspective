using UnityEngine;

namespace WCGL
{
    [ExecuteInEditMode]
    public class CustomPerspectiveCamera : MonoBehaviour
    {
        new Camera camera;
        ScreenspaceShadowMap screenspaceShadowMap;

        public ScreenspaceShadowMap.RenderPath projectSettingPath = ScreenspaceShadowMap.RenderPath.Forward;

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
            Shader.EnableKeyword("CUSTOM_PERSPECTIVE_DEPTH_PATH");
            var command = screenspaceShadowMap.updateBuffer(camera, projectSettingPath);
            command.DisableShaderKeyword("CUSTOM_PERSPECTIVE_DEPTH_PATH");
            Shader.SetGlobalTexture("_CustomPerspective_ViewPosTexture", screenspaceShadowMap.ViewPosTexture);
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