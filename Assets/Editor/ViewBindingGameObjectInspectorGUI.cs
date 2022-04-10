using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Ztail;

namespace ZtailEditor
{
	[InitializeOnLoad]
	public class ViewBindingGameObjectInspectorGUI
	{
		private static readonly GUIContent s_BindToggleText = new("Binding");

		public static GameObject lastSelectTarget;

		class TargetInfo
		{
			public readonly GameObject target;
			public readonly ViewBindings bindings;
			public ViewBindings.BindData bindData;

			public TargetInfo(GameObject target, ViewBindings bindings, ViewBindings.BindData bindData)
			{
				this.target = target;
				this.bindings = bindings;
				this.bindData = bindData;
			}
		}

		static ViewBindingGameObjectInspectorGUI()
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
							var bindings = ViewBindingsEditor.FindViewBindings(go);
							if (bindings)
							{
								var bindData = bindings.GetBindingFromTarget(go);
								targetInfos.Add(new TargetInfo(go, bindings, bindData));
								if (bindData != null)
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
						SetBinding(targetInfos, true);
					}
				}
				else if (bindingCount == editor.targets.Length)
				{
					GUILayout.BeginHorizontal();
					if (!GUILayout.Toggle(true, s_BindToggleText, GUILayout.ExpandWidth(false)))
					{
						SetBinding(targetInfos, false);
						GUIUtility.ExitGUI();
						return;
					}

					if (editor.targets.Length == 1)
					{
						var targetInfo = targetInfos[0];
						var newID = EditorGUILayout.DelayedTextField(targetInfo.bindData.id, GUILayout.ExpandWidth(true));
						if (!string.Equals(targetInfo.bindData.id, newID, StringComparison.Ordinal))
						{
							targetInfo.bindData.id = newID;
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
						SetBinding(targetInfos, true);
					}
				}
			}
		}

		static void SetBinding(List<TargetInfo> targetInfos, bool bind)
		{
			foreach (var targetInfo in targetInfos)
			{
				var bound = targetInfo.bindData != null;
				if (bound != bind)
				{
					if (bind)
					{
						targetInfo.bindData = targetInfo.bindings.AddBinding(targetInfo.target);
					}
					else
					{
						targetInfo.bindings.RemoveBinding(targetInfo.target);
						targetInfo.bindData = null;
					}

					EditorUtility.SetDirty(targetInfo.target);
				}
			}
		}
	}
}
