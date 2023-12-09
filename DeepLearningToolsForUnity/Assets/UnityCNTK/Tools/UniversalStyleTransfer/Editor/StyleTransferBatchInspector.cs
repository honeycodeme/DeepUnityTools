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
                transferHelper.UpdateMaterialList();
            }

            //effected materials
            EditorGUILayout.LabelField("Effected materials");
            List<Material> keys = new List<Material>(transferHelper.matTextureDic.Keys);
            foreach(var m in keys)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(m, typeof(Material),true);
                if (GUILayout.Button("Remove"))
                {
                    transferHelper.matTextureDic.Remove(m);
    