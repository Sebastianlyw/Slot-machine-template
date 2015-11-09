#region NameSpace
using UnityEngine;
using System.Collections;

#endregion


/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// 
/// Implementation of text window for game. 
/// </summary>
public class TextAndDigitDisp : MonoBehaviour {

	#region Variables
	/* Singleton Declaration*/
	// Singleton
	private static TextAndDigitDisp instance;
	
	// Constuct
	private TextAndDigitDisp() {}
	
	// Instance
	public static TextAndDigitDisp Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(TextAndDigitDisp)) as TextAndDigitDisp;
			
			return instance;
		}
		
	}
	/*End of Singleton Declaration*/

	
	public enum STATE
	{
		IDLE = 0,
		INCREMENT,
		FG_NUM_INCREMENT
	}
	
	OTTextSprite m_txtSpr_Win;
	OTTextSprite m_txtSpr_Bet;
	OTTextSprite m_txtSpr_Credit;
	OTTextSprite m_txtSpr_Win_Cash;
	OTTextSprite m_txtSpr_Bet_Cash;
	OTTextSprite m_txtSpr_Credit_Cash;	
	OTTextSprite m_txtSpr_Message_1;
	OTTextSprite m_txtSpr_Message_2;
	OTTextSprite m_textSpr_Ediotr;


	
	private float[] m_speedCtrl = new float[5] {1.5f, 3.13f, 4.75f, 6.38f, 8.0f};

	private bool m_IsCountEnd;
	public bool IS_COUNTEND
	{
		get{ 	return m_IsCountEnd;		}
		set{	m_IsCountEnd = value;	}
	}

	private float m_Counter;
	//!track current free games left.
	private int games = 0;  

	#endregion
	
	#region Game Play Message
	/// <summary>
	/// Update text window in top middle area per frame
	/// </summary>
	void Update () {

		string msg = " ";
		if(GameVariables.Instance.GetCurrentCredit() == 0)
		{
			msg ="INSERT CREDITS";
		}
		//take win messaage, except three scatters win.
		else if (WUXIA_SM.Instance.CURRENT_STATE == WUXIA_SM.STATE.GS_WIN_ANIMATION)
		{
			if(!GameVariables.Instance.IS_FREEGAME)
			{
				if(!GameVariables.Instance.IS_INCRESED)
					msg =  "TAKE WIN OR PLAY ON ";
				else
					msg = "GAME OVER    PLAY ON ";
			}
			//Over writter is trigger free game.
			 if (GameVariables.Instance.IsThreeScatters())
				msg =  FreeGame.Instance.NUM_OF_FGS.ToString() + " FREE GAMES WON      PRESS START FEATURE ";
		
		}
		else if ( (WUXIA_SM.Instance.CURRENT_STATE == WUXIA_SM.STATE.GS_IDLE || WUXIA_SM.Instance.CURRENT_STATE == WUXIA_SM.STATE.GS_COUNTING_FG)
		         && !GameVariables.Instance.IS_FREEGAME)
		{
			if(WinAmountAnim.Instance.CURRENT_WIN == 0)
				msg = "GAME OVER    PLAY ON ";
			else
				msg =  "TAKE WIN OR PLAY ON ";
		}
		else
		{
			msg = " ";
		}

		m_txtSpr_Message_2.text = msg;

	}

	/// <summary>
	///Counting free game numbers.
	/// </summary>
	public void CountingUpFGS()
	{
		//For Free Game
		if (GameVariables.Instance.IS_FREEGAME)
		{
			//if(WUXIA_SM.Instance.CURRENT_STATE == WUXIA_SM.STATE.GS_REEL_SPIN_FG)
			CountUpFG();
		}
	}

	/// <summary>
	/// show left free game numbers.
	/// </summary>
	public void ShowLeftGames()
	{
		if(m_IsCountEnd)
		{	
			games = FreeGame.Instance.FG_LEFT - 1;
			if (games <= 0)
			{
				m_txtSpr_Message_1.text = " ";
				return;
			}
			string msg = (games > 1)?  " FREE GAMES LEFT" : " FREE GAME LEFT";
			m_txtSpr_Message_1.text =  (games.ToString()) + msg;
		}
	}

	/// <summary>
	/// Counting free game numbers.
	/// </summary>
	private void CountUpFG()
	{
		string msg  = games.ToString() + " FREE GAMES LEFT";
		if(!m_IsCountEnd)
		{
			//AudioManager.Instance.PlaySound("GameTransition");
			AudioManager.Instance.StopBGM ();
			AudioManager.Instance.PlaySound("FGIconCounter", 0.2f);
			AudioManager.Instance.PlaySound("FGIconCounterEnd",1.9f);
			m_txtSpr_Message_1.text = msg;
		}
	
		if ( (int)(m_Counter += Time.deltaTime  * 5.5f) > FreeGame.Instance.FG_LEFT)
		{
			if(m_Counter > FreeGame.Instance.FG_LEFT + 7.5f)// dealy time 1.5 seconds
			{
				m_IsCountEnd = true;
				AudioManager.Instance.StopSound("FGIconCounter");
				AudioManager.Instance.StopSound("FGIconCounterEnd");
				AudioManager.Instance.PlayBGM();
			}
			return;
		}
		else
		{
			games = (int)m_Counter ;
		}
	}

	/// <summary>
	/// Reset Counting Variables.
	/// </summary>
	public	void  ResetCountUp()
	{
		m_IsCountEnd = false;
		m_Counter = games;
	}

	#endregion
	
	#region Meters  Message
	/// <summary>
	/// Check if credited button pressed.
	/// </summary>
	public bool CheckCreditPressed() {
	
		return (OT.Clicked (m_txtSpr_Credit));
	}
	
	/// <summary>
	/// Set editor message window with new text.
	/// </summary>
	/// <param name="msg"> Text to show </param>
	public void SetMessageEditor(string msg)
	{
		m_textSpr_Ediotr.text = msg;
	}

	/// <summary>
	/// Set  message window 1 with new text.
	/// </summary>
	/// <param name="strMessage"> Text to show </param>
	public void SetMessage1Text(string strMessage)
	{
		m_txtSpr_Message_1.text = strMessage;
	}

	/// <summary>
	/// Set  message window 2 with new text.
	/// </summary>
	/// <param name="strMessage"> Text to show </param>
	public void SetMessage2Text(string strMessage)
	{
		m_txtSpr_Message_2.text = strMessage;
	}	

	/// <summary>
	/// Set win amount meter.
	/// </summary>
	/// <param name="iWinAmount"> value to set </param>
	public void SetWinAmount(int iWinAmount)
	{
		m_txtSpr_Win.text = iWinAmount.ToString();
		
		SetWinAmount_Cash((long)iWinAmount);
	}

	/// <summary>
	/// Set bet amount meter.
	/// </summary>
	/// <param name="iBetAmount"> value to set </param>
	public void SetBetAmount(int iBetAmount)
	{
		GameVariables.Instance.SetTotalBetCredit (iBetAmount);
		m_txtSpr_Bet.text = iBetAmount.ToString();
		
		SetBetAmount_Cash(iBetAmount);
	}


	/// <summary>
	/// Set credits amount meter.
	/// </summary>
	/// <param name="lCreditAmount"> value to set </param>
	public void SetCreditAmount(long lCreditAmount)
	{
		m_txtSpr_Credit.text = lCreditAmount.ToString();
		
		SetCreditAmount_Cash(lCreditAmount);
	}

	/// <summary>
	/// Set win amount meter, cash value.
	/// </summary>
	/// <param name="lWinAmount"> value to set </param>
	public void SetWinAmount_Cash(long lWinAmount)
	{
		// Assume 1 credit = 1 cents
		double dAmtInCents = lWinAmount * 1;
		double dAmtInDollar = dAmtInCents / 100;
		
		m_txtSpr_Win_Cash.text = string.Format("{0:C2}", dAmtInDollar);
	}

	/// <summary>
	/// Set bet amount meter, cash value.
	/// </summary>
	/// <param name="iBetAmount"> value to set </param>
	public void SetBetAmount_Cash(int iBetAmount)
	{
		// Assume 1 credit = 1 cents
		double dAmtInCents = iBetAmount * 1;
		double dAmtInDollar = dAmtInCents / 100;

		m_txtSpr_Bet_Cash.text = string.Format("{0:C2}", dAmtInDollar);
	}


	/// <summary>
	/// Set credits amount meter, cash value.
	/// </summary>
	/// <param name="lCreditAmount"> value to set </param>
	public void SetCreditAmount_Cash(long lCreditAmount)
	{
		// Assume 1 credit = 1 cents
		double dAmtInCents = lCreditAmount * 1;
		double dAmtInDollar = dAmtInCents / 100;
		
		m_txtSpr_Credit_Cash.text = string.Format("{0:C2}", dAmtInDollar);
	}	

	#endregion
	

	/// <summary>
	/// Variables initialization.
	/// </summary>
	void Awake() {
		
		m_txtSpr_Win 				 = GameObject.Find ("Win_Digital_Sprite").GetComponent<OTTextSprite>();
		m_txtSpr_Bet 		 		 = GameObject.Find ("Bet_Digital_Sprite").GetComponent<OTTextSprite>();
		m_txtSpr_Credit 	 		 = GameObject.Find ("Credit_Digital_Sprite").GetComponent<OTTextSprite>();
		m_txtSpr_Win_Cash	 = GameObject.Find ("Win_Cash_Sprite").GetComponent<OTTextSprite>();
		m_txtSpr_Bet_Cash 	 = GameObject.Find ("Bet_Cash_Sprite").GetComponent<OTTextSprite>();
		m_txtSpr_Credit_Cash  = GameObject.Find ("Credit_Cash_Sprite").GetComponent<OTTextSprite>();
		m_txtSpr_Message_1	 = GameObject.Find ("Text_Message_Sprite_1").GetComponent<OTTextSprite>();
		m_txtSpr_Message_2 	 = GameObject.Find ("Text_Message_Sprite_2").GetComponent<OTTextSprite>();
		m_textSpr_Ediotr 		 = GameObject.Find ("Text_Message_Sprite_Editor").GetComponent<OTTextSprite> ();
		
		m_IsCountEnd = false;
		m_Counter = 0;
		// Scale down in non-mobile platform
		if (Application.platform != RuntimePlatform.Android)
		{
			m_txtSpr_Message_1.size = new Vector2(0.8f, 0.8f);
			m_txtSpr_Message_2.size = new Vector2(0.8f, 0.8f);
		}	
		
		m_txtSpr_Message_2.text  = "INSERT CREDITS";
		SetWinAmount(0);
		SetBetAmount(0);
		SetCreditAmount(0);	
		
	}


}
