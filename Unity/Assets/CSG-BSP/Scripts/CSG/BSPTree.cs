﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CSG
{
	public class BSPTree
	{
		private Node root;

		public BSPTree()
		{

		}

		public void AddTriangles(List<Triangle> triangles)
		{
			if (triangles == null || triangles.Count <= 0) return;
		
			if(root == null)
			{
				root = Node.Create(triangles[0].OrientationPlane);
			}

			AddTriangles(root, triangles);
		}

		private void AddTriangles(Node node, List<Triangle> triangles)
		{
			if (triangles == null || triangles.Count <= 0)return;
			if(node == null)return;

			List<Triangle> nodeTriangles =  node.GetTriangleList();
			
			List<Triangle> lessThan = new List<Triangle> ();
			List<Triangle> greaterThan = new List<Triangle> ();
			
			for (int i = 0; i < triangles.Count; i++) 
			{
				Partitioner.Orientation orient = Partitioner.SliceTriangle(triangles[i], node.SplitPlane, lessThan, greaterThan, nodeTriangles, nodeTriangles);
			}
			
			if(lessThan.Count > 0)
			{
				if(node.LessThan == null)
					node.LessThan = Node.Create(lessThan[0].OrientationPlane);
				AddTriangles(node.LessThan, lessThan);
			}
			
			if(greaterThan.Count > 0)
			{
				if( node.GreaterThan == null)
					node.GreaterThan = Node.Create(greaterThan[0].OrientationPlane);
				AddTriangles(node.GreaterThan, greaterThan);
			}
		}

		public void ClipOutTriangles(List<Triangle> triangles)
		{
			ClipOutTriangles (root, triangles);
		}

		private void ClipOutTriangles(Node node, List<Triangle> triangles)
		{
			if (triangles == null || triangles.Count <= 0)return;
			if(node == null)return;

			List<Triangle> lessThan = new List<Triangle>();
			List<Triangle> greaterThan = new List<Triangle>();

			for (int i = 0; i < triangles.Count; i++) 
			{
				Partitioner.Orientation orient = Partitioner.SliceTriangle(triangles[i], node.SplitPlane, lessThan, greaterThan, lessThan, greaterThan);
			}

			if(node.LessThan != null)
				ClipOutTriangles (node.LessThan, lessThan);
			else 
				lessThan.Clear();
			ClipOutTriangles (node.GreaterThan, greaterThan);

			triangles.Clear ();
			triangles.AddRange (lessThan);
			triangles.AddRange (greaterThan);
		}

		public void ClipByTree(BSPTree tree) 
		{
			ClipByTree (root, tree);
		}

		private void ClipByTree(Node node, BSPTree tree)
		{
			if(node == null)return;

			tree.ClipOutTriangles (node.GetTriangleList());
			ClipByTree (node.LessThan, tree);
			ClipByTree (node.GreaterThan, tree);
		}

		public List<Triangle> GetAllTriangles()
		{
			List<Triangle> allTriangles = new List<Triangle> ();
			GetAllTriangles (root, allTriangles);
			return allTriangles;
		}

		private void GetAllTriangles(Node node, List<Triangle> triangles)
		{
			if(node == null)return;

			for(int i = 0; i < node.TriangleCount(); i++)
			{
				triangles.Add(node.GetTriangle(i));
			}

			GetAllTriangles(node.LessThan, triangles);
			GetAllTriangles(node.GreaterThan, triangles);
		}

		public void Invert()
		{
			Invert (root);
		}

		private void Invert(Node node)
		{
			if(node == null)return;

			node.Invert ();
			Invert (node.LessThan);
			Invert (node.GreaterThan);
		}

		public BSPTree Clone()
		{
			BSPTree copy = new BSPTree();
			copy.root = Clone(root);
			return copy;
		}

		private Node Clone(Node node)
		{
			if(node == null) return null;
			Node copy = node.Clone();
			copy.LessThan = Clone(node.LessThan);
			copy.GreaterThan = Clone(node.GreaterThan);
			return copy;
		}

		private static List<Triangle> CloneTriangles(List<Triangle> triangles)
		{
			if(triangles == null) return null;

			List<Triangle> clones = new List<Triangle>();
			for(int i = 0; i < triangles.Count; i++)
			{
				clones.Add(triangles[i].Clone());
			}

			return clones;
		}

		private class Node
		{
			public Plane SplitPlane;
			public Node LessThan;
			public Node GreaterThan;

			private List<Triangle> triangles;

			public static Node Create(Plane splitPlane)
			{
				Node newNode = new Node();
				newNode.SplitPlane = splitPlane;
				return newNode;
			}

			private Node()
			{
				LessThan = null;
				GreaterThan = null;
				triangles = new List<Triangle>();
			}

			public Node Clone()
			{
				Node copy = new Node();

				copy.SplitPlane = SplitPlane;

				for(int i = 0; i < triangles.Count; i++)
				{
					copy.triangles.Add(triangles[i].Clone());
				}

				copy.LessThan = LessThan;
				copy.GreaterThan = GreaterThan;

				return copy;
			}

			public void Invert()
			{
				for (int i = 0; i < triangles.Count; i++) 
				{
					Triangle tri = triangles[i];
					tri.Flip();
					triangles[i] = tri;
				}

				SplitPlane.Flip ();

				Node tempList = LessThan;
				LessThan = GreaterThan;
				GreaterThan = tempList;
			}

			public int TriangleCount()
			{
				if(triangles != null)
					return triangles.Count;
				else 
					return 0;
			}

			public Triangle GetTriangle(int index)
			{
				if(triangles == null || index < 0 || index >= triangles.Count)
				{
					throw new System.IndexOutOfRangeException("Invalid triangle index.");
				}

				return triangles [index];
			}

			public List<Triangle> GetTriangleList()
			{
				return triangles;
			}
		}
	}
}