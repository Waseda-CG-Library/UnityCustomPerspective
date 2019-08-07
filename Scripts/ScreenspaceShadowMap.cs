using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

namespace WCGL
{
    public class ScreenspaceShadowMap
    {
        public enum RenderPath { Forward, Deferred };

        static Material ViewPosMaterial;

        CommandBuffer command;
        public RenderTexture ViewPosTexture { get; private set; }

        public ScreenspaceShadowMap(Camera camera)
        {
            if (ViewPosMaterial == null)
            {
                var viewPosShader = Shader.Find("Hidden/CustomPerspective/ViewPos");
                ViewPosMaterial = new Material(viewPosShader);
            }

            command = new CommandBuffer();
            command.name = "CustomPerspective Meshes ViewPos";

            resetTexture(camera);
        }

        void resetTexture(Camera camera)
        {
            ViewPosTexture = new RenderTexture(camera.pixelWidth, camera.scaledPixelHeight, 0, RenderTextureFormat.ARGBHalf);
            ViewPosTexture.name = "CustomPerspective_ViewPos";
            ViewPosTexture.filterMode = FilterMode.Point;
        }

        public CommandBuffer updateBuffer(Camera camera, RenderPath renderPath)
        {
            command.Clear();

            if (ViewPosTexture.width != camera.pixelWidth || ViewPosTexture.height != camera.pixelHeight) resetTexture(camera);

            var path = camera.renderingPath;
            if (path == RenderingPath.Forward) renderPath = RenderPath.Forward;
            else if (path == RenderingPath.DeferredShading) renderPath = RenderPath.Deferred;
            var depth = renderPath == RenderPath.Forward ?
                BuiltinRenderTextureType.Depth : BuiltinRenderTextureType.ResolvedDepth;

            command.SetRenderTarget(ViewPosTexture, depth);
            command.ClearRenderTarget(false, true, Color.clear);

            foreach (var cpm in CustomPerspectiveModel.GetActiveInstances())
            {
                if (cpm.CorrectShadow == false) continue;

                command.SetGlobalMatrix("CUSTOM_MATRIX_P", cpm.CustomMatrix);
                foreach (var mesh in cpm.Meshes)
                {
                    if (mesh == null || mesh.enabled == false) continue;

                    int count = mesh.sharedMaterials.Count();
                    for (int i = 0; i < count; i++)
                    {
                        command.DrawRenderer(mesh, ViewPosMaterial, i);
                    }
                }
            }

            return command;
        }

        public void enableCommandBuffer(Camera camera)
        {
            camera.AddCommandBuffer(CameraEvent.AfterDepthTexture, command);
            camera.AddCommandBuffer(CameraEvent.BeforeLighting, command);
        }

        public void disableCommandBuffer(Camera camera)
        {
            camera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, command);
            camera.RemoveCommandBuffer(CameraEvent.BeforeLighting, command);
        }
    }
}
