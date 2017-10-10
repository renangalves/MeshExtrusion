using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FaceExtrusion : MonoBehaviour
{

	Vector3[] vertPos = new Vector3[4];
	Vector3[] vertices;
	Vector3 startPos;
	
	List<GameObject> vertCubes = new List<GameObject> ();
	GameObject extrudingCube;
	GameObject endPos;
	public GameObject viewTarget;
	public GameObject vertCubeObject;

	bool faceSelected;
	bool createVerticesOnce;

	MeshCreator meshCreate;
	Mesh mesh;

	int rotateSpeed = 5;
	int[] triangles;

	float cubeSize = 0.5f;

	RaycastHit hit;
	
	Transform hitTransform;


	void Start ()
	{
		//Instantiate a copy of the Cube mesh so I can alter it without any problems to the original Cube mesh 
		Mesh newMesh = viewTarget.GetComponent<MeshFilter> ().sharedMesh;
		newMesh = (Mesh) Instantiate( newMesh );
		viewTarget.GetComponent<MeshFilter> ().sharedMesh = newMesh;
		viewTarget.GetComponent<MeshCollider> ().sharedMesh = newMesh;

		meshCreate = GetComponent<MeshCreator> ();
	}
	

	void Update ()
	{
	
		//If the left mouse button is clicked or held
		if (Input.GetMouseButton (0)) { 

			//If the mouse is being clicked on the cube, GetVertices will retrieve all vertices and faceSelected will become true
			if (!faceSelected)
				GetVertices ();
			else {
			
				//check if the vertices have been found by the number of cubes and check if the mouse is being dragged up (like the provided GIF example)
				if (vertCubes.Count != 0 && Input.GetAxis ("Mouse Y") > 0) {

					//create once a visual cube which gives feedback on the size of the extrusion
					if(!createVerticesOnce)
						CreateVisualExtrudeCube ();

					//move forward each vertice as the mouse is dragged
					for(int i = 0; i < vertPos.Length; i++)
					{
						vertCubes[i].transform.position += vertCubes[i].transform.forward * Time.deltaTime * 1;
					}
					//also move forward the endPos, which dictates the size of the visual extruded cube
					endPos.transform.position += endPos.transform.forward * Time.deltaTime * 1;
					//then calculate the between position of the startPos and endPos, and calculate the scale to keep resizing the cube
					Vector3 between = endPos.transform.position - startPos;
					float distance = between.magnitude;
					extrudingCube.transform.localScale = new Vector3(cubeSize, cubeSize, distance);
					extrudingCube.transform.position = startPos + (between / 2);
				}
			}
		}

		//when the mouse button is up, change the bool values and, if an extrusion cube was created, create the mesh and attach it to the original
		if (Input.GetMouseButtonUp (0)) {

			faceSelected = false;

			if(createVerticesOnce)
				CreateNewMesh();

			createVerticesOnce = false;
		}


		//if the right mouse button is held and moved, the camera will rotate around the cube
		if (Input.GetMouseButton (1)) {
			transform.LookAt (viewTarget.transform);
			transform.RotateAround (viewTarget.transform.position, Vector3.up, Input.GetAxis ("Mouse X") * rotateSpeed);
			transform.RotateAround (viewTarget.transform.position, Vector3.left, Input.GetAxis ("Mouse Y") * rotateSpeed);
		}

	}


	//This method will create the new mesh and attach it to the original mesh
	void CreateNewMesh()
	{

		List<Vector3> endPosVerts = new List<Vector3> ();
		//add the original vertices found on the mesh to the endPosVerts list
		endPosVerts.AddRange (vertPos);

		//each cube, which are the vertices, will be moved to the desired extrude position and here I get the positions of each of them
		foreach(GameObject cube in vertCubes)
		{
			endPosVerts.Add(cube.transform.position);
			Destroy(cube);
		}

		//finalVertArray will receive the ordered array of new vertices to be added to the mesh
		Vector3[] finalVertArray = meshCreate.ArrangeVerticesForCube (endPosVerts.ToArray ());

		//change all new vertices to local space which fixes the positioning
		for(int i = 0; i < finalVertArray.Length; i++)
		{
			finalVertArray[i] = hitTransform.InverseTransformPoint(finalVertArray[i]);
		}

		//add the mesh verts and the new vertices to a list, then add it to the mesh
		endPosVerts.Clear ();
		endPosVerts.AddRange (mesh.vertices);
		endPosVerts.AddRange (finalVertArray);
		mesh.vertices = endPosVerts.ToArray ();

		//calculate the new triangles and add them to the mesh triangles array
		List<int> triList = new List<int> ();
		int[] newTriangles = meshCreate.ArrangeTrianglesForCube (mesh.triangles.Length);
		triList.AddRange (mesh.triangles);
		triList.AddRange (newTriangles);
		mesh.triangles = triList.ToArray ();

		//reset and destroy some variables 
		vertCubes.Clear ();
		Destroy (extrudingCube);
		Destroy (endPos);
		//remove and add the MeshCollider so the collider is updated with the new mesh formation
		Destroy (hitTransform.gameObject.GetComponent<MeshCollider> ());
		hitTransform.gameObject.AddComponent <MeshCollider>();

	}


	//This method will create and position a visual representation cube which will show the extrusion size while the user drags the mouse
	void CreateVisualExtrudeCube ()
	{
		extrudingCube = Instantiate (vertCubeObject) as GameObject;
		Vector3 cubeScale = vertPos[0];
		Vector3 cubeScale2 = vertPos[0];

		//here I'm trying to find the most distant vertice from vertPos[0] so I can locate the middle point between them
		float dist = Vector3.Distance (cubeScale, cubeScale2);
		foreach(Vector3 point in vertPos)
		{
			if(Vector3.Distance(cubeScale, point) > dist)
			{
				dist = Vector3.Distance(cubeScale, point);
				cubeScale2 = point;
			}
		}

		//set the position, rotation and size of the cube related to the 2 most distant vertices of the selected face
		Vector3 between = cubeScale2 - cubeScale;
		extrudingCube.transform.localScale = new Vector3 (cubeSize, cubeSize, cubeSize);
		extrudingCube.transform.rotation = Quaternion.LookRotation (hit.normal);
		extrudingCube.transform.position = cubeScale + (between / 2);
		//save the starting point and end point which will be used to resize the cube as the endPos is moved
		startPos = extrudingCube.transform.position;
		endPos = Instantiate (vertCubeObject, extrudingCube.transform.position, extrudingCube.transform.rotation) as GameObject;

		createVerticesOnce = true;
	}


	//This method will find the 4 vertices of the clicked mesh face
	void GetVertices ()
	{ 
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition); 

		//if the mouse hits the MeshCollider of the cube
		if (Physics.Raycast (ray, out hit)) {

			//get the mesh, vertices and triangles
			MeshCollider meshCollider = hit.collider as MeshCollider;
			mesh = meshCollider.sharedMesh;
			vertices = mesh.vertices;
			triangles = mesh.triangles;
			
			hitTransform = hit.collider.transform;
			
			int[] vertsIndex = new int[4];
			int t = hit.triangleIndex * 3;

			//get all 3 vertices based on the triangle that was hit
			for (int i = 0; i < vertPos.Length-1; i++) {
				vertsIndex [i] = t + i;
				vertPos [i] = vertices [triangles [vertsIndex [i]]];
			}

			//here I round the float value of the vertices positions to avoid small differences like vertPos[0].x = 0.05015 and vertPos[1].x = 0.05014
			for (int i = 0; i < vertPos.Length; i++)
			{
				vertPos[i] = new Vector3(Mathf.Round(vertPos[i].x * 100f) / 100f, Mathf.Round(vertPos[i].y * 100f) / 100f, Mathf.Round(vertPos[i].z * 100f) / 100f);
			}

			//this method will find the fourth vertice and store it in vertPos[3]
			FindTheFourthVertice();

			//if there is no vertCubes, which are cubes that demonstrate the selected vertices, instantiate them
			//each vertCube will be positioned on the selected vertices positions
			if (vertCubes.Count == 0) {
				for (int i = 0; i < vertPos.Length; i++) {
					//using TransformPoint to change the position from local to world space so the cubes are correctly positioned
					vertPos [i] = hitTransform.TransformPoint (vertPos [i]);
					vertCubes.Add ((Instantiate (vertCubeObject, vertPos [i], Quaternion.LookRotation (hit.normal))) as GameObject);
				}
				//if cubes already have been instantiated just move them to the new vertices positions
			} else {
				for (int i = 0; i < vertPos.Length; i++) {
					//using TransformPoint to change the position from local to world space so the cubes are correctly positioned
					vertPos [i] = hitTransform.TransformPoint (vertPos [i]);
					vertCubes [i].transform.position = vertPos [i];
					vertCubes [i].transform.rotation = Quaternion.LookRotation (hit.normal);
				}
			}

			faceSelected = true;

		}
	}



	//This method will use the 3 vertices of the selected triangle to find the fourth vertice which results in the selected face of the mesh
	void FindTheFourthVertice()
	{
		//if the x position of vertice 1 and 0 is different
		if (vertPos [1].x != vertPos [0].x) {
			vertPos [3] = vertPos [2];
			vertPos [3].x = vertPos [0].x;
			//check if the fourth vertice is equal to any of the other 3 vertices
			if (vertPos [3] == vertPos [0] || vertPos [3] == vertPos [1] || vertPos [3] == vertPos [2]) {
				vertPos [3] = vertPos [1];
				vertPos [3].x = vertPos [0].x;
				//check one last time for some possible cases
				if (vertPos [3] == vertPos [0] || vertPos [3] == vertPos [1] || vertPos [3] == vertPos [2]) {
					vertPos [3] = vertPos [0];
					vertPos [3].x = vertPos [1].x;
				}
				//this method will fix the order of the vertices, which is necessary when the fourth vertice happens to be equal to any of the other 3 vertices
				CorrectVerticeOrder();	
			}

			//if the y position of vertice 1 and 0 is different
		} else if (vertPos [1].y != vertPos [0].y) {
			vertPos [3] = vertPos [2];
			vertPos [3].y = vertPos [0].y;
			//check if the fourth vertice is equal to any of the other 3 vertices
			if (vertPos [3] == vertPos [0] || vertPos [3] == vertPos [1] || vertPos [3] == vertPos [2]) {
				vertPos [3] = vertPos [0];
				vertPos [3].y = vertPos [1].y;
				//check one last time for some possible cases
				if (vertPos [3] == vertPos [0] || vertPos [3] == vertPos [1] || vertPos [3] == vertPos [2]) {
					vertPos [3] = vertPos [1];
					vertPos [3].y = vertPos [0].y;
				}
				//this method will fix the order of the vertices, which is necessary when the fourth vertice happens to be equal to any of the other 3 vertices
				CorrectVerticeOrder();
			}

			//if the z position of vertice 1 and 0 is different
		} else if (vertPos [1].z != vertPos [0].z) {
			vertPos [3] = vertPos [2];
			vertPos [3].z = vertPos [0].z;
			//check if the fourth vertice is equal to any of the other 3 vertices
			if (vertPos [3] == vertPos [0] || vertPos [3] == vertPos [1] || vertPos [3] == vertPos [2]) {
				vertPos [3] = vertPos [1];
				vertPos [3].z = vertPos [0].z;
				//check one last time for some possible cases
				if (vertPos [3] == vertPos [0] || vertPos [3] == vertPos [1] || vertPos [3] == vertPos [2]) {
					vertPos [3] = vertPos [0];
					vertPos [3].z = vertPos [1].z;
				}
				//this method will fix the order of the vertices, which is necessary when the fourth vertice happens to be equal to any of the other 3 vertices
				CorrectVerticeOrder();
			}
		}

	}



	//This method will ensure that the order of the vertices in vertPos will be correct
	void CorrectVerticeOrder()
	{
		Vector3 aux = vertPos[3];
		vertPos[3] = vertPos[0];
		vertPos[0] = aux;
	}


}




