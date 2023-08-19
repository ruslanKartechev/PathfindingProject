#define Measure__
#define CorrectDestination__
using System;
using System.Collections.Generic;
using Pathfinding.Algorithms.Impl;
using Pathfinding.Data;
using Pathfinding.Grid;
using TMPro;
using UnityEngine;

namespace Pathfinding.Agents
{
    [RequireComponent(typeof(Rigidbody))]
    public class PathAgent : MonoBehaviour, IPathfindingAgent
    {
        #region Constants
        private const float MinMove2 = 0.004f * 0.004f; // minimum movement squared
        private const float StuckTime = 0.5f; // time to be stuck in seconds
        private const float StuckSideMoveAmountStart = 2; // amount to move sideways, when stuck
        private const float StuckSideMoveAmountEnd = 5; // amount to move sideways, when stuck
        private const float StuckSideMoveAmountStep = 1; // amount to move sideways, when stuck
        #endregion
        
        #region Getters
        public Vector3 GetMoveDirection() => _movable.forward;
        public bool GetIsMoving() => _isMoving;
        public float GetSpeed() => _currentSpeed;
        public GridCoord2 GetCurrentCoord() => _currentCoord;
        public GridCoord2 GetNextCoord() => _gridCoordNext;
        public Vector3 GetPosition() => _movable.position;
        public Vector3 GetTargetPosition() => _currentTarget;
        public string GetName() => gameObject.name;
        public Vector3 GetNextPosition() => _nextPos;
        public float GetRadius() => _agentRadius * _movable.localScale.x;
        public AgentState CurrentState => _agentState;
        public Transform Movable => _movable;
        public float AgentRadius => _agentRadius;
        #endregion

        #region Serialized Settings
        [SerializeField] private  bool _doDebug = true;
        [SerializeField] private Rigidbody _rigidbody;
        [Header("MoveSettings")]
        [SerializeField] private float _moveSpeed = 1f;
        [SerializeField] private float _rotationSpeed = 720;
        [SerializeField] private float _agentRadius = 0.5f;
        [SerializeField] private float _smoothingRadius = 1f;
        [SerializeField] private Transform _movable;
        [SerializeField] private Transform _radiusPlate;
        #endregion

        #region Components
        private IPathfindingGrid _grid;
        private AStart _pathfinding;
        private MovementCalculator _mover;
        private PathCornerSmoother _cornerSmoother;
        private IPathAgentListener _listener;
        #endregion

        #region Grid Coordinates
        private GridCoord2 _currentCoord;
        private GridCoord2 _gridCoordNext;
        #endregion

        private AgentState _agentState;
        private Vector3 _prevPosition;
        private Vector3 _nextPos;
        private Vector3 _lookDirection;
        private float _pathTotalDistance;
        private float _walkedDistance;
        private double _percent;
        private double _nextPercent;
        private float _currentSpeed;
        private bool _isMoving;
        private int _posCorrectAttempts;
        private Vector3 _currentTarget;
        private Vector3 _setTarget;
        private float _stuckTime;
        private float _rotationPercent;
        private Quaternion _startRotation;
        private Quaternion _endRotation;
        
        private Action _moveAction;
        private Action _movementEndAction;
        
        private Vector3 Forward => _movable.forward;
        private Vector3 Right => _movable.right;
        private Vector3 Position
        {
            get => _movable.position;
            set => _movable.position = value;
        }
        private Quaternion Rotation
        {
            get => _movable.rotation;
            set => _movable.rotation = value;
        }
        private AgentState _State
        {
            get => _agentState;
            set
            {
                if (_agentState == value)
                    return;
                _listener.OnStateChanged(value, _agentState);
                _agentState = value;
            }
        }
        private float PercentSpeed => _currentSpeed / _pathTotalDistance;
        public double Percent => _percent;
        private string Lgn => $"[{gameObject.name}]";

