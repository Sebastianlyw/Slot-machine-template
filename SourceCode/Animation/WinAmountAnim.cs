#region NameSpace
using UnityEngine;
using System.Collections;
#endregion


/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// 
/// Implementation of win amount increament.
/// </summary>
public class WinAmountAnim : MonoBehaviour {

	#region Variables
	//! Singleton
	private static WinAmountAnim instance;
	
	//! Constuct
	private WinAmountAnim() {}
	
	//! Instance
	public static WinAmountAnim Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(WinAmountAnim)) as WinAmountAnim;
			
			return instance;
		}
		
	}
	/**********end of Singleton*******************/
	
	private long m_CurrentWin;
	public long CURRENT_WIN
	{
		get	{	return m_CurrentWin;	}
		set	{	m_CurrentWin = value;	}
	}

	private float m_MeterSpeed;
	public float METERSPEED
	{
		get { return m_MeterSpeed;}
		set { m_MeterSpeed = value;}
	}

	private float m_WinValue = 0;
	public void ResetWinVule() { m_WinValue = 0; }


	#endregion

	#region Increase WinAmount

	/// <summary>
	/// Wrapping play win souind function. 
	/// </summary>
	public void UpdateWinAmount()
	{
		//!Sound Related stuff.
		if (GameVariables.Instance.IS_INCRESED == true) 
		{	
		//	AudioManager.Instance.StopWinSound();
		
		//	if(!GameVariables.Instance.IS_FREEGAME)
		//	TextAndDigitDisp.Instance.SetWinAmount((int)WinManager.Instance.TOTALWIN);
			//TakeWinMeter();
		//	if(GameVariables.Instance.IsThreeScatters() && !GameVariables.Instance.IS_FREEGAME)
		//	{	
		//		AudioManager.Instance.PlaySound("Anticipation", 0.1f);
		//	}
			return;
		}

		m_IsDoneIncAction = false;
		if(GameVariables.Instance.IsThreeScatters())
		{

			if(!AudioManager.Instance.IsPlaying("FGHit"))
				WinAmountAnim.Instance.PlayWinIncSound();
			else
				return;

		}
		//!End of Sound stuff.


		//!Couting up the win amount value.
		if(WinManager.Instance.TOTALWIN > 0 && m_WinValue < WinManager.Instance.TOTALWIN)
			IncreAmountTo (WinManager.Instance.TOTALWIN);

		if(m_WinValue > WinManager.Instance.TOTALWIN)
		{
			if(GameVariables.Instance.IS_FREEGAME )
				OnFreeGameFinish();
			else
				OnMainGameFinish();
		}
	}

	private int m_cc = 0;
	private void TakeWinAmount()
	{
		TextAndDigitDisp.Instance.SetCreditAmount(m_cc);

	}
	/// <summary>
	/// Reset local data when increment done.
	/// </summary>
	private void OnMainGameFinish()
	{
		m_WinValue = 0;
		GameVariables.Instance.IS_INCRESED = true;
		if(GameVariables.Instance.IsThreeScatters())
		{	
			AudioManager.Instance.PlaySound("Anticipation", 0.1f);
		}
	}

	/// <summary>
	/// Reset local data when increament done.
	/// </summary>
	private void OnFreeGameFinish()
	{
		for (int i = 0; i < GameVariables.Instance.NUM_OF_COLS - 1; ++i)
			LineAnim.Instance.SPR_WINLINES  [i].alpha = 0f;
		
		m_CurrentWin += WinManager.Instance.TOTALWIN;
		TextAndDigitDisp.Instance.SetWinAmount((int)m_CurrentWin);
		
		m_WinValue = 0;
		GameVariables.Instance.IS_INCRESED = true;
		AudioManager.Instance.StopWinSound();
	}

	/// <summary>
	/// Update meters' data. 
	/// </summary>
	public void TakeWinMeter()
	{
		if(!GameVariables.Instance.IS_FREEGAME  && !GameVariables.Instance.IsThreeScatters())
		{
			TextAndDigitDisp.Instance.SetWinAmount((int)WinManager.Instance.TOTALWIN);
			m_cc = (int)GameVariables.Instance.GetCurrentCredit();
			Invoke("TakeWinAmount", 0.5f);
			//		TextAndDigitDisp.Instance.SetCreditAmount(GameVariables.Instance.GetCurrentCredit());
		}
		else
		{	
			WinAmountAnim.Instance.CURRENT_WIN = WinManager.Instance.END_WIN;
			
			TextAndDigitDisp.Instance.SetWinAmount((int) WinManager.Instance.END_WIN);// WinAmountAnim.Instance.CURRENT_WIN );
		}
	}



	/// <summary>
	/// Increase win amount value to end vlaue.
	/// </summary>
	/// <param name="_am"> Ending value of win amount. </param>
	private void IncreAmountTo( long _am)
	{
		if(_am-- > -1)
		{
			if(!GameVariables.Instance.IS_FREEGAME)
			{
				TextAndDigitDisp.Instance.SetWinAmount ( (int) (m_WinValue+= Time.deltaTime * m_MeterSpeed)  ); 
			}
			else
			{
				m_WinValue+= Time.deltaTime * m_MeterSpeed;
				//Notice: m_currentWin value has not been updated here.
				TextAndDigitDisp.Instance.SetWinAmount ( (int) (m_CurrentWin + m_WinValue));
			}
		}
	}
	#endregion

	/// <summary>
	/// Wrapping play win souind function. 
	/// </summary>
	public void PlayWinIncSound()
	{
		AudioManager.Instance.PlayWinSound (ref m_MeterSpeed);
	}

	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Awake()
	{
		m_CurrentWin = 0;
		
	}

	private bool m_IsDoneIncAction = false;
	void Update()
	{
		if (GameVariables.Instance.IS_INCRESED == true && !m_IsDoneIncAction)   
		{	
			m_IsDoneIncAction = true;
//			Debug.Log("DFJAS???");
			//!Auto Take win here 
			
			
			//Invoke("TakeWinAmount",0.3f);
			WinAmountAnim.Instance.TakeWinMeter ();
			WinAmountAnim.Instance.ResetWinVule();
			AudioManager.Instance.StopWinSound();
			
			if(!GameVariables.Instance.IS_FREEGAME)
				TextAndDigitDisp.Instance.SetWinAmount((int)WinManager.Instance.TOTALWIN);
		
			return;
		}

	}
	
}
