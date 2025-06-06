﻿using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace MaxyGames.UNode.Editors.Drawer {
	class ShortPropertyDrawer : UPropertyDrawer<short> {
		public override void Draw(Rect position, DrawerOption option) {
			EditorGUI.BeginChangeCheck();
			var fieldValue = GetValue(option.property);
			var att = ReflectionUtils.GetAttribute<RangeAttribute>(option.property.GetCustomAttributes());
			if(att != null) {
				fieldValue = (short)EditorGUI.IntSlider(position, option.label, fieldValue, (int)att.min, (int)att.max);
			} else {
				fieldValue = (short)EditorGUI.DelayedIntField(position, option.label, fieldValue);
			}
			if(EditorGUI.EndChangeCheck()) {
				option.value = fieldValue;
				GUI.changed = true;
			}
		}
	}
}