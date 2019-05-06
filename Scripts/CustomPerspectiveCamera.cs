using UnityEngine;
using UnityEngine.Rendering;

namespace WCGL
{
    [ExecuteInEditMode]
    public class CustomPerspectiveCamera : MonoBehaviour
    {
        new Camera camera;
        ScreenspaceShadowMap screenspaceShadowMap;
        CommandBuffer afterOpaque, afterAlpha;

        void Awake()
        {
            camera = GetComponent<Camera>();
            screenspaceShadowMap = new ScreenspaceShadowMap(camera);

            afterOpaque = new CommandBuffer();
            afterOpaque.name = "CancelCull_Opaque";
            camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, afterOpaque);

            afterAlpha = new CommandBuffer();
            afterAlpha.name = "CancelCull_Transparent";
            camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, afterAlpha);
        }

        void OnPreRender()
        {
            foreach (var cpm in CustomPerspectiveModel.GetActiveInstances())
            {
                cpm.UpdateMatrix(camera);
            }

            if (screenspaceShadowMap == null) Awake();
            var shadowTexture = screenspaceShadowMap.updateBuffer(camera);

            afterOpaque.Clear();
            afterAlpha.Clear();

            foreach (var cpm in CustomPerspectiveModel.GetActiveInstances())
            {
                cpm.EnableMatrix(camera, shadowTexture, afterOpaque, afterAlpha);
            }
        }

        void OnPostRender()
        {
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