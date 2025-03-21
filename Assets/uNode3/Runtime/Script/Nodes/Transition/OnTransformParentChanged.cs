﻿namespace MaxyGames.UNode.Transition {
	[TransitionMenu("OnTransformParentChanged", "OnTransformParentChanged")]
	public class OnTransformParentChanged : TransitionEvent {

		public override void OnEnter(Flow flow) {
			UEvent.Register(UEventID.OnTransformParentChanged, flow.target as UnityEngine.Component, () => Execute(flow));
		}

		public override void OnExit(Flow flow) {
			UEvent.Unregister(UEventID.OnTransformParentChanged, flow.target as UnityEngine.Component, () => Execute(flow));
		}

		void Execute(Flow flow) {
			Finish(flow);
		}

		public override string GenerateOnEnterCode() {
			if(!CG.HasInitialized(this)) {
				CG.SetInitialized(this);
				CG.InsertCodeToFunction(
					"OnTransformParentChanged",
					typeof(void),
					CG.Condition("if", CG.CompareNodeState(node.enter, null), CG.FlowTransitionFinish(this)));
			}
			return null;
		}
	}
}