        public void InitAgent()
        {
            _nextPos = Position;
            _grid = PathfindingManager.Grid;
            _pathfinding = new AStart(_grid, new DistanceHeuristic());
            _cornerSmoother = new PathCornerSmoother(_smoothingRadius);
            PathfindingManager.AddAgent(this);
            OccupyCurrent();
            if (_radiusPlate == null)
                return;
            var ss = _radiusPlate.localScale;
            ss.z = ss.x = _agentRadius * 2f;
            _radiusPlate.localScale = ss;
        }

        public void Kill()
        {
            Stop();
            PathfindingManager.RemoveAgent(this);
        }

        public void MoveTo(Vector3 position, bool avoidOthers = true)
        {
            _currentTarget = _setTarget = position;
#if Measure
            var watch = System.Diagnostics.Stopwatch.StartNew();
#endif
            if(avoidOthers)
                WalkToPoint(_setTarget, _grid.GetBusyCoords(), OnFailed, OnReachedEndPoint);
            else
                WalkToPoint(_setTarget, OnFailed, OnReachedEndPoint);
#if Measure
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Debug.Log($"[Agent] time: {elapsedMs:N12} ms");
#endif
        }
        
        public void Stop()
        {
            PathfindingManager.RemoveMovingAgent(this);
            _State = AgentState.Idle;
        }
        
        public void SetSpeed(float value) => _moveSpeed = value;
        public void SetPosition(Vector3 position) => Position = position;
        public void SetListener(IPathAgentListener listener) => _listener = listener;

        public void NextAction()
        {
            _moveAction.Invoke();
        }
        
        public void CorrectPosition()
        {
            var collisions = CollisionDetector.GetOverlapAgents(this);
            if (collisions.Count == 0) return;
            var myNextPos = _nextPos;
            foreach (var otherAgent in collisions)
            {
                var hisNextPos = otherAgent.GetNextPosition();
                var dirVec = (myNextPos - hisNextPos).normalized;
                var projOnMe = Vector3.Dot(Forward, dirVec);
                var projOnHim = Vector3.Dot(otherAgent.GetMoveDirection(), dirVec);
                if (projOnHim >= 0)
                {
                    // Dbg.Yellow($"{Lgn} Projection on him >= 0");
                    // Dbg.Log($"{Lgn} On me: {projOnMe}, on him: {projOnHim}");
                }
                // projection > 0  ====> I am ahead of the other
                if (projOnMe < 0 && projOnHim >= 0)
                {
                    Dbg.Green($"{Lgn} Head to head situation");
                    myNextPos += (myNextPos - hisNextPos).normalized 
                        * (GetRadius() + otherAgent.GetRadius());
                    continue;
                }
                if (projOnMe >= 0) 
                    continue;
                myNextPos = hisNextPos + (myNextPos - hisNextPos).normalized 
                                                * (GetRadius() + otherAgent.GetRadius());
            }
            if (_doDebug)
                Debug.DrawLine(myNextPos, _nextPos, Color.green, 0.2f);
            _nextPos = myNextPos;
        }

        public void ApplyPosition()
        {
            if (_State == AgentState.Rotating)
                return;
            _percent = _mover.GetPercent(_nextPos, _percent - 0.25f, _percent + 0.25f);
            var moveVector = _nextPos - Position;
            var distance2 = moveVector.sqrMagnitude;
            if (_nextPercent >= 1f)
            {
                // if(_doDebug)
                    // Dbg.Green($"[{gameObject.name}] NEXT PERCENT is 1f");
                _percent = 1f;
                // _nextPos = _mover.EvaluateAt(_percent);
                Position = _nextPos;
                return;
            }
            var evaluatedPos = _mover.EvaluateAt(_percent + 0.05f);
            _lookDirection = evaluatedPos - _nextPos;
            if (_lookDirection.x > 0.00001 || _lookDirection.z > 0.00001)
                Rotation = Quaternion.LookRotation(_lookDirection);
            #region Check For Stuck
            // if (distance2 < MinMove2)
            // {
            //     _nextPos = Position;
            //     _State = AgentState.Blocked;
            //     _stuckTime += Time.deltaTime;
            //     _rigidbody.velocity = Vector3.zero;
            //     if (_stuckTime >= StuckTime)
            //     {
            //         _stuckTime = 0f;
            //         if(MoveToUnstuckSideways())
            //             return;
            //         // OnFailed();
            //     }
            //     Position = _nextPos;
            //     return;
            // }
            #endregion
            _stuckTime = 0f;
            var dot = Vector3.Dot(_movable.forward, moveVector);
            if (dot >= 0)
                _State = AgentState.Running;
            else
                _State = AgentState.PushedBack;
            // Position = _nextPos;
            _rigidbody.velocity = (moveVector).normalized *(Time.deltaTime * _moveSpeed);
            SetNextCoord();
        }

