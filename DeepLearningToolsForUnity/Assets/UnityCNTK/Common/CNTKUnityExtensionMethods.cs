﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CNTK;
namespace UnityCNTK
{
    public static class CNTKUnityExtensionMethods
    {
        /// <summary>
        /// Shallow copy a IList and retrn a array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iList"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static T[] CopyToArray<T>(this IList<T> iList, int startIndex = 0)
        {
            T[] result = new T[iList.Count - startIndex];
            iList.CopyTo(result, startIndex);
            return result;
        }


        public static Parameter FindParameterByName(this Function func, string name)
        {
            var allInputs = func.Parameters();
            foreach (var p in allInputs)
            {
                if (p.Kind == VariableKind.Parameter)
                {
                    //look for the parameter of the same name from the other function
                    if (name == p.Name)