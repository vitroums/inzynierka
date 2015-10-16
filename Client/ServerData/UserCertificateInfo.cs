using System.Text;

namespace Client.ServerData
{
    public class UserCertificateInfo
    {
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Organization { get; set; }
        public string Unit { get; set; }
        public string CommonName { get; set; }
        public string Email { get; set; }

        public bool Validate()
        {
            //TODO: Przyśpieszyć i polepszyć o sprawdzanie "jakości"
            if (string.IsNullOrEmpty(Country))
            {
                return false;
            }
            if (string.IsNullOrEmpty(State))
            {
                return false;
            }
            if (string.IsNullOrEmpty(City))
            {
                return false;
            }
            if (string.IsNullOrEmpty(Organization))
            {
                return false;
            }
            if (string.IsNullOrEmpty(Unit))
            {
                return false;
            }
            if (string.IsNullOrEmpty(CommonName))
            {
                return false;
            }
            if (string.IsNullOrEmpty(Email))
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            var userCertificateInfoBuilder = new StringBuilder();
            userCertificateInfoBuilder.Append(Country);
            userCertificateInfoBuilder.Append(";");
            userCertificateInfoBuilder.Append(State);
            userCertificateInfoBuilder.Append(";");
            userCertificateInfoBuilder.Append(City);
            userCertificateInfoBuilder.Append(";");
            userCertificateInfoBuilder.Append(Organization);
            userCertificateInfoBuilder.Append(";");
            userCertificateInfoBuilder.Append(Unit);
            userCertificateInfoBuilder.Append(";");
            userCertificateInfoBuilder.Append(CommonName);
            return userCertificateInfoBuilder.ToString();
        }
    }
}
