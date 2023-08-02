using System;
using System.Collections;
using UnityEngine;

namespace Pathfinding.Examples.Agents
{
    public class MovingTarget : MonoBehaviour
    {
        public float speed;
        public Transform movable;
        public bool doMove = true;
        [Space(10)]
        public Transform p1;
        public Transform p2;
        private Coroutine _moving;
        private void Start()
        {
            if(doMove)
                _moving = StartCoroutine(Moving());
        }
        
        private IEnumerator Moving()
        {
            while (true)
            {
                yield return PosChange(p2.position);
                yield return PosChange(p1.position);
            }
        }

        private IEnumerator PosChange(Vector3 endPos)
        {
            var elapsed = 0f;
            var startPos = movable.position;
            var time = (endPos - startPos).magnitude / speed;
            while (elapsed <= time)
            {
                movable.position = Vector3.Lerp(startPos, endPos, elapsed / time);   
                elapsed += Time.deltaTime;
                yield return null;
            }
            movable.position = endPos;
        }
    }
}