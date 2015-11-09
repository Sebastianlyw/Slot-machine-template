#region NameSpace
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#endregion


/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// 
/// Loading xml files and write data to contianers.   
/// </summary>
public class FileManager :MonoBehaviour
{
	#region Variables

	//! Singleton
	private static FileManager instance;
		
	//! Constuct
	private FileManager() {}
		
	//! Instance
	public static FileManager Instance
	{
		get
		{
			if (instance == null)
			instance = GameObject.FindObjectOfType(typeof(FileManager)) as FileManager;
			
			return instance;
		}
		
	}
	/************ end of Singleton**************/
	
	/*------------------Local varibales declarations----------------*/
	// XML parser class for reading XML strings
	private XMLParser m_XMLparser = new XMLParser();	

	#endregion


	#region Common File Loading

	/// <summary> 
	/// Load reel strips from xml file.
	/// </summary>
	/// <param name="fileName"> xml file name. </param>
	public void LoadReelStrips(string fileName )
	{
		int level = 1;
	
		string filePath = Application.dataPath +"/StreamingAssets/" + fileName +".xml";
		string textAsset = System.IO.File.ReadAllText (filePath);
		
		//TextAsset textAsset = (TextAsset)Resources.Load(fileName, typeof(TextAsset));

		if (textAsset == null) 
		{
			Debug.Log ("'reel_strips.xml' not found or not readable");
		} 
		else
		{
			XMLNode reelStripsXML = m_XMLparser.Parse(textAsset);

			GameVariables.Instance.SCATTER_INDEX = int.Parse(reelStripsXML.GetValue("ReelStrips>0>ScatterIndex>0>_text"));
			GameVariables.Instance.SUBSTITUE_INDEX = int.Parse(reelStripsXML.GetValue("ReelStrips>0>SubstitueIndex>0>_text"));

			for (int i=0; i< GameVariables.Instance.NUM_OF_COLS; ++i)
			{
				
				string[] strSplitContentMG = reelStripsXML.GetValue("ReelStrips>0>Level" + level + ">0>MainGame>0>Reel" + (i+1) + ">0>Content>0>_text").Split(',');
				
				GameVariables.Instance.REEL_STRIPS_NG[i] = new int[strSplitContentMG.Length];

				for (int j=0; j<strSplitContentMG.Length; j++)
				{
					int t = j;
					if(j > strSplitContentMG.Length - 3)
					{
						t = j- strSplitContentMG.Length;  // -2 + 2 = 0; -1 + 2 =1; which are first two icons , who take last two sprites.
					}
					GameVariables.Instance.REEL_STRIPS_NG[i][t + 2] = int.Parse(strSplitContentMG[j]);
					//[0][0] refer the 1st icon with sencond last sprite.
					
				}
				
				string[] strSplitContentFG = reelStripsXML.GetValue("ReelStrips>0>Level" + level + ">0>FreeGame>0>Reel" + (i+1) + ">0>Content>0>_text").Split(',');
				GameVariables.Instance.REEL_STRIPS_FG[i] = new int[strSplitContentFG.Length];
				for (int j=0; j < strSplitContentFG.Length; j++)
				{
					int t = j;
					if(j > strSplitContentFG.Length - 3)
					{
						t = j- strSplitContentFG.Length;  // -2 + 2 = 0; -1 + 2 =1; which are first two icons , who take last two sprites.
					}
					GameVariables.Instance.REEL_STRIPS_FG[i][t + 2] = int.Parse(strSplitContentFG[j]);
				}			

			}
		}
//						
	}

