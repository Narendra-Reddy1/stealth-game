using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FollowTarget : MonoBehaviour
{

    #region Variables
    [SerializeField] private Transform m_targetToFollow;
    [SerializeField] private Vector3 m_Offset;
    #endregion

    private void Start()
    {
        try
        {
            m_Offset = transform.position - m_targetToFollow.position;
        }
        catch (UnassignedReferenceException)
        {
            Debug.LogError("Target to Follow is not assigned dude");
        }

    }

    private void LateUpdate()
    {
        FollowTheTarget();
    }

    private void FollowTheTarget()
    {
        try
        {
            transform.position = m_targetToFollow.position + m_Offset;
        }
        catch (UnassignedReferenceException)
        {
            Debug.LogError("Target to Follow is not assigned dude");
        }
    }
}