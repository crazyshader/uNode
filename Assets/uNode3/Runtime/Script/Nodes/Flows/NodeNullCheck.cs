﻿using UnityEngine;

namespace MaxyGames.UNode.Nodes {
	[NodeMenu("Flow", "Null Check", hasFlowInput = true, hasFlowOutput = true, inputs =new[] { typeof(object) })]
	[Description("Check for null value, and execute flow based on the result")]
	public class NodeNullCheck : BaseFlowNode {
		public ValueInput value { get; set; }
		[Tooltip("Flow to execute when the value is not null")]
		public FlowOutput onNotNull { get; set; }
		[Tooltip("Flow to execute when the value is null")]
		public FlowOutput onNull { get; set; }

		protected override void OnRegister() {
			base.OnRegister();
			value = ValueInput(nameof(value), typeof(object));
			onNotNull = FlowOutput(nameof(onNotNull)).SetName("Not Null");
			onNull = FlowOutput(nameof(onNull)).SetName("Null");
		}

		protected override void OnExecuted(Flow flow) {
			if(value.isAssigned) {
				var val = value.GetValue(flow);
				bool isNull;
				if(val == null) {
					isNull = true;
				} else {
					isNull = val.Equals(null);
				}
				if(isNull) {
					flow.state = StateType.Success;
					flow.Next(onNull);
				} else {
					flow.state = StateType.Failure;
					flow.Next(onNotNull);
				}
			}
		}

		protected override string GenerateFlowCode() {
			return CG.If(
				CG.Compare(value.CGValue(), CG.Null, ComparisonType.Equal),
				CG.FlowFinish(enter, true, CG.IsStateFlow(enter), onNull),
				CG.FlowFinish(enter, false, CG.IsStateFlow(enter), onNotNull)
			);
		}

		protected override bool IsCoroutine() {
			return onNotNull.IsCoroutine() || onNull.IsCoroutine();
		}

		public override void CheckError(ErrorAnalyzer analizer) {
			base.CheckError(analizer);
			if(value != null && value.isAssigned) {
				if(value.ValueType.IsValueType) {
					analizer.RegisterError(this, "The value is valid only for reference type and not support ValueType/struct");
				}
			}
		}
	}
}
