// CameraDolly.cs - smooth waypoint camera ride for trailers/cinematics.
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    /// <summary>
    /// Trailer camera: glides through waypoints at constant speed. Disables CameraFollow while active.
    /// </summary>
    public class CameraDolly : MonoBehaviour
    {
        public Vector2[] waypoints;
        public float speed = 3f;
        public bool loop = false;

        int _i;
        bool _done;

        void Start()
        {
            var follow = GetComponent<CameraFollow>();
            if (follow != null) follow.enabled = false;
            if (waypoints != null && waypoints.Length > 0)
                transform.position = new Vector3(waypoints[0].x, waypoints[0].y, transform.position.z);
        }

        void Update()
        {
            if (_done || waypoints == null || waypoints.Length < 2) return;
            Vector3 target = new Vector3(waypoints[_i + 1].x, waypoints[_i + 1].y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                _i++;
                if (_i >= waypoints.Length - 1)
                {
                    if (loop) _i = 0;
                    else _done = true;
                }
            }
        }
    }
}
