using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TargetScanner
{
    [Tooltip("Center of Scanner")]
    [SerializeField] Transform transform;
    [Tooltip("Radius within which scanner will always detect Target")]
    [SerializeField] private float alertRadius = 1f;
    [Tooltip("Detection Radius of the Scanner")]
    [SerializeField] private float viewRadius = 5f;
    [Tooltip("Field Of View of the Scanner")]
    [Range(0f, 360f)]
    [SerializeField] private float viewAngle = 60f;
    [Tooltip("Layer of Target")]
    [SerializeField] private LayerMask targetLayer;
    [Tooltip("Layer of objects which Scanner should not see through")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float heightOffset = 0;
    [Tooltip("Max Height Detection Range, Detect both Upward & Downward")]
    [SerializeField] private float maxHeightDifference = 1f;

    private List<Transform> targetList = new List<Transform>();
    private Vector3 eyePos;
    RaycastHit hit;

    public float ViewRadius { get { return viewRadius; } set { viewRadius = value; } }
    public float ViewAngle { get { return viewAngle; } set { viewAngle = value; } }
    public LayerMask TargetLayer { get { return targetLayer; } set { targetLayer = value; } }
    public LayerMask ObstacleLayer { get { return obstacleLayer; } set { obstacleLayer = value; } }
    public List<Transform> TargetList { get { return targetList; } }
    public float HeightOffset { get { return heightOffset; } set { heightOffset = value; } }
    public float MaxHeightDifference { get { return maxHeightDifference; } set { maxHeightDifference = value; } }

    private void RunScanner()
    {
        eyePos = transform.position + Vector3.up * heightOffset;

        FindVisibleTargets();
        RemoveTargetFromList();
    }

    /// <summary>
    /// Find the target within the view radius and store in List of Transform if follows particular paramerters
    /// </summary>
    private void FindVisibleTargets()
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetLayer);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Vector3 targetSize = targetsInViewRadius[i].bounds.size;
            Transform target = targetsInViewRadius[i].transform;

            Vector3 toPlayer = target.transform.position - eyePos;

            if (Mathf.Abs(toPlayer.y + heightOffset) > maxHeightDifference)
            {
                return;
            }

            if (Vector3.Distance(transform.position, target.position) < alertRadius)
            {
                if (!targetList.Contains(target))
                    targetList.Add(target);

                continue;
            }

            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {

                targetSize.y -= 0.05f;

                float offsetX = targetSize.x / 2;
                float offsetY = targetSize.y / 2;

                int rayCastIteration = 0;

                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 5; k++)
                    {
                        Vector3 targetPosition = target.position + new Vector3(offsetX, offsetY, 0);

                        float distToTarget = Vector3.Distance(transform.position, target.position);

                        dirToTarget = (targetPosition - transform.position).normalized;

                        Debug.DrawLine(transform.position, targetPosition);//----------------------------------------------Debug RayCast

                        if (!Physics.Raycast(transform.position, dirToTarget, out hit, distToTarget, obstacleLayer))
                        {
                            if (!targetList.Contains(target))
                            {
                                targetList.Add(target);
                            }

                            goto EndOfLoop;
                        }

                        offsetY -= targetSize.y / 4;
                    }

                    rayCastIteration++;
                    offsetY = targetSize.y / 2;
                    offsetX -= targetSize.x / 2;

                }

                if (rayCastIteration >= 3 && targetList.Contains(target))
                {
                    targetList.Remove(target);
                }

            }

        EndOfLoop:;
        }


    }

    /// <summary>
    /// Remove Targets which are not in Range
    /// </summary>
    private void RemoveTargetFromList()
    {
        if (targetList.Count == 0) return;

        for (int i = 0; i < targetList.Count; i++)
        {
            Transform target = targetList[i];

            //Null Check---------------------------------------------
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                targetList.Remove(target);
                continue;
            }

            //Out of View Radius---------------------------------------------
            if (Vector3.Distance(transform.position, target.position) > viewRadius)
            {
                targetList.Remove(target);
                continue;
            }

            //Inside Alert Radius---------------------------------------------
            if (Vector3.Distance(transform.position, target.position) < alertRadius) continue;

            //HeightCheck---------------------------------------------
            Vector3 toPlayer = target.transform.position - eyePos;
            if (Mathf.Abs(toPlayer.y + heightOffset) > maxHeightDifference)
            {
                targetList.Remove(target);
            }

            //Out of FOV---------------------------------------------
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) > viewAngle / 2)
            {
                targetList.Remove(target);
            }
            

        }

    }

    /// <summary>
    /// Return the first detected target by scanner
    /// </summary>
    /// <returns>(List of Transforms) Targets</returns>
    public Transform GetTarget()
    {
        RunScanner();

        if (targetList.Count == 0) return null;

        return targetList[0];
    }

    /// <summary>
    /// Return the nearest target among the detected target by scanner
    /// </summary>
    /// <returns>(Transform) Closest Target</returns>
    public Transform GetNearestTarget()
    {
        RunScanner();

        if (targetList.Count == 0) return null;

        Transform _nearestTargetPos;

        _nearestTargetPos = targetList[0];

        for (int i = 0; i < targetList.Count; i++)
        {
            if (Vector3.Distance(transform.position, targetList[i].position) <
                Vector3.Distance(transform.position, _nearestTargetPos.position))
            {
                _nearestTargetPos = targetList[i];
            }

        }

        return _nearestTargetPos;
    }

    /// <summary>
    /// Return the List of all target detected by scanner
    /// </summary>
    public List<Transform> GetTargetList()
    {
        RunScanner();

        if (targetList.Count == 0) return null;

        return targetList;
    }

    /// <summary>
    /// Show Gizmos of scanner
    /// </summary>
    public void ShowGizmos()
    {
        if (transform == null) return;

        Gizmos.color = Color.red;

        if (hit.collider != null) Gizmos.DrawSphere(hit.point, 0.15f);

        //Eye Location
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * heightOffset, 0.2f);

        //Height Check Area
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(transform.position + Vector3.up * maxHeightDifference, Vector3.up, viewRadius);
        UnityEditor.Handles.DrawWireDisc(transform.position + Vector3.down * maxHeightDifference, Vector3.up, viewRadius);

        //Always Detect Radius
        Color r = new Color(0.5f, 0f, 0f, 0.5f);
        UnityEditor.Handles.color = r;
        UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, Vector3.forward, 360f, alertRadius);

        //View Radius
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, viewRadius);

        //View Angle
        Color b = new Color(0, 0.5f, 0.7f, 0.2f);
        UnityEditor.Handles.color = b;
        Vector3 rotatedForward = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, rotatedForward, viewAngle, viewRadius);

        //To Target Line
        Gizmos.color = Color.red;
        foreach (Transform t in targetList)
        {
            Gizmos.DrawLine(transform.position, t.position);
            Gizmos.DrawCube(t.position, new Vector3(0.3f, 0.3f, 0.3f));
        }
    }

























}//class
