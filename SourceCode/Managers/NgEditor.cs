using UnityEngine;
using System.Collections;


/// <summary>
/// <para>Version: 1.0.0</para>	 
/// <para>Author: Li Ye Wei</para>
/// Main game editor.
/// 
/// </summary>
public class NgEditor : SlotMachineEditor {

	
	//! Singleton
	private static NgEditor instance;
	
	//! Constuct
	private NgEditor() {}
	
	//! Instance
	public static NgEditor Instance
	{
		get
		{
			if (instance == null)
				instance = GameObject.FindObjectOfType(typeof(NgEditor)) as NgEditor;
			
			return instance;
		}
	}
	/*end of Singleton*/


	/// <summary>
	/// Initialize Datas used in main game mode.
	/// </summary>
	public void NgInitEditor()
	{
		InitEditor ();
		Icons.Instance.StoreIcons (out Icons.Instance.ORIGIN_ICONS);
		Icons.Instance.StoreHeadTail (out Icons.Instance.ORIGIN_HEADTAIL);
		IconAnim.Instance.HideAnimIcons ();
	}
	

	/// <summary>
	/// Update editor status, should be called per frame if game is in Main Game editor state.
	/// </summary>
	public void ControlUpdate()
	{
		TextAndDigitDisp.Instance.SetMessageEditor ("Main Game Editor.\n Press 'J' to save and exit.");
		EditorUpdate ( );

		if (Input.GetKeyDown (KeyCode.J)) 
		{
	
			Icons.Instance.StoreHeadTail (out Icons.Instance.TARGET_HEADTAIL);
			DestoryMaker();
			if (WinManager.Instance.IsTriggerFG ())
			{	
				TextAndDigitDisp.Instance.SetMessageEditor ("Free Game Editor.\n Press 'J' to save and exit.");
				// free game editor idle state, ready to edite free games.
				GameVariables.Instance.EDITOR_FLAG = GameVariables.EDIT_STATES.FG_IDLE;
				//FgEditor.Instance.InitFreeGameEditor();
			}
			else
			{
				TextAndDigitDisp.Instance.SetMessageEditor (" ");
				Icons.Instance.ResetIcons ();
				//Icons.Instance.ResetTempHeadTail(GameVariables.Instance.REEL_STRIPS_NG);
				GameVariables.Instance.EDITOR_FLAG = GameVariables.EDIT_STATES.NG_WAIT_TO_SPIN;

				ResetBG();
			}
		}
	}




}




