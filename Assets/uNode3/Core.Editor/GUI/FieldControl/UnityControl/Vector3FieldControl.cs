﻿using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MaxyGames.UNode.Editors.Control {
	class Vector3FieldControl : FieldControl<Vector3> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Vector3)value;
			position = EditorGUI.PrefixLabel(position, label);
			var newValue = EditorGUI.Vector3Field(position, GUIContent.none, oldValue);
			if(EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}

		public override void DrawLayouted(object value, GUIContent label, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			DrawDecorators(settings);
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (Vector3)value;
			var newValue = EditorGUILayout.Vector3Field(label, oldValue);
			if(EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}
}