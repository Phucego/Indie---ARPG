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

    public RoomCameraPair[] roomCameras; // Assign pairs of roomID, roomContainer, and cameras in the Inspector
    public Dictionary<string, RoomCameraPair> roomMap; // Maps roomID to RoomCameraPair
    public CinemachineVirtualCamera activeCamera;
    private GameObject activeRoom;

    public Transform followTarget { get; set; }

    protected void Awake()
    {
        followTarget = gameObject.transform;

        if (activeCamera != null)
        {
            activeCamera.Follow = followTarget;
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

        // Ensure the new camera follows the player
        activeCamera.Follow = followTarget;
    }

// Coroutine to disable the room after a delay
    private IEnumerator DisableRoomAfterDelay(GameObject room, float delay)
    {
        yield return new WaitForSeconds(delay);
        room.SetActive(false);
    }

}
