﻿using UnityEngine;
using System.Collections;
using System.Linq;

namespace MaxyGames.UNode.Nodes {
    [NodeMenu("Collections", "Last Item", icon = typeof(IList), inputs = new[] { typeof(IList) })]
	public class LastItem : ValueNode {
		public ValueInput target { get; set; }

		protected override void OnRegister() {
			base.OnRegister();
			target = ValueInput(nameof(target), typeof(IEnumerable));
		}

		protected override System.Type ReturnType() {
			if(target.isAssigned) {
				return target.ValueType.ElementType();
			}
			return typeof(object);
		}

		public override object GetValue(Flow flow) {
			var val = target.GetValue<IEnumerable>(flow);
			if(val is IList list) {
				return list[list.Count -1];
			} else {
				return val.Cast<object>().Last();
			}
		}

		protected override string GenerateValueCode() {
			var type = target.ValueType;
			if(type.IsCastableTo(typeof(IList))) {
				return CG.AccessElement(target, CG.Value(target).CGAccess(nameof(IList.Count)).CGSubtract(CG.Value(1)));
			}
			//Because the function is using Linq we need to make sure that System.Linq namespaces is registered.
			CG.RegisterUsingNamespace("System.Linq");
			return CG.GenericInvoke<object>(target, "Cast").CGInvoke("Last");
		}

		public override string GetTitle() {
			return "Last Item";
		}

		public override string GetRichName() {
			return target.GetRichName().Add(".Last");
		}
	}
}