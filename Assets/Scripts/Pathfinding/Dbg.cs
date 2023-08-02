namespace Pathfinding
{
    public class Dbg
    {
        public static bool EnableDebugs { get; set; } = true;
        
        public static void Red(string message)
        {
                Log("<color=red>" + message + "</color>");
        }
        
        public static void Green(string message)
        {
            Log("<color=green>" + message + "</color>");
        }
        
        public static void Blue(string message)
        {
            Log("<color=blue>" + message + "</color>");
        }
        
        public static void Yellow(string message)
        {
            Log("<color=yellow>" + message + "</color>");
        }

        public static void LogException(string message)
        {
            Log("<color=red> " + "CAUGHT: " + message + "</color>");
        }
        
        public static void LogStars(string message)
        {
            Log( "*****" + message);
        }

        public static void Log(string message)
        {
            if(EnableDebugs)
                UnityEngine.Debug.Log($"{message}");
        }
    }
}