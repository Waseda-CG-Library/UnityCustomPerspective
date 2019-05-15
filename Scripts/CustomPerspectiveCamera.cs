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
            var shadowTexture = screenspaceShadowMap.updateBuffer(camera);

            foreach (var cpm in CustomPerspectiveModel.GetActiveInstances())
            {
                cpm.EnableMatrix(camera, shadowTexture);
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