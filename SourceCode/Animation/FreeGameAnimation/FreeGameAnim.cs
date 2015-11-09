#region NameSpace
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#endregion

/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// 
/// Implementation of dart and other non-icon animation in free game.
/// </summary>
public class FreeGameAnim : MonoBehaviour {
	#region Variables

	//! Singleton
	private static FreeGameAnim instance;
	
	//! Constuct
	private FreeGameAnim() {}
	
	//! Instance
	public static FreeGameAnim Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(FreeGameAnim)) as FreeGameAnim;
			
			return instance;
		}
		
	}
	/**********end of Singleton*******************/

	public GameObject m_PrefebDart;
	private GameObject[] m_Darts;
	private Vector3[] m_TargetLoc;


	private float m_zRotation;
	public float RotationSpeed = 20;
	private bool m_IsPauseRotation; 

	public float JUMPY = 0.1f;
	#endregion

	/// <summary>
	/// Use this for initialization 
	/// </summary>
	void Start () {

		m_zRotation = 0;
	

		m_TargetLoc = new Vector3[3];
		m_TargetLoc [0] = new Vector3 (340, -155, -11);
		m_TargetLoc [1] = new Vector3 (510, -155, -11);
		m_TargetLoc [2] = new Vector3 (675, -155, -11);
		m_IsPauseRotation = true;
	
	

	}



	/// <summary>
	/// Gets the sign
	/// </summary>
	/// <returns>The sign.</returns>
	private int GetSign()
	{
		return  (Random.Range (-1, 1) >= 0) ? 1 : -1;
	}
	
	/// <summary>
	/// Plaies the particles. It's an on complete function
	/// </summary>
	/// <param name="hash">game object as an argument taken .</param>
	private void PlayParticles(object hash )
	{
		GameObject go = hash as GameObject;// (GameObject)hash["gameObject"]; 
		go.GetComponent<ParticleSystem> ().Play ();
	//	GetComponent<ParticleSystem> ().Play ();
	}

	/// <summary>
	/// Destories the darts.
	/// </summary>
	public void DestoryDarts()
	{
		GameObject parObj = GameObject.Find ("DartsOnIcon");
		for(int i = 0; i < parObj.transform.childCount; ++i)
			Destroy(parObj.transform.GetChild(i).gameObject);
	}




	/// <summary>
	/// Animation of scatters. shrink & scale up, rotation and dynamic movement
	/// </summary>
	public void RatationWithFire()
	{
		Debug.Log ("Rotate and fly center once");
		int size = GameVariables.Instance.NUM_OF_COLS * GameVariables.Instance.NUM_OF_ROWS;
		List<int> scatterIndex = new List<int> ();
		List<Vector3> scatterLoc = new List<Vector3> ();
		for(int i = 0; i < size ; ++i)
		{
			if(Icons.Instance.m_Icons[i].frameIndex == GameVariables.Instance.SCATTER_INDEX)
			{
				Icons.Instance.m_Icons[i].gameObject.AddComponent("RotationScatter");
				Icons.Instance.m_Icons[i].GetComponent<RotationScatter>().ResetRotation();
				scatterIndex.Add(i);
				scatterLoc.Add(Icons.Instance.m_Icons[i].position);
			}
		}

		LeanTween.move (Icons.Instance.m_Icons [scatterIndex [0]].gameObject, scatterLoc[1], 0.6f);
		LeanTween.move (Icons.Instance.m_Icons [scatterIndex [2]].gameObject, scatterLoc[1], 0.6f);
		LeanTween.move (Icons.Instance.m_Icons [scatterIndex [0]].gameObject, scatterLoc[0], 0.6f).setDelay(0.65f);
		LeanTween.move (Icons.Instance.m_Icons [scatterIndex [2]].gameObject, scatterLoc[2], 0.6f).setDelay(0.65f);

	}

	


	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
//		//!rotation
//		if(!m_IsPauseRotation)
//		{
//			m_zRotation += RotationSpeed;
//		//	foreach (GameObject go in m_Darts)
//			for(int i = 0; i < 3; ++i)
//				if(m_HasRoateIcon[i]  == true)
//				m_Darts[i].transform.eulerAngles = new Vector3(0, 0, m_zRotation);
//
//		}
//
//		if (Input.GetKeyDown (KeyCode.Y))
//						FadeIntDarts ();
	}




	

}
