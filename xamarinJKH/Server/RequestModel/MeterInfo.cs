using System.Collections.Generic;

namespace xamarinJKH.Server.RequestModel
{
    public class MeterInfo
    {
        // Номер лицевого счета
        public string Ident { get; set; }
        // ID прибора
        public int ID { get; set; }
        // Заводской номер
        public string FactoryNumber { get; set; }
        public string NextCheckupDate { get; set; }
        public string CustomName  { get; set; }
        // Дата последней поверки
        public string LastCheckupDate { get; set; }
        // Межповерочный интервал
        public int RecheckInterval { get; set; }
        // Ресурс
        public string Resource { get; set; }
        // Адрес
        public string Address { get; set; }
        // Уникальный номер
        public string UniqueNum { get; set; }
        // Количество знаков после запятой
        public int NumberOfDecimalPlaces { get; set; }
        // Показания
        public List<MeterValueInfo> Values { get; set; }
        // отключен
        public bool IsDisabled { get; set; }

        public int ValuesStartDay { get; set; }
        public int ValuesEndDay { get; set; }
        public bool ValuesCanAdd { get; set; }
        public bool ValuesPeriodStartIsCurrent { get; set; }
        
        public bool ValuesPeriodEndIsCurrent { get; set; }

        // ед измерения
        public string Units { get; set; }
        public bool AutoValueGettingOnly { get; set; }
        public string PeriodMessage { get; set; }
        //тарифов число, строкой 
        public string TariffNumber { get; set; }
        //тарифов число, числом
        public int TariffNumberInt { get; set; }
    // Название тарифа 1
    public string Tariff1Name { get; set; }
    // Название тарифа 2
    public string Tariff2Name { get; set; }
    // Название тарифа 3
    public string Tariff3Name { get; set; }
        public int NumberOfIntegerPart { get; set; }
    }

    public class MeterValueInfo
    {
        // Дата
        public string Period { get; set; }
        // Значение (Т1 для 2х 3х тарифныз)
        public bool IsCurrentPeriod  { get; set; }
        public decimal Value { get; set; }
        // Значение Т2
        public decimal? ValueT2 { get; set; }
        // Значение Т3
        public decimal? ValueT3 { get; set; }
    }
}