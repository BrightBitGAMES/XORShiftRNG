///////////////////////////////////////////////////////////
//                                                       //
//  This file is part of BrightBit's XORShiftRNG.        //
//                                                       //
//  Copyright (c) 2016 by BrightBit                      //
//                                                       //
//  https://github.com/BrightBitGames/XORShiftRNG        //
//                                                       //
//  This software may be modified and distributed under  //
//  the terms of the MIT license. See the LICENSE file   //
//  for details.                                         //
//                                                       //
///////////////////////////////////////////////////////////

using System;

namespace BrightBit
{

/// <summary>
/// A pseudo random number generator for uniformly distributed random numbers as described by George Marsaglia.
/// See https://www.jstatsoft.org/article/view/v008i14/xorshift.pdf for more details.
/// </summary>
public class XORShiftRNG
{
    protected const uint POSITIVE_INTEGER_BITMASK = 0x7FFFFFFF; // most significant bit is missing to avoid negative values

    // the following constants basically represent divisions by a certain maximum value
    protected const double MAX_INT_DIVISION_INCLUSIVE  = 1.0 / ((double) int.MaxValue);
    protected const double MAX_INT_DIVISION_EXCLUSIVE  = 1.0 / ((double) int.MaxValue  + 1.0);
    protected const double MAX_UINT_DIVISION_INCLUSIVE = 1.0 / ((double) uint.MaxValue);
    protected const double MAX_UINT_DIVISION_EXCLUSIVE = 1.0 / ((double) uint.MaxValue + 1.0);

    static readonly uint[] InitialState = new uint[] { 0, 3579545447, 340436397, 842436295 };
    static readonly uint   LastIndex    = (uint) InitialState.Length - 1;

    uint[] state = new uint[InitialState.Length];

    static long Uniquifier = 0;

    public XORShiftRNG()
    {
        SetSeed(Environment.TickCount + Uniquifier++);
    }

    public XORShiftRNG(int seed)
    {
        SetSeed(seed);
    }

    public void SetSeed(int seed)
    {
        unchecked
        {
            state[0] = (uint) seed;
        }

        for (int i = 1; i < state.Length; ++i) state[i] = InitialState[i];
    }

    public void SetSeed(long seed)
    {
        int a = (int)(seed & uint.MaxValue);
        int b = (int)(seed >> 32);

        SetSeed(a, b);
    }

    public void SetSeed(int seed1, int seed2)
    {
        unchecked
        {
            state[0] = (uint) seed1;
            state[1] = (uint) seed2;
        }

        for (int i = 2; i < state.Length; ++i) state[i] = InitialState[i];
    }

    public void SetSeed(int seed1, int seed2, int seed3)
    {
        unchecked
        {
            state[0] = (uint) seed1;
            state[1] = (uint) seed2;
            state[2] = (uint) seed3;
        }

        for (int i = 3; i < state.Length; ++i) state[i] = InitialState[i];
    }

    /// <summary>
    /// Generates a random int between 0 (inclusive) and int.MaxValue (inclusive).
    /// </summary>
    public int NextInt()
    {
        return (int) (POSITIVE_INTEGER_BITMASK & NextRandomBits());
    }

    /// <summary>
    /// Generates a random int between 0 (inclusive) and max (exclusive).
    /// </summary>
    public int NextInt(int max)
    {
        if (max < 0) throw new ArgumentOutOfRangeException("max", max, "'max' must not be smaller than 0");

        return (int) ((MAX_INT_DIVISION_EXCLUSIVE * NextInt()) * max);
    }

    /// <summary>
    /// Generates a random int between 'min' (inclusive) and 'max' (exclusive).
    /// </summary>
    public int Range(int min, int max)
    {
        if (min > max) throw new ArgumentOutOfRangeException("min", min, "'min' must not be larger than 'max'");

        return min + (int) ((MAX_UINT_DIVISION_EXCLUSIVE * (double) NextRandomBits()) * (double)((long) max - (long) min));
    }

    /// <summary>
    /// Generates a random double between 0.0 (inclusive) and 1.0 (exclusive).
    /// </summary>
    public double NextDouble()
    {
        return (MAX_INT_DIVISION_EXCLUSIVE * NextInt());
    }

    /// <summary>
    /// Generates a random double between 'min' (inclusive) and 'max' (exclusive).
    /// </summary>
    public double Range(double min, double max)
    {
        if (min > max) throw new ArgumentOutOfRangeException("min", min, "'min' must not be larger than 'max'");

        return min + (double) ((MAX_UINT_DIVISION_EXCLUSIVE * (double) NextRandomBits()) * (max - min));
    }

    /// <summary>
    /// Generates random bytes to fill them into the user-supplied byte array.
    /// </summary>
    public void NextBytes(byte[] bytes)
    {
        int i = 0;

        while (i < bytes.Length - 3)
        {
            uint randomBits = NextRandomBits();

            bytes[i++] = (byte) randomBits;
            bytes[i++] = (byte)(randomBits >>  8);
            bytes[i++] = (byte)(randomBits >> 16);
            bytes[i++] = (byte)(randomBits >> 24);
        }

        if (i < bytes.Length)
        {
            uint randomBits = NextRandomBits();

            bytes[i++] = (byte) randomBits;

            if (i < bytes.Length) bytes[i++] = (byte)(randomBits >>  8);
            if (i < bytes.Length) bytes[i++] = (byte)(randomBits >> 16);
            if (i < bytes.Length) bytes[i]   = (byte)(randomBits >> 24);
        }
    }

    /// <summary>
    /// Generates a random bool.
    /// </summary>
    public bool NextBool()
    {
        return (NextRandomBits() & 0x1) == 0;
    }

    protected uint NextRandomBits()
    {
        uint tmp = (state[0] ^ (state[0] << 11));

        for (int i = 0; i < LastIndex; ++i) state[i] = state[i + 1];

        state[LastIndex] = (state[LastIndex] ^ (state[LastIndex] >> 19)) ^ (tmp ^ (tmp >> 8));

        return state[LastIndex];
    }
}

} // of namespace BrightBit
