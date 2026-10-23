using System;
using System.Collections.Generic;

namespace MKtest.Checklists
{
    /// <summary>
    /// Editable cover/header fields. Stored with the inspection run, independent
    /// from individual test-case definitions or the editable master catalog.
    /// </summary>
    public sealed class ChecklistPrintHeader
    {
        public bool Detailed { get; set; } = true;
        public string Project { get; set; } = "МК Android";
        // Additional hardware/environment details; platform comes from RunPlatform.
        public string EnvironmentDetails { get; set; } =
            "встроенный плеер медиапанели, TVS GPS";
        public string Purpose { get; set; } =
            "Проверка работоспособности и надежности функций ПО в медиакомплексе";

        public string Widgets { get; set; } =
            "Дата/время, погода, курс валют, QR-код";
        public string Announcers { get; set; } = "ЛК-ВИЗ и ИР-0652";
        public string Media { get; set; } = "Воспроизведение видео, аудио";
        public string ServiceData { get; set; } =
            "Команды МЧС, статистика неплательщиков";
        public string PaymentProtocols { get; set; } =
            "СЭКОП, JSON-RPC, HTTP";
        public string Reliability { get; set; } =
            "Устойчивость к потере сети и питания";

        public string EnvironmentSetup { get; set; } =
            "Создание и конфигурация локального сервера, программная настройка тестовых маршрутов для автоинформаторов ЛК-ВИЗ и ИР-0652";
        public string ProtocolOperations { get; set; } =
            "Формирование, отправка и парсинг запросов JSON-RPC, СЭКОП и HTTP для выполнения транзакционных операций";
        public string DataValidation { get; set; } =
            "Проверка контрольных сумм в файле, проверка порядка байтов";

        public ChecklistPrintHeader Clone() => new()
        {
            Detailed = Detailed,
            Project = Project,
            EnvironmentDetails = EnvironmentDetails,
            Purpose = Purpose,
            Widgets = Widgets,
            Announcers = Announcers,
            Media = Media,
            ServiceData = ServiceData,
            PaymentProtocols = PaymentProtocols,
            Reliability = Reliability,
            EnvironmentSetup = EnvironmentSetup,
            ProtocolOperations = ProtocolOperations,
            DataValidation = DataValidation
        };

        public IReadOnlyList<(string Caption, string Value)> ScopeRows() =>
            new (string, string)[]
            {
                ("Базовые виджеты", Widgets),
                ("Автоинформаторы", Announcers),
                ("Медиа", Media),
                ("Служебные данные", ServiceData),
                ("Протоколы оплаты", PaymentProtocols),
                ("Надежность", Reliability)
            };

        public IReadOnlyList<(string Caption, string Value)> ToolRows() =>
            new (string, string)[]
            {
                ("Настройка тестового окружения", EnvironmentSetup),
                ("Работа с прикладными протоколами", ProtocolOperations),
                ("Валидация данных", DataValidation)
            };
    }
}
