using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WCGL
{
    [ExecuteInEditMode]
    public class CustomPerspectiveModel : MonoBehaviour
    {
        public enum EmphasisMode { PointOfView, FocalLength};

        static List<CustomPerspectiveModel> instances = new List<CustomPerspectiveModel>();
        public static CustomPerspectiveModel[] GetActiveInstances()
        {
            return instances.Distinct().Where(cpm => cpm?.isActiveAndEnabled == true).ToArray();
        }

        public EmphasisMode EmphasisType;
        public Transform PointOfView;
        public float FocalLength = 50;
        [Space]
        public Transform VanishingPoint;
        [Space]
        public Transform Focus;
        public bool correctShadow = true;
        [SerializeField] bool correctRimLight = true;

        public HashSet<CustomPerspectiveMesh> Meshes { get; private set; } = new HashSet<CustomPerspectiveMesh>();
        public Matrix4x4 CustomMatrix { get; private set; }
        public Material ViewPosMaterial { get; private set; }
        Vector3 viewDirectionCorrectWorld;

        (Matrix4x4, float) createEmphasisMatrix(Camera camera)
        {
            Matrix4x4 viewMat = camera.worldToCameraMatrix;
            Vector3 focusPos = (Focus == null) ? transform.position : Focus.position;
            Vector3 focusView = viewMat.MultiplyPoint3x4(focusPos);

            float pointViewZ, fovY;
            float tanHalfFovY = Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView / 2.0f);
            if (EmphasisType == EmphasisMode.PointOfView)
            {
                pointViewZ = viewMat.MultiplyPoint3x4(PointOfView.position).z;
                float zLength = pointViewZ - focusView.z; //Unity camera coordinates is right hand.
                float halfHeight = -focusView.z * tanHalfFovY;
                fovY = Mathf.Atan2(halfHeight, zLength) * 2 * Mathf.Rad2Deg;
            }
            else
            {
                pointViewZ = focusView.z - FocalLength * focusView.z * tanHalfFovY / 12.0f;
                fovY = Mathf.Atan2(12.0f, FocalLength) * 2 * Mathf.Rad2Deg;
            }

            float zn = Mathf.Max(0.01f, camera.nearClipPlane + pointViewZ); //adjust so that clip plane is not minus.
            float zf = camera.farClipPlane + pointViewZ;

            Matrix4x4 viewTranslate = Matrix4x4.Translate(new Vector3(0, 0, -pointViewZ));
            Matrix4x4 customProj = Matrix4x4.Perspective(fovY, camera.aspect, zn, zf) * viewTranslate;

            return (customProj, -pointViewZ);
        }

        (Matrix4x4, Vector2) createOnePointMatrix(Matrix4x4 viewMat, Matrix4x4 projMat)
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

            return (customProj, -viewPos);
        }

        public void UpdateMatrix(Camera camera)
        {
            Matrix4x4 proj = camera.projectionMatrix;
            float viewZ = 0.0f;
            if (PointOfView != null || EmphasisType == EmphasisMode.FocalLength)
            {
                (proj, viewZ) = createEmphasisMatrix(camera);
            }

            Vector2 viewXY = Vector2.zero;
            if (VanishingPoint != null)
            {
                (proj, viewXY) = createOnePointMatrix(camera.worldToCameraMatrix, proj);
            }

            float version = float.Parse(Application.unityVersion.Substring(0, 3));
            bool renderIntoTexture = version >= 5.6f;
            CustomMatrix = GL.GetGPUProjectionMatrix(proj, renderIntoTexture);

            var viewXYZW = new Vector4(viewXY.x, viewXY.y, viewZ, 0);
            viewDirectionCorrectWorld = camera.cameraToWorldMatrix.MultiplyVector(viewXYZW);
        }

        public void EnableMatrix(Camera camera)
        {
            float version = float.Parse(Application.unityVersion.Substring(0, 3));
            bool renderIntoTexture = version >= 5.6f;
            Matrix4x4 unityProj = GL.GetGPUProjectionMatrix(camera.projectionMatrix, renderIntoTexture);
            Matrix4x4 invVP = (unityProj * camera.worldToCameraMatrix).inverse;

            var proj = CustomMatrix;
            foreach (var mesh in Meshes)
            {
                if(mesh.isActiveAndEnabled) mesh.enableCustomMatrix(ref proj, ref invVP,
                    ref viewDirectionCorrectWorld, correctRimLight);
            }
        }

        public void DisableMatrix()
        {
            foreach (var mesh in Meshes)
            {
                if (mesh.isActiveAndEnabled) mesh.disableCustomMatrix(correctShadow);
            }
        }

        void Start(){ } //for GUI

        void Awake()
        {
            var viewPosShader = Shader.Find("Hidden/CustomPerspective/ViewPos");
            ViewPosMaterial = new Material(viewPosShader);

            instances.Add(this);
        }

        void OnDestroy()
        {
            instances.Remove(this);
        }
    }
}
