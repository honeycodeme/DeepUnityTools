using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace UnityCNTK
{

    /// <summary>
    /// 2D Vector of integer
    /// </summary>
    [System.Serializable]
    public struct Vector2i
    {
        public Vector2i(int x = 0, int y = 0)
        {
            this.x = x; this.y = y;
        }

        public int ManhattanDistanceTo(Vector2i goal)
        {
            return Mathf.Abs(goal.x - x) + Mathf.Abs(goal.y - y);
        }

        public bool Equals(Vector2i v)
        {
            return v.x == x && v.y == y;
        }
        public int x, y;
    }
    /// <summary>
    /// helps to get the average of data
    /// </summary>
    public class AutoAverage
    {
        private int interval;
        public int Interval
        {
            get { return interval; }
            set { interval = Mathf.Max(value, 1); }
        }

        public float Average
        {
            get
            {
                return lastAverage;
            }
        }

        public bool JustUpdated
        {
            get; private set;
        }

        private float lastAverage = 0;
        private int currentCount = 0;
        private float sum = 0;

        public AutoAverage(int interval = 1)
        {
            Interval = interval;
            JustUpdated = false;
        }

        public void AddValue(float value)
        {
            sum += value;
            currentCount += 1;
            JustUpdated = false;
            if (currentCount >= Interval)
            {
                lastAverage = sum / currentCount;
                cu