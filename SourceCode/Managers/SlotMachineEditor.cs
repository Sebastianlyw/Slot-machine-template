using UnityEngine;
using System.Collections;


/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// Base class of editors.
/// 
/// </summary>
public class SlotMachineEditor : MonoBehaviour {


	public const float LEFT_EDGE = 181; //180- x coordinate of marker for 1st col.
	public const float RIGHT_EDGE = 843; //844- x coordinate of marker for 5th col.
	public const float m_Xoffset = 166f;
	
	public GameObject m_Prefabs;

	//! instance for children class.
	public GameObject m_ReelMaker;
	public short m_CurrentColumn;  // 0 - 4
	

	/// <summary>
	/// Layout Initialization.
	/// </summary>
	public void InitEditor()
	{
		m_CurrentColumn = 0;
		
		m_ReelMaker = Instantiate(m_Prefabs, new Vector3(180,-420, 0), Quaternion.identity) as GameObject;
		
		if (m_ReelMaker)
			m_ReelMaker.GetComponent<SpriteRenderer> ().enabled = true;
		else
			Debug.Log ("The Reel Marker is not attached!");

		GameObject.Find (GameVariables.Instance.BG_NAME).GetComponent<OTSprite> ().tintColor = new Color (0, 0, 0, 1f);
		
	}

	/// <summary>
	/// Destoy marker when quiting editor.
	/// </summary>
	public void  DestoryMaker() 
	{
		Destroy(m_ReelMaker);
	}
	
	/// <summary>
	/// Udpate Layout of screen when user editng.
	/// </summary>
	public void EditorUpdate()
	{
		if (Input.GetKeyDown (KeyCode.LeftArrow) && (m_ReelMaker.transform.position.x > LEFT_EDGE)) 
		{
			--m_CurrentColumn;
			m_ReelMaker.transform.position -= new Vector3(m_Xoffset , 0, 0);
		}
		else if (Input.GetKeyDown (KeyCode.RightArrow) && (m_ReelMaker.transform.position.x < RIGHT_EDGE )) 
		{
			++m_CurrentColumn;	
			m_ReelMaker.transform.position += new Vector3(m_Xoffset , 0, 0);
		}
		
		if (Input.GetKeyDown (KeyCode.UpArrow)) 
		{
			Icons.Instance.SpinSingleReel (true, m_CurrentColumn );
			//	++EDIT_COUNTER[m_CurrentColumn];
		}
		else if (Input.GetKeyDown (KeyCode.DownArrow)) 
		{
			Icons.Instance.SpinSingleReel (false, m_CurrentColumn );
			//	--EDIT_COUNTER[m_CurrentColumn];
		}
	}

	/// <summary>
	/// Reset the  backgroud color.
	/// </summary>
	public void ResetBG()
	{
			GameObject.Find (GameVariables.Instance.BG_NAME).GetComponent<OTSprite> ().tintColor = new Color (1f, 1f, 1f, 1f);
	}


}
