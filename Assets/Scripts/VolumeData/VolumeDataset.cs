﻿using System;
using UnityEngine;

[Serializable]
public class VolumeDataset
{
    public int[] data = null;
    public Texture3D texture = null;
    public int minDataValue;
    public int maxDataValue;
    public int dimX, dimY, dimZ;
}
