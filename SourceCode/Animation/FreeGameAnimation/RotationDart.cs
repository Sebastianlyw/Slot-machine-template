using UnityEngine;
using System.Collections;

// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// 
/// Implementation of Simple rotation around Z-axis.
/// </summary>
public class RotationDart : MonoBehaviour {
	//! the angle rotate to.
	private float m_zRotation;

	public float RotationSpeed = 5;


	/// <summary> 
	/// Use this for initialization
	/// </summary>
	void Start () {
		m_zRotation = 0;

	}

	/// <summary> 
	/// Roate to new position per frame.
	/// </summary>
	void Update () {
		m_zRotation += RotationSpeed;

	
		transform.eulerAngles = new Vector3(0, m_zRotation, 0);

	}



}
