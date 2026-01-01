using System;
using UnityEngine;

namespace CompetitivePuckTweaks.src
{
    /// <summary>
    /// Custom class for attaching float to gameobjects
    /// </summary>
    public class FloatComponent : MonoBehaviour
    {
        public float value { get; set; } = 0f;
    }
}