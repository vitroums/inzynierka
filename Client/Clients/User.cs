using Client.Certificates;

namespace Client.Clients
{
    public class User
    {
        public string ID { get; set; }
        public string CommonName { get; set; }
        public string Email { get; set; }

        public User(UserCertificate certificate)
        {
            ID = certificate.ID;
            CommonName = certificate.CommonName;
            Email = certificate.Email;
        }

        public User(string id, string commonName, string email)
        {
            ID = id;
            CommonName = commonName;
            Email = email;
        }

        public static implicit operator User(UserCertificate certificate)
        {
            return new User(certificate);
        }

        public static bool operator ==(User left, User right)
        { 
            return left.ID == right.ID;
        }

        public static bool operator !=(User left, User right)
        {
            return left.ID != right.ID;
        }

        public override string ToString()
        {
            return CommonName;
        }
    }
}
