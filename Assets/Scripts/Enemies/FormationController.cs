using System.Collections.Generic;
using UnityEngine;

namespace ChickenShooter
{
    /// <summary>
    /// Controls a group of chickens: horizontal sway and a slow descent as a
    /// single formation anchor. Chickens only store their local slot.
    /// </summary>
    public class FormationController : MonoBehaviour
    {
        private float _amplitude;
        private float _speed;
        private float _descent;
        private float _time;
        private Vector3 _origin;

        public void Configure(float amplitude, float speed, float descent)
        {
            _amplitude = amplitude;
            _speed = speed;
            _descent = descent;
            _origin = transform.position;
            _time = 0f;
        }

        private void Update()
        {
            _time += Time.deltaTime;

            Vector3 position = _origin;
            position.x += Mathf.Sin(_time * _speed) * _amplitude;
            position.y -= Mathf.Min(_descent * _time, 1.5f);
            transform.position = position;
        }

        public Vector3 GetSlotWorld(Vector2 slot)
        {
            return transform.TransformPoint(new Vector3(slot.x, slot.y, 0f));
        }
    }
}
