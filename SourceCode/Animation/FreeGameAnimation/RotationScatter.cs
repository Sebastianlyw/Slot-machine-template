using UnityEngine;
using System.Collections;

public class RotationScatter : MonoBehaviour {


	private float m_zRotation;
	
	public float RotationSpeed = 15;

	public bool IsA;
	
	private bool IsRotateZ;

	public GameObject temp;

	// Use this for initialization
	void Start () {

		IsA = false;
		IsRotateZ = false;
		m_zRotation = 0;


	}

	public void ResetRotation()
	{
		IsA = false;
		IsRotateZ = false;
	
	}

	private void PauseRotation()
	{
		transform.eulerAngles = Vector3.zero;
		IsRotateZ = true;
		for(int i = 0; i < transform.childCount; ++i)
			Destroy (transform.GetChild (i).gameObject);
	}
	private void scaleUp(object hash)
	{
		GameObject go = hash as GameObject;
		LeanTween.scale (go, new Vector3 (200, 200, 1), 0.5f).setOnComplete (ScaleBack);
	}

	private void ScaleBack()
	{
		LeanTween.scale (gameObject, new Vector3 (154, 154, 1), 0.25f).setOnComplete (PauseRotation);
	}

	// Update is called once per frame
	void Update () {
	
		if(!IsA)
		{
			IsA = true;
			LeanTween.scale(gameObject, new Vector3(33,33,1), 0.5f).setOnComplete(scaleUp).setOnCompleteParam(gameObject);
			Vector3 offset = new Vector3(72, -72,0);
			Vector3 pos = transform.position + offset;
			GameObject temp = GameObject.Find("Trailer");
			temp.transform.position = Vector3.zero;
			if(temp ==  null)
			{
				Debug.Log("NOtclone sd");
			}
			GameObject tempTrailer = Instantiate(temp ,pos, Quaternion.identity) as GameObject;
			tempTrailer.transform.parent = transform;
		}
		if(!IsRotateZ)
		{
			m_zRotation += RotationSpeed;
			transform.eulerAngles = new Vector3(0, 0, m_zRotation);
		
		}

		if(Input.GetKeyDown(KeyCode.Alpha1))
		{
		   IsA = false;
			IsRotateZ = false;
		}

	}


}
