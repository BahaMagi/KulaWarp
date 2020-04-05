using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraClipping : MonoBehaviour
{
    void Awake()
    {
        // Resize it to reach to just before the player and just above the current box
        Vector3 upSize       = (PlayerController.pc.world_up * CameraController.cc.upOffset * 1.8f).Abs();
        Vector3 offSize      = (PlayerController.pc.world_direction * 2.0f).Abs();
        Vector3 dirSize      = (Vector3.Cross(PlayerController.pc.world_direction, PlayerController.pc.world_up) * 0.85f).Abs();
        Vector3 colliderSize = upSize + dirSize + offSize;

        gameObject.GetComponent<BoxCollider>().size = colliderSize;
    }

    void Update()
    {
        transform.position = CameraController.cc.transform.position - 0.5f * CameraController.cc.dirOffset * PlayerController.pc.world_direction; ;
        transform.LookAt(PlayerController.pc.transform.position.SnapToGridUp(PlayerController.pc.world_up) + CameraController.cc.upOffset * PlayerController.pc.world_up);
    }

    private void OnTriggerEnter(Collider other)
    {
        other.gameObject.GetComponents<Renderer>()[0].enabled = false;
    }

    private void OnTriggerExit(Collider other)
    {
        other.gameObject.GetComponents<Renderer>()[0].enabled = true;
    }
}
