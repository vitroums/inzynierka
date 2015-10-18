namespace Client.ClientData
{
    public class UserInfo
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
