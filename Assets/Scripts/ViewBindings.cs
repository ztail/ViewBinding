using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ztail
{
	[ExecuteInEditMode]
	public class ViewBindings : MonoBehaviour
	{
		[Serializable]
		public class Binding
		{
			public string id;
			public GameObject target;

			public Binding(string id, GameObject target)
			{
				this.id = id;
				this.target = target;
			}
		}

		[SerializeField] private List<Binding> m_Bindings = new List<Binding>();

		public List<Binding> bindings => m_Bindings;

		public GameObject Find(string id)
		{
			var binding = FindBinding(id);
			return binding?.target;
		}

		public T Find<T>(string id) where T : Component
		{
			var binding = FindBinding(id);
			return binding?.target.GetComponent<T>();
		}

		public Component Find(string id, Type type)
		{
			var binding = FindBinding(id);
			return binding?.target.GetComponent(type);
		}

		public Component Find(string id, string type)
		{
			var binding = FindBinding(id);
			return binding?.target.GetComponent(type);
		}

		public Binding FindBinding(string id)
		{
			return m_Bindings.Find(x => x.id == id && x.target);
		}

		public Binding GetBindingFromTarget(GameObject target)
		{
			return m_Bindings.Find(x => x.target == target);
		}

		public Binding AddBinding(GameObject target)
		{
			if (target)
			{
				var binding = GetBindingFromTarget(target);
				if (binding == null)
				{
					binding = new Binding(target.name, target);
					m_Bindings.Add(binding);
				}

				return binding;
			}

			return null;
		}


		public void RemoveBinding(GameObject target)
		{
			if (target)
			{
				var binding = GetBindingFromTarget(target);
				if (binding != null)
				{
					m_Bindings.Remove(binding);
				}
			}
		}

		private void Awake()
		{
#if UNITY_EDITOR
			if (transform.parent)
			{
				var parentViewBindings = transform.parent.GetComponentInParent<ViewBindings>();
				if (parentViewBindings)
				{
					for (int i = parentViewBindings.bindings.Count - 1; i >= 0; i--)
					{
						var binding = parentViewBindings.bindings[i];
						var viewBindings = GetTargetViewBindings(binding.target);
						if (viewBindings == this)
						{
							parentViewBindings.bindings.RemoveAt(i);
							bindings.Add(binding);
						}

					}
				}
			}
#endif
		}


		public static ViewBindings GetTargetViewBindings(GameObject go)
		{
			if (go && go.transform.parent)
			{
				return go.transform.parent.GetComponentInParent<ViewBindings>();
			}

			return null;
		}
	}
}
