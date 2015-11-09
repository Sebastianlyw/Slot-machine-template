using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
	
	public Vector3 m_v3ShakeOffset = new Vector3(0f, 0f, 0f);
	public float m_fShakeDuration = 0f;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void ShakeCamera () {
		
		iTween.ShakePosition(GameObject.Find("Main Camera"), m_v3ShakeOffset, m_fShakeDuration);
	}
}
