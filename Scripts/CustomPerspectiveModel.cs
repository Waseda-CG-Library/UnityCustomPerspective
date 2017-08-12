using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WCGL
{
    public class CustomPerspectiveModel : MonoBehaviour
    {
        static List<CustomPerspectiveModel> instances = new List<CustomPerspectiveModel>();
        public static CustomPerspectiveModel[] GetInstances() { return instances.ToArray(); }

        public Transform PointOfView;
        public Transform VanishingPoint;
        public Transform Focus;

        Matrix4x4 createEmphasisMatrix(Camera camera)
        {
            Matrix4x4 viewMat = camera.worldToCameraMatrix;
            Vector3 focusPos = (Focus == null) ? transform.position : Focus.position;
            Vector3 focusView = viewMat.MultiplyPoint3x4(focusPos);

            float pointViewZ = viewMat.MultiplyPoint3x4(PointOfView.position).z;
            float zLength = pointViewZ - focusView.z; //UnityではCamera座標は右手系

            float halfHeight = -focusView.z * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView / 2);
            float fovY = Mathf.Atan2(halfHeight, zLength) * 2 * Mathf.Rad2Deg;

            float zn = Mathf.Max(0.01f, camera.nearClipPlane + pointViewZ); //クリップ面がマイナスにならないよう対応
            float zf = camera.farClipPlane + pointViewZ;

            Matrix4x4 viewTranslate = Matrix4x4.Translate(new Vector3(0, 0, -pointViewZ));
            Matrix4x4 customProj = Matrix4x4.Perspective(fovY, camera.aspect, zn, zf) * viewTranslate;

            return customProj;
        }

        Matrix4x4 createOnePointMatrix(Matrix4x4 viewMat, Matrix4x4 projMat)
        {
            Vector3 viewPos = viewMat.MultiplyPoint3x4(VanishingPoint.position);
            Vector3 focusPos = (Focus == null) ? transform.position : Focus.position;

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

        static void setMatrix(Transform target, bool enable, ref Matrix4x4 customProj)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                foreach (var material in renderer.materials)
                {
                    if (enable == true)
                    {
                        material.SetFloat("EnableCustomMatrix", 1.0f);
                        material.SetMatrix("CUSTOM_MATRIX_P", customProj);
                    }
                    else
                    {
                        material.SetFloat("EnableCustomMatrix", 0.0f);
                    }
                }
            }

            foreach(Transform child in target)
            {
                setMatrix(child, enable, ref customProj);
            }
        }

        public void UpdateMatrix(Camera camera)
        {
            Matrix4x4 proj = camera.projectionMatrix;
            if (PointOfView != null)
            {
                proj = createEmphasisMatrix(camera);
            }

            if (VanishingPoint != null)
            {
                proj = createOnePointMatrix(camera.worldToCameraMatrix, proj);
            }

            float version = float.Parse(Application.unityVersion.Substring(0, 3));
            bool renderIntoTexture = version >= 5.6f;
            proj = GL.GetGPUProjectionMatrix(proj, renderIntoTexture);

            setMatrix(transform, true, ref proj);
        }

        public void DisableMatrix()
        {
            Matrix4x4 dummy = Matrix4x4.identity;
            setMatrix(transform, false, ref dummy);
        }

        void Start()
        {
            instances.Add(this);
        }

        void OnDestroy()
        {
            instances.Remove(this);
        }
    }
}