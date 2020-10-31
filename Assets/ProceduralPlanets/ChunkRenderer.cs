﻿using System;
using UnityEditor;
using UnityEngine;

public class ChunkRenderer : MonoBehaviour
{
	Planet planet;

	public Chunk chunk;

	public Chunk.GeneratedData generatedData; // chunk may refresh, and create new generated data so lets keep alive the data we are showing

	Material material;
	MeshFilter meshFilter;
	MeshCollider meshCollider;
	FloatingOriginTransform floatingTransform;

	public static ChunkRenderer CreateFor(Planet planet)
	{
		var go = new GameObject(nameof(ChunkRenderer));
		//go.transform.parent = planet.transform;
		var renderer = go.AddComponent<ChunkRenderer>();
		renderer.planet = planet;
		renderer.CreateComponents();
		return renderer;
	}

	void CreateComponents()
	{
		MyProfiler.BeginSample("Procedural Planet / ChunkRenderer / CreateComponents");

		meshFilter = gameObject.AddComponent<MeshFilter>();
		
		floatingTransform = gameObject.AddComponent<FloatingOriginTransform>();

		material = new Material(planet.chunkConfig.chunkMaterial);
		var meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.sharedMaterial = material;

		if (planet.chunkConfig.createColliders)
		{
			MyProfiler.BeginSample("Procedural Planet / ChunkRenderer / CreateComponents / Collider");
			meshCollider = gameObject.AddComponent<MeshCollider>();	
			MyProfiler.EndSample();
		}

		MyProfiler.EndSample();
	}

	public void RenderChunk(Chunk chunk)
	{
		this.chunk = chunk;

		if (chunk.GenerateUsingPlanetGlobalPos)
			floatingTransform.BigPosition = planet.BigPosition;
		else
			floatingTransform.BigPosition = planet.BigPosition + chunk.bigPositionLocalToPlanet;

		this.transform.rotation = planet.transform.rotation;

		generatedData = chunk.FullyGeneratedData;

		meshFilter.sharedMesh = generatedData.mesh;
		if (meshCollider) meshCollider.sharedMesh = generatedData.mesh;

		if (material && generatedData.chunkDiffuseMap) material.mainTexture = generatedData.chunkDiffuseMap;
		if (material && generatedData.chunkNormalMap) material.SetTexture("_BumpMap", generatedData.chunkNormalMap);
	}

	public void Hide()
	{
		generatedData = null;
		chunk = null;

		meshFilter.sharedMesh = null;
		if (meshCollider) meshCollider.sharedMesh = null;
	}

	private void OnDrawGizmosSelected()
	{
		if (gameObject && gameObject.activeSelf && chunk != null)
		{
			Gizmos.color = Color.cyan;
			//rangePosRealSubdivided.DrawGizmos();
			chunk.rangePosToCalculateScreenSizeOn_localToPlanet.DrawGizmos(chunk.planet.transform.position);

			Vector3 unityCenter = chunk.rangePosToCalculateScreenSizeOn_localToPlanet.CenterPos + chunk.planet.transform.position;

			Gizmos.DrawSphere(unityCenter, 0.1f);

			Handles.Label(unityCenter, 
				"weight " + chunk.lastGenerationWeight.ToString() + "\n" +
				"dist " + chunk.lastDistanceToCamera.ToString() + "\n" +
				"radius " + chunk.lastRadiusWorldSpace.ToString() + "\n" +
				"slopeModifier " + chunk.SlopeModifier.ToString() + "\n"
			);
		}
	}
}