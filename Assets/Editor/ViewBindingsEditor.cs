using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Ztail;

namespace ZtailEditor
{
	[Serializable]
	[CustomEditor(typeof(ViewBindings))]
	public class ViewBindingsEditor : Editor
	{
		public static ViewBindings FindViewBindings(GameObject go)
		{
			if (go)
			{
				return FindViewBindings(go.transform);
			}

			return null;
		}

		public static ViewBindings FindViewBindings(Transform transform)
		{
			if (transform && transform.parent)
			{
				return transform.parent.GetComponentInParent<ViewBindings>();
			}

			return null;
		}

		private SerializedProperty m_Bindings;

		private ReorderableList m_List;

		private bool m_ScheduleRemove;

		private void OnEnable()
		{
			m_Bindings = serializedObject.FindProperty("m_Bindings");
			m_List = new ReorderableList(serializedObject, m_Bindings, true, true, false, false)
			{
				drawHeaderCallback = DrawListHeader,
				drawElementCallback = DrawElement,
				drawElementBackgroundCallback = DrawElementBackgroundCallback,
				drawFooterCallback = DrawFooter,
				footerHeight = 20,
			};

			if (ViewBindGameObjectInspectorGUI.lastSelectTarget)
			{
				if (target is ViewBindings viewBindings)
				{
					var index = viewBindings.bindings.FindIndex(x => x.target == ViewBindGameObjectInspectorGUI.lastSelectTarget);

					if (index >= 0)
					{
						m_List.Select(index, true);
						ViewBindGameObjectInspectorGUI.lastSelectTarget = null;
					}
				}
			}
		}

		public override void OnInspectorGUI()
		{
			var current = Event.current;
			if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Delete)
			{
				m_ScheduleRemove = true;
				Event.current.Use();
			}

			serializedObject.Update();

			m_List.DoLayoutList();

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawListHeader(Rect rect)
		{
			rect.width = rect.width / 2 - 20;
			rect.x += 20;
			GUI.Label(rect, "ID");

			rect.x += rect.width;
			GUI.Label(rect, "Target");
		}

		private void DrawElement(Rect rect, int index, bool selected, bool focused)
		{
			var totalWidth = rect.width;
			rect.height -= 2;
			rect.width = rect.width / 2 - 10;

			var binding = m_Bindings.GetArrayElementAtIndex(index);

			var bindId = binding.FindPropertyRelative("id");
			bindId.stringValue = EditorGUI.TextField(rect, bindId.stringValue);

			rect.x += rect.width + 5;
			rect.width = totalWidth - rect.width - 5;

			var previous = GUI.enabled;
			GUI.enabled = false;

			var bindTarget = binding.FindPropertyRelative("target");
			bindTarget.objectReferenceValue = EditorGUI.ObjectField(rect, bindTarget.objectReferenceValue, typeof(GameObject), true);

			GUI.enabled = previous;
		}

		private void DrawElementBackgroundCallback(Rect rect, int index, bool isActive, bool isFocused)
		{
			ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, isActive, isActive, isActive);
		}

		private void DrawFooter(Rect rect)
		{
			var width = Math.Min(rect.width * 0.5f - 1, 80);
			rect.x = rect.x + rect.width - width;
			rect.width = width;

			if (GUI.Button(rect, "Remove") || GUI.enabled && m_ScheduleRemove)
			{

				var selections = m_List.selectedIndices.ToList();
				selections.Sort();
				selections.Reverse();

				m_List.ClearSelection();

				foreach (var idx in selections)
				{
					m_Bindings.DeleteArrayElementAtIndex(idx);
				}
			}

			m_ScheduleRemove = false;

			rect.x -= width + 1;

			if (GUI.Button(rect, "Clean"))
			{
				var viewBindings = target as ViewBindings;
				if (viewBindings)
				{
					for (int i = viewBindings.bindings.Count - 1; i >= 0; i--)
					{
						var bindData = viewBindings.bindings[i];
						if (!bindData.target || FindViewBindings(bindData.target) != viewBindings)
						{
							m_Bindings.DeleteArrayElementAtIndex(i);
						}
					}
				}
			}

			rect.x -= width + 1;
		}

		[MenuItem("CONTEXT/ViewBindings/Get Bindings From Parent")]
		static void GetBindingsFromParent(MenuCommand command)
		{
			var viewBindings = command.context as ViewBindings;
			if (viewBindings)
			{
				var parentViewBindings = FindViewBindings(viewBindings.transform);
				if (parentViewBindings)
				{
					for (int i = parentViewBindings.bindings.Count - 1; i >= 0; i--)
					{
						var bindData = parentViewBindings.bindings[i];
						if (FindViewBindings(bindData.target) == viewBindings)
						{
							parentViewBindings.bindings.RemoveAt(i);
							viewBindings.bindings.Add(bindData);
						}
					}
				}
			}
		}

		[MenuItem("CONTEXT/ViewBindings/Get Bindings From Parent", true)]
		static bool IsValidateGetBindingsFromParent(MenuCommand command)
		{
			var viewBindings = command.context as ViewBindings;
			if (viewBindings)
			{
				return FindViewBindings(viewBindings.transform);
			}

			return false;
		}

		[MenuItem("CONTEXT/ViewBindings/Go Back Bindings To Parent")]
		static void GoBindingsFromParent(MenuCommand command)
		{
			var viewBindings = command.context as ViewBindings;
			if (viewBindings)
			{
				var parentViewBindings = FindViewBindings(viewBindings.transform);
				if (parentViewBindings)
				{
					for (int i = viewBindings.bindings.Count - 1; i >= 0; i--)
					{
						var bindData = viewBindings.bindings[i];
						parentViewBindings.bindings.Add(bindData);
					}
				}

				viewBindings.bindings.Clear();
			}
		}

		[MenuItem("CONTEXT/ViewBindings/Go Back Bindings To Parent", true)]
		static bool IsValidateGoBindingsFromParent(MenuCommand command)
		{
			var viewBindings = command.context as ViewBindings;
			if (viewBindings)
			{
				return FindViewBindings(viewBindings.transform);
			}

			return false;
		}
	}
}
