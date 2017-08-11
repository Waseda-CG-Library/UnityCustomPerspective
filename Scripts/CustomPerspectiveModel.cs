using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WCGL
{
    public class CustomPerspectiveModel : MonoBehaviour
    {
        public Transform VanishingPoint;

        Matrix4x4 createOnePointMatrix(Camera camera)
        {
            Matrix4x4 viewMat = camera.worldToCameraMatrix;
            Matrix4x4 projMat = camera.projectionMatrix;

            Vector3 viewPos = viewMat.MultiplyPoint3x4(VanishingPoint.position);
            Vector3 focusPos = this.transform.position;

            float viewZ = viewMat.MultiplyPoint3x4(focusPos).z;
            viewPos *= viewZ / viewPos.z;

            Vector3 projPos = projMat.MultiplyPoint(viewPos);

            viewPos.z = 0;
            projPos.z = 0;

            Matrix4x4 transView = Matrix4x4.Translate(-viewPos);
            Matrix4x4 transProj = Matrix4x4.Translate(projPos);
            Matrix4x4 customProj = transProj * projMat * transView;

            return customProj;
        }

        void setMatrix(Matrix4x4 customProj)
        {
            var renderer = GetComponent<Renderer>();
            foreach (var material in renderer.materials)
            {
                material.SetFloat("EnableCustomMatrix", 1.0f);
                material.SetMatrix("CUSTOM_MATRIX_P", customProj);
            }
        }

        void updateMatrix()
        {
            Matrix4x4 proj = Camera.main.projectionMatrix;
            if (VanishingPoint != null) proj = createOnePointMatrix(Camera.main);

            float version = float.Parse(Application.unityVersion.Substring(0, 3));
            bool renderIntoTexture = version >= 5.6f;
            proj = GL.GetGPUProjectionMatrix(proj, renderIntoTexture);

            setMatrix(proj);
        }

        void Start()
        {
        }

        void Update()
        {
            updateMatrix();
        }
    }
}