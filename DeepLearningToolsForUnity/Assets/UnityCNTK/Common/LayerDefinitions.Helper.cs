using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CNTK;

namespace UnityCNTK.LayerDefinitions
{
    public static class LayerDefineHelper
    {
        public static ResNodeDenseDef[] ResNodeLayers(int numOfNodes, int hiddenSize, NormalizationMethod normalizatoin = NormalizationMethod.None,float dropout = 0.0f)
        {
            var result = new ResNodeDenseDef[numOfNodes];
            for(int i = 0; i < numOfNodes; ++i)
            {
                result[i]=new ResNodeDenseDef(hiddenSize, normalizatoin, dropout);
            }
   