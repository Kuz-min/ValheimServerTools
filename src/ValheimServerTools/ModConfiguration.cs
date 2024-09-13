using BepInEx.Configuration;

namespace ValheimServerTools
{
    public interface IModConfiguration
    {
        ConfigEntry<int> PortOffset { get; }
    }


    static class BepInExConfigExtansion
    {
        private class ModConfiguration : IModConfiguration
        {
            public ConfigEntry<int> PortOffset { get; set; }
        }

        public static IModConfiguration Load(this ConfigFile config)
        {
            return new ModConfiguration
            {
                PortOffset = config.BindPortOffset(),
            };
        }
    }


    static class PortOffset
    {
        public const string SECTION = "General.WebServer";
        public const string KEY = "PortOffset";
        public const int DEFAULT_VALUE = 4;
        public const string DESCRIPTION = "Web server port offset relative to the game port. For example: default Valheim port is 2456, default PortOffset is 4 then web server port will be 2460";

        public static ConfigEntry<int> BindPortOffset(this ConfigFile config)
            => config.Bind(SECTION, KEY, DEFAULT_VALUE, new ConfigDescription(DESCRIPTION, new Validator()));

        private class Validator : AcceptableValueBase
        {
            public Validator() : base(typeof(int)) { }
            public override object Clamp(object value) => IsValid(value) ? value : DEFAULT_VALUE;
            public override bool IsValid(object value) => value is int v && v != 0 && v != 1;
            public override string ToDescriptionString() => "# Values 0 and 1 is invalid";
        }
    }
}
