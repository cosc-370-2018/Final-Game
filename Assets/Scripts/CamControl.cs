using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour {

	public Transform player;
	private Vector3 pos;
	private float rotx = 0.0f;
	private float roty = 0.0f;
	public Camera cam;


	// Use this for initialization
	void Start () {
		pos = new Vector3(player.position.x, player.position.y, player.position.z);
		cam = GetComponentInChildren<Camera> ();
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = player.position + pos;
		roty += Input.GetAxis ("Mouse X") * 80 * Time.deltaTime;
		rotx += Input.GetAxis ("Mouse Y") * 40 * Time.deltaTime;
		rotx = Mathf.Clamp (rotx, -45, 45);
		transform.rotation = Quaternion.Euler (rotx, roty, 0.0f);
	}
}
