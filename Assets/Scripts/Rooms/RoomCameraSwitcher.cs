using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class RoomCameraSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class RoomCameraPair
    {
        public GameObject roomContainer; // The GameObject representing the room
        public string roomID; // Unique identifier for the room
        public CinemachineVirtualCamera camera; // The Cinemachine Virtual Camera
    }
    private Vector3 lastVelocity = Vector3.zero; // Velocity reference for SmoothDamp

    public RoomCameraPair[] roomCameras; // Assign pairs of roomID, roomContainer, and cameras in the Inspector
    public Dictionary<string, RoomCameraPair> roomMap; // Maps roomID to RoomCameraPair
    public CinemachineVirtualCamera activeCamera;
    private GameObject activeRoom;

    public Transform followTarget { get; set; }

    public Vector3 isometricOffset = new Vector3(0, 10, -10); // Position offset for the isometric view
    public Vector3 isometricRotation = new Vector3(30, 0, 0); // Rotation to give the isometric view

    public float cameraMoveThreshold = 2f; // The distance the player has to move outside the camera view before the camera moves
    public float cameraMoveSpeed = 5f; // Speed at which the camera adjusts its position

    private Vector3 lastPlayerPosition;

    protected void Awake()
    {
        followTarget = gameObject.transform;

        if (activeCamera != null)
        {
            // Set the camera to look at the follow target, but not follow it
            activeCamera.LookAt = followTarget;

            // Apply isometric settings
            SetIsometricView();
        }

        // Initialize the dictionary
        roomMap = new Dictionary<string, RoomCameraPair>();
        foreach (var pair in roomCameras)
        {
            if (!roomMap.ContainsKey(pair.roomID))
            {
                roomMap.Add(pair.roomID, pair);
                pair.camera.gameObject.SetActive(false); // Ensure all cameras are off initially
                pair.roomContainer.SetActive(false); // Ensure all rooms are inactive initially
            }
        }

        lastPlayerPosition = followTarget.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RoomTrigger"))
        {
            string newRoomID = other.GetComponent<RoomTrigger>().roomID;

            if (roomMap.ContainsKey(newRoomID) && roomMap[newRoomID].camera != activeCamera)
            {
                SwitchRoom(newRoomID);
            }
        }
    }

    private void SwitchRoom(string newRoomID)
    {
        // Disable the current active camera
        if (activeCamera != null)
        {
            activeCamera.gameObject.SetActive(false);
        }

        // Schedule the deactivation of the current active room
        if (activeRoom != null)
        {
            StartCoroutine(DisableRoomAfterDelay(activeRoom, 2f)); 
        }

        // Get the new room pair and set the new active camera and room
        var newRoomPair = roomMap[newRoomID];
        activeCamera = newRoomPair.camera;
        activeRoom = newRoomPair.roomContainer;

        // Activate the new camera and room
        activeCamera.gameObject.SetActive(true);
        activeRoom.SetActive(true);

        // Set the new camera to a fixed position, no longer looking at the player
        SetIsometricView();
    }

    // Apply isometric view by setting position and rotation
    private void SetIsometricView()
    {
        if (activeCamera != null)
        {
            // Adjust the camera's position and rotation to simulate an isometric view
            activeCamera.transform.position = followTarget.position + isometricOffset;
            activeCamera.transform.rotation = Quaternion.Euler(isometricRotation);
        }
    }

    private void FixedUpdate()
    {
        // Only move the camera if the player has moved outside of the camera's current view
        if (Vector3.Distance(followTarget.position, lastPlayerPosition) > cameraMoveThreshold)
        {
            UpdateCameraPosition();
            lastPlayerPosition = followTarget.position;
        }
    }

    // Update camera position only when the player moves outside the view threshold
    private void UpdateCameraPosition()
    {
        if (activeCamera != null)
        {
            Vector3 targetPosition = followTarget.position + isometricOffset;

            // Use SmoothDamp for smoother camera movement
            activeCamera.transform.position = Vector3.SmoothDamp(
                activeCamera.transform.position,       // Current position
                targetPosition,                        // Target position
                ref lastVelocity,                      // A velocity reference for smoothing
                0.3f,                                  // Smooth time (adjust this value to control the smoothness)
                cameraMoveSpeed,                       // Maximum speed (determines how fast the camera can move)
                Time.deltaTime                          // Delta time for frame rate independence
            );
        }
    }


    // Coroutine to disable the room after a delay
    private IEnumerator DisableRoomAfterDelay(GameObject room, float delay)
    {
        yield return new WaitForSeconds(delay);
        room.SetActive(false);
    }
}
