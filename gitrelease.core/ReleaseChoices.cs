﻿namespace gitrelease.core
{
    public class ReleaseChoices
    {
        public ReleaseType ReleaseType { get; set; }
    }

    public enum ReleaseType
    {
        Major,
        Minor,
        Patch,
        Custom
    }
}