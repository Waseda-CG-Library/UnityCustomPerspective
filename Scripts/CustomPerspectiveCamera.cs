using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WCGL
{
    [ExecuteInEditMode]
    public class CustomPerspectiveCamera : MonoBehaviour
    {
        private void Start() //for Inspector's Enabled
        {
        }

        void OnPreRender()
        {
            foreach (var cpm in CustomPerspectiveModel.GetInstances())
            {
                if (cpm.isActiveAndEnabled) { cpm.UpdateMatrix(Camera.current); }
            }
        }

        private void OnPostRender()
        {
            foreach (var cpm in CustomPerspectiveModel.GetInstances())
            {
                if (cpm.isActiveAndEnabled) { cpm.DisableMatrix(); }
            }
        }
    }
}