namespace Client.ServerData
{
    public class GroupInfo
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string RepeatPassword { get; set; }

        public bool Validate()
        {
            //TODO: Przyśpieszyć i polepszyć o sprawdzanie "jakości"
            if (string.IsNullOrEmpty(Name))
            {
                return false;
            }
            if (string.IsNullOrEmpty(Password))
            {
                return false;
            }
            if (string.IsNullOrEmpty(RepeatPassword))
            {
                return false;
            }
            if (!Password.Equals(RepeatPassword))
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
