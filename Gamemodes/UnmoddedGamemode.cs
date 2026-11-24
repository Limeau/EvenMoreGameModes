namespace EvenMoreGameModes
{
    public class UnmoddedGamemode : CustomGamemode
    {
        public UnmoddedGamemode()
        {
            Gamemode = Gamemodes.None;
            PetAction = false;
            DisableTasks = false;
        }
        public static UnmoddedGamemode instance;
    }
}