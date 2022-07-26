using UnityEngine;
using System.Collections;

/**
 * Detects clicks and responds in 3D space.
 */
namespace CBSK
{
    public class InputControl3D : MonoBehaviour
    {

        public Camera inputCamera;
        public GameObject gameView;
        public float acceleration = 4;
        public float maxZoom = 45;
        public float minZoom = 20;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        public float zoomSpeed = 1.0f;
#else
        public float zoomSpeed = 4;
#endif

        float lastActionTimer;
        bool mousePressed;
        bool isDragging;
        Vector3 lastMousePosition;
        GameObject dragTarget;

        //Zoom to cursor variables
        private float previousZoom;
        //private Vector2 prevCursorPos;
        //private Vector2 cursorDelta;
        private float scrollTarget;
        private float scrollDelta;
        private Vector3 zoomCenter;

        private Vector3 targetCameraPosition;

        const float MAX_CLICK_TIME = 1.0f;
        const float MAX_CLICK_DELTA = 10;
        const float MIN_DRAG_DELTA = 1.0f;

        //This can be switched off by certain full screen panels. Expecially useful for ones that use their own dragging or scrolling to display content
        internal bool acceptInput = true;

        void Start()
        {
            scrollTarget = inputCamera.fieldOfView;
            targetCameraPosition = inputCamera.transform.position;
        }

        void Update()
        {
            UpdateZoom();

            lastActionTimer += Time.deltaTime;

            // Mouse button down
            if (Input.GetMouseButtonDown(0) && !mousePressed)
            {
                mousePressed = true;
                lastActionTimer = 0;
                lastMousePosition = Input.mousePosition;
            }

            RaycastHit hit;
            Ray ray = inputCamera.ScreenPointToRay(Input.mousePosition);


            // Mouse button up
            if (Input.GetMouseButtonUp(0))
            {
                if (lastActionTimer < MAX_CLICK_TIME && Vector2.Distance(lastMousePosition, Input.mousePosition) < MAX_CLICK_DELTA)
                {
                    // Check for building
                    if (Physics.Raycast(ray, out hit, 10000, 1 << BuildingManager3D.BUILDING_LAYER))
                    {
                        hit.collider.gameObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
                    }
                }
                mousePressed = false;
                isDragging = false;
                lastActionTimer = 0;
            }


            if (mousePressed && !isDragging)
            {
                if (Vector2.Distance(lastMousePosition, Input.mousePosition) >= MAX_CLICK_DELTA)
                {
                    
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << BuildingManager3D.BUILDING_LAYER))
                    {
                        //Drag building
                        isDragging = true;
                        dragTarget = hit.collider.gameObject;
                    }
                    else if(!isDragging)
                    {
                        //Drag camera
                        isDragging = true;
                        dragTarget = null;
                    }
                }
            }

            if (isDragging && Vector2.Distance(lastMousePosition, Input.mousePosition) >= MIN_DRAG_DELTA)
            {
                if (Physics.Raycast(ray, out hit, 10000, 1 << BuildingManager3D.TERRAIN_LAYER))
                {
                    if (dragTarget != null)
                    {
                        //Drag building
                        dragTarget.SendMessage("OnDrag", hit.point - gameView.transform.position, SendMessageOptions.DontRequireReceiver);
                        lastActionTimer = 0;
                    }
                    else
                    {
                        //Drag camera
                        RaycastHit hitDragCamera;
                        if (Physics.Raycast(inputCamera.ScreenPointToRay(lastMousePosition), out hitDragCamera, 10000, 1 << BuildingManager3D.TERRAIN_LAYER))
                        {
                            inputCamera.transform.Translate(hitDragCamera.point - hit.point,Space.World);
                            targetCameraPosition = inputCamera.transform.position;
                        }
                    }
                    lastMousePosition = Input.mousePosition;
                }
            }

            UpdateDrag();
        }

        void UpdateZoom()
        {
            //TODO add perspective camera
#if UNITY_STANDALONE || UNITY_EDITOR
            if (acceptInput)
            {
                scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            }
            else
            {
                scrollDelta = 0;
            }
            scrollTarget -= scrollDelta * zoomSpeed;
            scrollTarget = Mathf.Clamp(scrollTarget, minZoom, maxZoom);
            Vector3 zoomCenter = Input.mousePosition;

#endif
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        // If there are two touches on the device...
        if (acceptInput && Input.touchCount == 2)
        {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            scrollDelta = prevTouchDeltaMag - touchDeltaMag;
			scrollTarget += scrollDelta * zoomSpeed * Time.deltaTime;
			scrollTarget = Mathf.Clamp(scrollTarget, minZoom, maxZoom);

            //Find focal point of the zoom
            zoomCenter = (touchZero.position + touchOne.position)/2;
        }
        else
        {
            scrollDelta = 0;
        }
#endif
            //Get cursor delta
            RaycastHit hit1, hit2;
            Physics.Raycast(inputCamera.ScreenPointToRay(zoomCenter), out hit1, Mathf.Infinity, 1 << BuildingManager3D.TERRAIN_LAYER);

            //// Only do this if we are scrolling
            //if (Mathf.Abs(scrollTarget - inputCamera.fieldOfView) > 0.001f)
            //{
            //    //Make sure that if we start dragging before the zoom lerp has completed, that we don't fight with the drag position.
            //    if (!isDragging)
            //    {
            //        if (Physics.Raycast(inputCamera.ScreenPointToRay(lastMousePosition), out hit2, Mathf.Infinity, 1 << BuildingManager3D.TERRAIN_LAYER))
            //        {
            //            inputCamera.transform.Translate(hit2.point - hit1.point, Space.World);
            //            targetCameraPosition = inputCamera.transform.position;
            //        }
            //    }
            //}

            //prevCursorPos = inputCamera.ScreenToWorldPoint(zoomCenter);
            inputCamera.fieldOfView = Mathf.Lerp(inputCamera.fieldOfView, scrollTarget, acceleration * Time.deltaTime);

            // Only do this if we are scrolling
            if (Mathf.Abs(scrollTarget - inputCamera.fieldOfView) > 0.001f)
            {
                //Make sure that if we start dragging before the zoom lerp has completed, that we don't fight with the drag position.
                if (!isDragging)
                {
                    if (Physics.Raycast(inputCamera.ScreenPointToRay(zoomCenter), out hit2, Mathf.Infinity, 1 << BuildingManager3D.TERRAIN_LAYER))
                    {
                        inputCamera.transform.Translate(hit1.point - hit2.point, Space.World);
                        targetCameraPosition = inputCamera.transform.position;
                    }
                }
            }
        }

        void UpdateDrag()
        {
            // Clamp first so that targetDragPosition guaranteed to be in bounds
            inputCamera.transform.position = Vector3.Lerp(inputCamera.transform.position, targetCameraPosition, acceleration * Time.deltaTime);
        }
    }
}