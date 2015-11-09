#region NameSpace
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#endregion


/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// 
/// Implementation of win line animation.
/// </summary>
public class LineAnim : MonoBehaviour {

	#region Variables
	//! Singleton
	private static LineAnim instance;
	
	//! Constuct
	private LineAnim() {}
	
	//! Instance
	public static LineAnim Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(LineAnim)) as LineAnim;
			
			return instance;
		}
		
	}
	/**********end of Singleton*******************/


	//winning line type named by gradient
	public enum LINE_TYPE
	{
		LINE_ZERO,
		LINE_POSITIVE_ONE,
		LINE_NEGATIVE_ONE,
		LINE_NEGATIVE_TWO, 
		LINE_POSITIVE_TWO,
		LINE_NEGATIVE_THREE,
		LINE_POSITIVE_THREE
	}
	
	//[System.Serializable]
	//public class LineConnector
	public struct LineConnector
	{
		
		public Vector2 mPos;  // starting position of line.
		public LINE_TYPE mType;
		public LineConnector (Vector2 _p, LINE_TYPE _t){ 
			mPos 	= _p;
			mType  = _t;
		}
	}
	
	private List< LineConnector[]>  m_WinLines ;
	public List< LineConnector[]>  WINLINES
	{
		get	{	return m_WinLines;	}
	}
	
	private OTSprite[] m_SprWinLInes ;
	public OTSprite[] SPR_WINLINES
	{
		get{	return m_SprWinLInes;	}
	}


	private List<Pair<int[], int>> m_winLinesToDraw ;
	public  List<Pair<int[], int>> WINLINES_TODRAW
	{
		get	{	return m_winLinesToDraw;	}
	}

	private float m_winBlinkTimer;
	public float BLINKTIMER
	{
		get	{	return m_winBlinkTimer;	}
		set{	m_winBlinkTimer= value;	}
	}

	/// <summary> 
	/// Use this for local variables initialization.
	/// </summary>
	void Awake()
	{
		m_winBlinkTimer = 0;
		m_WinLines = new List< LineConnector[] >();
		
		m_winLinesToDraw = new List< Pair<int[], int>> ();
	}
	
	/// <summary> 
	/// Use this for line connector generation.
	/// </summary>
	void Start () {
		InitalLines ();
	}

	#endregion

	#region WinLines
	/// <summary> 
	/// Store win line type and location for all wins' lines.
	/// </summary>
	public void SetWinLineData()
	{
		m_WinLines.Clear ();
		int numLines = m_winLinesToDraw.Count;
		for(int i = 0; i < numLines; ++i)
		{
			int numIcons =  m_winLinesToDraw[i].First.Length;
			LineConnector[] lineCns; 
			UpdateEachLine(out lineCns,numIcons, i);
			m_WinLines.Add(lineCns);
		}
		
	}
	
	/// <summary> 
	/// Adjust line connector sprite frame index and location.
	/// </summary>
	private void UpdateEachLine(out LineConnector[] lineCns,  int n, int curline)
	{
		lineCns = new LineConnector[n-1];
		
		float s = GameVariables.ICON_SIZE / 2;
		Vector2 offset_Zero 	 = new Vector2 (s, 0);
		Vector2 offset_One	 = new Vector2 (s, s);
		Vector2 offset_Two	 = new Vector2 (s, -s);
		Vector2 offset_Three	 = new Vector2 (s, 2 * s);
		Vector2 offset_Four	 = new Vector2 (s, -2 * s);
		
		for(int j = 0; j < n - 1 ; ++j)
		{
			int curIndex   = m_winLinesToDraw[curline].First[j] ;
			int nextIndex  = m_winLinesToDraw[curline].First[j+1] ;
			
			// different game type will casue different diff value.
			int diff 		  = ( nextIndex % GameVariables.Instance.NUM_OF_ROWS )  
				-(curIndex % GameVariables.Instance.NUM_OF_ROWS );
			
			
			lineCns[j].mPos = Icons.Instance.m_Icons[curIndex].position;
			switch(diff)
			{
			case 0:
				lineCns[j].mType = LINE_TYPE.LINE_ZERO; 
				lineCns[j].mPos += offset_Zero;
				break;
			case 1:
				lineCns[j].mType = LINE_TYPE.LINE_NEGATIVE_ONE; 
				lineCns[j].mPos += offset_Two;
				break;
			case 2:
				lineCns[j].mType = LINE_TYPE.LINE_NEGATIVE_TWO; 
				lineCns[j].mPos += offset_Four;
				break;
			case 3:
				lineCns[j].mType = LINE_TYPE.LINE_NEGATIVE_THREE; 
				lineCns[j].mPos += offset_Four;
				break;
				
			case -1:
				lineCns[j].mType = LINE_TYPE.LINE_POSITIVE_ONE; 
				lineCns[j].mPos += offset_One;
				break;
				
			case -2:
				lineCns[j].mType = LINE_TYPE.LINE_POSITIVE_TWO; 
				lineCns[j].mPos += offset_Three;
				break;
				
			case -3:
				lineCns[j].mType = LINE_TYPE.LINE_POSITIVE_THREE; 
				lineCns[j].mPos += offset_Three;
				break;
				
			}
		}
	}
	
	/// <summary> 
	/// Generate 5 line animation sprite object at the begining for standing by. (alpha -> 0). 
	/// </summary>
	void InitalLines()
	{
		m_SprWinLInes = new OTSprite[GameVariables.Instance.NUM_OF_COLS - 1];
		for (int i = 0; i < GameVariables.Instance.NUM_OF_COLS - 1; ++i) 
		{
			m_SprWinLInes[i] = GameVariables.Instance.GenarateOTSpriet("WinLines","PlayLineConnector_Atlas","winLine" + i, new Vector2(), 0,0,0,1);
		}
	}

	/// <summary> 
	/// Blnking the win line connectors.
	/// </summary>
	public void DrawPlayLines()
	{
		//it's the case only  have 3 scatters win. no win line to draw.
		if (WinManager.Instance.WINICONINDICIES.Count == 0) 
				return;  
		if (IconAnim.Instance.IsFinshScatter == false && GameVariables.Instance.IsThreeScatters())
				return;

		int k = IconAnim.Instance.CURRENT_WINLINE;
		for (int i = 0; i < m_WinLines[k].Length; ++i) 
		{
			//				Debug.Log("K :   " + k); 
			m_SprWinLInes [i].position = m_WinLines [k] [i].mPos;
			m_SprWinLInes [i].frameIndex = (int)m_WinLines[k] [i].mType;
			// Blink the lines                          // use this formular to make sure each line blink twice.
			
			m_SprWinLInes [i].size = GameObject.Find ("PlayLineConnector_Atlas").GetComponent<OTSpriteAtlasCocos2D> ().
				atlasData [m_SprWinLInes [i].frameIndex].size;
			
			m_SprWinLInes [i].alpha =  ( (int)( (m_winBlinkTimer+= Time.deltaTime) * 0.49f) % 2 == 1)? 1: 0; //( (t) % (LINEANI_SPEED / 2) < (LINEANI_SPEED /4) ) ? 1 : 0;   

			if(m_SprWinLInes[i].alpha == 1)
				LineButtons.Instance.SetLineButtonColorSize(m_winLinesToDraw[k].Second, new Vector2(44f, 25f), false);
			else 
				LineButtons.Instance.SetLineButtonColorSize(m_winLinesToDraw[k].Second, new Vector2(66f, 37.5f), true);

		}	
	}

	/// <summary> 
	/// Hide the win line connectors.
	/// </summary>
	public void HideLines(int k)
	{
		for (int i = 0; i < m_SprWinLInes.Length; ++i) 
			m_SprWinLInes [i].alpha = 0f;
		LineButtons.Instance.StopAnimation ();
	}

	/// <summary> 
	/// Show Static Icons and hide win lines.
	/// </summary>
	public void ResetIconsAlpha()
	{
		for(int i = 0; i < GameVariables.Instance.NUM_OF_COLS  * GameVariables.Instance.NUM_OF_ROWS ; ++i)
			Icons.Instance.m_Icons[i].alpha = 1;
		
		for (int i = 0; i < GameVariables.Instance.NUM_OF_COLS - 1; ++i)
			m_SprWinLInes [i].alpha = 0f;	
		
		return;
	}

	#endregion


}
