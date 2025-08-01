using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaxyGames.UNode.Nodes {
	public class StateTransition : Node, ISuperNode, INodeWithEnterExitEvent, INodeWithEventHandler, IStateTransitionNode, INodeWithCustomCanvas {
		public IStateNodeWithTransition StateNode {
			get {
				return nodeObject.GetNodeInParent<IStateNodeWithTransition>();
			}
		}

		private static readonly string[] m_styles = new[] { "state-node", "state-transition" };
		public override string[] Styles => m_styles;

		public IEnumerable<NodeObject> NestedFlowNodes => nodeObject.GetObjectsInChildren<NodeObject>(obj => obj.node is BaseEventNode);

		[Tooltip("If enable, this transition can be triggered from anywhere with Trigger Transition node")]
		public bool IsExpose = false;
		
		[System.NonSerialized]
		public FlowInput enter;
		[System.NonSerialized]
		public FlowOutput exit;

		string ISuperNode.SupportedScope => NodeScope.State + "," + NodeScope.FlowGraph;

		UGraphElement INodeWithCustomCanvas.ParentCanvas {
			get {
				if(StateNode is Node node) {
					return node.nodeObject.parent;
				}
				return nodeObject.parent;
			}
		}

		private event System.Action<Flow> m_onEnter;
		public event System.Action<Flow> OnEnterCallback {
			add {
				m_onEnter -= value;
				m_onEnter += value;
			}
			remove {
				m_onEnter -= value;
			}
		}
		private event System.Action<Flow> m_onExit;
		public event System.Action<Flow> OnExitCallback {
			add {
				m_onExit -= value;
				m_onExit += value;
			}
			remove {
				m_onExit -= value;
			}
		}

		protected override void OnRegister() {
			exit = FlowOutput(nameof(exit)).SetName("");
			enter = FlowInput(nameof(enter), (flow) => throw new System.InvalidOperationException()).SetName("");
		}

		public override string GetTitle() {
			var type = GetType();
			if(!string.IsNullOrEmpty(name)) {
				return name;
			}
			else {
				return type.PrettyName();
			}
		}

		/// <summary>
		/// Called once after state Enter, generally used for Setup.
		/// </summary>
		public virtual void OnEnter(Flow flow) {
			m_onEnter?.Invoke(flow);
		}

		///// <summary>
		///// Called every frame when state is running.
		///// </summary>
		//public virtual void OnUpdate(Flow flow) {
		//	m_onEnter?.Invoke(flow);
		//}

		/// <summary>
		/// Called once after state exit, generally used for reset.
		/// </summary>
		public virtual void OnExit(Flow flow) {
			m_onExit?.Invoke(flow);
			////Stop all running coroutine flows
			//foreach(var element in nodeObject.GetObjectsInChildren(true)) {
			//	if(element is NodeObject node && node.node is not BaseEventNode) {
			//		foreach(var port in node.FlowInputs) {
			//			flow.instance.StopState(port);
			//		}
			//	}
			//}
		}

		/// <summary>
		/// Call to finish the transition.
		/// </summary>
		public void Finish(Flow flow) {
			var state = flow.GetUserData(StateNode as Node) as StateMachines.IState;
			var targetState = flow.GetUserData(exit.GetTargetNode()) as StateMachines.IState;
			if(state.IsActive) {
				state.FSM.ChangeState(targetState);
#if UNITY_EDITOR
				if(GraphDebug.useDebug) {
					GraphDebug.Flow(flow.instance.target, nodeObject.graphContainer.GetGraphID(), id, nameof(exit));
				}
#endif
			}
		}

		public override void OnGeneratorInitialize() {
			base.OnGeneratorInitialize();
			if(exit.GetTargetNode() != null) {
				CG.RegisterNode(exit.GetTargetNode());
				foreach(var node in NestedFlowNodes) {
					CG.RegisterNode(node);
				}
				CG.RegisterNodeSetup(this, () => {
					var state = CG.GetVariableNameByReference(StateNode);
					string onEnter = null;
					string onExit = null;
					foreach(var evt in nodeObject.GetNodesInChildren<BaseGraphEvent>()) {
						if(evt != null) {
							if(evt is StateOnEnterEvent) {
								onEnter += evt.GenerateFlows().AddLineInFirst().Replace("yield ", "");
							}
							else if(evt is StateOnExitEvent) {
								onExit += evt.GenerateFlows().AddLineInFirst();
							}
							else {

							}
						}
					}
					//nodeObject.ForeachInChildrens(element => {
					//	if(element is NodeObject nodeObject) {
					//		foreach(var flow in nodeObject.FlowInputs) {
					//			if(/*flow.IsSelfCoroutine() &&*/ CG.IsStateFlow(flow)) {
					//				onExit += CG.StopEvent(flow).AddLineInFirst();
					//			}
					//		}
					//	}
					//});
					if(string.IsNullOrEmpty(state) == false) {
						if(onEnter != null) {
							onEnter = CG.Set(state.CGAccess(nameof(StateMachines.State.onEnter)), CG.Lambda(onEnter), SetType.Add);
							onEnter = CG.WrapWithInformation(CG.Flow(CG.Comment($"Transition: {GetTitle()}"), onEnter), this);
						}
						if(onExit != null) {
							onExit = CG.Set(state.CGAccess(nameof(StateMachines.State.onExit)), CG.Lambda(onExit), SetType.Add);
							onExit = CG.WrapWithInformation(CG.Flow(CG.Comment($"Transition: {GetTitle()}"), onExit), this);
						}
					}
					if(onEnter != null || onExit != null) {
						CG.InsertCodeToFunction("Awake", CG.Flow(
							onEnter, onExit
						));
					}
				});
			}
		}

		public override Type GetNodeIcon() {
			return null;
		}

		public override void CheckError(ErrorAnalyzer analyzer) {
			if(exit.isConnected == false) {
				analyzer.RegisterError(this, "No target");
			}
		}

		public bool AllowCoroutine() => false;

		public bool CanTrigger(GraphInstance instance) {
			return StateNode.CanTrigger(instance);
		}

		string INodeWithEventHandler.GenerateTriggerCode(string contents) {
			var state = CG.GetVariableNameByReference(StateNode);
			contents = CG.Flow(CG.Comment($"Transition: {GetTitle()}"), contents);
			if(state == null)
				return CG.WrapWithInformation(CG.WrapWithInformation(contents, this), StateNode);
			return CG.WrapWithInformation(CG.WrapWithInformation(CG.If(state.CGAccess(nameof(StateMachines.IState.IsActive)), contents), this), StateNode);
		}
	}
}