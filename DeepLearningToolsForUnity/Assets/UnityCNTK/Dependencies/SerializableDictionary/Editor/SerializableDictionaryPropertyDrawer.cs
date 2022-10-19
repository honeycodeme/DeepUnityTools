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
	static GUIContent m_iconMinus = IconContent ("Toolbar Minus", "Rem