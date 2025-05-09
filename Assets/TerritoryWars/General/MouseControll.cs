using System;
using DG.Tweening;
using UnityEngine;

namespace TerritoryWars.General
{

    public class MouseControll : MonoBehaviour
    {
        [Header("Camera Movement")] [SerializeField]
        private float panSpeed = 20f;

        [SerializeField] private float zoomSpeed = 20f;
        [SerializeField] private float minZoom = 2f;
        [SerializeField] private float maxZoom = 4f;
        [SerializeField] private float pinchZoomSpeed = 0.5f;
        [SerializeField] private float _contestZoom = 2.5f;

        private Camera _mainCamera;
        private Camera _camera;
        private bool _isMainCamera = false;
        private Vector3 lastMousePosition;
        private bool isDragging = false;
        private Vector3 minBounds = new Vector3(-8f, -8f, 0f);
        private Vector3 maxBounds = new Vector3(8f, 8f, 0f);
        private bool _isCameraMoveLocked = false;

        // Для мобільних жестів
        private float lastPinchDistance;
        private bool isPinching = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _camera = GetComponent<Camera>();
            _mainCamera = Camera.main;
            if (_camera == _mainCamera)
            {
                _isMainCamera = true;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!_isMainCamera)
            {
                _camera.orthographicSize = _mainCamera.orthographicSize;
                _camera.transform.position = _mainCamera.transform.position;
                return;
            }
            
            if (!_isCameraMoveLocked)
            {
                HandlePanning();
                HandleZoom();
            }
        }

        private void HandlePanning()
        {
            if (Input.touchCount == 1) // Один палець для переміщення
            {
                Touch touch = Input.GetTouch(0);
                
                if (touch.phase == TouchPhase.Began)
                {
                    isDragging = true;
                    lastMousePosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    isDragging = false;
                }

                if (isDragging)
                {
                    Vector3 delta = touch.position - (Vector2)lastMousePosition;
                    // Конвертуємо дельту з екранних координат у світові
                    Vector3 worldDelta = _camera.ScreenToWorldPoint(touch.position) - 
                                       _camera.ScreenToWorldPoint(lastMousePosition);
                    Vector3 move = -worldDelta;
                    float horizontalExtent = _mainCamera.orthographicSize * _mainCamera.aspect;
                    float verticalExtent = _mainCamera.orthographicSize;

                    Vector3 newPosition = transform.position + move;
                    newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x + horizontalExtent, maxBounds.x - horizontalExtent);
                    newPosition.y = Mathf.Clamp(newPosition.y, minBounds.y + verticalExtent, maxBounds.y - verticalExtent);

                    transform.position = newPosition;
                    lastMousePosition = touch.position;
                }
            }
            else if (Input.GetMouseButtonDown(0)) // ПК управління
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging && Input.touchCount == 0) // ПК управління
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                // Конвертуємо дельту з екранних координат у світові
                Vector3 worldDelta = _camera.ScreenToWorldPoint(Input.mousePosition) - 
                                   _camera.ScreenToWorldPoint(lastMousePosition);
                Vector3 move = -worldDelta;
                float horizontalExtent = _mainCamera.orthographicSize * _mainCamera.aspect;
                float verticalExtent = _mainCamera.orthographicSize;

                Vector3 newPosition = transform.position + move;
                newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x + horizontalExtent, maxBounds.x - horizontalExtent);
                newPosition.y = Mathf.Clamp(newPosition.y, minBounds.y + verticalExtent, maxBounds.y - verticalExtent);

                transform.position = newPosition;
                lastMousePosition = Input.mousePosition;
            }
        }

        private void HandleZoom()
        {
            if (!_isMainCamera)
            {
                _camera.orthographicSize = _mainCamera.orthographicSize;
                _camera.transform.position = _mainCamera.transform.position;
                return;
            }

            // Мобільний зум
            if (Input.touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                {
                    lastPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                    isPinching = true;
                }
                else if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
                {
                    isPinching = false;
                }

                if (isPinching)
                {
                    float currentPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                    float pinchDelta = currentPinchDistance - lastPinchDistance;
                    
                    float newSize = _camera.orthographicSize - pinchDelta * pinchZoomSpeed * Time.deltaTime;
                    _camera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
                    
                    lastPinchDistance = currentPinchDistance;
                }
            }
            // ПК зум
            else
            {
                float scrollDelta = Input.mouseScrollDelta.y;
                if (scrollDelta != 0)
                {
                    float newSize = _camera.orthographicSize - scrollDelta * zoomSpeed * Time.deltaTime;
                    _camera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
                }
            }
        }

        public void SetCameraLock(bool isLocked)
        {
            _isCameraMoveLocked = isLocked;
        }
        
        public void SetCameraPosition(Vector3 position, Action callback = null)
        {
            _mainCamera.transform.DOMove(position, 0.7f).OnComplete(() =>
            {
                callback?.Invoke();
            });
        }
    }
}