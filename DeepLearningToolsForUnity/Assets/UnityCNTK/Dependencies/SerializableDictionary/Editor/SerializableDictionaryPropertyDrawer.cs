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
	