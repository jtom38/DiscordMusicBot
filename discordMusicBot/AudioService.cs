using Discord;
using Discord.Commands;


namespace discordMusicBot
{
    internal class AudioService
    {
        private DependencyMap map;
        private CommandService _commands;
        private AudioService _audioService;

        public AudioService(CommandService commands, DependencyMap map)
        {
            this.map = map;

        }


    }
}