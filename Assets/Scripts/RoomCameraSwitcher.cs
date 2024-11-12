using UnityEngine;
using Cinemachine;

public class RoomCameraSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera[] roomCameras; // Assign your room cameras here

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Check if the player entered
        {
            foreach (CinemachineVirtualCamera cam in roomCameras)
            {
                cam.enabled = false; // Disable all cameras first
            }

            // Enable the camera for the current room
            CinemachineVirtualCamera currentRoomCamera = GetComponent<CinemachineVirtualCamera>();
            if (currentRoomCamera != null)
            {
                currentRoomCamera.enabled = true;
            }
        }
    }
}