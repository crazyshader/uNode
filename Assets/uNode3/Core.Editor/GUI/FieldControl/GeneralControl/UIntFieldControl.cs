﻿using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MaxyGames.UNode.Editors.Control {
	class UIntFieldControl : FieldControl<uint> {
		public override void Draw(Rect position, GUIContent label, object value, Type type, Action<object> onChanged, uNodeUtility.EditValueSettings settings) {
			EditorGUI.BeginChangeCheck();
			ValidateValue(ref value);
			var oldValue = (uint)value;
			var newValue = (uint)EditorGUI.DelayedIntField(position, label, (int)oldValue);
			if(EditorGUI.EndChangeCheck()) {
				onChanged(newValue);
			}
		}
	}
}