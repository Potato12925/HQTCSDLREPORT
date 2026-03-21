using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HQTCSDL.Models
{
    public class DbConnectionModel
    {
        [DefaultValue("(localdb)\\MSSQLLocalDB")]   
        public string Server { get; set; }
        [DefaultValue("QLVT_DATHANG")]
        public string Database { get; set; }

    }
}

