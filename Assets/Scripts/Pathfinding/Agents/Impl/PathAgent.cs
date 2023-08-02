#define DoDebug__
#define Measure__
using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Algorithms.Impl;
using Pathfinding.Data;
using Pathfinding.Grid;
using UnityEngine;

namespace Pathfinding.Agents
{
    public class PathAgent : MonoBehaviour, IPathfindingAgent
    {
        #region Constants
        private const float MinMove2 = 0.005f * 0.005f; // minimum movement squared
        private const float StuckTime = 0.5f; // time to be stuck in seconds
        private const float StuckSideMoveAmountStart = 3; // amount to move sideways, when stuck
        private const float StuckSideMoveAmountEnd = 4; // amount to move sideways, when stuck
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
        public float GetRadius() => _agentRadius;
        public AgentState CurrentState => _agentState;
        public Transform Movable => _movable;
        public float AgentRadius => _agentRadius;
        #endregion

        #region Serialized Settings
        [SerializeField] private  bool _doDebug = true;
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

        public void MoveTo(Vector3 position)
        {
            _currentTarget = _setTarget = position;
#if Measure
            var watch = System.Diagnostics.Stopwatch.StartNew();
#endif
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
                var otherAgentNextPos = otherAgent.GetNextPosition();
                var dirVec = (myNextPos - otherAgentNextPos).normalized;
                var proj = Vector3.Dot(Forward, dirVec);
                // projection > 0  ====> I am ahead of the other
                if (proj >= 0) continue;
                myNextPos = otherAgentNextPos + (myNextPos - otherAgentNextPos).normalized 
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
            var moveVector = _nextPos - Position;
            var distance2 = moveVector.sqrMagnitude;
            if (_nextPercent >= 1f)
            {
                if (distance2 < Mathf.Pow((PathfindingManager.DeltaTime * _currentSpeed), 2))
                {
                    _percent = 1f;
                    _nextPos = _mover.EvaluateAt(_percent);
                }
                Position = _nextPos;
                return;
            }
            var prev = _percent;
            _percent = _mover.GetPercent(_nextPos, _percent * 0.5f, _percent * 1.5f);
            if(_doDebug)
                Log($"Proj percent: {_percent:N6}, prev percent: {prev:N6}");
            
            _lookDirection = _mover.EvaluateAt(_percent + PercentSpeed * PathfindingManager.DeltaTime)
                             - Position;
            if (_lookDirection.x > 0.000001 || _lookDirection.z > 0.000001)
                Rotation = Quaternion.LookRotation(_lookDirection); 
            
            #region Check For Stuck
            if (distance2 < MinMove2)
            {
                _nextPos = Position;
                _State = AgentState.Blocked;
                _stuckTime += Time.deltaTime;
                if (_stuckTime >= 0.5f)
                {
                    _stuckTime = 0f;
                    if(MoveToUnstuck())
                        return;
                }
                Position = _nextPos;
                return;
            }
            #endregion
       
            _stuckTime = 0f;
            var dot = Vector3.Dot(_movable.forward, moveVector);
            if (dot >= 0)
                _State = AgentState.Running;
            else
                _State = AgentState.PushedBack;
            Position = _nextPos;
            var nextCoord = _grid.GetGridCoordinate(_nextPos);
            if (nextCoord != _currentCoord)
            {
                _grid.FreeCoord(_currentCoord);
                _grid.SetBusy(nextCoord);
                _currentCoord = nextCoord;
            }
        }

        private bool MoveToUnstuck()
        {
            CorrectDestination(_setTarget);
            Vector3 sidePos;
            var sign = 1f;
            var dirToTarget = _currentTarget - Position;
            if (Vector3.Dot(dirToTarget, Right) > 0)
                sign = 1;
            else
                sign = -1;
            sidePos = _movable.position + Right * (sign * StuckSideMoveAmountStart);
            var ditToSidePos = sidePos - Position;
            var angle = Vector3.SignedAngle(dirToTarget, ditToSidePos, Vector3.up);
            var walkable = _grid.CheckWalkableAndFree(_grid.GetGridCoordinate(sidePos));
            if (!walkable)
            {
                sidePos = _movable.position - Right * StuckSideMoveAmountStart;
                walkable = _grid.CheckWalkableAndFree(_grid.GetGridCoordinate(sidePos));
            }
            if (walkable)
            {
                if (Math.Abs(angle) > 10)
                    WalkToWaypoints(new List<Vector3>(){sidePos, _currentTarget},OnFailed, OnReachedEndPoint);
                else
                {
                    Log($"Moving right to point");
                    WalkToPoint(_currentTarget, OnFailed, OnReachedEndPoint);
                }
                return true;
            }
            Dbg.Red($"[{gameObject.name}] Side not walkable, sorry ((");
            return false;
        }

        private Vector3 GetSidePosition(Vector3 from, Vector3 to, int side)
        {
            var direction = (to - from).normalized;
            direction = Quaternion.Euler(0, 90 * side, 0) * direction;
            var sideMoveAmount = UnityEngine.Random.Range(StuckSideMoveAmountStart, StuckSideMoveAmountEnd);
            return Vector3.Lerp(from, to, 0.5f) + direction * sideMoveAmount;
        }

