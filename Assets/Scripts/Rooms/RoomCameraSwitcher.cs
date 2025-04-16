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

    public RoomCameraPair[] roomCameras; // Assign in Inspector
    public Dictionary<string, RoomCameraPair> roomMap;
    public CinemachineVirtualCamera activeCamera;
    private GameObject activeRoom;

    public Transform followTarget { get; set; }

    [Header("Isometric Camera Settings")]
    public Vector3 isometricOffset = new Vector3(0, 15f, -10f); // Camera offset
    public Vector3 isometricRotation = new Vector3(30f, 0f, 0f); // Less steep angle to see sky

    private void Awake()
    {
        followTarget = gameObject.transform;

        if (activeCamera != null)
        {
            SetIsometricView();
        }

        roomMap = new Dictionary<string, RoomCameraPair>();
        foreach (var pair in roomCameras)
        {
            if (!roomMap.ContainsKey(pair.roomID))
            {
                roomMap.Add(pair.roomID, pair);
                pair.camera.gameObject.SetActive(false);
                pair.roomContainer.SetActive(false);
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
        if (activeCamera != null)
        {
            activeCamera.gameObject.SetActive(false);
        }

        if (activeRoom != null)
        {
            StartCoroutine(DisableRoomAfterDelay(activeRoom, 2f));
        }

        var newRoomPair = roomMap[newRoomID];
        activeCamera = newRoomPair.camera;
        activeRoom = newRoomPair.roomContainer;

        activeCamera.gameObject.SetActive(true);
        activeRoom.SetActive(true);

        SetIsometricView();
    }

    private void SetIsometricView()
    {
        if (activeCamera != null && followTarget != null)
        {
            activeCamera.Follow = null; // Disable following
            activeCamera.LookAt = followTarget; // Only look at player

            Transform camTransform = activeCamera.transform;
            camTransform.position = followTarget.position + isometricOffset;
            camTransform.rotation = Quaternion.Euler(isometricRotation);
        }
    }

    private void LateUpdate()
    {
        if (activeCamera != null && followTarget != null)
        {
            Vector3 targetPosition = followTarget.position + isometricOffset;

            activeCamera.transform.position = Vector3.Lerp(
                activeCamera.transform.position,
                targetPosition,
                Time.deltaTime * 5f
            );
        }
    }

    private IEnumerator DisableRoomAfterDelay(GameObject room, float delay)
    {
        yield return new WaitForSeconds(delay);
        room.SetActive(false);
    }
}
