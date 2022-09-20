
﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityCNTK
{
    public class DataBuffer
    {
        public enum DataType
        {
            Float,
            Integer,
            Boolean
        }
        public struct DataInfo
        {
            public DataInfo(string name, DataType type, int unitLength)
            {
                this.type = type;
                this.unitLength = unitLength;
                this.name = name;
            }
            public string name;
            public DataType type;
            public int unitLength;
        }

        protected struct DataContainer
        {
            public DataContainer(DataInfo info, int maxLength)
            {
                this.info = info;
                if (info.type == DataType.Float)
                {
                    dataList = new float[info.unitLength * maxLength];
                }
                else if (info.type == DataType.Integer)
                {
                    dataList = new int[info.unitLength * maxLength];
                }
                else
                {
                    dataList = new bool[info.unitLength * maxLength];
                }
            }
            public DataInfo info;
            public Array dataList;
        }


        protected Dictionary<string, DataContainer> dataset;

        public int MaxCount { get; private set; } = 0;
        private int nextBufferPointer = 0;
        public int CurrentCount { get; private set; } = 0;



        public DataBuffer(int maxSize, params DataInfo[] dataInfos)
        {

            MaxCount = maxSize;
            dataset = new Dictionary<string, DataContainer>();


            foreach (var i in dataInfos)
            {
                Debug.Assert(!dataset.ContainsKey(i.name));
                dataset[i.name] = new DataContainer(i, maxSize);
            }
        }




        /// <summary>
        /// Add data to the buffer
        /// </summary>
        /// <param name="data"></param>
        public void AddData(params Tuple<string, Array>[] data)
        {
            //check whether the input data are correct
            Debug.Assert(data.Length == dataset.Count, "Input data number is not the same as the buffer required");
            int size = data[0].Item2.Length / dataset[data[0].Item1].info.unitLength;
            foreach (var k in dataset.Keys)
            {
                bool found = false;
                foreach (var d in data)
                {
                    if (d.Item1.Equals(k))
                    {