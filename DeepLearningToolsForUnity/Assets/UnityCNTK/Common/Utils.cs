﻿using System.Collections;
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
                currentCount = 0;
                sum = 0;
                JustUpdated = true;
            }
        }


    }

    public static class Utils 
    {
        public static float[] GenerateWhiteNoise(int size, float min, float max)
        {
            if (size <= 0)
                return null;
            float[] result = new float[size];
            for (int i = 0; i < size; ++i)
            {
                result[i] = UnityEngine.Random.Range(min, max);
            }
            return result;
        }


        public static float NextGaussianFloat()
        {
            float u, v, S;

            do
            {
                u = 2.0f * Random.value - 1.0f;
                v = 2.0f * Random.value - 1.0f;
                S = u * u + v * v;
            }
            while (S >= 1.0);

            float fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
            return u * fac;
        }
    }

    public static class RLUtils
    {
        public static float[] DiscountedRewards(float[] rewards, float discountFactor = 0.99f, float nextValue = 0)
        {
            float accum = nextValue;
            float[] result = new float[rewards.Length];
            for (int i = rewards.Length - 1; i >= 0; --i)
            {
                accum = accum * discountFactor + rewards[i];
                result[i] = accum;
            }

            return result;
        }

        public static float[] GeneralAdvantageEst(float[] rewards, float[] estimatedValues, float discountedFactor = 0.99f, float GAEFactor = 0.95f, float nextValue = 0)
        {
            Debug.Assert(rewards.Length == estimatedValues.Length);
            float[] deltaT = new float[rewards.Length];
            for(int i = 0; i < rewards.Length; ++i)
            {
                if(i != rewards.Length - 1)
                {
                    deltaT[i] = rewards[i] + discountedFactor * estimatedValues[i + 1] - estimatedValues[i];
                }
                else
                {
                    deltaT[i] = rewards[i] + discountedFactor * nextValue - estimatedValues[i];
                }

            }
            return DiscountedRewards(deltaT, GAEFactor * discountedFactor);
        }

    }


    public static class MathUtils
    {
        public enum InterpolateMethod
        {
            Linear,
            Log
        }

        /// <summary>
        /// interpolate between x1 and x2 to ty suing the interpolate method
        /// </summary>
        /// <param name="method"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static float Interpolate(float x1, float x2, float t, InterpolateMethod method = InterpolateMethod.Linear)
        {
            if (method == InterpolateMethod.Linear)
            {
                return Mathf.Lerp(x1, x2, t);
            }
            else
            {
                return Mathf.Pow(x1, 1 - t) * Mathf.Pow(x2, t);
            }
        }

        /// <summary>
        /// Return a in