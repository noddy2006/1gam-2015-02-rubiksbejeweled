﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Board : MonoBehaviour {

    public Spawner spawner;

    public enum Axis
    {
        X = 0, Y = 1, Z = 2
    }

    public Cube[, ,] Cubes
    {
        get;
        set;
    }

    /// <summary>
    /// Usage: faces[face][x][y];
    /// The faces array is set up as follows:
    /// face = a Face.Direction enum
    /// x, y = the x/y index of the face if you were to look at the face "front on"
    /// </summary>
    public Face[][][] Faces
    {
        get;
        set;
    }

    public GameObject[][] Axes
    {
        get;
        set;
    }

    private int boardSize;

    public void Init(int boardSize)
    {
        this.boardSize = boardSize;

        Faces = new Face[6][][];
        for (int i = 0; i < 6; i++)
        {
            Faces[i] = new Face[boardSize][];
            for (int j = 0; j < boardSize; j++)
            {
                Faces[i][j] = new Face[boardSize];
            }
        }
    }

    public void RotateAxis(Axis axis, int index, int numberTurns)
    {
        for (int i = 0; i < numberTurns % 4; i++)
        {
            RotateCubesOnce(axis, index);
            RotateFacesOnce(axis, index);
        }

        ReassignNeighbours(axis, index);
        CheckMatches(axis, index);
    }

    private void RotateCubesOnce(Axis axis, int index)
    {
        Cube[,] original = new Cube[boardSize, boardSize];

        switch (axis)
        {
            case Axis.X:
                for (int i = 0; i < boardSize; i++)
                {
                    for (int j = 0; j < boardSize; j++)
                    {
                        original[i, j] = Cubes[index, i, j];
                    }
                }
                for (int i = 0; i < boardSize; i++)
                {
                    for (int j = 0; j < boardSize; j++)
                    {
                        Cubes[index, boardSize - j - 1, i] = original[i, j];
                        if (Cubes[index, boardSize - j - 1, i])
                        {
                            Cubes[index, boardSize - j - 1, i].Index = new Vector3(index, boardSize - j - 1, i);
                        }
                    }
                }
                break;
            case Axis.Y:
                for (int i = 0; i < boardSize; i++)
                {
                    for (int j = 0; j < boardSize; j++)
                    {
                        original[i, j] = Cubes[i, index, j];
                    }
                }
                for (int i = 0; i < boardSize; i++)
                {
                    for (int j = 0; j < boardSize; j++)
                    {
                        Cubes[j, index, boardSize - i - 1] = original[i, j];
                        if (Cubes[j, index, boardSize - i - 1])
                        {
                            Cubes[j, index, boardSize - i - 1].Index = new Vector3(j, index, boardSize - i - 1);
                        }
                    }
                }
                break;
            case Axis.Z:
                for (int i = 0; i < boardSize; i++)
                {
                    for (int j = 0; j < boardSize; j++)
                    {
                        original[i, j] = Cubes[i, j, index];
                    }
                }
                for (int i = 0; i < boardSize; i++)
                {
                    for (int j = 0; j < boardSize; j++)
                    {
                        Cubes[boardSize - j - 1, i, index] = original[i, j];
                        if (Cubes[boardSize - j - 1, i, index])
                        {
                            Cubes[boardSize - j - 1, i, index].Index = new Vector3(boardSize - j - 1, i, index);
                        }
                    }
                }
                break;
        }
    }

    private void RotateFacesOnce(Axis axis, int index)
    {
        Face[] temp;

        switch (axis)
        {
            case Axis.X:
                temp = Faces[(int)Face.Direction.Front][index];
                Faces[(int)Face.Direction.Front][index] = ReverseArray(Faces[(int)Face.Direction.Bottom][index]);
                Faces[(int)Face.Direction.Bottom][index] = Faces[(int)Face.Direction.Back][index];
                Faces[(int)Face.Direction.Back][index] = ReverseArray(Faces[(int)Face.Direction.Top][index]);
                Faces[(int)Face.Direction.Top][index] = temp;
                
                if (index == 0)
                {
                    RotateWholeSideFaces(Face.Direction.Left, true);
                }
                else if (index == boardSize - 1) 
                {
                    RotateWholeSideFaces(Face.Direction.Right, true);
                }

                AssignFacesDirection(Faces[(int)Face.Direction.Front][index], Face.Direction.Front);
                AssignFacesDirection(Faces[(int)Face.Direction.Back][index], Face.Direction.Back);
                AssignFacesDirection(Faces[(int)Face.Direction.Top][index], Face.Direction.Top);
                AssignFacesDirection(Faces[(int)Face.Direction.Bottom][index], Face.Direction.Bottom);

                break;
            case Axis.Y:
                temp = GetVerticalArray(Faces[(int)Face.Direction.Front], index);
                SetVerticalArray(Faces[(int)Face.Direction.Front], index, GetVerticalArray(Faces[(int)Face.Direction.Right], index));
                SetVerticalArray(Faces[(int)Face.Direction.Right], index, ReverseArray(GetVerticalArray(Faces[(int)Face.Direction.Back], index)));
                SetVerticalArray(Faces[(int)Face.Direction.Back], index, GetVerticalArray(Faces[(int)Face.Direction.Left],index));
                SetVerticalArray(Faces[(int)Face.Direction.Left], index, ReverseArray(temp));
                
                if (index == 0)
                {
                    RotateWholeSideFaces(Face.Direction.Bottom, true);
                }
                else if (index == boardSize - 1) 
                {
                    RotateWholeSideFaces(Face.Direction.Top, true);
                }

                AssignFacesDirection(GetVerticalArray(Faces[(int)Face.Direction.Front],index), Face.Direction.Front);
                AssignFacesDirection(GetVerticalArray(Faces[(int)Face.Direction.Back],index), Face.Direction.Back);
                AssignFacesDirection(GetVerticalArray(Faces[(int)Face.Direction.Left],index), Face.Direction.Left);
                AssignFacesDirection(GetVerticalArray(Faces[(int)Face.Direction.Right],index), Face.Direction.Right);

                break;
            case Axis.Z:
                temp = Faces[(int) Face.Direction.Left][index];
                Faces[(int) Face.Direction.Left][index] = GetVerticalArray(Faces[(int) Face.Direction.Top], index);
                SetVerticalArray(Faces[(int) Face.Direction.Top], index, ReverseArray(Faces[(int)Face.Direction.Right][index]));
                Faces[(int)Face.Direction.Right][index] = GetVerticalArray(Faces[(int) Face.Direction.Bottom], index);
                SetVerticalArray(Faces[(int) Face.Direction.Bottom], index, ReverseArray(temp));

                if (index == 0)
                {
                    RotateWholeSideFaces(Face.Direction.Front, false);
                }
                else if (index == boardSize - 1) 
                {
                    RotateWholeSideFaces(Face.Direction.Back, false);
                }

                AssignFacesDirection(GetVerticalArray(Faces[(int)Face.Direction.Top], index), Face.Direction.Top);
                AssignFacesDirection(GetVerticalArray(Faces[(int)Face.Direction.Bottom], index), Face.Direction.Bottom);
                AssignFacesDirection(Faces[(int)Face.Direction.Left][index], Face.Direction.Left);
                AssignFacesDirection(Faces[(int)Face.Direction.Right][index], Face.Direction.Right);

                break;
        }

    }



    private void RotateWholeSideFaces(Face.Direction faceDirection, bool reverse)
    {
        Face[][] original = new Face[boardSize][];
        for (int i = 0; i < boardSize; i++)
        {
            original[i] = new Face[boardSize];
        }

        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                original[i][j] = Faces[(int)faceDirection][i][j];
            }
        }
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (!reverse)
                {
                    Faces[(int)faceDirection][boardSize - 1 - j][i] = original[i][j];
                }
                else
                {
                    Faces[(int)faceDirection][j][boardSize - 1 - i] = original[i][j];
                }
            }
        }
    }

    private void CheckMatches(Axis axis, int index)
    {
        switch (axis)
        {
            case Axis.X:
                CheckMatches(Faces[(int)Face.Direction.Front][index]);
                CheckMatches(Faces[(int)Face.Direction.Back][index]);
                CheckMatches(Faces[(int)Face.Direction.Top][index]);
                CheckMatches(Faces[(int)Face.Direction.Bottom][index]);
                break;
            case Axis.Y:
                CheckMatches(GetVerticalArray(Faces[(int)Face.Direction.Front], index));
                CheckMatches(GetVerticalArray(Faces[(int)Face.Direction.Back], index));
                CheckMatches(GetVerticalArray(Faces[(int)Face.Direction.Left], index));
                CheckMatches(GetVerticalArray(Faces[(int)Face.Direction.Right], index));
                break;
            case Axis.Z:
                CheckMatches(GetVerticalArray(Faces[(int)Face.Direction.Top], index));
                CheckMatches(GetVerticalArray(Faces[(int)Face.Direction.Bottom], index));
                CheckMatches(Faces[(int)Face.Direction.Left][index]);
                CheckMatches(Faces[(int)Face.Direction.Right][index]);
                break;
        }
    }

    private void CheckMatches(Face[] faces)
    {
        foreach (Face f in faces) {
            List<Face> matches = new List<Face>();
            if (f.MatchNeighbours(matches))
            {
                foreach (Face m in matches)
                {
                    m.Colour(Color.white);
                    m.FaceMatched();
                    m.Cube.Destroy();
                }
            }
        }
    }

    private void AssignFacesDirection(Face[] faces, Face.Direction newDirection)
    {
        foreach (Face f in faces)
        {
            f.FaceDirection = newDirection;
        }
    }

    private T[] ReverseArray<T>(T[] array)
    {
        T[] tmp = (T[]) array.Clone();
        Array.Reverse(tmp);
        return tmp;
    }

    private T[] GetVerticalArray<T>(T[][] array, int vertIndex)
    {
        T[] tmp = new T[array.Length];
        for (int i = 0; i < tmp.Length; i++)
        {
            tmp[i] = array[i][vertIndex];
        }
        return tmp;
    }

    private void SetVerticalArray<T>(T[][] array, int vertIndex, T[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            array[i][vertIndex] = values[i];
        }
    }

    private void ReassignNeighbours(Axis axis, int index)
    {
        // TODO this is currently a naive implementation and could be improved to
        // only regenerate the faces which were actually changed
        spawner.GenerateFaceAdjacencies();
    }

    #region Cube mapping

    public GameObject MapCubesToAxis(Axis axis, int index, out Cube[] mappedChildrenCubes)
    {
        GameObject ax = Axes[(int)axis][index];
        SetParentAllCubes(axis, index, ax, out mappedChildrenCubes);
        return ax;
    }

    public void UnmapCubesFromAxis(Axis axis, int index, out Cube[] mappedChildrenCubes)
    {
        SetParentAllCubes(axis, index, gameObject, out mappedChildrenCubes);
    }

    private void SetParentAllCubes(Axis axis, int index, GameObject newParent, out Cube[] mappedChildrenCubes)
    {
        if (index == 0 || index == boardSize - 1)
        {
            mappedChildrenCubes = new Cube[boardSize * boardSize];
        }
        else
        {
            mappedChildrenCubes = new Cube[boardSize * 4 - 4];
        }

        int childIndex = 0;
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                Cube child = null;

                switch (axis)
                {
                    case Axis.X:
                        child = Cubes[index, i, j];
                        break;
                    case Axis.Y:
                        child = Cubes[i, index, j];
                        break;
                    case Axis.Z:
                        child = Cubes[i, j, index];
                        break;
                }

                if (child)
                {
                    mappedChildrenCubes[childIndex] = child;
                    childIndex++;

                    child.transform.parent = newParent.transform;
                }
            }
        }
    }

    #endregion

    public void UpdateCube(Vector3 index, Cube newCube)
    {
        Cubes[(int)index.x, (int)index.y, (int)index.z] = newCube;
    }


    internal void UpdateFace(int f, int x, int y, Face newFace)
    {
        Faces[f][x][y] = newFace;

        spawner.GenerateFaceAdjacencies(newFace);
        foreach (Face n in newFace.Neighbours)
        {
            spawner.GenerateFaceAdjacencies(n);
        }
    }


}
