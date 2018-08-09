namespace angleintegration
{
    public struct language_string
    {
        public readonly string sheetTitle;
        public readonly string key;
        public readonly string value;
        public readonly int priority;
        public readonly bool allowOverride;

        public language_string(string sheetTitle, string key, string value,
            bool allowOverride = false, int priority = 0)
        {
            this.sheetTitle = sheetTitle;
            this.key = key;
            this.value = value;
            this.allowOverride = allowOverride;
            this.priority = priority;
        }
    }

    public struct player_bool
    {
        public readonly string boolKey;
        public readonly int priority;
        public readonly bool allowOverride;
        public readonly bool state;

        public player_bool(string key, bool state, bool allowOverride = false, int priority = 0)
        {
            this.boolKey = key;
            this.state = state;
            this.allowOverride = allowOverride;
            this.priority = priority;
        }
    }

    public struct player_int
    {
        public readonly string intKey;
        public readonly int priority;
        public readonly bool allowOverride;
        public readonly int value;

        public player_int(string key, int value, bool allowOverride = false, int priority = 0)
        {
            this.intKey = key;
            this.value = value;
            this.allowOverride = allowOverride;
            this.priority = priority;
        }
    }
    
    
    
    public class message_parsing
    {
        
        
        
    }
}