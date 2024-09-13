namespace ValheimServerTools
{
    class ValheimCommandLineArgs
    {
        public int? GamePort { get; set; } = null;
        public int? WebServerPort { get; set; } = null;

        private ValheimCommandLineArgs() { }

        public static ValheimCommandLineArgs Parse(string[] arguments)
        {
            var args = new ValheimCommandLineArgs();

            for (int i = 0; i < arguments.Length; i++)
            {
                var arg = arguments[i].ToLower();
                switch (arg)
                {
                    case "-port":
                        if (i + 1 < arguments.Length && int.TryParse(arguments[i + 1], out var gamePort))
                        {
                            args.GamePort = gamePort;
                            i++;
                        }
                        break;
                    case "-webserverport":
                        if (i + 1 < arguments.Length && int.TryParse(arguments[i + 1], out var webServerPort))
                        {
                            args.WebServerPort = webServerPort;
                            i++;
                        }
                        break;
                    default:
                        break;
                }
            }

            return args;
        }
    }
}
