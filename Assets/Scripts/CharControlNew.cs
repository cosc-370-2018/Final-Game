using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharControlNew : MonoBehaviour {

	public Animator anim;
	public Camera cam;
	public CharacterController cc;
    private Vector3 MoveDirection;
    private Vector3 LookDir;
    private Quaternion LookAt;
    public bool ragdoll = false;
    public Rigidbody[] bodies;
    private Rigidbody rb;
    private float speed = 0.5f;
    private string dir;

    public Transform[] children;
    public List<Vector3> resetPositions;
    public List<Quaternion> resetRotations;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
		cc = GetComponent<CharacterController> ();
		anim = GetComponent<Animator>();
        resetPositions = new List<Vector3>();
        bodies = GetComponentsInChildren<Rigidbody>();
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            resetPositions.Add(child.transform.position);
        }
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            resetRotations.Add(child.transform.rotation);
        }
    }

    // Update is called once per frame
    void FixedUpdate () {

        if (Input.GetKey(KeyCode.W)) {
            anim.SetBool("run", true);
            if (speed <= 10) {
                speed *= 1.05f;
            }
            if (Input.GetKey (KeyCode.A)) {
                dir = "NE";
            } else if (Input.GetKey (KeyCode.D)) {
                dir = "NW";
            } else {
                dir = "N";
            }

        } else if (Input.GetKey (KeyCode.S)) {
            anim.SetBool("run", true);
            if (speed <= 10)
            {
                speed *= 1.05f;
            }
            if (Input.GetKey (KeyCode.A)) {
                dir = "SE";
			} else if (Input.GetKey (KeyCode.D)) {
                dir = "SW";
			} else {
                dir = "S";
            }
        } else if (Input.GetKey (KeyCode.A)) {
            if (speed <= 10)
            {
                speed *= 1.05f;
            }
            dir = "E";
		} else if (Input.GetKey (KeyCode.D)) {
            if (speed <= 10)
            {
                speed *= 1.05f;
            }
            dir = "W";
			anim.SetBool ("run", true);
		} else {
            if (speed >= 0.75f)
            {
                speed /= 1.05f;
            }
        }


        if (speed > 0.75f)
        {
            anim.speed = speed / 10;
            MoveDirection = transform.forward * speed * Time.deltaTime;
            anim.SetBool("run", true);
        }
        else
        {
            anim.SetBool("run", false);
            MoveDirection = Vector3.zero;
        }

        switch (dir)
        {
            case "N":
                LookDir = cam.transform.forward * speed * Time.deltaTime;
                break;
            case "NE":
                LookDir = (cam.transform.forward - cam.transform.right) * speed * Time.deltaTime;
                break;
            case "NW":
                LookDir = (cam.transform.forward + cam.transform.right) * speed * Time.deltaTime;
                break;
            case "S":
                LookDir = -cam.transform.forward * speed * Time.deltaTime;
                break;
            case "SE":
                LookDir = (-cam.transform.forward - cam.transform.right) * speed * Time.deltaTime;
                break;
            case "SW":
                LookDir = (-cam.transform.forward + cam.transform.right) * speed * Time.deltaTime;
                break;
            case "E":
                LookDir = -cam.transform.right * speed * Time.deltaTime;
                break;
            case "W":
                LookDir = cam.transform.right * speed * Time.deltaTime;
                break;
        }

        if (!cc.isGrounded) {
			cc.Move (-transform.up * 10 * Time.deltaTime);
		}
		cc.Move (MoveDirection);

		if (LookDir != Vector3.zero) {
			LookAt = Quaternion.LookRotation (LookDir);
		}

		Quaternion LookRotationLimit = Quaternion.Euler (transform.rotation.eulerAngles.x, LookAt.eulerAngles.y, transform.rotation.eulerAngles.z);
		transform.rotation = Quaternion.Slerp (transform.rotation, LookRotationLimit, 0.05f);
		
	}

    void toggleRagdoll()
    {
        if (!ragdoll)
        {
            ragdoll = true;
            for (var i = 0; i < bodies.Length; i++)
            {
                bodies[i].isKinematic = false;
            }
            this.gameObject.GetComponent<Animator>().enabled = false;
            StartCoroutine(ragdollEnable());
            StartCoroutine(ragdollDisable());

        }
        else
        {
            ragdoll = false;
            for (var i = 0; i < bodies.Length; i++)
            {
                bodies[i].isKinematic = true;
            }
            Debug.Log(resetPositions);
            children = GetComponentsInChildren<Transform>();
            for (int i =0;i<resetPositions.Count;i++)
            {
                children[i].transform.position = resetPositions[i];
                children[i].transform.rotation = resetRotations[i];

            }
            this.gameObject.GetComponent<CharacterController>().enabled = true;
            this.gameObject.GetComponent<Animator>().enabled = true;
        }
    }

    void OnTriggerEnter(Collider collider)
    {

        if (collider.gameObject.tag == "RagdollCollider")
        {
            toggleRagdoll();
        }
    }

    public IEnumerator ragdollEnable()
    {
        yield return new WaitForSeconds(.5f);
        this.gameObject.GetComponent<CharacterController>().enabled = false;
    }

    public IEnumerator ragdollDisable()
    {
        yield return new WaitForSeconds(5);
        toggleRagdoll();
    }
}
