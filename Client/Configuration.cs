namespace Client
{
    class Configuration
    {
        private static Configuration _instance;
        
        private Configuration()
        {
            IP = "127.0.0.1";
            Port = 12345;

            ApiKey = "grew37lbe3smob4";
            AppSecret = "dbtvg6rym3qbg2l";
            AccessToken = "q7RSg2cm1vAAAAAAAAAAC7sy1AfF2zsSStdhuG0KdJs3ieupiQ6A2Izek-5r8DE-";

            UsersListFileName = "list.xml";
            UsersNodeName = "Users";
            UserNodeName = "User";
            UserIdAttributeName = "guid";
            UserCommonNameAttributeName = "nick";
            UserEmailAttributeName = "mail";

            GroupListFileName = "group.xml";
            GroupsNodeName = "Groups";
            GroupNodeName = "Group";
            GroupNameAttributeName = "name";
            GroupPasswordAttributeName = "password";
        }

        public string IP { get; private set; }
        public int Port { get; private set; }

        public string ApiKey { get; private set; }
        public string AppSecret { get; private set; }
        public string AccessToken { get; private set; }

        public string UsersListFileName { get; private set; }
        public string UsersNodeName { get; private set; }
        public string UserNodeName { get; private set; }
        public string UserIdAttributeName { get; private set; }
        public string UserCommonNameAttributeName { get; private set; }
        public string UserEmailAttributeName { get; private set; }

        public string GroupListFileName { get; private set; }
        public string GroupsNodeName { get; private set; }
        public string GroupNodeName { get; private set; }
        public string GroupNameAttributeName { get; private set; }
        public string GroupPasswordAttributeName { get; private set; }

        public static Configuration Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Configuration();
                }
                return _instance;
            }
        }
    }
}
