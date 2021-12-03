// Top level game states

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerState{

    private const int START_OXYGEN = 500;
    private const int START_HEAT = 1000;
    private const int START_POWER = 1000;

    public enum ResourceType {
        Oxygen,
        Heat,
        Power
    }

    public static class Resources {
        public static int Oxygen = START_OXYGEN; 
        public static int Heat = START_HEAT;
        public static int Power = START_POWER;
    };

    public static class State {
        public static int Level = 1;
        public static bool GameRunning = false;
        public static bool Alive = false;
        public static bool easyMode = false;
    }

    static PlayerState()
    {

    }

    public static void resetResources(){
        Resources.Oxygen = START_OXYGEN; 
        Resources.Heat = START_HEAT;
        Resources.Power = START_POWER;
    }
}
