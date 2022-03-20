using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Ztail;

namespace ZtailEditor
{
	[InitializeOnLoad]
	public class ViewBindGameObjectInspectorGUI
	{
		private static readonly GUIContent s_BindToggleText = new GUIContent("Binding");

		public static  GameObject lastSelectTarget;

		class TargetInfo
		{
			public readonly GameObject target;
			public readonly ViewBindings bindings;
			public ViewBindings.Binding binding;

			public TargetInfo(GameObject target, ViewBindings bindings, ViewBindings.Binding binding)
			{
				this.target = target;
				this.bindings = bindings;
				this.binding = binding;
			}
		}

		static ViewBindGameObjectInspectorGUI()
		{
			Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
		}

		static void OnPostHeaderGUI(Editor editor)
		{
			if (editor.targets.Length > 0)
			{
				var targetInfos = new List<TargetInfo>();
				var bindingCount = 0;
				foreach (var target in editor.targets)
				{
					if (target is GameObject go)
					{
						if (go.transform.parent)
						{
							var bindings = go.transform.parent.GetComponentInParent<ViewBindings>();
							if (bindings)
							{
								var binding = bindings.GetBindingFromTarget(go);
								targetInfos.Add(new TargetInfo(go, bindings, binding));
								if (binding != null)
								{
									bindingCount++;
								}
							}
						}
					}
				}

				if (targetInfos.Count == 0)
				{
					return;
				}

				if (bindingCount == 0)
				{
					if (GUILayout.Toggle(false, s_BindToggleText, GUILayout.ExpandWidth(false)))
					{
						SetBinding(targetInfos, editor, true);
					}
				}
				else if (bindingCount == editor.targets.Length)
				{
					GUILayout.BeginHorizontal();
					if (!GUILayout.Toggle(true, s_BindToggleText, GUILayout.ExpandWidth(false)))
					{
						SetBinding(targetInfos, editor, false);
						GUIUtility.ExitGUI();
						return;
					}

					if (editor.targets.Length == 1)
					{
						var targetInfo = targetInfos[0];
						var newID = EditorGUILayout.DelayedTextField(targetInfo.binding.id, GUILayout.ExpandWidth(true));
						if (!string.Equals(targetInfo.binding.id, newID, StringComparison.Ordinal))
						{
							targetInfo.binding.id = newID;
							EditorUtility.SetDirty(targetInfo.target);
						}

						if (GUILayout.Button("Select"))
						{
							lastSelectTarget = targetInfo.target;

							EditorGUIUtility.PingObject(targetInfo.bindings.gameObject);
							Selection.activeObject = targetInfo.bindings.gameObject;
						}
					}

					GUILayout.EndHorizontal();
				}
				else
				{
					if (GUILayout.Toggle(false, s_BindToggleText, "ToggleMixed", GUILayout.ExpandWidth(false)))
					{
						SetBinding(targetInfos, editor, true);
					}
				}
			}
		}

		static void SetBinding(List<TargetInfo> targetInfos, Editor editor, bool bind)
		{
			foreach (var targetInfo in targetInfos)
			{
				var bound = targetInfo.binding != null;
				if (bound != bind)
				{
					if (bind)
					{
						targetInfo.binding = targetInfo.bindings.AddBinding(targetInfo.target);
					}
					else
					{
						targetInfo.bindings.RemoveBinding(targetInfo.target);
						targetInfo.binding = null;
					}

					EditorUtility.SetDirty(targetInfo.target);
				}
			}
		}
	}
}
