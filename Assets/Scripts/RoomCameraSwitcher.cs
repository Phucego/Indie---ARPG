using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class RoomCameraSwitcher : Singleton<RoomCameraSwitcher>
{
    [System.Serializable]
    public class RoomCameraPair
    {
        public string roomID;
        public CinemachineVirtualCamera camera;
    }
    
    public RoomCameraPair[] roomCameras; // Assign pairs of roomID and cameras in the Inspector

    public Dictionary<string, CinemachineVirtualCamera> cameraMap; // Maps roomID to camera
    public CinemachineVirtualCamera activeCamera;

    public Transform followTarget { get; set; }
    public Transform lookAtTarget { get; set; }

    private void Awake()
    {
        followTarget = gameObject.transform;
        lookAtTarget = gameObject.transform;

        //TODO: Assign the player transform to the camera to make sure it follows
        if (activeCamera != null)
        {
            activeCamera.LookAt = lookAtTarget;
            activeCamera.Follow = followTarget;
        }
        // Initialize the dictionary
        cameraMap = new Dictionary<string, CinemachineVirtualCamera>();
        foreach (var pair in roomCameras)
        {
            if (!cameraMap.ContainsKey(pair.roomID))
            {
                cameraMap.Add(pair.roomID, pair.camera);
                pair.camera.gameObject.SetActive(false); // Ensure all cameras are off initially
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RoomTrigger"))
        {
            string newRoomID = other.GetComponent<RoomTrigger>().roomID;

            if (cameraMap.ContainsKey(newRoomID) && cameraMap[newRoomID] != activeCamera)
            {
                SwitchCamera(newRoomID, cameraMap[newRoomID]);
            }
        }
    }

 

    private void SwitchCamera(string newRoomID, CinemachineVirtualCamera targetCamera)
    {
        // Disable the current active camera if any
        if (activeCamera != null)
        {
            activeCamera.gameObject.SetActive(false);
        }

        // Activate the new camera and set it as the active camera
        activeCamera = cameraMap[newRoomID];
        activeCamera.gameObject.SetActive(true);
        
        // Switch to a different camera if needed
        activeCamera = targetCamera;

        // Set Look At and Follow targets on the new camera
        activeCamera.LookAt = lookAtTarget;
        activeCamera.Follow = followTarget;
    }
}