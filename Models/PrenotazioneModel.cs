using ASPNETIdentityManager.Entities;
using System;
using System.Collections.Generic;

namespace ASPNETIdentityManager.Models
{
    public class PrenotazioneModel
    {
        public List<Prenotazione> Prenotazione { get; set; } = new List<Prenotazione>();
        public int Id { get; set; }
        public Guid IdUser{ get; set; }
        public string Dal { get; set; }
        public string Al { get; set; }
        public int Persone { get; set; }
        public string Pacchetto { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