	/// <summary> 
	/// Load win line definition from xml file.
	/// </summary>
	public void LoadLinesDefinition(GameVariables.GAME_DEFINE _gd)
	{
		GameVariables.Instance.winLineData.Clear ();

		string strXmlContent = "";
		string strLineDefFileName;
		if (_gd == GameVariables.GAME_DEFINE.THREE_X_FIVE)
			strLineDefFileName = "/StreamingAssets/Xml/3x5_lines_definition.xml";//  + (GameVariables.NUM_OF_ROWS -1) + "x" + GameVariables.Instance.NUM_OF_COLS + "_lines_definition";
		else 
			strLineDefFileName = "/StreamingAssets/Xml/4x5_lines_definition.xml";
		
		string filePath = Application.dataPath + strLineDefFileName;
		
		
		string textAsset = System.IO.File.ReadAllText (filePath);
		
		//TextAsset textAsset = (TextAsset)Resources.Load(strLineDefFileName, typeof(TextAsset));

		
		if (textAsset == null)
		{
			Debug.Log("'lines_definition.xml' not found or not readable");
		}
		else
		{
			XMLNode lineDefinitionXML = m_XMLparser.Parse(textAsset);

			int winIndex = 0;

			for (int i=0; i<GameVariables.NUM_OF_LINES; i++)
			{			
				int[] wl = new int[GameVariables.Instance.NUM_OF_COLS]; 
				for (int j=0; j<GameVariables.Instance.NUM_OF_COLS; j++)
				{		
				
					strXmlContent = lineDefinitionXML.GetValue("LinesDefinition>0>Line_"+(i+1)+">0>Reel_"+(j+1)+">0>_text");
					
					string[] strSplitContent = strXmlContent.Split(',');

					for (int k=0; k< GameVariables.Instance.NUM_OF_ROWS -1; k++)
					{
						if (int.Parse(strSplitContent[k]) > 0)
						{					
							winIndex = j * GameVariables.Instance.NUM_OF_ROWS + k + 1;
							wl[j] = winIndex;
							break;
						}

					}

				}

				GameVariables.Instance.winLineData.Add(wl);
		
			}			
		}
	}	


	/// <summary> 
	/// Load odds Table from xml file.
	/// </summary>
	public void LoadOddsTable()
	{
		string strXmlContent = "";
		string filePath = Application.dataPath + "/StreamingAssets/Xml/odds_table.xml";
		
		
		string textAsset = System.IO.File.ReadAllText (filePath);
		//TextAsset textAsset =  (TextAsset)Resources.Load("Xml/odds_table", typeof(TextAsset));  //"Xml/odds_table"

		if (textAsset == null)
		{
			Debug.Log("'odds_table.xml' not found or not readable");
		}
		else
		{
			XMLNode oddsTableXML = m_XMLparser.Parse(textAsset);

			// number of icons 
			int _rows 	= GameVariables.Instance.ODDSTABLE_NG.GetLength(0);
			// number of repeatting time for one win icon.
			int _cols = GameVariables.Instance.ODDSTABLE_NG.GetLength(1);

			for (int i=0; i<_rows; i++)
			{
				// Main Game
				strXmlContent = oddsTableXML.GetValue("OddsTable>0>MainGame>0>Icon_"+(i+1)+">0>_text");
				
				string[] strSplitContentMG = strXmlContent.Split(',');
				
				for (int j=0; j<_cols; j++)
				{
					GameVariables.Instance.ODDSTABLE_NG[i, j] = int.Parse(strSplitContentMG[j]);
				}
				
				// Free Game
				strXmlContent = oddsTableXML.GetValue("OddsTable>0>FreeGame>0>Icon_"+(i+1)+">0>_text");
				
				string[] strSplitContentFG = strXmlContent.Split(',');
				
				for (int j=0; j<_cols; j++)
				{
					GameVariables.Instance.ODDSTABLE_FG[i, j] = int.Parse(strSplitContentFG[j]);
				}				
			}	
		}
	}
	#endregion

	#region File Saving
	public void SaveFileTo(string _fileName, string _data)
	{
		string dirPath = Application.streamingAssetsPath + "/SavedFiles";
		
		//!Create folder
		if (!Directory.Exists (dirPath)) 
		{
			Directory.CreateDirectory(dirPath);		
		}
		//!will auto create a file if empty, overwritten otherwise.
		System.IO.File.WriteAllText(dirPath + _fileName, _data);
	}
	
	#endregion

	/// <summary> 
	/// File Loading shoud be done at starting of game. 
	/// </summary>
	public void Initialize()
	{
		LoadLinesDefinition(GameVariables.Instance.GAME_DEFINATION);
		LoadOddsTable ();
	
	}

	
}

