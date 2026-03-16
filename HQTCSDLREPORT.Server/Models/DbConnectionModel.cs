using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HQTCSDL.Models
{
    public class DbConnectionModel
    {
        [DefaultValue("POTATO-LAPTOP")]
        public string Server { get; set; }

        [DefaultValue("QLVT_DATHANG")]
        public string Database { get; set; }

        [DefaultValue("sa")]
        public string Username { get; set; }

        [DefaultValue("123")]
        public string Password { get; set; }
    }
}

