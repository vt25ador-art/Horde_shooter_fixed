public static class GameStats
{
    public static int TotalKills;

    public static void ResetStats()
    {
        TotalKills = 0;
    }

    public static void AddKill()
    {
        TotalKills++;
    }
}