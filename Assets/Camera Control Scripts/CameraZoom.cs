using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraZoom : MonoBehaviour
{
    private float zoom;
    private float zoomMultiplier = 4f;
    private float minZoom = 0.4f;
    private float maxZoom = 8f;
    private float velocity = 0f;
    private float smoothTime = 0.25f;

    private Vector3 dragOrigin;

    [SerializeField] private Camera cam;
    [SerializeField] private SpriteRenderer spriteRenderer;

    //NOT MY CODE
    void Start()
    {
        zoom = cam.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            zoom -= scroll * zoomMultiplier;
            zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        }

        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, zoom, ref velocity, smoothTime);

        PanCamera();
    }

    private void PanCamera()
    {
        // save pos of mouse in world space when drag starts

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        //calc distance between drag origin and new pos if it is still held down

        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);

            //move camera by the difference
            cam.transform.position += difference;
        }
    }
}
