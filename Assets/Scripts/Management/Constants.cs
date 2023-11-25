using UnityEngine;

public class Constants
{
    // The main spot where we can define constants to use in our scripts
    public const int MAX_PLAYERS = 4;
    public const int MAX_REQUIRED_READY_UP = 2;
    public const bool REQUIRE_MAX_PLAYERS = false;
    public const bool SPAWN_MID_MATCH = false;

    // Distance calculations
    public enum DistanceType { Meters, Feet};
    public const DistanceType DISTANCE_TYPE = DistanceType.Feet;
    public const int DISTANCE_TO_FADE = 10;

}
