﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MaxyGames.UNode.Editors {

	[NodeCustomEditor(typeof(Nodes.MacroNode))]
	public class MacroNodeView : BaseNodeView {
		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
			evt.menu.AppendAction("Open Macro", (e) => {
				owner.graphEditor.graphData.currentCanvas = targetNode.nodeObject;
				owner.graphEditor.Refresh();
				owner.graphEditor.UpdatePosition();
			}, DropdownMenuAction.AlwaysEnabled);
			base.BuildContextualMenu(evt);
		}

		protected override void OnReloadView() {
			base.OnReloadView();
			titleContainer.RegisterCallback<MouseDownEvent>(e => {
				if(e.button == 0 && e.clickCount == 2) {
					owner.graphEditor.graphData.currentCanvas = targetNode.nodeObject;
					owner.graphEditor.Refresh();
					owner.graphEditor.UpdatePosition();
				}
			});
		}
	}
}