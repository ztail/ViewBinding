using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ztail
{
	[ExecuteInEditMode]
	public class ViewBindings : MonoBehaviour
	{
		[Serializable]
		public class BindData
		{
			public string id;
			public GameObject target;

			public BindData(string id, GameObject target)
			{
				this.id = id;
				this.target = target;
			}
		}

		[SerializeField] private List<BindData> m_Bindings = new();

		internal List<BindData> bindings => m_Bindings;

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

		public BindData FindBinding(string id)
		{
			return m_Bindings.Find(x => x.id == id && x.target);
		}

		public BindData GetBindingFromTarget(GameObject target)
		{
			return m_Bindings.Find(x => x.target == target);
		}

		public BindData AddBinding(GameObject target)
		{
			if (target)
			{
				var bindData = GetBindingFromTarget(target);
				if (bindData == null)
				{
					bindData = new BindData(target.name, target);
					m_Bindings.Add(bindData);
				}

				return bindData;
			}

			return null;
		}

		public void RemoveBinding(GameObject target)
		{
			if (target)
			{
				var bindData = GetBindingFromTarget(target);
				if (bindData != null)
				{
					m_Bindings.Remove(bindData);
				}
			}
		}
	}
}