        private void Prepare()
        {
            _isMoving = true;
            _percent = 0f;
            _currentSpeed = _moveSpeed;
            _nextPercent = PercentSpeed * PathfindingManager.DeltaTime;
            _lookDirection = _mover.EvaluateAt(_nextPercent) - Position;
            // Debug.Log($"Next pos: {_mover.GetPosition(_nextPercent)}, Pos: {Position}, look: {_lookDirection}");
            _listener.OnBeganRotation();
            if (_lookDirection.x != 0 || _lookDirection.z != 0)
            {
                BeginRotation(_lookDirection);
                return;
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
            var dt = PathfindingManager.DeltaTime;
            _nextPercent = _percent + PercentSpeed * dt;
            var currentPos = Position;
            var diff = _mover.EvaluateAt(_nextPercent) - currentPos;
            _nextPos = currentPos + (diff).normalized 
                * (dt * _currentSpeed);
            // Dbg.Green($"Calculate next, np: {_nextPercent}");
        }

        protected void BeginRotation(Vector3 lookVector)
        {
            _rotationPercent = 0f;
            _endRotation = Quaternion.LookRotation(lookVector);
            _startRotation = Rotation;
            _moveAction = Rotating;
            _listener.OnBeganRotation();
            _State = AgentState.Rotating;
        }

        protected void Rotating()
        {
            _rotationPercent += Time.deltaTime * _rotationSpeed;
            Rotation = Quaternion.Lerp(_startRotation, _endRotation, _rotationPercent);
            if (_rotationPercent >= 1)
            {
                _listener.OnEndedRotation();
                BeginMovement();
            }
        }
        
        protected GridCoord2 CorrectDestination(Vector3 targetPosition)
        {
            _grid.FreeCoord(_currentCoord);
            var currentWorld = Position;
            _currentCoord  = _grid.GetGridCoordinate(currentWorld);
            var toGrid = _grid.GetGridCoordinate(targetPosition);
            var freeGrid = FreePositionFinder.GetClosestFree(_grid, _currentCoord, toGrid);
            if (toGrid != freeGrid)
                targetPosition = _grid.GetWorldPosition(freeGrid);
            targetPosition.y = currentWorld.y;
            _currentTarget = targetPosition;
            OccupyCurrent();
            // Debug.DrawLine(currentWorld + Vector3.up * 0.2f, _currentTarget + Vector3.up * 0.2f, Color.white, 4f);
            return freeGrid;
        }
        
        protected void WalkToPoint(Vector3 targetPosition, Action onFailed, Action onReached)
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

        protected void WalkToWaypoints(List<Vector3> waypoints, Action onFailed, Action onReached)
        {
            PathfindingManager.RemoveMovingAgent(this);
            var currentPosition = _nextPos = Position;
            var fromGridCoord  = _grid.GetGridCoordinate(currentPosition);
            var waypointsGrid = new List<GridCoord2>(waypoints.Count);
            if (_grid.GetGridCoordinate(waypoints[^1]) == fromGridCoord)
            {
                Dbg.Red($"[{gameObject.name}] Free grid coord = current coord");
                onReached.Invoke();
                return;   
            }
            foreach (var waypoint in waypoints)
            {
                var coord = _grid.GetGridCoordinate(waypoint);
                waypointsGrid.Add(coord);
            }
            var path = _pathfinding.FindPathOnWaypoints(fromGridCoord, waypointsGrid, 
                new List<GridCoord2>());
            SetupMoverForPath(path, onFailed, onReached);
        }
        
        protected void SetupMoverForPath(Path path, Action onFailed, Action onReached)
        {
            if (path == null || path.Points.Count < 2)
            {
                var message = $"No Path Found!";
                if (path == null)
                    message += " Path == null";
                else if (path.Points.Count < 2)
                    message += " Points count < 2";
                Log(message); return;
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

        protected void RefreshFieldsForNewPath()
        {
            _percent = _nextPercent = 0f;
            // _percentSpeed = _moveSpeed / _pathTotalDistance;
            _grid.FreeCoord(_currentCoord);
            OccupyCurrent();   
        }
        
        protected void OnFailed()
        {
            WalkToPoint(_setTarget, OnFailed, OnReachedEndPoint);
        }

        protected void OccupyCurrent()
        {
            _currentCoord = _grid.GetGridCoordinate(Position);
            _grid.SetBusy(_currentCoord);
        }

        protected void OnReachedEndPoint()
        {
            _listener.OnStopped();
            _listener.OnReachedFinalPoint();
        }
        
        protected void Log(string message)
        {
            Debug.Log($"[{gameObject.name}] {message}");
        }
        
        
        protected IEnumerator Moving(Action onEnd)
        {
            _isMoving = true;
            _currentSpeed = _moveSpeed;
            _nextPercent = PercentSpeed * PathfindingManager.DeltaTime;
            _lookDirection = _mover.EvaluateAt(_nextPercent) - Position;
            Debug.Log($"Next pos: {_mover.EvaluateAt(_nextPercent)}, Pos: {Position}, look: {_lookDirection}");
            _currentSpeed = 0f;
            _listener.OnBeganRotation();
            if (_lookDirection.x != 0 || _lookDirection.z != 0)
            {
                // Rotation = Quaternion.LookRotation(_lookDirection);
                Dbg.Blue("Calling rotate to look cor");
                _State = AgentState.Rotating;
                // yield return RotatingToLook(_lookDirection);
            }
            else
                Dbg.Red($"Look direction is zero vector");
            _listener.OnEndedRotation();
            _currentSpeed = _moveSpeed;
            _listener.OnBeganMovement();
            _State = AgentState.Running;
            Dbg.Green($"Set Run state");
            while (_percent < 1f)
            {
                var dt = PathfindingManager.DeltaTime;
                _nextPercent = _percent + PercentSpeed * dt;
                var currentPos = Position;
                var diff = _mover.EvaluateAt(_nextPercent) - currentPos;
                _nextPos = currentPos + (diff).normalized 
                    * (dt * _currentSpeed);
                yield return null;    
            }
            _isMoving = false;
            _nextPos = Position;
            _nextPercent = 1f;
            _State = AgentState.Idle;
            onEnd.Invoke();
        }
    }
}