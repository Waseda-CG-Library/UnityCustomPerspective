using UnityEngine;

namespace WCGL
{
    [ExecuteInEditMode]
    public class CustomPerspectiveCamera : MonoBehaviour
    {
        new Camera camera;
        ScreenspaceShadowMap screenspaceShadowMap;

        [Range(1.0f, 2.0f)] public float viewVolumeScale = 1.0f;
        public ScreenspaceShadowMap.RenderPath projectSettingPath = ScreenspaceShadowMap.RenderPath.Forward;

        private void OnPreCull()
        {
            var proj = camera.projectionMatrix;
            proj.m00 /= viewVolumeScale;
            proj.m11 /= viewVolumeScale;
            camera.projectionMatrix = proj;
        }

        void OnPreRender()
        {
            camera.ResetProjectionMatrix();

            foreach (var cpm in CustomPerspectiveModel.GetActiveInstances())
            {
                cpm.UpdateMatrix(camera);
            }

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
            if (screenspaceShadowMap == null)
            {
                camera = GetComponent<Camera>();
                screenspaceShadowMap = new ScreenspaceShadowMap(camera);
            }
            screenspaceShadowMap.enableCommandBuffer(camera);
        }

        void OnDisable()
        {
            screenspaceShadowMap?.disableCommandBuffer(camera);
        }
    }
}