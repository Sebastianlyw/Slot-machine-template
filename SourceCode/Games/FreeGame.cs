#region NameSpace
using UnityEngine;
using System.Collections;
using  System.Collections.Generic;
#endregion

/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// 
/// Free Game Manager has two parts:
/// Part1: Checking free game status and update free game member data.
/// Part2: Free game algorithm( win and retrigger) implementation.       
/// </summary>
public class FreeGame : MonoBehaviour {

	#region Variables

	/* Start of  Singleton */
	private static FreeGame instance;
	
	//! Constuct
	private FreeGame() {}
	
	//! Instance
	public static FreeGame Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(FreeGame)) as FreeGame;
			return instance;
		}
	}
	/* End of  Singleton */


	/*Start of Cheke Free Game Vriables */
	public int NUM_OF_FGS ;

	private int m_FreeGameLeft;
	private int m_FreeGameID; 
	public int FG_ID
	{
		get{ return m_FreeGameID;}
	}

//	private int 	m_TotalFreeGames;
	public int FG_LEFT
	{
		get{ return m_FreeGameLeft; }
		set{ m_FreeGameLeft = value; }//(m_TotalFreeGames - m_FreeGameCounter); }
	}

	private bool m_IsToggle;
	public bool IsGMTransition
	{
		get { return m_IsToggle;} 
		set { m_IsToggle = value;}
	}
	/*End of Cheke Free Game Vriables */

	


	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Start () {
		//awake called before const varabile initilization..
		//m_TotalFreeGames = 10;///NUM_OF_FGS;
		NUM_OF_FGS = 10;
		m_FreeGameLeft = NUM_OF_FGS;
		m_FreeGameID = 0;
		//m_FreeGameCounter  		= -1;

		
	}

	#endregion

	#region Normal_FreeGameFunciton
	/// <summary>
	/// Tracking free games. Checking whether free game ends. 
	/// </summary>
	public void CheckFreeGame()
	{
		if(GameVariables.Instance.IS_FREEGAME)
		{
			--m_FreeGameLeft ;
			++m_FreeGameID;
		}
		// trigger free game
		m_IsToggle = false;
		if (WinManager.Instance.IsTriggerFG () && !GameVariables.Instance.IS_FREEGAME) 
		{
			m_IsToggle = true;
			GameVariables.Instance.IS_FREEGAME = true;
			m_FreeGameLeft = NUM_OF_FGS;
		//	AnimManager.Instance.IsEndCounFG_Win  = false;
		}
//		Debug.Log ("FREE GAME LEFT:  " + m_FreeGameLeft);
		if(m_FreeGameLeft <= 0 && GameVariables.Instance.IS_FREEGAME ) //m_FreeGameCounter >= m_TotalFreeGames)
		{
			m_IsToggle = true;
			//m_FreeGameCounter = -1;
			//m_FreeGameLeft = NUM_OF_FGS;
			GameVariables.Instance.IS_FREEGAME = false;
		}
	}

	/// <summary>
	/// Change back graound and play audios when transition occurs.
	/// </summary>
	public void FreeGameEnd()
	{
		GameObject.Find(GameVariables.Instance.BG_NAME).GetComponent<OTSprite>().frameIndex = 0;
		TextAndDigitDisp.Instance.SetMessage1Text(" ");
		AudioManager.Instance.StopBGM();
		AudioManager.Instance.PlaySound("GameTransition");

		AnimManager.Instance.SetStartEndWinAmount ();

		m_FreeGameID = 0;
	}

	public bool IsEntryFreeGame()
	{
		return (m_IsToggle && GameVariables.Instance.IS_FREEGAME);
	}
	
	public bool IsExitFreeGame()
	{
		return (m_IsToggle && !GameVariables.Instance.IS_FREEGAME);
	}


	#endregion




}



