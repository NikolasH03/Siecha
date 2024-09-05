using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class LockOnSystem : MonoBehaviour
{
    public CinemachineTargetGroup targetGroup;
    public CinemachineVirtualCamera lockOnCamera;
    public CinemachineFreeLook freeLookCamera;
    public float lockOnRadius = 20f;
    public Transform player;  
    private Transform currentTarget;
    public bool locked;
    public float maxLockOnDistance;
    //private LookAtEnemy lookAtEnemy;
    void Start()
    {
        // Initialize the target group with two slots: one for the player and one for the enemy
        targetGroup.m_Targets = new CinemachineTargetGroup.Target[2];

        locked = false;
        // Add the player to the target group
        targetGroup.m_Targets[0].target = player;
        targetGroup.m_Targets[0].weight = 2;
        targetGroup.m_Targets[0].radius = 2; // Adjust as needed

        // Get the LookAtEnemy script component
        //lookAtEnemy = GetComponent<LookAtEnemy>();

        // Set the initial settings for the lock-on camera
        SetupLockOnCamera();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q )) // Change to your desired lock-on key
        {
            if (currentTarget == null)
            {
                FindNewTarget();
            }
            else
            {
                ClearTarget();
            }
        }

        if (currentTarget != null)
        {
            targetGroup.m_Targets[1].target = currentTarget;
            targetGroup.m_Targets[1].weight = 1;
            targetGroup.m_Targets[1].radius = 2; // Adjust as needed

            float distance = Vector3.Distance(player.position, currentTarget.position);
            if (distance > maxLockOnDistance)
            {
                ClearTarget();  // Desactiva el lock-on si la distancia es mayor que el umbral
            }
        }
    }

    void FindNewTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("enemy");
        float closestDistance = lockOnRadius;
        Transform nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }

        if (nearestEnemy != null)
        {
            currentTarget = nearestEnemy;
            AddTargetToGroup(nearestEnemy);
            SwitchToLockOnCamera();
            //lookAtEnemy.SetTarget(currentTarget);  // Set the target in the LookAtEnemy script
        }
    }

    void ClearTarget()
    {
        currentTarget = null;
        RemoveTargetFromGroup();
        SwitchToFreeLookCamera();
        //lookAtEnemy.ClearTarget();  // Clear the target in the LookAtEnemy script
    }

    void AddTargetToGroup(Transform target)
    {
        targetGroup.m_Targets[1].target = target;
        targetGroup.m_Targets[1].weight = 1;
        targetGroup.m_Targets[1].radius = 1; // Adjust as needed
    }

    void RemoveTargetFromGroup()
    {
        targetGroup.m_Targets[1].target = null;
        targetGroup.m_Targets[1].weight = 0;
        targetGroup.m_Targets[1].radius = 0;
    }

    void SwitchToLockOnCamera()
    {
        freeLookCamera.Priority = 0;
        lockOnCamera.Priority = 10;
        locked = true;
    }

    void SwitchToFreeLookCamera()
    {
        lockOnCamera.Priority = 0;
        freeLookCamera.Priority = 10;
        locked = false;
    }

    void SetupLockOnCamera()
    {
        // Set up the lock-on camera with a Cinemachine Transposer and Composer
        var transposer = lockOnCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer == null)
        {
            transposer = lockOnCamera.AddCinemachineComponent<CinemachineTransposer>();
        }
        transposer.m_FollowOffset = new Vector3(0, 2, -3); // Adjust this to position the camera behind the player

        var composer = lockOnCamera.GetCinemachineComponent<CinemachineComposer>();
        if (composer == null)
        {
            composer = lockOnCamera.AddCinemachineComponent<CinemachineComposer>();
        }
        composer.m_TrackedObjectOffset = new Vector3(0, 1, 0); // Adjust this to look at the target correctly
        composer.m_LookaheadTime = 0;
        composer.m_LookaheadSmoothing = 0;
        composer.m_HorizontalDamping = 0.5f;
        composer.m_VerticalDamping = 0.5f;
        composer.m_ScreenX = 0.5f;
        composer.m_ScreenY = 0.55f;
    }

}