        private bool MoveToUnstuck()
        {
            var busy = _grid.GetBusyCoords();
            WalkToPoint(_setTarget, busy, OnFailed, OnReachedEndPoint);
            return true;
        }
        
        private bool MoveToUnstuckSideways()
        {
            Debug.Log($"MoveToUnstuckSideways");
            if ((_currentTarget - Position).sqrMagnitude <= Math.Pow(GetRadius() * 2f, 2))
            {
                // Dbg.Red("Already close");
                OnReachedEndPoint();
                return true;
            }
            CorrectDestination(_setTarget);
            var dirToTarget = _currentTarget - Position;
            var sidePos = Position;
            var willWalk = false;
            var side = Math.Sign(Vector3.Dot(dirToTarget, Right));
            for (var distance = StuckSideMoveAmountStart; 
                 distance <= StuckSideMoveAmountEnd; 
                 distance += StuckSideMoveAmountStep)
            {
                if (GetSidePosition(out sidePos, distance, side))
                {
                    willWalk = true;
                    break;
                }
            }
            var ditToSidePos = sidePos - Position;
            var angle = Vector3.SignedAngle(dirToTarget, ditToSidePos, Vector3.up);
            if (willWalk)
            {
                if (Math.Abs(angle) > 10)
                    WalkToWaypoints(new List<Vector3>(){sidePos, _currentTarget},OnFailed, OnReachedEndPoint);
                else
                    WalkToPoint(_currentTarget, OnFailed, OnReachedEndPoint);
                return true;
            }
            Dbg.Red($"[{gameObject.name}] Side not walkable, sorry ((");
            return false;
        }

        private bool GetSidePosition(out Vector3 result, float distance, int preferredSide)
        {
            // Dbg.Red($"Stuck vector: {_stuckVector.normalized}");
            var sideVec = Right;
            // Debug.DrawLine(Position + Vector3.up * 0.5f, Position + sideVec  + Vector3.up * 0.5f, Color.black, 10f);
            var sidePos = Position + sideVec * (preferredSide * distance);
            if (!_grid.CheckWalkableAndFree(_grid.GetGridCoordinate(sidePos)))
            {
                sidePos = Position + sideVec * (-preferredSide * distance);
                if (!_grid.CheckWalkableAndFree(_grid.GetGridCoordinate(sidePos)))
                {
                    result = sidePos;
                    return false;
                }
            }
            result = sidePos;
            return true;
        }

        private void Prepare()
        {
            _isMoving = true;
            _percent = 0f;
            _currentSpeed = _moveSpeed;
            // _nextPercent = PercentSpeed * PathfindingManager.DeltaTime;
            _lookDirection = _mover.EvaluateAt(0.05f) - Position;
            _listener.OnBeganRotation();
            if (_lookDirection.x != 0 || _lookDirection.z != 0)
            {
                var rot = Quaternion.LookRotation(_lookDirection);
                if (CheckRotationCondition(rot))
                {
                    BeginRotation(rot);
                    return;
                }
            }
            BeginMovement();
        }

        private void BeginMovement()
        {
            _currentSpeed = _moveSpeed;
            _listener.OnBeganMovement();
            _State = AgentState.Running;
            _moveAction = CalculateNextPosition;   
        }

        
        
        
        
        
        
        
        
