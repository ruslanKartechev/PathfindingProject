using System;
using UnityEngine;

namespace TestGame
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private int fps;
        private void Awake()
        {
            Application.targetFrameRate = fps;
        }
    }
}