﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MaxyGames.UNode {
	class FakeParameter : RuntimeParameter<ParameterInfo>, IFakeMember {
		private readonly Type parameterType;

		public FakeParameter(ParameterInfo target, Type parameterType) : base(target) {
			this.parameterType = parameterType;
		}

		public override string Name => target.Name;

		public override Type ParameterType => parameterType ?? target.ParameterType;

		public override ParameterAttributes Attributes => target.Attributes;

		public object Original => target;
		public bool IsNativeMember => false;

		public override string ToString() {
			return ParameterType.ToString() + " " + Name;
		}
	}

}