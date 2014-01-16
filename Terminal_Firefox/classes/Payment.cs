namespace Terminal_Firefox.classes {
    class Payment {
        public long id { get; set; }
        public short id_uslugi { get; set; }
        public string nomer { get; set; }
        public string nomer2 { get; set; } //Sms information
        public double summa { get; set; }
        public double summa_zachis { get; set; }
        public long status { get; set; }
        public string date_create { get; set; }
        public string date_send { get; set; }
        public string id_inkas { get; set; }
        public int val1 { get; set; }
        public int val3 { get; set; }
        public int val5 { get; set; }
        public int val10 { get; set; }
        public int val20 { get; set; }
        public int val50 { get; set; }
        public int val100 { get; set; }
        public int val200 { get; set; }
        public int val500 { get; set; }
        public string hash_id { get; set; }
        public string checkn { get; set; }
        public double summa_komissia { get; set; }
        public double rate { get; set; }
        public string curr { get; set; }
    }
}
