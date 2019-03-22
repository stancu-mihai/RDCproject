using System;

namespace Classes
{
    public class Transmission
    {
        public string nickname { get; set; }
        public string message { get; set; }
        public DateTime time { get; set; }

        public Transmission(string nick, string mess, DateTime date)
        {
            this.nickname = nick;
            this.message = mess;
            this.time = date;
        }
    }
}