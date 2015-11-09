using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T: Singleton<T> {



	public static T _mInstance;

	private Singleton(){ } 

	public static Singleton<T> m_Instance
	{
		get
		{
			if(!_mInstance)
			{
				T[] gos = GameObject.FindObjectsOfType(typeof(T)) as T[];
				if(gos.Length == 1)
				{
					_mInstance = gos[0];
					_mInstance.gameObject.name = typeof(T).Name;
				}
				else  // object more than one or no such type obejct.
				{
					Debug.Log ("You have more than one " + typeof(T).Name  +" in current scene.");
					foreach(T go in gos)
					{
						Destroy(go.gameObject);
					}
					GameObject gob = new GameObject(typeof(T).Name, typeof(T));
					_mInstance = gob.GetComponent<T>();
					DontDestroyOnLoad(gob);
				}							
			}
			return _mInstance;
		}
		set{  _mInstance = value as T; }
	}

	public void TEST(){}



}
