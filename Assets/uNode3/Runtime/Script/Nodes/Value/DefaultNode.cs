﻿using System.Linq;
using UnityEngine;

namespace MaxyGames.UNode.Nodes {
	[NodeMenu("Data", "Default", typeof(object))]
	[Description("Return the default value of a type, a reference type will always null")]
	public class DefaultNode : ValueNode {
		[Filter(OnlyGetType = true)]
		public SerializedType type = SerializedType.None;

		protected override void OnRegister() {
			base.OnRegister();
			output.SetAutoType(true);
		}

		protected override System.Type ReturnType() {
			if(type.isFilled) {
				try {
					System.Type t = type.type;
					if(!object.ReferenceEquals(t, null)) {
						return t;
					}
				}
				catch { }
			}
			return typeof(object);
		}

		public override object GetValue(Flow flow) {
			return Operator.Default(type.type);
		}

		protected override string GenerateValueCode() {
			if(type.isAssigned) {
				return "default(" + CG.Type(type) + ")";
			}
			throw new System.Exception("Type is unassigned.");
		}

		public override string GetTitle() {
			return "Default";
		}

		public override string GetRichName() {
			return $"default({type.GetRichName()})";
		}
	}
}