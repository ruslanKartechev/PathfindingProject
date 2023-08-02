using System.Collections;
using System.Collections.Generic;
using Pathfinding.Grid;
using UnityEngine;

namespace Pathfinding.Examples.Agents
{
    public class AgentSpawner : MonoBehaviour
    {
        public bool coroutineSpawn;
        public float spawnDelay = 0.5f;
        [Space(5)]
        public Transform targetPointDefault;
        public List<Transform> targets;
        public bool randomTargets;
        [Space(5)]
        public PositionRectGrid rectGrid;
        public PathTestAgent agentPrefab;
        public float moveSpeed;
        public bool spawnOnGrid;
        public bool initPrespawned;
        public List<PathTestAgent> prespawned;
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            rectGrid.Draw();
        }
        #endif

        private void OnValidate()
        {
            rectGrid.Generate();
        }

        private void Start()
        {
            if (spawnOnGrid)
            {
                if (coroutineSpawn)
                    StartCoroutine(Spawning());
                else
                    Spawn();
            }
            if(initPrespawned)
                InitPreSpawned();
        }

        public void InitPreSpawned()
        {
            foreach (var agent in prespawned)
            {
                if(agent == null || agent.gameObject.activeInHierarchy == false)
                    continue;
                agent.MoveAgentToTarget(targetPointDefault);
            }
        }

        public void Spawn()
        {
            var i = 1;
            foreach (var position in rectGrid.worldPositions)
            {
                SpawnOne(position, i);
                i++;
            }
        }

        private void SpawnOne(Vector3 position, int i)
        {
            var instance = Instantiate(agentPrefab, transform);
            instance.SetPosition(position);
            instance.gameObject.name = $"A {i.ToString()}";
            instance.SetSpeed(moveSpeed);
            if (randomTargets)
            {
                instance.MoveAgentToTarget(targets.GetRandom());
            }
            else
                instance.MoveAgentToTarget(targetPointDefault);
            
        }
        
        private IEnumerator Spawning()
        {
            yield return new WaitForSeconds(spawnDelay);
            var i = 1;
            foreach (var position in rectGrid.worldPositions)
            {
                SpawnOne(position, i);
                i++;
                yield return new WaitForSeconds(spawnDelay);
            }
        }
    }
}