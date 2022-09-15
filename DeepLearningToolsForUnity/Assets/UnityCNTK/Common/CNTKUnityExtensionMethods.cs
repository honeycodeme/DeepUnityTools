using System.Collections;
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
            T[] result =