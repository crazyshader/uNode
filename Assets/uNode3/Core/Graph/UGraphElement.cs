﻿using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace MaxyGames.UNode {
	[Serializable]
	public class UGroupElement : UGraphElement, IGroup, IIcon {
		[AllowAssetReference]
		public Texture2D icon;

		public UGroupElement() {
			expanded = true;
		}

		public Type GetIcon() {
			if(icon == null)
				return typeof(TypeIcons.FolderIcon);
			return TypeIcons.FromTexture(icon);
		}
	}

	[Serializable]
	public abstract class UGraphElement : IGraphElement, ISummary {
		#region Fields
		[SerializeField]
		private string _name;
		[SerializeField, HideInInspector]
		private int _id;
		[SerializeField]
		private string _comment;
		[SerializeReference]
		private UGraphElement _parent;
		[SerializeReference]
		protected List<UGraphElement> childs = new List<UGraphElement>();

		[HideInInspector]
		public bool expanded = true;

		/// <summary>
		/// True when the element is destroyed
		/// The ID of the element should be -1 if it is destroyed.
		/// </summary>
		protected bool isDestroyed {
			get {
				return _id == -1;
			}
			set {
				_id = -1;
			}
		}

		[NonSerialized]
		private bool m_isMarkedInvalid;
		/// <summary>
		/// True if the element is not destroyed or destroyed with safe mode.
		/// </summary>
		public bool IsValid => isDestroyed == false && m_isMarkedInvalid == false;
		#endregion

		#region Classes
		private class Enumerator : IEnumerator<UGraphElement> {
			private UGraphElement outer;

			private int currentIndex = -1;

			UGraphElement IEnumerator<UGraphElement>.Current => outer.GetChild(currentIndex);

			public object Current => outer.GetChild(currentIndex);

			internal Enumerator(UGraphElement outer) {
				this.outer = outer;
			}

			public bool MoveNext() {
				int childCount = outer.childCount;
				return ++currentIndex < childCount;
			}

			public void Reset() {
				currentIndex = -1;
			}

			public void Dispose() {
				Reset();
			}
		}
		#endregion

		#region Properties
		[NonSerialized]
		private Graph _graph;
		/// <summary>
		/// The graph that own this element
		/// </summary>
		public Graph graph {
			get {
				if(object.ReferenceEquals(_graph, null)) {
					_graph = root as Graph ?? this as Graph;
				}
				return _graph;
			}
		}

		/// <summary>
		/// The graph container of the element
		/// </summary>
		public IGraph graphContainer {
			get {
				if(!object.ReferenceEquals(graph, null)) {
					return graph.owner;
				}
				return null;
			}
		}

		/// <summary>
		/// The name of the element
		/// </summary>
		public string name {
			get {
				return _name;
			}
			set {
				_name = value;
			}
		}

		/// <summary>
		/// The unique ID of graph element ( obtained from graph every change the parent, return 0 if no parent ) and return -1 if the element is destroyed.
		/// </summary>
		public int id {
			get {
				return _id;
			}
		}

		/// <summary>
		/// The user commentary of the element if any
		/// </summary>
		public string comment {
			get {
				return _comment;
			}
			set {
				_comment = value;
			}
		}

		/// <summary>
		/// The parent of the element
		/// </summary>
		public UGraphElement parent {
			get {
				return _parent;
			}
			set {
				SetParent(value);
			}
		}

		/// <summary>
		/// The root parent of the element
		/// </summary>
		public UGraphElement root {
			get {
				if(!object.ReferenceEquals(parent, null)) {
					var root = parent;
					while(true) {
						if(!object.ReferenceEquals(root.parent, null)) {
							root = root.parent;
						} else {
							return root;
						}
					}
				}
				return null;
			}
		}

		/// <summary>
		/// True if the element is the root element
		/// </summary>
		public bool isRoot => root == null;

		/// <summary>
		/// Get count of the child elements
		/// </summary>
		public int childCount => childs.Count;

		[NonSerialized]
		private RuntimeGraphID m_runtimeID;
		internal RuntimeGraphID runtimeID {
			get {
				if(m_runtimeID == default) {
					m_runtimeID = new RuntimeGraphID(graphContainer.GetHashCode(), id);
				}
				return m_runtimeID;
			}
			set {
				m_runtimeID = value;
			}
		}

		//For debugging purpose, don't remove this
		internal string DebugDisplay => GraphException.GetMessage(this);
		#endregion

		#region Utility
		public UGraphElement GetChild(int index) {
			return childs[index];
		}

		public void ForeachInChildrens(Action<UGraphElement> action, bool recursive = false) {
			if(recursive) {
				foreach(var child in childs) {
					if(object.ReferenceEquals(child, null)) continue;
					action(child);
					child.ForeachInChildrens(action, recursive);
				}
			} else {
				childs.ForEach(action);
			}
		}

		public void ForeachInParents(Action<UGraphElement> action) {
			if(parent != null) {
				action(parent);
				parent.ForeachInParents(action);
			}
		}

		public IEnumerable<UGraphElement> GetObjectsInChildren(bool recursive = false, bool findInsideGroup = false) {
			if(recursive) {
				foreach(var child in childs) {
					if(object.ReferenceEquals(child, null)) continue;
					yield return child;
					foreach(var cc in child.GetObjectsInChildren(recursive)) {
						if(object.ReferenceEquals(cc, null)) continue;
						yield return cc;
					}
				}
			}
			else {
				foreach(var child in childs) {
					if(object.ReferenceEquals(child, null)) continue;
					yield return child;
					if(findInsideGroup && child is UGroupElement) {
						foreach(var cc in child.GetObjectsInChildren(recursive, findInsideGroup)) {
							if(object.ReferenceEquals(cc, null)) continue;
							yield return cc;
						}
					}
				}
			}
		}

		public IEnumerable<T> GetObjectsInChildren<T>(bool recursive = false, bool findInsideGroup = false) {
			if(recursive) {
				foreach(var child in childs) {
					if(object.ReferenceEquals(child, null)) continue;
					if(child is T c) {
						yield return c;
					}
					foreach(var cc in child.GetObjectsInChildren<T>(recursive)) {
						if(object.ReferenceEquals(cc, null)) continue;
						yield return cc;
					}
				}
			} else {
				foreach(var child in childs) {
					if(object.ReferenceEquals(child, null)) continue;
					if(child is T c) {
						yield return c;
					}
					if(findInsideGroup && child is UGroupElement) {
						foreach(var cc in child.GetObjectsInChildren<T>(recursive, findInsideGroup)) {
							if(object.ReferenceEquals(cc, null)) continue;
							yield return cc;
						}
					}
				}
			}
		}

		public IEnumerable<T> GetObjectsInChildren<T>(Predicate<T> predicate, bool recursive = false, bool findInsideGroup = false) {
			if(recursive) {
				foreach(var child in childs) {
					if(object.ReferenceEquals(child, null)) continue;
					if(child is T c && predicate(c)) {
						yield return c;
					}
					foreach(var cc in child.GetObjectsInChildren<T>(predicate, recursive)) {
						if(object.ReferenceEquals(cc, null)) continue;
						yield return cc;
					}
				}
			} else {
				foreach(var child in childs) {
					if(object.ReferenceEquals(child, null)) continue;
					if(child is T c && predicate(c)) {
						yield return c;
					}
					if(findInsideGroup && child is UGroupElement) {
						foreach(var cc in child.GetObjectsInChildren<T>(predicate, recursive, findInsideGroup)) {
							if(object.ReferenceEquals(cc, null)) continue;
							yield return cc;
						}
					}
				}
			}
		}

		public T GetObjectInChildren<T>(bool recursive = false, bool findInsideGroup = false) {
			if(recursive) {
				foreach(var child in childs) {
					if(object.ReferenceEquals(child, null)) continue;
					if(child is T c) {
						return c;
					}
					var cc = child.GetObjectInChildren<T>(recursive);
					if(cc != null) {
						return cc;
					}
				}
			} else {
				foreach(var child in childs) {
					if(object.ReferenceEquals(child, null)) continue;
					if(child is T c) {
						return c;
					}
					if(findInsideGroup && child is UGroupElement) {
						var cc = child.GetObjectInChildren<T>(recursive, findInsideGroup);
						if(cc != null) {
							return cc;
						}
					}
				}
			}
			return default;
		}

		public T GetObjectInChildren<T>(Predicate<T> predicate, bool recursive = false, bool findInsideGroup = false) {
			if(recursive) {
				foreach(var child in childs) {
					if(object.ReferenceEquals(child, null)) continue;
					if(child is T c && predicate(c)) {
						return c;
					}
					var cc = child.GetObjectInChildren<T>(predicate, recursive);
					if(cc != null) {
						return cc;
					}
				}
			} else {
				foreach(var child in childs) {
					if(object.ReferenceEquals(child, null)) continue;
					if(child is T c && predicate(c)) {
						return c;
					}
					if(findInsideGroup && child is UGroupElement) {
						var cc = child.GetObjectInChildren<T>(predicate, recursive, findInsideGroup);
						if(cc != null) {
							return cc;
						}
					}
				}
			}
			return default;
		}

		/// <summary>
		/// Get object of type T from this to parent root.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IEnumerable<T> GetObjectsInParent<T>() {
			var parent = this;
			while(parent != null) {
				if(parent is T p) {
					yield return p;
				}
				parent = parent.parent;
			}
		}

		/// <summary>
		/// Get object of type T from this to parent root.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetObjectInParent<T>() {
			var parent = this;
			while(parent != null) {
				if(parent is T p) {
					return p;
				}
				parent = parent.parent;
			}
			//In case it is linked graph
			if(graph?.linkedOwner != null) {
				return graph.linkedOwner.GetObjectInParent<T>();
			}
			return default;
		}
		#endregion

		#region Functions
		/// <summary>
		/// True if the parent can be changed
		/// </summary>
		/// <returns></returns>
		public virtual bool CanChangeParent() {
			return true;
		}

		/// <summary>
		/// Add child to the element
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="child"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public T AddChild<T>(T child) where T : UGraphElement {
			if(!child.CanChangeParent())
				throw new Exception("Unable to change Add Child because the child is forbidden to Change it's parent");
			child.SetParent(this);
			return child;
		}

		/// <summary>
		/// Insert child to the element
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="index"></param>
		/// <param name="child"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public T InsertChild<T>(int index, T child) where T : UGraphElement {
			if(!child.CanChangeParent())
				throw new Exception("Unable to change Add Child because the child is forbidden to Change it's parent");
			if(index > parent.childs.Count) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			child.SetParent(this);
			child.SetSiblingIndex(index);
			return child;
		}

		/// <summary>
		/// Set the parent of element
		/// </summary>
		/// <param name="parent"></param>
		public void SetParent(UGraphElement parent) {
			if(parent == null)
				throw new ArgumentNullException(nameof(parent));
			if(!CanChangeParent())
				throw new Exception("Unable to change parent because it is forbidden");
			if(isDestroyed)
				throw new Exception("The object was destroyed but you're trying to access it.");
			if(this.parent == parent) {
				//Do nothing if the parent are same for prevent additional calls
				return;
			}
			{
				//For prevent circles parenting and self parenting
				var p = parent;
				while(p != null) {
					if(p == this) {
						//Do nothing if the parent is inside of this childs.
						return;
						//throw new Exception("Unable to change parent to it's children");
					}
					p = p.parent;
				}
			}
			if(parent != null) {
				if(graphContainer != parent.graphContainer) {
					var graph = parent.graph;
					if(graph != null) {
						//Set the new ID when the graph is changed
						_id = graph.GetNewUniqueID();
						foreach(var child in GetObjectsInChildren(true, true)) {
							child._id = graph.GetNewUniqueID();
							child._graph = null;
						}
					}
				}
				if(!object.ReferenceEquals(this.parent, null)) {
					this.parent.RemoveChild(this);
				}
				parent.AddChild(this);
				_graph = null;
				parent.OnChildrenChanged();
			} else {
				if(!object.ReferenceEquals(this.parent, null)) {
					this.parent.RemoveChild(this);
				}
				//Set the id to zero if there's no parent.
				_id = 0;
				foreach(var child in GetObjectsInChildren(true, true)) {
					child._id = 0;
				}
			}
		}

		/// <summary>
		/// Destroy the element and it's childrens
		/// </summary>
		public void Destroy() {
			if(!isDestroyed) {
				OnDestroy();
				if(parent != null) {
					parent.RemoveChild(this);
				}
				isDestroyed = true;
			} else if(parent != null) {
				throw new Exception("The object was destoyed but it look like still alive.");
			}
		}

		/// <summary>
		/// Mark element to invalid.
		/// </summary>
		internal void MarkInvalid() {
			if(!m_isMarkedInvalid) {
				MarkInvalidChilds();
				m_isMarkedInvalid = true;
			}
		}

		private void MarkInvalidChilds() {
			for(int i = 0; i < childs.Count; i++) {
				childs[i]?.MarkInvalid();
			}
		}

		/// <summary>
		/// Callback when the object is being destroyed.
		/// </summary>
		protected virtual void OnDestroy() {
			while(childs.Count > 0) {
				if(object.ReferenceEquals(childs[0], null)) {
					childs.RemoveAt(0);
					continue;
				}
				childs[0].Destroy();
			}
		}

		/// <summary>
		/// Get the element slibing index
		/// </summary>
		/// <returns></returns>
		public int GetSiblingIndex() {
			if(parent != null) {
				return GetSiblingIndex(this);
			}
			return -1;
		}

		/// <summary>
		/// Set the element slibing index
		/// </summary>
		/// <param name="index"></param>
		public void SetSiblingIndex(int index) {
			if(parent != null) {
				if(index > parent.childs.Count) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				if(isDestroyed)
					throw new Exception("The object was destroyed but you're trying to access it.");
				if(index == parent.childs.Count) {
					var slibing = parent.childs[index - 1];
					if(slibing != this) {
						PlaceInFront(slibing);
					}
				}
				else {
					var slibing = parent.childs[index];
					if(slibing != this) {
						if(slibing.GetSiblingIndex() > GetSiblingIndex()) {
							PlaceInFront(slibing);
						}
						else {
							parent.childs.Remove(this);
							parent.childs.Insert(slibing.GetSiblingIndex(), this);
							parent.OnChildrenChanged();
						}
					}
				}
			} else {
				throw new System.NullReferenceException("The parent object is null");
			}
		}

		/// <summary>
		/// Place this element in behind ( before ) the target `element`
		/// </summary>
		/// <param name="element"></param>
		public void PlaceBehind(UGraphElement element) {
			if(parent != null) {
				if(!parent.childs.Contains(element)) {
					throw new ArgumentException("The value parent must same with this parent.", nameof(element));
				}
				if(isDestroyed)
					throw new Exception("The object was destroyed but you're trying to access it.");
				var slibing = element;
				if(slibing != this) {
					parent.childs.Remove(this);
					parent.childs.Insert(slibing.GetSiblingIndex(), this);
					parent.OnChildrenChanged();
				}
			} else {
				throw new System.NullReferenceException("The parent object is null");
			}
		}

		/// <summary>
		/// Place this element in front ( after ) the target `element`
		/// </summary>
		/// <param name="element"></param>
		public void PlaceInFront(UGraphElement element) {
			if(parent != null) {
				if(!parent.childs.Contains(element)) {
					throw new ArgumentException("The value parent must same with this parent.", nameof(element));
				}
				if(isDestroyed)
					throw new Exception("The object was destroyed but you're trying to access it.");
				var slibing = element;
				if(slibing != this) {
					parent.childs.Remove(this);
					var index = slibing.GetSiblingIndex();
					if(index < parent.childCount) {
						parent.childs.Insert(index + 1, this);
					}  else {
						parent.childs.Add(this);
					}
					parent.OnChildrenChanged();
				}
			} else {
				throw new System.NullReferenceException("The parent object is null");
			}
		}

		/// <summary>
		/// True if the element is child of <paramref name="parent"/>
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public bool IsChildOf(UGraphElement parent) {
			if(parent != null) {
				return parent.childs.Contains(this);
			}
			return false;
		}

		private void AddChild(UGraphElement child) {
			if(!object.ReferenceEquals(child._parent, null)) {
				child._parent.RemoveChild(child);
			}
			child._parent = this;
			childs.Add(child);
			OnChildAdded(child);
		}

		private void RemoveChild(UGraphElement child) {
			child._parent = null;
			childs.Remove(child);
			OnChildRemoved(child);
			OnChildrenChanged();
		}

		protected int GetSiblingIndex(UGraphElement child) {
			for(int i = 0; i < parent.childs.Count; i++) {
				if(parent.childs[i] == child)
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Callback when children is changed ( added, removed, re-ordered )
		/// </summary>
		protected virtual void OnChildrenChanged() {
			//onChildChanged?.Invoke();
			if(parent != null) {
				parent.OnChildrenChanged();
			}
		}

		/// <summary>
		/// Callback whem the child is added
		/// </summary>
		/// <param name="element"></param>
		protected virtual void OnChildAdded(UGraphElement element) {
			if(parent != null) {
				parent.OnChildAdded(element);
			}
		}


		/// <summary>
		/// Callback when the child is removed
		/// </summary>
		/// <param name="element"></param>
		protected virtual void OnChildRemoved(UGraphElement element) {
			if(parent != null) {
				parent.OnChildRemoved(element);
			}
		}

		public IEnumerator<UGraphElement> GetEnumerator() {
			return new Enumerator(this);
		}

		public static implicit operator bool(UGraphElement exists) {
			return !CompareBaseObjects(exists, null);
		}

		public static bool operator ==(UGraphElement x, UGraphElement y) {
			return CompareBaseObjects(x, y);
		}

		public static bool operator !=(UGraphElement x, UGraphElement y) {
			return !CompareBaseObjects(x, y);
		}

		private static bool CompareBaseObjects(UGraphElement lhs, UGraphElement rhs) {
			if(object.ReferenceEquals(lhs, null) || lhs.isDestroyed) {
				return object.ReferenceEquals(rhs, null) || rhs.isDestroyed;
			}
			if(object.ReferenceEquals(rhs, null) || rhs.isDestroyed) {
				return object.ReferenceEquals(lhs, null) || lhs.isDestroyed;
			}
			return object.ReferenceEquals(lhs, rhs);
		}

		public override bool Equals(object obj) {
			return obj is UGraphElement element && element == this;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		/// <summary>
		/// Initialize the element for runtime
		/// </summary>
		/// <param name="instance"></param>
		public virtual void OnRuntimeInitialize(GraphInstance instance) { }

		string ISummary.GetSummary() {
			return comment;
		}
		#endregion
	}
}