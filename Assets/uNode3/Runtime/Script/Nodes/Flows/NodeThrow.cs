﻿using UnityEngine;

namespace MaxyGames.UNode.Nodes {
	[NodeMenu("Flow", "Throw", hasFlowInput = true, inputs = new[] { typeof(System.Exception) })]
	[Description("Use this node to throw an exception. Use the try node to catch and handle exceptions that might occur during execution of a flow.")]
	public class NodeThrow : BaseFlowNode {
		[System.NonSerialized]
		public ValueInput value;

		protected override void OnRegister() {
			base.OnRegister();
			value = ValueInput(nameof(value), typeof(System.Exception), MemberData.Null);
		}

		protected override void OnExecuted(Flow flow) {
			throw value.GetValue<System.Exception>(flow);
		}

		protected override string GenerateFlowCode() {
			if(!value.isAssigned) throw new System.Exception("Unassigned value");
			return CG.Value(value).AddFirst("throw ").Add(";");
		}

		public override string GetTitle() {
			return "Throw";
		}

		public override string GetRichName() {
			return uNodeUtility.WrapTextWithKeywordColor("throw ") + value.GetRichName();
		}
	}
}