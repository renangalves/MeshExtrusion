using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshCreator : MonoBehaviour {
	
	//This class is purely for arrangement of vertices and triangles for creation of meshes


	//This method will arrange the vertices in the correct positions so the cube will be looking correctly 
	public Vector3[] ArrangeVerticesForCube(Vector3[] verts)
	{

		Vector3[] vertices = new Vector3[]
		{
			// Bottom
			verts[0], verts[1], verts[2], verts[3],
			
			// Left
			verts[7], verts[4], verts[0], verts[3],
			
			// Front
			verts[4], verts[5], verts[1], verts[0],
			
			// Back
			verts[6], verts[7], verts[3], verts[2],
			
			// Right
			verts[5], verts[6], verts[2], verts[1],
			
			// Top
			verts[7], verts[6], verts[5], verts[4]
		};

		return vertices;
	}


	//This method will arrange the triangles so the cube will be looking correctly 
	public int[] ArrangeTrianglesForCube(int curValue)
	{
		List<int> triangles = new List<int> ();
		int index = 0;

		//if the current mesh already has 36 triangles, then the creation of new triangles should start at 6 (36 / 6)
		if(curValue > 0)
			index = curValue / 6;

		for (int i = 0; i < 6; i++) {

			if(index == 0)
			{
				triangles.Add(3);
				triangles.Add(1);
				triangles.Add(0);
				triangles.Add(3);
				triangles.Add(2);
				triangles.Add(1);
			}
			else
			{
				triangles.Add(3 + 4 * index);
				triangles.Add(1 + 4 * index);
				triangles.Add(0 + 4 * index);
				triangles.Add(3 + 4 * index);
				triangles.Add(2 + 4 * index);
				triangles.Add(1 + 4 * index);
			}

			index++;
		}

		return triangles.ToArray ();
	}
	
}
