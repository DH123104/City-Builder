using UnityEngine;
using System.Collections;

namespace CBSK
{
    public class WorldSpaceUIExtensions : MonoBehaviour
    {

        public bool faceMainCamera = true;

        private Camera cam;
        // Use this for initialization
        void Start()
        {
            cam = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
            if (faceMainCamera)
            {
                transform.LookAt(cam.transform.position);
            }
        }
    }
}
