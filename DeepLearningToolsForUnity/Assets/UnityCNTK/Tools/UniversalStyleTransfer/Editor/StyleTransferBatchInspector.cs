using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityCNTK
{

    [CustomEditor(typeof(StyleTransferBatchHelper))]
    public class StyleTransferBatchInspector : UnityEditor.Editor
    {
        StyleTransferBatchHelper transferHelper;

        private void OnEnable()
        {
            transferHelper = (StyleTransferBatchHelper)target;
        }

        public override void OnInspectorGUI()
        {

            Undo.RecordObject(transferHelper, "StyleTransfer");


            if(GUILayout.Button("Update Materials"))
            {
            