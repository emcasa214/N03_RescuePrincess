using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bundos.MovingPlatforms
{
    public enum WaypointPathType
    {
        Closed,
        Open
    }

    public enum WaypointBehaviorType
    {
        Loop,
        PingPong
    }

    public class PlatformController : MonoBehaviour
    {
        [HideInInspector]
        public List<Vector3> waypoints = new List<Vector3>();

        [Header("Editor Settings")]
        public float handleRadius = .5f;
        public Vector2 snappingSettings = new Vector2(.1f, .1f);
        public Color gizmoDeselectedColor = Color.blue;

        [Header("Platform Waypoint Settings")]
        [SerializeField] private Rigidbody2D rb;
        public bool editing = false;

        public WaypointPathType pathType = WaypointPathType.Closed;
        public WaypointBehaviorType behaviorType = WaypointBehaviorType.Loop;

        public float moveSpeed = 5f;
        public float stopDistance = 0.1f;

        private int lastWaypointIndex = -1;
        private int currentWaypointIndex = 0;
        private int direction = 1;

        private Vector2 _lastPosition;
        private Vector2 _velocity;

        public Vector2 Velocity { get { return _velocity; } }

        private void Start()
        {
            _lastPosition = transform.position;
            rb.isKinematic = true; // Ensure platform is kinematic
        }

        private void Update()
        {
            if (waypoints.Count == 0)
                return;

            if (Vector2.Distance(transform.position, waypoints[currentWaypointIndex]) <= stopDistance)
            {
                if (pathType == WaypointPathType.Closed)
                {
                    switch (behaviorType)
                    {
                        case WaypointBehaviorType.Loop:
                            lastWaypointIndex = currentWaypointIndex;
                            currentWaypointIndex = mod((currentWaypointIndex + direction), waypoints.Count);
                            break;
                        case WaypointBehaviorType.PingPong:
                            if ((lastWaypointIndex == 1 && currentWaypointIndex == 0 && direction < 0) || 
                                (lastWaypointIndex == waypoints.Count - 1 && currentWaypointIndex == 0 && direction > 0))
                            {
                                direction *= -1;
                            }

                            lastWaypointIndex = currentWaypointIndex;
                            currentWaypointIndex = mod((currentWaypointIndex + direction), waypoints.Count);
                            break;
                    }
                }
                else if (pathType == WaypointPathType.Open)
                {
                    switch (behaviorType)
                    {
                        case WaypointBehaviorType.Loop:
                            int nextWaypointIndex = mod((currentWaypointIndex + direction), waypoints.Count);

                            if ((lastWaypointIndex == 1 && currentWaypointIndex == 0 && direction < 0) || 
                                (lastWaypointIndex == waypoints.Count - 2 && currentWaypointIndex == waypoints.Count - 1 && direction > 0))
                            {
                                transform.position = waypoints[nextWaypointIndex];
                            }

                            lastWaypointIndex = currentWaypointIndex;
                            currentWaypointIndex = mod((currentWaypointIndex + direction), waypoints.Count);
                            break;
                        case WaypointBehaviorType.PingPong:
                            if ((lastWaypointIndex == 1 && currentWaypointIndex == 0 && direction < 0) || 
                                (lastWaypointIndex == waypoints.Count - 2 && currentWaypointIndex == waypoints.Count - 1 && direction > 0))
                            {
                                direction *= -1;
                            }

                            lastWaypointIndex = currentWaypointIndex;
                            currentWaypointIndex = mod((currentWaypointIndex + direction), waypoints.Count);
                            break;
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            // Calculate velocity
            Vector2 currentPosition = transform.position;
            _velocity = (currentPosition - _lastPosition) / Time.fixedDeltaTime;
            _lastPosition = currentPosition;

            MoveToWaypoint(waypoints[currentWaypointIndex]);
        }

        private void MoveToWaypoint(Vector3 waypoint)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, waypoint, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
        }

        private void OnDrawGizmos()
        {
            if (IsSelected() && editing)
                return;

            if (pathType == WaypointPathType.Closed)
            {
                for (int i = 0; i < waypoints.Count; i++)
                {
                    Gizmos.color = gizmoDeselectedColor;

                    Vector3 nextPoint = waypoints[(i + 1) % waypoints.Count];
                    Gizmos.DrawLine(waypoints[i], nextPoint);

                    Gizmos.DrawSphere(waypoints[i], handleRadius / 2);
                }
            }
            else
            {
                for (int i = 0; i < waypoints.Count; i++)
                {
                    Gizmos.color = gizmoDeselectedColor;

                    Vector3 nextPoint = waypoints[(i + 1) % waypoints.Count];
                    if (i != waypoints.Count - 1)
                        Gizmos.DrawLine(waypoints[i], nextPoint);

                    Gizmos.DrawSphere(waypoints[i], handleRadius / 2);
                }
            }
        }

        private bool IsSelected()
        {
            #if UNITY_EDITOR
            return UnityEditor.Selection.activeGameObject == transform.gameObject;
            #else
            return false;
            #endif
        }

        int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.transform.SetParent(transform);
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.transform.SetParent(null);
            }
        }
    }
}