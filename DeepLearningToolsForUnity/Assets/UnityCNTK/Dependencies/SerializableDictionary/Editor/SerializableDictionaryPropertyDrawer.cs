using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

public class SerializableDictionaryPropertyDrawer : PropertyDrawer
{
	const string KeysFieldName = "m_keys";
	const string ValuesFieldName = "m_values";

	static GUIContent m_iconPlus = IconContent ("Toolbar Plus", "Add entry");
	static GUIContent m_iconMinus = IconContent ("Toolbar Minus", "Remove entry");
	static GUIContent m_warningIconConflict = IconContent ("console.warnicon.sml", "Conflicting key, this entry will be lost");
	static GUIContent m_warningIconOther = IconContent ("console.infoicon.sml", "Conflicting key");
	static GUIContent m_warningIconNull = IconContent ("console.warnicon.sml", "Null key, this entry will be lost");
	static GUIStyle m_buttonStyle = GUIStyle.none;

	object m_conflictKey = null;
	object m_conflictValue = null;
	int m_conflictIndex = -1 ;
	int m_conflictOtherIndex = -1 ;
	bool m_conflictKeyPropertyExpanded = false;
	bool m_conflictValuePropertyExpanded = false;
	float m_conflictLineHeight = 0f;

	enum Action
	{
		None,
		Add,
		Remove
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		label = EditorGUI.BeginProperty(position, label, property);

		Action buttonAction = Action.None;
		int buttonActionIndex = 0;

		var keyArrayProperty = property.FindPropertyRelative(KeysFieldName);
		var valueArrayProperty = property.FindPropertyRelative(ValuesFieldName);

		if(m_conflictIndex != -1)
		{
			keyArrayProperty.InsertArrayElementAtIndex(m_conflictIndex);
			var keyProperty = keyArrayProperty.GetArrayElementAtIndex(m_conflictIndex);
			SetPropertyValue(keyProperty, m_conflictKey);
			keyProperty.isExpanded = m_conflictKeyPropertyExpanded;

			valueArrayProperty.InsertArrayElementAtIndex(m_conflictIndex);
			var valueProperty = valueArrayProperty.GetArrayElementAtIndex(m_conflictIndex);
			SetPropertyValue(val