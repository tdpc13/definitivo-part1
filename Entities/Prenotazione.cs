using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASPNETIdentityManager.Entities
{
    public class Prenotazione
    {
        [NotMapped]
        public string User { get; set; }
        public int Id { get; set; }
        public Guid IdUser { get; set; }
        public string Dal { get; set; }
        public string Al { get; set; }
        public int Persone { get; set; }
        public string Pacchetto { get; set; }

    }
}
