using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
using System.Collections.Generic;


public class Extrusion : MonoBehaviour
{
	float cubeSize = 0.5f;

	int[] startingPos = new int[4] {0, 1, 4, 5};
	int[] endingPos = new int[4] {2, 3, 6, 7}; 
 
	Vector3[] newVertices = new Vector3[8];
	List<Vector3> vertList = new List<Vector3>();

	List<int> trianglesList = new List<int> ();

	public Transform[] positions;

	Mesh mesh;
	MeshCreator createMesh;


	void Start ()
	{
		createMesh = GetComponent<MeshCreator> ();

		//add and clear a new meshFilter to the object
		MeshFilter filter = gameObject.AddComponent< MeshFilter >();
		mesh = filter.mesh;
		mesh.Clear();

		//add the four vertices in the position 1 and the four vertices in the position 2
		AddStartingPositions (positions[1].position);
		AddEndingPositions (positions[0].position);

		//createMesh will return the organized array of vertices and triangles for the first cube, which is stored in vertList and triangleList
		vertList.AddRange(createMesh.ArrangeVerticesForCube(newVertices));
		trianglesList.AddRange(createMesh.ArrangeTrianglesForCube(trianglesList.Count));

		//now add to the ending positions of newVertices the four vertices of position 2
		AddEndingPositions (positions[2].position);

		//now vertList and triangleList will receive the organized array of vertices and triangles between positions 1 and 2
		vertList.AddRange(createMesh.ArrangeVerticesForCube(newVertices));
		trianglesList.AddRange(createMesh.ArrangeTrianglesForCube(trianglesList.Count));

		//now add to the beggining positions of newVertices the four vertices of position 3
		AddStartingPositions (positions[3].position);

		//now vertList and triangleList will receive the organized array of vertices and triangles between positions 2 and 3
		vertList.AddRange(createMesh.ArrangeVerticesForCube(newVertices));
		trianglesList.AddRange(createMesh.ArrangeTrianglesForCube(trianglesList.Count));

		//now add to the mesh the list of vertices and triangles, which will result in the extruded mesh between all 4 points
		mesh.vertices = vertList.ToArray ();;
		mesh.triangles = trianglesList.ToArray();
		
		mesh.RecalculateBounds();
		;

	}


	//This method will add to the starting points of newVertices the four vertices of the desired initial position
	void AddStartingPositions(Vector3 pos)
	{
		newVertices[startingPos[0]] = new Vector3(pos.x - cubeSize, pos.y - cubeSize, pos.z + cubeSize);
		newVertices[startingPos[1]] = new Vector3(pos.x + cubeSize, pos.y - cubeSize, pos.z + cubeSize);
		newVertices[startingPos[2]] = new Vector3(pos.x - cubeSize, pos.y + cubeSize, pos.z + cubeSize);
		newVertices[startingPos[3]] = new Vector3(pos.x + cubeSize, pos.y + cubeSize, pos.z + cubeSize);
	}

	//This method will add to the ending points of newVertices the four vertices of the desired ending position, 
	//completing a cube with the four vertices of AddStartingPositions
	void AddEndingPositions(Vector3 pos)
	{
		newVertices[endingPos[0]] = new Vector3(pos.x + cubeSize, pos.y - cubeSize, pos.z - cubeSize);
		newVertices[endingPos[1]] = new Vector3(pos.x - cubeSize, pos.y - cubeSize, pos.z - cubeSize);	
		newVertices[endingPos[2]] = new Vector3(pos.x + cubeSize, pos.y + cubeSize, pos.z - cubeSize);
		newVertices[endingPos[3]] = new Vector3(pos.x - cubeSize, pos.y + cubeSize, pos.z - cubeSize);
	}

}
