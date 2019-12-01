﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeSpawnController : MonoBehaviour
{
    [SerializeField]
    private float startDelayTime;
    [SerializeField]
    private int addNewShapeAfter;

    private Camera cam;
    private Bounds bgBounds;
    private int shapeSpawnedBeforeNewShape;
    private List<ShapeMatch> spawnedShapes;
    private ShapeScriptable nextShape;
    private IEnumerator spawnWaveRoutine;

    public void Setup(GameManager _gm)
    {
        cam = _gm.GetCamera();
        bgBounds = _gm.GetBackgroundManager().GetBackgroundBounds();
        spawnedShapes = new List<ShapeMatch>();
    }

    public void StartSpawn()
    {
        ShapeController.OnNewShapeAdd += HandleOnNewShapeAdd;
        ShapeMatch.ShapeDestroied += HandleShapeDestroyed;

        shapeSpawnedBeforeNewShape = 0;
        nextShape = null;

        spawnWaveRoutine = SpawnShapeCoroutine();
        StartCoroutine(spawnWaveRoutine);
    }

    public void SpawnShape(ShapeScriptable _shapeToSpawn)
    {
        //Calculate Random Position
        float randomXValue = UnityEngine.Random.Range((bgBounds.center.x - bgBounds.extents.x) + 1f, (bgBounds.center.x + bgBounds.extents.x) - 1f);
        Vector3 spawnVector = new Vector3(randomXValue, transform.position.y, transform.position.z);

        //Calculate Random Rotation
        float randomRoation = UnityEngine.Random.Range(-60f, 60f);
        Quaternion spawnRotation = Quaternion.Euler(0, 0, randomRoation);

        ShapeMatch newShape = PoolManager.instance.GetPooledObject(ObjectTypes.Shape, gameObject).GetComponent<ShapeMatch>();

        if (newShape != null)
        {
            newShape.transform.SetPositionAndRotation(spawnVector, spawnRotation);
            newShape.Setup(_shapeToSpawn, cam);
            spawnedShapes.Add(newShape);
        }
    }

    private IEnumerator SpawnShapeCoroutine()
    {
        yield return new WaitForSeconds(startDelayTime);

        while (true)
        {
            shapeSpawnedBeforeNewShape++;
            if (shapeSpawnedBeforeNewShape == addNewShapeAfter)
            {
                ShapeController.AddNewShape();
                shapeSpawnedBeforeNewShape = 0;
                yield return new WaitForSeconds(0.1f);
            }

            ShapeScriptable shapeToSpawn;
            if (nextShape != null)
            {
                shapeToSpawn = nextShape;
                nextShape = null;
            }
            else
                shapeToSpawn = ShapeController.GetRandomShapeMatch();

            SpawnShape(shapeToSpawn);

            yield return new WaitForSeconds(DifficultyManager.GetCurrentSpawnRate());
        }
    }

    private void HandleOnNewShapeAdd(ShapeScriptable _newShape)
    {
        DestroyShapes();
        nextShape = _newShape;
    }

    private void HandleShapeDestroyed(ShapeMatch _shapeDestroied)
    {
        spawnedShapes.Remove(_shapeDestroied);
    }

    public void StopSpawn()
    {
        if (spawnWaveRoutine != null)
            StopCoroutine(spawnWaveRoutine);

        DestroyShapes();
        nextShape = null;

        ShapeController.OnNewShapeAdd -= HandleOnNewShapeAdd;
        ShapeMatch.ShapeDestroied -= HandleShapeDestroyed;
    }

    private void DestroyShapes()
    {
        if (spawnedShapes != null && spawnedShapes.Count > 0)
        {
            for (int i = spawnedShapes.Count - 1; i >= 0; i--)
                spawnedShapes[i].DestroyShape();

            spawnedShapes.Clear();
        }
    }
}
