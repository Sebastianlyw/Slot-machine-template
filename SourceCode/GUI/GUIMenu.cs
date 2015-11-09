using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GUIMenu : MonoBehaviour {

	public int Width = 135;
	public int Height = 175;
	public int TableWidth = 600;
	public int TableHeight = 600;
	//	public string stringToEdit = "Hello World";
	private bool m_IsMenuOn;
	
	private int m_Selection;
	private string[] m_SelectionString = {"Odds Table","Reel Strip", "Variables" };
	
	private int[] 		m_IconsY;
	private int[] 		m_IconsX;
	private string[,] m_Odds;
	private string 	m_Ngf;
	private string[] 	m_StackP;
	private int 		m_NgfOri;
	private int[] 		m_StackPOri;
	private bool 		m_iSAlwaysWinFG;
	private bool 		m_iSAlwayRtrigger; 
	
	public GUIStyle customBox;
	public GUIStyle customBox2;
	public GUIStyle customButton;
	public GUIStyle customLabel;
	public GUISkin mySkin;
	
	public GameObject FadeInOutObj;
	public Color FadeTo = new Color(1f,1f,1f,0.69f);
	
	
	// Use this for initialization
	void Start () {
		
		m_Selection 	= -1;
		m_IsMenuOn 	= false;
		m_IconsY 		= new int[20];
		m_IconsX 		= new int[10];
		m_Odds 			= new string[20, 10];
	
		m_iSAlwaysWinFG = false;
		m_iSAlwayRtrigger = false;
		
		m_ReelStripsMg = new List<List<string>> ();
		m_ReelStripsMg_Ori = new List<List<int>>();
		m_ReelStripsMg_Ori.Capacity = 5;
		//	m_ReelStripsMg_Ori = new int[5,100];
		m_MenuPosX = -135;
		m_ReelsPosY = 45;
		
		IsShiftLeft = true;
		IsShiftRight = false;
		IsShiftUp = true;
		IsShiftDown = false;
		
		FadeTo 			= new Color(1f,1f,1f,0.69f);
		FadeInOutObj.GetComponent<SpriteRenderer> ().color = new Color(1f,1f,1f,0f);;
		
		InitReelstrip ();
		LoadOddsTable ();
		LoadReelStrips ();
		LoadVariables ();
		
	}
	
	
	private int m_MenuPosX = 0;
	public Vector4 MP = new Vector4(23,40, 90, 25);
	//! main gui function. undapte per frame.
	void OnGUI() {
		
		if(Input.GetKeyDown(KeyCode.Y))
		{
			Debug.Log("Up:  " + IsShiftUp);
			Debug.Log("Down:  " + IsShiftDown);
			Debug.Log("Left:  " + IsShiftLeft);
			Debug.Log("Right:  " + IsShiftRight);
			
		}
	
		if (!m_IsMenuOn)
			return;
		customButton = "button";
		customBox2 = "box";
		
		//1st - level menu. 
		GUI.BeginGroup (new Rect(m_MenuPosX, 65, Width, Height));
		GUI.Box(new Rect(0,0,Width, Height), "  ",customBox2);
		
		if(GUI.Button (new Rect (MP.x, MP.y, MP.z, MP.w), "Odds Table", customButton))
		{
			m_Selection =  0;
			TweenOpen();
		}
		if(GUI.Button (new Rect (MP.x, MP.y + 40, MP.z, MP.w), "Reel Strips", customButton))
		{
			m_Selection =  1;
			TweenOpen();
		}
		if(GUI.Button (new Rect (MP.x, MP.y + 80, MP.z, MP.w), "Variables", customButton))
		{
			m_Selection =  2;
			TweenOpen();
		}
		
		
		GUI.EndGroup ();
		
		switch(m_Selection)
		{
		case 0: 
			EditOddsTable();
			break;
		case 1: 
			EditReelStrip();
			break;
		case 2:
			EditVariables();
			break;
		}
		
	}
	
	
	
	
	#region ODDS TABLE
	private int m_ReelsPosY;
	private void EditOddsTable()
	{
		GUI.Box( new Rect(Screen.width / 2 - TableWidth / 2, m_ReelsPosY, TableWidth, TableHeight), "Odds Table",customBox);
		GUI.BeginGroup (new Rect (Screen.width / 2 - TableWidth / 2, m_ReelsPosY, TableWidth, TableHeight)); 
		
		int yOffset = 25;
		int xOffset = 50;
		for(int c = 0; c < 10; ++c)
		{
			m_IconsX [c]= 95 + c * xOffset;
			GUI.Label(new Rect(m_IconsX [c], 20, 20, 20), (c+1).ToString(), customLabel);
		}
		
		for(int i = 0 ; i < 20; ++i)
		{
			string name = "Icon" + (i+1);
			
			int h  = 20;
			m_IconsY[i] = 50 + i * yOffset;
			//! label of Number of icons "1 to 10"
			GUI.Label(new Rect( 20 , m_IconsY[i], 40, h) , name, customLabel);
			for(int j = 0; j < 10; ++j)
			{
				m_IconsX [j]= 80 + j * xOffset;
				//! Edit the odds table values.
				m_Odds[i,j]= GUI.TextField(new Rect(m_IconsX[j], m_IconsY[i], 40, h), m_Odds[i,j], 40);
			}
		}
		
		GUI.BeginGroup (new Rect (0,0, TableWidth, TableHeight));
		if(GUI.Button (new Rect (m_IconsX [9] - 25, m_IconsY [19] + 40, 65, 25), "Update", customButton)) 
		{
			UpdateOddsTable();
		}
		if(GUI.Button (new Rect (m_IconsX [9] - 115, m_IconsY [19] + 40, 65, 25), "Reload", customButton)) 
		{
			ResetOddsTable();
		}
		
		if(GUI.Button (new Rect (m_IconsX [9] - 230, m_IconsY [19] + 40, 65, 25), "Exit", customButton)) 
		{
			TweenExit();
		}
		if(GUI.Button (new Rect (m_IconsX [9] - 345, m_IconsY [19] + 40, 65, 25), "Save", customButton)) 
		{
			SaveOddTableToFile();
		}
		
		GUI.EndGroup();
		
		
		GUI.EndGroup();
		
		
	}


	private void SaveOddTableToFile()
	{
		string fileName ="/Odds_Table_" + DateTime.Now.ToString ("MM-dd-yyyy_hhss")+".txt";
		
		string storeData ="";
		for(int i = 0; i < 20; ++i)
		{
			for(int j = 0; j < 10; ++j)
			{
				storeData += m_Odds[i,j] +" ";
			}
			storeData += "\n";
		}
		FileManager.Instance.SaveFileTo (fileName, storeData);
	}

	
	/// <summary>
	/// Loads current odds table.
	/// </summary>
	private void LoadOddsTable()
	{
		for(int i = 0; i < 20; ++i)
			for(int j = 0; j < 10; ++j)
				m_Odds[i,j] = GameVariables.Instance.ODDSTABLE_NG[i,j].ToString();
	}
	
	private void UpdateOddsTable()
	{
		for(int i = 0; i < 20; ++i)
			for(int j = 0; j < 10; ++j)
				GameVariables.Instance.ODDSTABLE_NG[i,j] = int.Parse(m_Odds[i,j]);
	}
	
	private void ResetOddsTable()
	{
		FileManager.Instance.LoadOddsTable ();
		LoadOddsTable ();
	}
	
	#endregion
	
	#region Reel Strip
	private Vector2 scrollViewVector = Vector2.zero;
	
	private List<List<string>> 	m_ReelStripsMg ;//= new string[5,];
	private List<List<int>>	 m_ReelStripsMg_Ori ;//= new List<List<int>>();
	private const int MaxSize = 100;
	
	
	private  int NumsPerLine = 17;
	public int H = 500;
	private bool isTR = false;
	private int m_RS_selection;
	private string[] m_Rs_String = {"Reel 1" , " Reel 2" , "Reel 3", "Reel 4", "Reel 5"};
	private void EditReelStrip ()
	{
		GUI.Box( new Rect(Screen.width / 2 - TableWidth / 2, m_ReelsPosY, TableWidth, TableHeight * 0.8f), "Reel Strip" );  //
		GUI.BeginGroup (new Rect (Screen.width / 2 - TableWidth / 2, m_ReelsPosY, TableWidth, TableHeight * 0.8f)); 
		
		//GUI.Label(new Rect(10, 50, labelWidth, 20), "Reel 1:");
		//isTR = GUI.Toggle (new Rect (10, 50, 100, 20), isTR, "Reel 1");
		GUI.BeginGroup(new Rect(50,90,TableWidth,TableHeight * 0.8f) );
		m_RS_selection = GUILayout.SelectionGrid ( m_RS_selection, m_Rs_String, 5, GUILayout.Width(500));
		GUI.EndGroup();
		//! vertical bar shown-> 40.
		switch (m_RS_selection) 
		{
		case 0:		DrawSingleReelStrip("Reel 1", 0); break;
		case 1:			DrawSingleReelStrip("Reel 2", 1); break;
		case 2:		DrawSingleReelStrip("Reel 3", 2); break;
		case 3:		DrawSingleReelStrip("Reel 4", 3); break;
		case 4:		DrawSingleReelStrip("Reel 5", 4); break;
		}
		
		GUI.EndGroup ();
	}
	
	private void DrawSingleReelStrip(string _n, int _j)
	{
		GUI.BeginGroup(new Rect(15,50, 570,400));
		GUI.Box(new Rect(0,90, 570, 260), _n);
		for(int i = 0; i < MaxSize; ++i)
		{
			m_ReelStripsMg[_j][ i] = GUI.TextField(new Rect(5 + (i % NumsPerLine)* 33 , 120 + (i / NumsPerLine) * 30 , 28, 20), m_ReelStripsMg[_j][ i],  40);
		}
		
		//! Update and Rest button
		Vector2 UpdatePos = new Vector2 (480, 315);
		if(GUI.Button (new Rect (UpdatePos.x, UpdatePos.y,  65, 25), "Update", customButton)) 
		{
			UpdateReelStrip(_j);
		}
		if(GUI.Button (new Rect (UpdatePos.x - 115, UpdatePos.y , 65, 25), "Reload", customButton)) 
		{
			ResetReelStrip(_j);
		}
		
		if(GUI.Button (new Rect (UpdatePos.x - 230, UpdatePos.y , 65, 25), "Exit", customButton)) 
		{
			TweenExit();
		}

		if(GUI.Button (new Rect (UpdatePos.x - 345, UpdatePos.y , 65, 25), "Save", customButton)) 
		{
			SaveReelStripsToFile();
		}
		



		GUI.EndGroup ();
	}


	private void SaveReelStripsToFile()
	{
		string fileName ="/ReelStrips" + DateTime.Now.ToString ("MM-dd-yyyy_hhss")+".txt";
		
		string storeData ="";
		for(int i = 0; i < 5; ++i)
		{
			for(int j = 0; j < m_ReelStripsMg[i].Count; ++j)
			{
				storeData += m_ReelStripsMg[i][j] +" ";
			}
			storeData += "\n";
		}
		FileManager.Instance.SaveFileTo (fileName, storeData);
	}
	
	
	//! olnly used when initialization.
	private void LoadReelStrips()
	{
		for (int i = 0; i < 5; ++i)
		{
			StoroeOrigReelStrip(i);
			LoadSingleReelStrip (i);
		}
	}

	private void InitReelstrip()
	{
		for(int _i = 0; _i < 5;++_i)
		{
			List<string> temp = new List<string>();
			for(int j = 0; j < MaxSize; ++j)
			{
				temp.Add("null");
			}
			m_ReelStripsMg.Add(temp);
		}
		
	}

	
	private void LoadSingleReelStrip(int _i)
	{
		for(int j = 0; j < MaxSize; ++j)
		{
			if(m_ReelStripsMg_Ori [_i][ j] != NullINdex)
				m_ReelStripsMg[_i][j] = m_ReelStripsMg_Ori [_i][ j].ToString();
			else
				m_ReelStripsMg[_i][j] = "null";
		}
	}
	
	private const int NullINdex = 999;
	private void StoroeOrigReelStrip (int _i)
	{	
		//	int len = GameVariables.Instance.REEL_STRIPS_NG [_i].Length;
		List<int> temp = new List<int> ();
		for (int j = 0; j < MaxSize; ++j)
		{
			if(j < GameVariables.Instance.REEL_STRIPS_NG [_i].Length)
				temp.Add(GameVariables.Instance.REEL_STRIPS_NG [_i][ j]);
			else
				temp.Add(NullINdex);
		}
		m_ReelStripsMg_Ori.Add (temp);
	}
	
	private void UpdateReelStrip(int _i)
	{
		int len = GetNewSize (_i);
		for(int j = 0; j < len; ++j)
		{
			GameVariables.Instance.REEL_STRIPS_NG[_i] = new int[len];
			GameVariables.Instance.REEL_STRIPS_NG[_i][ j] = int.Parse(m_ReelStripsMg[_i][ j]);
		}
	}
	
	private int GetNewSize(int _i)
	{
		for (int j = 0; j < MaxSize; ++j)
		{
			if( m_ReelStripsMg [_i][ j] == "null")
				return (j);
		}
		return 100;
	}
	
	private void ResetReelStrip(int _i)
	{
		LoadSingleReelStrip(_i);
		UpdateReelStrip(_i);
	}
	
	#endregion



	#region Variables Table
	public int labelWidth = 135;
	private void EditVariables()
	{
		int w = (int) (TableWidth * 1f);
		int h = (int) (TableHeight * 0.5f);
		
		
		GUI.Box( new Rect(Screen.width / 2 - TableWidth / 2, m_ReelsPosY, w, h), "Variables");
		GUI.BeginGroup (new Rect (Screen.width / 2 - TableWidth / 2, m_ReelsPosY, w, h)); 
		
		//! change number of free game
		GUI.Label(new Rect(10, 40, labelWidth, 20), "Numer of Free Games:");
		m_Ngf = GUI.TextField(new Rect(20 + labelWidth, 40, 40, 20), m_Ngf,  40);

		
		//! change number of free game
		m_iSAlwaysWinFG = GUI.Toggle(new Rect(10, 110, labelWidth, 20), m_iSAlwaysWinFG, "Always Win Free Game");
		InputManager.Instance.IsAlwaysWin = m_iSAlwaysWinFG;
		
		m_iSAlwayRtrigger = GUI.Toggle(new Rect(10, 150, labelWidth * 2, 20), m_iSAlwayRtrigger, "Always Retrigger Free Game");
		InputManager.Instance.IsAlwaysRetr = m_iSAlwayRtrigger;
		
		
		//! Update and Rest button
		Vector2 UpdatePos = new Vector2 (500, 250);
		if(GUI.Button (new Rect (UpdatePos.x, UpdatePos.y,  65, 25), "Update", customButton)) 
		{
			UpdateVariables();
		}
		if(GUI.Button (new Rect (UpdatePos.x - 115, UpdatePos.y , 65, 25), "Reset", customButton)) 
		{
			ResetVariables();
		}
		if(GUI.Button (new Rect (UpdatePos.x - 230, UpdatePos.y , 65, 25), "Exit", customButton)) 
		{
			TweenExit();
		}
		
		GUI.EndGroup();
	}



	private void LoadVariables()
	{
		
		m_NgfOri = FreeGame.Instance.NUM_OF_FGS;
		m_Ngf = m_NgfOri.ToString();
	
	}
	
	private void UpdateVariables()
	{
		FreeGame.Instance.NUM_OF_FGS = int.Parse(m_Ngf);

	}
	private void ResetVariables()
	{
		FreeGame.Instance.NUM_OF_FGS = m_NgfOri;

		
		LoadVariables ();
	}

	#endregion


	
	#region TweeningMove
	//!ToDo: fuck it. the gui can not move out of view pot. ... 
	private bool IsShiftLeft = false;
	private bool IsShiftRight = true;
	private void TweeningLeftRight(float _from, float _to, float _time, ref bool _isShifted)
	{
		if(!_isShifted)
		{
			_isShifted = true;
			
			LeanTween.value (gameObject, SetPos, _from, _to, _time).setOnComplete(ShiftFinishLR);
		}
	}
	private void SetPos(float _pos)
	{
		m_MenuPosX = (int)_pos;
	}
	
	private bool IsShiftUp = true;
	private bool IsShiftDown = false;
	private void TweeningUpDwon(float _from, float _to, float _time, ref bool _isShifted)
	{
		if(!_isShifted)
		{
			_isShifted = true;
			
			LeanTween.value (gameObject, SetPosTwo, _from, _to, _time);
		}
	}
	
	private void SetPosTwo(float _pos)
	{
		m_ReelsPosY = (int)_pos;
	}
	
	
	private void ShiftFinishLR()
	{
		
		
	}
	
	private void TweenOpen()
	{
		// Move meu to outside of viewport
		TweeningLeftRight(85f, -135f, 0.25f, ref IsShiftLeft);
		// Move tables down
		TweeningUpDwon(-600f, 45f, 0.25f, ref IsShiftDown);
		IsShiftRight = false;
		IsShiftUp = false;
	}
	
	private void TweenExit()
	{
		TweeningLeftRight(-135f, 85f, 0.25f, ref IsShiftRight);
		TweeningUpDwon(45f, -600f, 0.25f, ref IsShiftUp);
		IsShiftLeft = false;
		IsShiftDown = false;
	}
	
	private void FadeOutSceen()
	{
		FadeInOutObj.GetComponent<FadeInOut> ().FadeOut (0.5f,0f);
	}
	private void FadeInSceen()
	{
		FadeInOutObj.GetComponent<FadeInOut> ().FadeIn (0.69f, 0.5f, 0f);
	}
	
	private void ResetTweenState()
	{
		IsShiftLeft = true;
		IsShiftRight = false;
		IsShiftUp = true;
		IsShiftDown = false;
		m_MenuPosX = -135;
		//	TweenOpen ();
	}
	
	#endregion
	
	// Update is called once per frame
	void Update () {
		
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			ResetTweenState();
			m_IsMenuOn = !m_IsMenuOn;
			if(m_IsMenuOn)
			{
				TweeningLeftRight(-135f, 85f, 0.25f, ref IsShiftRight);
				IsShiftLeft = false;
				FadeInSceen();
				m_Selection = -1; // reset to the state that all windows hide.
				WUXIA_SM.Instance.isPasue = true;
				
				
			}
			else
			{
				WUXIA_SM.Instance.isPasue = false;
				FadeOutSceen();
				
			}
		}
		
	}
}
