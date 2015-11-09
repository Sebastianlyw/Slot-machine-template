#region NameSpace
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#endregion

/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// 
/// Testing win/non-win for main/free game.
/// Managing  win related data.  
/// </summary>
public class WinManager : MonoBehaviour {

	#region Variables

	//! Singleton
	private static WinManager instance;
	
	//! Constuct
	private WinManager() {}
	
	//! Instance
	public static WinManager Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(WinManager)) as WinManager;
		
			return instance;
		}
		
	}
	/**********end of Singleton*******************/



	public List< int[] > WINICONINDICIES
	{
		get
		{
			return m_winIconIndices;
		}
	}

	private short 			m_NumOfScatters;
	private int 	 			m_winCost;
	private int 				m_winCounter;
	private long 			m_TotalWin; // totoal win amount of current turn.
	public long TOTALWIN
	{
		get	{	return m_TotalWin;		}
		set{	m_TotalWin = value;	}
	}

	public long END_WIN; // track ending win of each turn in free game.
	// store actual win icon indcies for win conditon. it's a sub set of m_winLinesToDraw container.
	private List< int[] > m_winIconIndices;
	//store win incon frame index for each win line.
	private List<Pair<int, int>	> m_winIconFrame ;
	private List<int> winIcons;
	private short m_ScatterNum;
	private const short ANIM_LOOP_TIMES = 2;
	private const int BIG_WIN_COIN_OFFSET = 400;
	private const int BIG_WIN_COIN_MULT = 32;

	#endregion

	#region Common Check Win
	/// <summary>
	///	testing if trigger free game in main game or editor
	/// </summary>
	public bool IsTriggerFG()
	{
		// count the amount of scatters 
		m_ScatterNum = 0;
		GameVariables.Instance.SCATTERINDICES_WIN.Clear ();
		//! For HOUST game, scatter only appear in reel 1 and 5.
		for (int j = 0; j < GameVariables.Instance.NUM_OF_COLS; ++j) 
		{
			//index start from 1, skip the first row, since this row is invisible.
			for (int k = 1; k < GameVariables.Instance.NUM_OF_ROWS; ++k) 
			{
				int tInedx =  j * GameVariables.Instance.NUM_OF_ROWS  + k;
				if(GameVariables.Instance.SCATTER_INDEX == Icons.Instance.m_Icons[tInedx].frameIndex)
				{
					++m_ScatterNum;
					GameVariables.Instance.SCATTERINDICES_WIN.Add(tInedx);
					break;
				}
			}
		}
		if (m_ScatterNum >= GameVariables.Instance.FG_SCATTERS && TextAndDigitDisp.Instance.IS_COUNTEND ) 
		{
			TextAndDigitDisp.Instance.ResetCountUp();
		}
		return (m_ScatterNum >= GameVariables.Instance.FG_SCATTERS);
	}

	/// <summary>
	///	testing if Game win or not. Store win data for animation if win occurs.
	/// </summary>
	public bool IsGameWin()
	{
		GameVariables.Instance.IS_WIN = false;		
		m_winIconFrame.Clear ();
	
		// check win and store whole win symbol indices, win line data.
		CheckWin ();
		
		if (m_winIconIndices.Count != 0) 
		{
			GameVariables.Instance.IS_WIN = true;		
		}

		RecordIconFrame ();

		LineAnim.Instance.SetWinLineData ();

		return GameVariables.Instance.IS_WIN;
	}


	/// <summary>
	///	check win and store whole win symbol indices .
	/// </summary>
	public void CheckWin()
	{
		LineAnim.Instance.WINLINES_TODRAW.Clear ();
		m_winIconIndices.Clear ();
		int winFrame = -1; //store frame index of winning sprite.
		winIcons.Clear();
		//m_winLinesToDraw.Clear ();
		for (int i = 0; i < GameVariables.Instance.GetPlayLine(); ++i) 
		{
			int curCol = -1; // since 'j' will increse at the end of for loop. its update will behinde curCol.
			for(int j = 0; j < GameVariables.Instance.NUM_OF_COLS; ++j) 
			{
				
				//index start from 1, skip the first row, since this row is invisible.
				for(int k = 1; k < GameVariables.Instance.NUM_OF_ROWS ; ++k)
				{
					int tInedx = j * GameVariables.Instance.NUM_OF_ROWS  + k;
					
					if((GameVariables.Instance.winLineData[i][j] == tInedx) && 
					   (Icons.Instance.m_Icons[tInedx].frameIndex ==  GameVariables.Instance.SCATTER_INDEX) )
					{
						break;
					}
					if( (GameVariables.Instance.winLineData[i][j] == tInedx)
					   && ( (j == 0) || (winFrame == -1) ) )
					{
						if(GameVariables.Instance.SUBSTITUE_INDEX != Icons.Instance.m_Icons[tInedx].frameIndex)
						{
							winFrame =  Icons.Instance.m_Icons[tInedx].frameIndex;
						}
						++curCol;
						winIcons.Add(tInedx);
						++m_winCounter;
						break;
					}
					
					if( (GameVariables.Instance.winLineData[i][j] == tInedx)
					   &&  ( (Icons.Instance.m_Icons[tInedx].frameIndex == winFrame) ||
					     (Icons.Instance.m_Icons[tInedx].frameIndex == GameVariables.Instance.SUBSTITUE_INDEX ) )
					   && (j > 0) )
					{
						++curCol;
						//win icon must be continuous
						if(j != curCol)
							break;
						
						//first time encounter non-susbtittue icon, set it to winning icon
						if( (Icons.Instance.m_Icons[tInedx].frameIndex != winFrame) &&
						   (Icons.Instance.m_Icons[tInedx].frameIndex != GameVariables.Instance.SUBSTITUE_INDEX ) )
						{
							winFrame = Icons.Instance.m_Icons[tInedx].frameIndex;
						} 
						
						winIcons.Add(tInedx);
						++m_winCounter;
						break;  // once get sub icon or win icon, skip the rest  checking
					}
					
				} // end of 5 rows for one column
				
				
				if(j > 0 && m_winCounter == 1)
					break;
				
			}
			int minWinNum = 6;
			if(winFrame != -1)
				minWinNum  = GetMinWinIconNum(winFrame);

			if(m_winCounter >= minWinNum )
			{
				int[] tA = winIcons.ToArray();
				m_winIconIndices.Add(tA);
				LineAnim.Instance.WINLINES_TODRAW.Add(new Pair<int[], int>(GameVariables.Instance.winLineData[i], i) );
				//	foreach(int tt in GameVariables.Instance.winLineData[i])
				//				Debug.Log("Win LIne Number: " + i);
			}
			m_winCounter = 0;
			winFrame = -1;
			winIcons.Clear();
		}
	}

	private int GetMinWinIconNum(int _f)
	{
		for(int i = 0; i < GameVariables.Instance.NUM_OF_COLS; ++i)		
		{
			if(!GameVariables.Instance.IS_FREEGAME && GameVariables.Instance.ODDSTABLE_NG[_f,i] >0)
				return i+1 ;
			else if(GameVariables.Instance.IS_FREEGAME && GameVariables.Instance.ODDSTABLE_FG[_f,i] >0)
				return i+1;
		}
		return 0;
	}


	/// <summary>
	///	m_winIconFrame container store the win icon frame index.
	/// will used in future win amout computation.
	/// </summary>
	private void RecordIconFrame ()
	{
		for (int i = 0; i < m_winIconIndices.Count; ++i) 
		{
			int t = m_winIconIndices[i].Length;
			int q = 1;  // store the number of substitutes in win line. 
			int j = -1; // store the win icon frame index.
			for(int k = 0; k < t; ++k)
			{
				if( k+1 < t  && q == k+1 && 
				   (Icons.Instance.m_Icons[m_winIconIndices[i][k]].frameIndex
				  	 == GameVariables.Instance.SUBSTITUE_INDEX ) &&
				   (Icons.Instance.m_Icons[m_winIconIndices[i][k+1]].frameIndex
					 == GameVariables.Instance.SUBSTITUE_INDEX ) )
				{
					++q;
				}
				else
				{
					if(Icons.Instance.m_Icons[m_winIconIndices[i][k]].frameIndex
					   != GameVariables.Instance.SUBSTITUE_INDEX )
					j = Icons.Instance.m_Icons[m_winIconIndices[i][k]].frameIndex;
				}
			}   
			if(j == -1)
				j = GameVariables.Instance.SUBSTITUE_INDEX ;
			
			if( (q > 1) && (j != GameVariables.Instance.SUBSTITUE_INDEX) && 
			  (GameVariables.Instance.ODDSTABLE_NG[ j, t-1] 
			   < GameVariables.Instance.ODDSTABLE_NG[GameVariables.Instance.SUBSTITUE_INDEX, q-1]) )
			{
				// the case substitue win is higher than normal. need to romve normal icons from win line container.
				int[] temp = (int[])m_winIconIndices[i].Clone();
				m_winIconIndices[i] = new int[q];
				for(int k = 0; k < q; ++k)
				{
					m_winIconIndices[i][k] = temp[k];
				}

				j = GameVariables.Instance.SUBSTITUE_INDEX;
			}
			else
			{
				q = t;
			}
			m_winIconFrame.Add(new Pair<int,int>(j,q-1) );

		}
	}

	/// <summary>
	///	Compute total win for current game. 
	/// </summary>
	/// <param name="_betPerLine"> how many bets put per line </param>
	public void ComputeTotalWin(int _betPerLine)
	{
		m_TotalWin 	    = 0;
		for (int i = 0; i < m_winIconIndices.Count; ++i) 
		{
			m_winCost = GameVariables.Instance.ODDSTABLE_NG[m_winIconFrame[i].First, m_winIconFrame[i].Second];

			m_TotalWin +=  (long)m_winCost ;
		}
		m_TotalWin *= _betPerLine;
	
		if (GameVariables.Instance.IsThreeScatters())
			m_TotalWin += GameVariables.Instance.GetPlayLine() * GameVariables.Instance.ODDSTABLE_NG[GameVariables.Instance.SCATTER_INDEX,
			                                                                						GameVariables.Instance.SCATTERINDICES_WIN.Count -1];

	}

	public bool Is_BigWin()
	{
		int lines = GameVariables.Instance.PLAYLINE_OPTIONS [4];
		return  ((m_TotalWin >= 20 * lines + BIG_WIN_COIN_OFFSET) &&
				  (m_TotalWin >= GameVariables.Instance.GetCreditPerLine () * lines * BIG_WIN_COIN_MULT)
			     );
	}
	#endregion

	#region WX Check Win
	/// <summary>
	///	Checking if any retrigger in WX free game.
	/// </summary>
	public bool IsRetriggerWX()
	{
		//prbability to retrigger free games : 0.008308118
	//	Debug.Log ("FREE GAME LEFT : " + (FreeGame.Instance.FG_LEFT));
//		if (FreeGame.Instance.FG_LEFT == 8)
//						return true;
		if (!InputManager.Instance.IsAlwaysRetr)
			return (Random.Range (0, 1000000000) < 8308118);
		else
			return true;
	}


	/// <summary>
	///	Checking if a win in WX free game.
	/// </summary>
	public bool IsFreeGameWinWX()
	{
		GameVariables.Instance.IS_FG_WIN_WX = false;
		int _pL = GameVariables.Instance.GetPlayLine ();
		int _wP = 0;
		switch (_pL)
		{
			case 1  : _wP = 0; 	break;
			case 10:  _wP = 1;	break; 
			case 20: _wP = 2;   break;
			case 35: _wP = 3;	break;
			case 50: _wP = 4; 	break;
		}
		// read probability from xml , convert to integer. 
		_wP = GameVariables.Instance.PROB_WIN_FG [_wP];
		int r = Random.Range (0, 10000);
//		Debug.Log ("Random Generator :  " + r);
		if(InputManager.Instance.IsAlwaysWin)
			GameVariables.Instance.IS_FG_WIN_WX = true;//(r < _wP)? true : false;
		else
			GameVariables.Instance.IS_FG_WIN_WX = (r < _wP)? true : false;
		return  GameVariables.Instance.IS_FG_WIN_WX ;
	}

	#endregion

	/// <summary>
	///	Use this for initialization
	/// </summary>
	void Awake()
	{
		m_winCounter = 0;
		m_TotalWin  = 0;
		m_NumOfScatters = 0;
		m_winCost  = 2;
		m_ScatterNum = 0;
		END_WIN = 0;
		
		
		m_winIconIndices = new List< int[]> ();
		m_winIconFrame = new List<Pair<int, int>> ();
		winIcons = new List<int> ();
	}



}
