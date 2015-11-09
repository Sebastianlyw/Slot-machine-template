#region NameSpace
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#endregion

/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// 
/// Manage the game animation.   
/// </summary>
public class AnimManager : MonoBehaviour {

	#region Variables
	//! Singleton
	private static AnimManager instance;
	
	//! Constuct
	private AnimManager() {}
	
	//! Instance
	public static AnimManager Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(AnimManager)) as AnimManager;
			
			return instance;
		}
		
	}
	/**********end of Singleton*******************/


	//Animation States
	public enum STATE
	{
		AS_IDLE,
		AS_STAEONE,  		// icons blink
		AS_STATETWO,	// icons animation, lines blink, win amount increasment.
		AS_END				
	
	}
	public STATE CURRENT_STATE 	= STATE.AS_IDLE;
	public STATE NEXT_STATE 	= STATE.AS_IDLE;
	private STATE TEMP_NEXT = STATE.AS_IDLE;

	public enum INNER_STATE
	{
		IS_ENTER,
		IS_UPDATE,
		IS_EXIT
	}
	private INNER_STATE m_InnerSate = INNER_STATE.IS_ENTER;

	public bool IS_SKIP;
	public bool m_IsEnd;
	public bool IsPlayBigWin = false;

	#endregion

	#region Animation State Machine
	/// <summary> 
	/// State machine of game animation. which is a sub state of main State machine.
	/// </summary>
	public void UpdateAnimation()
	{
		if (m_IsEnd)
			return;

	
		//! nomal win case: sound play at stop of spinning.
		if(!GameVariables.Instance.IsThreeScatters() && !GameVariables.Instance.IS_FREEGAME)
			WinAmountAnim.Instance.PlayWinIncSound();

		if (CURRENT_STATE != NEXT_STATE) 
		{
		//	Debug.Log ("Animation :  " + CURRENT_STATE + "->" + NEXT_STATE);
			CURRENT_STATE = NEXT_STATE;
		}
	//	Debug.Log ("Animation Current State :  " + CURRENT_STATE );
		//Animation State Machine
		switch (CURRENT_STATE) 
		{
			//Notice: all delay will cause update block to run many times... ,it bring some unexpected issue.
		case STATE.AS_IDLE:
			switch(m_InnerSate)
			{
			case INNER_STATE.IS_ENTER:   
				if(GameVariables.Instance.IS_FREEGAME || GameVariables.Instance.IsThreeScatters())
					WinManager.Instance.END_WIN = WinAmountAnim.Instance.CURRENT_WIN + WinManager.Instance.TOTALWIN;
			
				m_InnerSate = INNER_STATE.IS_UPDATE;

				break;
			case INNER_STATE.IS_UPDATE:   
				TEMP_NEXT = STATE.AS_STAEONE;
				m_InnerSate = INNER_STATE.IS_EXIT;

				break;
			case INNER_STATE.IS_EXIT:
				NEXT_STATE = TEMP_NEXT;
				m_InnerSate = INNER_STATE.IS_ENTER;

				break;

			}
			break;


		case STATE.AS_STAEONE:
			switch(m_InnerSate)
			{
			case INNER_STATE.IS_ENTER:   
				
				m_InnerSate = INNER_STATE.IS_UPDATE;
				
				break;
			case INNER_STATE.IS_UPDATE:   
				if(IconAnim.Instance.IS_START_LINEANI == true)// && GameVariables.Instance.IS_INCRESED == true)
				{
					m_InnerSate = INNER_STATE.IS_EXIT;
					TEMP_NEXT = STATE.AS_STATETWO;
				}
				else
				{
					IconAnim.Instance.BlinkWinIcons();
					WinAmountAnim.Instance.UpdateWinAmount();
					
					if(WinManager.Instance.Is_BigWin() && !IsPlayBigWin) 
					{
						IsPlayBigWin = true;
						GoldCoinEmitter.Instance.StartGoldCoinAnimation();
					}

				}
				break;
			case INNER_STATE.IS_EXIT:
				NEXT_STATE = TEMP_NEXT;
				m_InnerSate = INNER_STATE.IS_ENTER;
				
				break;
				
			}
			break;

		case STATE.AS_STATETWO:
			switch(m_InnerSate)
			{
			case INNER_STATE.IS_ENTER:   
				//make scatter animation play frist
				IconAnim.Instance.IsFinshScatter = false;  
				m_InnerSate = INNER_STATE.IS_UPDATE;
				
				break;
			case INNER_STATE.IS_UPDATE:   
				AnimManager.Instance.CheckSkipInput();
				//Blinking icon finsh, start to blink line and other animations.

				if (Input.GetKeyDown (KeyCode.E)) 
				{
					GameVariables.Instance.EDITOR_FLAG  = GameVariables.EDIT_STATES.NG_IDLE;
				}



				if(IconAnim.Instance.IS_START_LINEANI)
				{
					LineAnim.Instance.DrawPlayLines();
					IconAnim.Instance.PlayAnimation();
					WinAmountAnim.Instance.UpdateWinAmount();
				}
				//skip input detected(main game/ free game) or finish win amount increament(free game), exit animation state. 
				if( (GameVariables.Instance.IS_INCRESED && GameVariables.Instance.IS_FREEGAME && !GameVariables.Instance.IsThreeScatters())
				        ||  !IconAnim.Instance.IS_START_LINEANI)
				{
					TEMP_NEXT = STATE.AS_END;
					m_InnerSate = INNER_STATE.IS_EXIT;
				}
				break;
			case INNER_STATE.IS_EXIT:

				NEXT_STATE = TEMP_NEXT;

				m_InnerSate = INNER_STATE.IS_ENTER;
				
				break;
				
			}
			break;

		case STATE.AS_END:
			switch(m_InnerSate)
			{
			case INNER_STATE.IS_ENTER:   
				
				m_InnerSate = INNER_STATE.IS_UPDATE;
				
				break;
			case INNER_STATE.IS_UPDATE:   
			
				m_InnerSate = INNER_STATE.IS_EXIT;
				
				break;
			case INNER_STATE.IS_EXIT:

				m_IsEnd = true;
				if(GameVariables.Instance.IsThreeScatters())
				{
					GameObject.Find(GameVariables.Instance.BG_NAME).GetComponent<OTSprite>().frameIndex = 1;
				
					//! ToDo:  Animation for Game Transition (Main-Free)
				
				}
				IconAnim.Instance.IS_START_LINEANI = false; 


				NEXT_STATE = STATE.AS_IDLE;
				m_InnerSate = INNER_STATE.IS_ENTER;
		
				break;
				
			}
			break;

		}// End of Animation State Machine.

	}

	#endregion

	public void ResetAnimationState()
	{
		IconAnim.Instance.IS_START_LINEANI = false;
		CURRENT_STATE = STATE.AS_IDLE;
		NEXT_STATE = STATE.AS_IDLE;
		m_InnerSate = INNER_STATE.IS_ENTER;
		GameVariables.Instance.IS_INCRESED = true;
	}

	/// <summary> 
	/// Check if any skip input during animation.
	/// Animation will keep looping if no skip input detected.
	/// </summary>	
	public void CheckSkipInput()
	{
		IS_SKIP = false;

		//free game no need to check the input validation. since free.
		if ((InputManager.Instance.IsValidInput() && !GameVariables.Instance.IS_FREEGAME)
		    || (Input.GetKeyDown(KeyCode.J) && GameVariables.Instance.IS_FREEGAME) )
		{
			//		GameVariables.Instance.IS_ANIMATED = 1;
			IS_SKIP = true;
			IconAnim.Instance.IS_START_LINEANI = false;
			IconAnim.Instance.IsFinshScatter = true;
			
			IconAnim.Instance.Reset();
			
			OnTakeWinFinsh();
		
			if(GameVariables.Instance.IsThreeScatters() && !GameVariables.Instance.IS_FREEGAME)
			{
				AudioManager.Instance.StopSound("Anticipation");
			
				AudioManager.Instance.PlaySound("GameTransition");
				AudioManager.Instance.StopSound("GameTransition");
				Debug.Log("Go to Free Game, Transition Music!");
			}

		}
		if (Input.GetKeyDown (KeyCode.Alpha7)) 
		{
			OnTakeWinFinsh();
			if(GameVariables.Instance.IsThreeScatters() && !GameVariables.Instance.IS_FREEGAME)
			{	
				AudioManager.Instance.PlaySound("Anticipation", 0.1f);
			}

		}

	}


	/// <summary>
	/// Counts up free game win amount.
	/// </summary>
	private float im = 0;
	public bool IsEndCounFG_Win  = false;
	private int cc = 0;
	private int eA = 0;
	public void SetStartEndWinAmount()
	{
		cc = (int)GameVariables.Instance.GetCurrentCredit ();
		eA = (int)(GameVariables.Instance.GetCurrentCredit () + WinAmountAnim.Instance.CURRENT_WIN);
//		Debug.Log (GameVariables.Instance.GetCurrentCredit ());
	}
	public void DeductBets()
	{
		eA -= (int)GameVariables.Instance.GetTotalBetCredit ();
	}
	public void CountUpFreeGameWin()
	{
		if (GameVariables.Instance.IS_FREEGAME)
			return;
		if (IsEndCounFG_Win)
			return;		

		//AudioManager.Instance.PlaySound ("EndFGTakeWin");
		float speed =  (int)WinAmountAnim.Instance.CURRENT_WIN / 0.01f;
		im += Time.deltaTime * speed;
		if(im <=  (int)WinAmountAnim.Instance.CURRENT_WIN)
		{

			TextAndDigitDisp.Instance.SetCreditAmount((int) (cc + im));
//			Debug.Log (cc);
		}
		else
		{
			//AudioManager.Instance.StopSound ("EndFGTakeWin");
			GameVariables.Instance.SetCurrentCredit (eA);
			TextAndDigitDisp.Instance.SetCreditAmount((int)(eA) );
			im = 0f;
			IsEndCounFG_Win = true;
			InputManager.Instance.IsCountingFGwin = 3;
			WinAmountAnim.Instance.CURRENT_WIN = 0;
		}


	}

	/// <summary> 
	/// Actions once take win button pressed..
	/// </summary>	
	private void OnTakeWinFinsh()
	{

		WinAmountAnim.Instance.TakeWinMeter ();
		WinAmountAnim.Instance.ResetWinVule();
		GameVariables.Instance.IS_INCRESED = true;
		AudioManager.Instance.StopWinSound ();
	}

	/// <summary> 
	/// Use this for initialization
	/// </summary>	
	void Start () {
		m_IsEnd 				= false;
		IS_SKIP 			= false;

	}


}