        // MAIN MOVEMENT FUNCTION
        private void CalculateNextPosition()
        {
            if (_percent >= 1f)
            {
                _isMoving = false;
                _nextPos = Position;
                _nextPercent = 1f;
                _State = AgentState.Idle;
                _movementEndAction.Invoke();
                return;
            }

            _percent = _mover.GetPercent(Position);
            var forwardPosition = Position + Forward;
            _nextPercent = _mover.GetPercent(forwardPosition);
            var nextPosOnSpline = _mover.EvaluateAt(_nextPercent);
            _nextPos = Vector3.Lerp(nextPosOnSpline, forwardPosition, .6f);
            _rigidbody.velocity = (nextPosOnSpline - Position).normalized * _currentSpeed;
            
            var rotVec = _nextPos - _prevPosition;
            if(rotVec.x > 0.00001  || rotVec.z > 0.00001)
                Rotation = Quaternion.Lerp(Rotation,Quaternion.LookRotation(rotVec), .05f);
            Debug.DrawRay(Position, rotVec, Color.red, 3);
            
            #region Check For Stuck
            var distance2 = (Position - _prevPosition).sqrMagnitude;
            if (distance2 < MinMove2)
            {
                // _State = AgentState.Blocked;
                _stuckTime += Time.deltaTime;
                if (_stuckTime >= StuckTime)
                {
                    _stuckTime = 0f;
                    _prevPosition = Position;
                    Debug.Log($"Stuck completely");
                    // if(MoveToUnstuckSideways())
                        // return;
                }
            }
            else
                _stuckTime = 0;
            #endregion
            _prevPosition = Position;

            if ((_currentTarget - Position).sqrMagnitude <= 0.1f * 0.1f)
            {
                Debug.Log($"Reached Final position");
                _rigidbody.velocity = Vector3.zero;
                OnReachedEndPoint();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            
        }


        private bool CheckRotationCondition(Quaternion rotation)
        {
            var diff = Math.Abs(rotation.eulerAngles.y - Rotation.eulerAngles.y);
            // Dbg.Red($"Angles diff: {diff}");
            return diff > 10;
        }
        
        private void BeginRotation(Quaternion rotation)
        {
            _rotationPercent = 0f;
            _endRotation = rotation;
            _startRotation = Rotation;
            _moveAction = CalculateNextRotation;
            _listener.OnBeganRotation();
            _State = AgentState.Rotating;
        }

        private void CalculateNextRotation()
        {
            _rotationPercent += Time.deltaTime * _rotationSpeed;
            Rotation = Quaternion.Lerp(_startRotation, _endRotation, _rotationPercent);
            if (_rotationPercent >= 1)
            {
                _listener.OnEndedRotation();
                BeginMovement();
            }
        }
        
        private GridCoord2 CorrectDestination(Vector3 targetPosition)
        {
            _grid.FreeCoord(_currentCoord);
            var currentWorld = Position;
            _currentCoord  = _grid.GetGridCoordinate(currentWorld);
            var toGrid = _grid.GetGridCoordinate(targetPosition);
            #if CorrectDestination
            var freeWorld = FreePositionFinder.GetClosestPos(_grid, this, currentWorld, _currentTarget, _agentRadius);
            #else
            var freeWorld = targetPosition;
            #endif
            var freeGrid = _grid.GetGridCoordinate(freeWorld);
            if (toGrid != freeGrid)
                targetPosition = _grid.GetWorldPosition(freeGrid);
            targetPosition.y = currentWorld.y;
            _currentTarget = targetPosition;
            OccupyCurrent();
            // Debug.DrawLine(currentWorld + Vector3.up * 0.2f, _currentTarget + Vector3.up * 0.2f, Color.white, 4f);
            
            
            
            return freeGrid;
        }

        private void WalkToPoint(Vector3 targetPosition, ICollection<GridCoord2> excluded, Action onFailed, Action onReached)
        {
            PathfindingManager.RemoveMovingAgent(this);
            _mover = null;
            var correctedTarget = CorrectDestination(targetPosition);
            if (correctedTarget == _currentCoord)
            {
                onReached.Invoke();
                return;
            }
            var path = _pathfinding.FindPath(_currentCoord, correctedTarget, excluded);
            SetupMoverForPath(path, onFailed, onReached);
        }
        
        private void WalkToPoint(Vector3 targetPosition, Action onFailed, Action onReached)
        {
            PathfindingManager.RemoveMovingAgent(this);
            var freeGrid = CorrectDestination(targetPosition);
            if (freeGrid == _currentCoord)
            {
                // Dbg.Red($"[{gameObject.name}] Free grid coord = current coord");
                onReached.Invoke();
                return;
            }
            var path = _pathfinding.FindPath(_currentCoord, freeGrid);
            SetupMoverForPath(path, onFailed, onReached);
        }

        private void WalkToWaypoints(List<Vector3> waypoints, Action onFailed, Action onReached)
        {
            PathfindingManager.RemoveMovingAgent(this);
            var currentPosition = _nextPos = Position;
            var fromGridCoord  = _grid.GetGridCoordinate(currentPosition);
            var waypointsGrid = new List<GridCoord2>(waypoints.Count);
            if (_grid.GetGridCoordinate(waypoints[^1]) == fromGridCoord)
            {
                // Dbg.Red($"[{gameObject.name}] Free grid coord = current coord");
                onReached.Invoke();
                return;   
            }
            foreach (var waypoint in waypoints)
            {
                var coord = _grid.GetGridCoordinate(waypoint);
                waypointsGrid.Add(coord);
            }
            var busy = _grid.GetBusyCoords();
            var path = _pathfinding.FindPathOnWaypoints(fromGridCoord, waypointsGrid, busy);
            SetupMoverForPath(path, onFailed, onReached);
        }
        
        private void SetupMoverForPath(Path path, Action onFailed, Action onReached)
        {
            if (path == null || path.Points.Count < 2)
            {
                // var message = $"No Path Found!";
                // if (path == null)
                //     message += " Path == null";
                // else if (path.Points.Count < 2)
                //     message += " Points count < 2";
                /* Log(message); */ return;
            }
            RefreshFieldsForNewPath();
            _movementEndAction = onReached;
            var endWorldPos = _currentTarget;
            if (!path.foundPathToDestination)
            {
                _currentTarget = _grid.GetWorldPosition(path.Points[0]);
                _movementEndAction = onFailed;
            }
            _mover = new MovementCalculator(path, Position, 
                endWorldPos, _grid, _cornerSmoother, _doDebug);
            _pathTotalDistance = _mover.GetTotalLength();
            if (_pathTotalDistance == 0f) 
            { 
                Debug.LogError($"[{gameObject.name}] Path Length = 0! Error! Agent Stopped"); 
                Stop();
                _State = AgentState.Idle;
                return;
            }
            _moveAction = Prepare;
            PathfindingManager.AddMovingAgent(this);
        }

        private void RefreshFieldsForNewPath()
        {
            _percent = _nextPercent = 0f;
            _grid.FreeCoord(_currentCoord);
            OccupyCurrent();   
        }
        
        private void OnFailed()
        {
            PathfindingManager.RemoveMovingAgent(this);
            Debug.Log($"Failed to walk to: {_setTarget}");
            // WalkToPoint(_setTarget, OnFailed, OnReachedEndPoint);
        }

        private void OccupyCurrent()
        {
            _currentCoord = _grid.GetGridCoordinate(Position);
            _grid.SetBusy(_currentCoord);
        }

        private void OnReachedEndPoint()
        {
            PathfindingManager.RemoveMovingAgent(this);
            _State = AgentState.Idle;
            _listener.OnReachedFinalPoint();
        }
        
        private void SetNextCoord()
        {
            var nextCoord = _grid.GetGridCoordinate(_nextPos);
            if (nextCoord != _currentCoord)
            {
                _grid.FreeCoord(_currentCoord);
                _grid.SetBusy(nextCoord);
                _currentCoord = nextCoord;
            }
        }
        
        protected void Log(string message)
        {
            Debug.Log($"[{gameObject.name}] {message}");
        }
        
    }
}