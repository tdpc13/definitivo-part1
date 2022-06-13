using ASPNETIdentityManager.Contexts;
using ASPNETIdentityManager.Entities;
using System.Collections.Generic;
using System.Linq;

namespace ASPNETIdentityManager.DB
{
    public class Repository
    {
        private PrenotazioneDBContext DBContext;

        public Repository(PrenotazioneDBContext DBContext)
        {
            this.DBContext = DBContext;
        }
        public List<Prenotazione> GetPrenotazioni()
        {
            //select * from prenotazione
            List<Prenotazione> result = this.DBContext.Prenotazione.ToList();  //nome della tebella

            return result;
        }
    }
}
