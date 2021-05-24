using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Realms;
using xamarinJKH.Server.RequestModel;
using RestSharp;
using Xamarin.Essentials;
using Xamarin.Forms;
using xamarinJKH.Server.DataModel;
using xamarinJKH.Utils;
using System.Linq;

namespace xamarinJKH.Server
{
    public class RestClientMP
    {
        // public const string SERVER_ADDR = "https://api.sm-center.ru/test_erc_udm"; // ОСС
         // public const string SERVER_ADDR = "https://api.sm-center.ru/komfortnew"; // Гранель
        public const string SERVER_ADDR = "https://api.sm-center.ru/water2"; // Тихая гавань water/ water2 - тихая гавань - 2 
         // public const string SERVER_ADDR = "https://api.sm-center.ru/komfortnew"; // Гранель
        // public const string SERVER_ADDR = "https://api.sm-center.ru/kapitall_all"; // Основа
        //public const string SERVER_ADDR = "https://api.sm-center.ru/newjkh"; // Еще одна тестовая база
        // public const string SERVER_ADDR = "https://api.sm-center.ru/dgservicnew"; // Домжил (дом24)
        // public const string SERVER_ADDR = "https://api.sm-center.ru/UKUpravdom"; //Управдом Чебоксары
        // public const string SERVER_ADDR = "https://api.sm-center.ru/uk_sibir_alians"; //Альянс
         //public const string SERVER_ADDR = "https://api.sm-center.ru/ooo_yegkh"; //Легкая жизнъ
        // public const string SERVER_ADDR = "https://api.sm-center.ru/vodokanal_narof"; // Водоканал
        //public const string SERVER_ADDR = "https://api.sm-center.ru/uk_egokomfort"; // Эгокомфорт
        // public const string SERVER_ADDR = "https://api.sm-center.ru/tsg_sivtsev_vrazhek14"; // ТСЖ Сивцев Вражек 14
        // public const string SERVER_ADDR = "https://api.sm-center.ru/ooo_zip"; // ЗИП
        // public const string SERVER_ADDR = "https://api.sm-center.ru/ukom"; // УК Огни Москвы
        //public const string SERVER_ADDR = "https://api.sm-center.ru/tsg_svyato-troitskii15"; // УК Свято троицк
        //public const string SERVER_ADDR = "https://api.sm-center.ru/ooo_uk_rks"; // УК РКС
        //public const string SERVER_ADDR = "https://api.sm-center.ru/profikomfort"; // Профи комфорт
        //public const string SERVER_ADDR = "https://api.sm-center.ru/tafgai"; // Тафгай
        //public const string SERVER_ADDR = "https://api.sm-center.ru/eco_lk"; // Авалон эко
        //public const string SERVER_ADDR = "https://api.sm-center.ru/ci_lk"; // Центр инвестиций
        // public const string SERVER_ADDR = "https://api.sm-center.ru/ur_lk"; // Универсальные решения
        // public const string SERVER_ADDR = "https://api.sm-center.ru/chg_lk/"; // Чистый город

        //public const string SERVER_ADDR = "https://api.sm-center.ru/tsgopaliha/"; // Новая Опалиха
        //public const string SERVER_ADDR = "https://api.sm-center.ru/sklider/"; // Мобильный Мир
        //public const string SERVER_ADDR = "https://api.sm-center.ru/avalon_alfagkh/"; // Альфа ЖКХ
        // public const string SERVER_ADDR = "https://api.sm-center.ru/stolitsa/"; // Жилищник столица

        //public const string SERVER_ADDR = "https://api.sm-center.ru/grinvay/"; // Грин-Вэй Сочи
        //public const string SERVER_ADDR = "https://api.sm-center.ru/ikon/"; // Айкон

        // public const string SERVER_ADDR = "https://api.sm-center.ru/mup_kc/"; // МУП КС г. Новочебоксарска
        //public const string SERVER_ADDR = "https://api.sm-center.ru/ooo_cdo/"; // ООО ЦДО г.Тверь

        //public const string SERVER_ADDR = "https://api.sm-center.ru/vestaesteit/"; // Веста Эстейт
        //public const string SERVER_ADDR = "https://api.sm-center.ru/uk_divnomorskoe/"; // Дивноморское
        //public const string SERVER_ADDR = "https://api.sm-center.ru/ooo_interkapstroy/"; // ИнтерКапСтрой

        //public const string SERVER_ADDR = "https://api.sm-center.ru/suhanovo_park/"; // Суханово Парк

        //public const string SERVER_ADDR = "https://api.sm-center.ru/stroim-bud/"; // Строим будущее
        // public const string SERVER_ADDR = "https://api.sm-center.ru/tsg_novaya_zvezda/"; // Новая Звезда
        //public const string SERVER_ADDR = "https://api.sm-center.ru/vestsnab_xml/"; // ВестСнаб
        //public const string SERVER_ADDR = "https://api.sm-center.ru/tsg_saburova/"; // Сабурово
        //public const string SERVER_ADDR = "https://api.sm-center.ru/tsg_krasnoarmeiskii12/"; // ТСЖ Красноармейская 12
        // public const string SERVER_ADDR = "https://api.sm-center.ru/ooo_lider_ulan_ude"; // КТК Galaxy



        public const string SEND_TEACH_MAIL = "Public/TechSupportAppeal"; // Создание обращения в тех поддержк
        public const string GET_PERSONAL_POLICY = "Public/MobilePersonalDataPolicy"; // Создание обращения в тех поддержк
        public const string LOGIN_DISPATCHER = "auth/loginDispatcher"; // Аутентификация сотрудника
        public const string LOGIN = "auth/Login"; // Аутентификация пользователя
        public const string REQUEST_CODE = "auth/RequestAccessCode"; // Запрос кода подтверждения
        public const string REQUEST_CHECK_CODE = "auth/CheckAccessCode"; // Подтверждение кода подтверждения
        public const string REGISTR_BY_PHONE = "auth/RegisterByPhone"; // Регистрация по телефону
        public const string SEND_CHECK_CODE = "auth/SendCheckCode"; // Запрос проверочного кода
        public const string SEND_CHECK_CODE_WHATSAPP = "auth/SendCheckCodeWhatsApp"; // Запрос проверочного кода whatsapp
        public const string VALIDATE_CHECK_CODE = "auth/ValidateCheckCode "; // Проверка кода из смс

        public const string GET_MOBILE_SETTINGS = "Config/MobileAppSettings"; // Регистрация по телефону
        public const string GET_EVENT_BLOCK_DATA = "Common/EventBlockData"; // Блок события

        public const string GET_PHOTO_ADDITIONAL = "AdditionalServices/logo"; // Картинка доп услуги
        public const string GET_PHOTO_ADDITIONAL_DOP = "AdditionalServices/DescriptionImage"; // Картинка доп услуги
        public const string GET_ACCOUNTING_INFO = "Accounting/Info"; // инфомация о начислениях
        public const string GET_CHECH = "Accounting/Check"; // инфомация о начислениях
        public const string GET_SUM_COMISSION = "Accounting/SumWithComission"; // Возвращает сумму с комиссией

        public const string
            GET_POINS_BALANCE = "Accounting/GetAccountBonusBalance "; // Возвращает бонусный баланс лицевого счета

        public const string GET_FILE_BILLS = "Bills/Download"; // Получить квитанцию

        public const string REGISTR_DISPATCHER_DEVICE = "Dispatcher/RegisterDevice"; //Регистрация устройства.

        public const string REQUEST_LIST_CONST = "RequestsDispatcher/List"; // Заявки сотрудника
        public const string REQUEST_DETAIL_LIST_CONST = "RequestsDispatcher/Details"; // Детали заявки сотрудника
        public const string REQUEST_UPDATES_CONST = "RequestsDispatcher/GetUpdates"; // Обновление заявок сотрудника
        public const string CLOSE_APP_CONST = "RequestsDispatcher/Close "; // Закрытие заявки
        public const string CLOSE_APP_FOR_USER = "RequestsDispatcher/CancelRequestByUser "; // Закрытие заявки
        public const string LOCK_APP_CONST = "RequestsDispatcher/Lock "; // Прием заявки к работе
        public const string PERFORM_APP_CONST = "RequestsDispatcher/Perform "; // Выполнение заявки
        public const string GET_ALL_NEWS = "Common/AllNews";
        public const string
            CHANGE_DISPATCHER_CONST = "RequestsDispatcher/ChangeDispatcher "; // Перевод заявки диспетчеру

        public const string DISPATCHERS_LIST_CONST = "RequestsDispatcher/DispatchersList "; // Список диспетчеров
        public const string GET_TYPE_CONST = "RequestsDispatcher/RequestTypes"; // Получение типов заявок
        public const string NEW_APP_CONST = "RequestsDispatcher/New"; // Добавление заявки
        public const string GET_FILE_APP_CONST = "RequestsDispatcher/File"; // Получение файлов
        public const string ADD_FILE_CONST = "RequestsDispatcher/AddFile "; // Отправка файла
        public const string ADD_MESSAGE_CONST = "RequestsDispatcher/AddMessage"; // Отправка сообщения
        public const string GET_HOUSES_GROUP = "RequestsDispatcher/HouseGroups"; // Возвращает список районов
        public const string GET_HOUSES = "RequestsDispatcher/Houses"; // Возвращает список домов. 
        public const string GET_HOUSE_DATA = "RequestsDispatcher/HouseData"; // Возвращает список домов. 
        public const string CLOSE_APP_LIST_DISP = "RequestsDispatcher/CloseList"; // Закрытие списка заявки. 
        public const string PERFORM_APP_LIST_DISP = "RequestsDispatcher/PerformList"; // Закрытие списка заявки. 
        public const string SET_READED_APP = "RequestsDispatcher/SetReadedFlag"; // Метод, который устанавливает что заявка прочитана сотрудником
        public const string CREATE_PUSH = "Dispatcher/NewAnnouncement"; //создание уведомления
        public const string SEND_PUSH = "Dispatcher/SendAnnouncement"; //создание уведомления
        public const string GET_AREA_GROUPS = "RequestsDispatcher/GroupsOfDistrincts"; //Получение групп районов
        public const string REQUEST_MULTIPLE = "RequestsDispatcher/RequestStatsMultiple"; //Запрос по нескольким ID
        public const string GET_REQUEST_STATUSES = "RequestsDispatcher/RequestStatuses"; //- получение статусов заявок
        public const string GET_REQUEST_PRIORITETS = "RequestsDispatcher/RequestPriorities"; //- получение статусов заявок
        public const string FILTR_REQUESTS = "RequestsDispatcher/Search "; //- поиск заявок по критериям
        public const string CHANGE_MESS_VISIBILITY = "RequestsDispatcher/ChangeMessageVisibility"; //- Метод для смены флага видимости сообщения
        
        
        
        
            //Методы для работы диспетчера с заявками
        public const string CHANGE_CONSULTANT = "SupportService/ChangeConsultant"; //Перевод заявки. 
        public const string GET_CONSULTANTS = "SupportServiceData/GetConsultants"; //Возвращает список консультантов с подразделениями. Если указан houseID - отфильтровынных по дому. Входные параметры: 
        public const string GET_REQUEST_POOL = "SupportServiceData/GetRequestPools"; //Возвращает список подразделений. Возвращаемый результа
        public const string GET_DEPATMENTS = "SupportServiceData/GetDepartments"; //Возвращает список подразделений. Возвращаемый результа



        public const string
            GET_REQUESTS_STATS = "RequestsDispatcher/RequestStats"; // Возвращает статистику по заявкам.  


        public const string REQUEST_LIST = "Requests/List"; // Заявки
        public const string REQUEST_DETAIL_LIST = "Requests/Details"; // Детали заявки
        public const string REQUEST_UPDATES = "Requests/GetUpdates"; // Обновление заявок
        public const string ADD_MESSAGE = "Requests/AddMessage"; // Отправка сообщения
        public const string ADD_FILE = "Requests/AddFile "; // Отправка файла
        public const string NEW_APP = "Requests/New"; // Добавление заявки
        public const string GET_TYPE = "Requests/RequestTypes"; // Получение типов заявок
        public const string GET_FILE_APP = "Requests/File"; // Получение файлов
        public const string CLOSE_APP = "Requests/Close "; // Закрытие заявки
        public const string PAY_CASH_VALIDATE_CODE = "Requests/SendPaidRequestCompleteCodeCash"; // Отправка пин-кода в SMS по платной заявке после онлайн оплаты
        public const string PAY_CARD_VALIDATE_CODE = "Requests/SendPaidRequestCompleteCodeCash"; // Отправка пин-кода в SMS по платной заявке после оплаты наличными
        public const string GET_CAR_BRAND = "Requests/VehicleMarks"; // Возвращает список марок автомобилей 
        public const string UPDATE_PROFILE_CONST = "Dispatcher/UpdateProfile"; // Обновить данные диспетчера
        public const string UPDATE_PROFILE = "User/UpdateProfile"; // Обновить данные профиля
        public const string ADD_IDENT_PROFILE = "User/AddAccountByIdent"; // Привязать ЛС к профилю
        public const string DEL_IDENT_PROFILE = "User/DeleteAccountByIdent"; // отвязать ЛС от профиля

        public const string
            GET_PERSONAL_DATA = "User/GetPersonalDataByIdent"; // Получение данных о физ лице по номеру л/сч

        public const string ADD_PERSONAL_DATA = "User/AddPersonalData"; // Добавление/обновление информации о физ лице
        public const string REGISTR_DEVICE = "User/RegisterDevice"; // регистрация устройства
        public const string REGISTR_DEVICE_NOT_AVTORIZATION = "Public/RegisterDevice"; // регистрация устройства

        public const string GET_METERS_THREE = "Meters/List"; // Получить последние 3 показания по приборам
        public const string SAVE_METER_VALUE = "Meters/SaveMeterValue"; // Сохранение показаний
        public const string SET_METER_NAME = "Meters/SetMeterCustomName"; // Смена произвольного имени прибора
        public const string DELETE_METER_VALUE = "Meters/DeleteMeterValue "; // Удаляет значение показаний прибора учета
        public const string GET_METER_VALUE = "Meters/MeterValues"; // Получить историю показаний по прибору

        public const string GET_NEWS_FULL = "News/Content"; // Получить полную инфу по новости
        public const string GET_NEWS_IMAGE = "News/Image"; // Получить полную инфу по новости

        public const string GET_SHOPS_GOODS = "Shops/Goods"; // Получить товары магазина
        public const string GET_SHOPS_GOODS_IMAGE = "Shops/GoodsImage"; // Получить картинку товара
        public const string SAVE_RESULT_POLL = "Polls/SaveResult"; // Получить картинку товара

        public const string GET_OSS = "OSS/GetOSS"; // Получить список ОСС. 
        public const string SAVE_ANSWER_OSS = "OSS/SaveAnswer"; // Сохранить ответ на вопрос.
        public const string FINISH_OSS = "OSS/CompleteVote"; // Завершить голосование 
        public const string GET_OSS_BASE = "OSS/List"; // Получить список ОСС (краткая информация). 

        public const string
            GET_OSS_BY_ID = "OSS/OssById"; // Возвращает данные по осс с указанным id. Результат вызова – OSS (см. выше)

        public const string
            SET_ACQUAINTED_OSS =
                "OSS/SetAcquainted"; // Записывает в лог, что участник голосования ознакомился с повесткой собрания 

        public const string
            SET_START_OSS = "OSS/SetStartVoiting"; // Записывает в лог, что участник начал голосование   

        public const string OSS_CHECK_PIN = "OSS/ValidatePinCode"; // Проверка пин-кода
        public const string OSS_CHECK_CODE = "OSS/SendCheckCode"; // Запрос проверочного кода

        public const string
            OSS_SAVE_PIN =
                "OSS/ValidateCheckCode"; // Проверка кода из смс и установка пин-кода аккаунта (если проверка пройдена).

        public const string PAY_ONLINE = "PayOnline/GetPayLink"; // Метод возвращает ссылку на оплату
        public const string GET_PAYMENT_LIST = "PayOnline/GetPaymentSystemsList"; // Метод возвращает список доступных систем оплаты
        public const string GET_PAYMENT_IMAGE = "Public/PaymentSystemImage"; // Возвращает изображение для системы оплаты возвращаемой методом GetPaymentSystemsList по указанному name 

        public const string
            SEND_CODE = "RequestsDispatcher/CheckPaidRequestCompleteCode"; //Проверка кода подтверждения заказа
        public const string BONUS_HISTORY = "Accounting/GetAccountBonusBalanceHistory";
        public const string DISABLE_INSURANCE_ACC = "Accounting/DisableAccountInsurance";
        public const string
            TRANSIT_ORDER =
                "RequestsDispatcher/SetPaidRequestStatusOnTheWay"; // Установка статуса платной заявки в 'курьер в пути'

        public const string UPDATE_RECEIPT = "RequestsDispatcher/UpdateRequestReceipts"; //Обновление элементов чека в магазине
        public const string CAMERAS_LIST = "Houses/WebCams"; // Список веб-камер


        public const string READ_REQUEST = "Requests/SetReadedFlag"; // Установка флага прочтения заявки
        public const string READ_NOTIFICATION = "Announcements/SetReadedFlag"; //Установка флага прочтения объявления
        public const string READ_POLL = "Polls/SetReadedFlag";//Установка флага прочтения опроса
        public const string READ_NEW = "Common/SetNewsReadedFlag";//Установка флага прочтения новости

        // Тех.Поддержка
        public const string GET_TECH_MESSAGE = "https://help.1caero.ru/MobileAPI/TechSupport/RequestDetails.ashx";//скрипт, который отдает переписку по заявке 
        public const string ADD_TECH_MESSAGE = "https://help.1caero.ru/MobileAPI/TechSupport/AddMessage.ashx";// Скрипт, который сохранит коммент от пользователя и отдаст его специалисту поддержки в ксп 
        public const string ADD_TECH_FILE = "https://help.1caero.ru/MobileAPI/TechSupport/AddFile.ashx";//  Скрипт,  который отдаст файл от пользователя в тех поддеркжку
        public const string GET_TECH_FILE = "https://help.1caero.ru/MobileAPI/TechSupport/DownloadFile.ashx";//  Скрипт,  который отдаст файл от пользователя в тех поддеркжку

        public const string GEOLOCATION = "Dispatcher/AddGeolocating";
        public const string GET_METER = "Dispatcher/GetMeter"; //Возвращаюься все найденные приборы.
        public const string SAVE_VALUE_DISPATCH_METER = "Dispatcher/SaveMeterValue"; //Сохранение показаний

        /// <summary>
        /// Аунтификация сотрудника
        /// </summary>
        /// <param name="login">Логин сотрудника</param>
        /// <param name="password">Пароль сотрудника</param>
        /// <returns>LoginResult</returns>
        public async Task<LoginResult> LoginDispatcher(string login, string password)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(LOGIN_DISPATCHER, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddBody(new
            {
                login,
                password,
            });
            var response = await restClientMp.ExecuteTaskAsync<LoginResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new LoginResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }
        public async Task<List<MeterInfo>> GetMeter (string uniqueNumber )
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_METER, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddParameter("uniqueNumber", uniqueNumber);
            
            var response = await restClientMp.ExecuteTaskAsync<List<MeterInfo>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new List<MeterInfo>();
            }

            return response.Data;
        }
        /// <summary>
        /// Аунтификация пользователя по номеру телефона
        /// </summary>
        /// <param name="phone">Номер телефона</param>
        /// <param name="password">Пароль</param>
        /// <returns>LoginResult</returns>
        public async Task<LoginResult> Login(string phone, string password)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(LOGIN, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddBody(new
            {
                phone,
                password,
            });
            var response = await restClientMp.ExecuteTaskAsync<LoginResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new LoginResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Запрос кода доступа
        /// </summary>
        /// <param name="phone">Номер телефона</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> RequestAccessCode(string phone)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(REQUEST_CODE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddBody(new
            {
                phone
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Создание обращения в тех поддержку.
        /// </summary>
        /// <param name="appeal"></param>
        /// <returns>CommonResult</returns>
        public async Task<TechId> TechSupportAppeal(TechSupportAppealArguments appeal)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(SEND_TEACH_MAIL, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddBody(new
            {
                appeal.Login,
                appeal.Phone,
                appeal.Mail,
                appeal.Text,
                appeal.Address,
                appeal.OS,
                appeal.Info,
                appeal.AppVersion
            });
            var response = await restClientMp.ExecuteTaskAsync<TechId>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new TechId()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Регистрация пользователя по номеру телефона
        /// </summary>
        /// <param name="fio">ФИО пользователя</param>
        /// <param name="phone">Номер телефона пользователя</param>
        /// <param name="password">Пароль</param>
        /// <param name="code">Код доступа необходимо запросить методом RequestAccessCode</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> RegisterByPhone(string fio, string phone, string password, string code,
            string birthday)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(REGISTR_BY_PHONE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddBody(new
            {
                fio,
                phone,
                password,
                code,
                birthday
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }


        /// <summary>
        /// Подтверждение кода доступа
        /// </summary>
        /// <param name="phone">Номер телефона</param>
        /// <param name="code">Код подтверждения</param>
        /// <returns>CommonResult</returns>
        public async Task<CheckResult> RequestChechCode(string phone, string code)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(REQUEST_CHECK_CODE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddBody(new
            {
                phone,
                code
            });
            var response = await restClientMp.ExecuteTaskAsync<CheckResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CheckResult()
                {
                    IsCorrect = false
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Получение настроек приложения
        /// </summary>
        /// <param name="appVersion">Версия приложения</param>
        /// <param name="dontCheckAppBlocking">Проверка версии</param>
        /// <returns>MobileSettings</returns>
        public async Task<MobileSettings> MobileAppSettings(string appVersion, string dontCheckAppBlocking)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_MOBILE_SETTINGS, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddParameter("appVersion", appVersion);
            restRequest.AddParameter("dontCheckAppBlocking", dontCheckAppBlocking);

            var response = await restClientMp.ExecuteTaskAsync<MobileSettings>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new MobileSettings()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<string> MobilePersonalDataPolicy()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_PERSONAL_POLICY, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

            var response = await restClientMp.ExecuteTaskAsync<string>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return $"Ошибка {response.StatusDescription}";
            }

            return response.Data;
        }

        /// <summary>
        /// Возвращает данные для болка события мообильного приложения: новости, объявления, опросы, доп. услуги.
        /// </summary>
        /// <returns>EventBlockData</returns>
        public async Task<EventBlockData> GetEventBlockData()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_EVENT_BLOCK_DATA, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);

            var response = await restClientMp.ExecuteTaskAsync<EventBlockData>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new EventBlockData()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Получение данных о начислениях
        /// </summary>
        /// <returns>AccountAccountingInfo</returns>
        public async Task<ItemsList<AccountAccountingInfo>> GetAccountingInfo()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_ACCOUNTING_INFO, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);

            var response = await restClientMp.ExecuteTaskAsync<ItemsList<AccountAccountingInfo>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<AccountAccountingInfo>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Добавление новой заявки
        /// </summary>
        /// <param name="ident">номер л.с</param>
        /// <param name="typeID">Тип заявки</param>
        /// <param name="Text">Текст заявки</param>
        /// <returns>id новой заявки</returns>
        public async Task<IDResult> newApp(string ident, string typeID, string Text, int? SubTypeID = null, string Floor = null, string Entrance = null)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(NEW_APP, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ident,
                typeID,
                Text,
                SubTypeID,
                Floor,
                Entrance
            });
            var response = await restClientMp.ExecuteTaskAsync<IDResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new IDResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }
        public async Task<IDResult> newAppPass(string ident, string typeID, string Text,
            int PassCategoryId, bool PassIsConstant, string PassDateEnd, string? PassFIO, string? PassPassportData,
            string? PassVehicleMark, string? PassVehicleNumber, string? PassVehicleColor)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(NEW_APP, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ident,
                typeID,
                Text,
                PassCategoryId,
                PassFIO,
                PassPassportData,
                PassVehicleMark,
                PassVehicleNumber,
                PassVehicleColor,
                PassIsConstant,
                PassDateEnd
            });
            var response = await restClientMp.ExecuteTaskAsync<IDResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new IDResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<IDResult> newAppConst(string ident, string typeID, string Text, string Phone = "",
            string AutoLockDisptacherId = "", int? DistrictId = null, int? HouseId = null,
            int? PremiseId = null, string HouseStreet = null, int? SubTypeID = null)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(NEW_APP_CONST, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ident,
                typeID,
                Text,
                AutoLockDisptacherId,
                DistrictId,
                HouseId,
                PremiseId,
                HouseStreet,
                SubTypeID,
                Phone
            });
            var response = await restClientMp.ExecuteTaskAsync<IDResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new IDResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Добавление платной заявки
        /// </summary>
        /// <param name="ident">Лицевой счет</param>
        /// <param name="typeID">Тип заявки</param>
        /// <param name="Text">Текст заявки</param>
        /// <param name="isPaid">Платная заявка</param>
        /// <param name="paidSum">Сумма оплаты</param>
        /// <param name="paidServiceText">Текст для оплаты</param>
        /// <returns>id новой заявки</returns>
        public async Task<IDResult> newAppPay(string ident, string typeID, string Text, bool isPaid, decimal paidSum,
            string paidServiceText, List<RequestsReceiptItem> ReceiptItems, int? ShopId)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(NEW_APP, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ident,
                typeID,
                Text,
                isPaid,
                paidSum,
                paidServiceText,
                ReceiptItems,
                ShopId
            });
            var response = await restClientMp.ExecuteTaskAsync<IDResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new IDResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<IDResult> newAppPayConst(string ident, string typeID, string Text, bool isPaid,
            decimal paidSum,
            string paidServiceText, string AutoLockDisptacherId = "")
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(NEW_APP_CONST, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ident,
                typeID,
                Text,
                AutoLockDisptacherId,
                isPaid,
                paidSum,
                paidServiceText
            });
            var response = await restClientMp.ExecuteTaskAsync<IDResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new IDResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Запрос списка заявок без сообщений и файлов
        /// </summary>
        /// <returns>RequestList</returns>
        public async Task<RequestList> GetRequestsList()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(REQUEST_LIST, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);

            var response = await restClientMp.ExecuteTaskAsync<RequestList>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Analytics.TrackEvent("GetRequestsList Error=" + RestResponceToString(response));
                return new RequestList()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<RequestListDao> GetRequestsListConst()
        {
            string OS = Device.RuntimePlatform;
            if (OS.ToLower() == "ios")
                await Task.Delay(500);

            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(REQUEST_LIST_CONST, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);

            var response = await restClientMp.ExecuteTaskAsync<RequestListDao>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Analytics.TrackEvent("GetRequestsListConst Error="+ RestResponceToString(response));

                return new RequestListDao()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }
            Realm _realm = Realm.GetInstance();
            
           await _realm.Write(async () => { 
                _realm.RemoveAll();
                if (_realm.All<RequestInfoDao>().Any())
                {
                    while (_realm.All<RequestInfoDao>().Any())
                        await Task.Delay(50);
                }                
                _realm.Add(response.Data.Requests); });

            // List<RequestInfoDao> requestInfoDaos = response.Data.Requests.ConvertAll(new Converter<RequestInfo, RequestInfoDao>(RequestInfo.InfoToDao));
            
            //_realm.Write(() => _realm.RemoveAll());            

            //if (_realm.All<RequestInfoDao>().Any())
            //{
            //    while (_realm.All<RequestInfoDao>().Any())
            //        await Task.Delay(50);
            //}

            //_realm.Write(() => _realm.Add(response.Data.Requests));
            return response.Data;
        }
        public async Task<List<RequestInfo>> Search (List<CustomSearchCriteria> Criterias)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(FILTR_REQUESTS, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                Criterias
            });
            var response = await restClientMp.ExecuteTaskAsync< List<RequestInfo>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Analytics.TrackEvent("Search Error=" + RestResponceToString(response));

                return null;
            }
            return response.Data;
        }

        string RestResponceToString(IRestResponse response)
        {
            return $"response.StatusCode={response.StatusCode} ; response.StatusDescription={response.StatusDescription}; response.ErrorException={response.ErrorException}; response.ErrorMessage={response.ErrorMessage}";
        }

        /// <summary>
        /// Получение типов заявок
        /// </summary>
        /// <returns></returns>
        public async Task<ItemsList<RequestType>> GetRequestsTypes()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_TYPE, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = await restClientMp.ExecuteTaskAsync<ItemsList<RequestType>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<RequestType>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<ItemsList<RequestType>> GetRequestsTypesConst()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_TYPE_CONST, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = await restClientMp.ExecuteTaskAsync<ItemsList<RequestType>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<RequestType>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<ItemsList<NamedValue>> GetDispatcherList()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(DISPATCHERS_LIST_CONST, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = await restClientMp.ExecuteTaskAsync<ItemsList<NamedValue>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<NamedValue>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }
        public async Task<ItemsList<NamedValue>> RequestStatuses ()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_REQUEST_STATUSES, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = await restClientMp.ExecuteTaskAsync<ItemsList<NamedValue>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<NamedValue>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }
            return response.Data;
        } 
        public async Task<ItemsList<NamedValue>> RequestPriorities ()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_REQUEST_PRIORITETS, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = await restClientMp.ExecuteTaskAsync<ItemsList<NamedValue>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<NamedValue>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }
            return response.Data;
        }
        public async Task<List<ConsultantInfo>> GetConsultants()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_CONSULTANTS, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = await restClientMp.ExecuteTaskAsync<List<ConsultantInfo>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new List<ConsultantInfo>()
                {
                    new ConsultantInfo{Name = $"Ошибка {response.StatusDescription}"}
                };
            }

            return response.Data;
        }
        public async Task<List<NamedValue>> GetDepartments()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_DEPATMENTS, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = await restClientMp.ExecuteTaskAsync<List<NamedValue>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new List<NamedValue>
                {
                  new NamedValue{Name = $"Ошибка {response.StatusDescription}"}
                };
            }

            return response.Data;
        } 
        public async Task<List<NamedValue>> GetRequestPools()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_REQUEST_POOL, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = await restClientMp.ExecuteTaskAsync<List<NamedValue>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new List<NamedValue>
                {
                  new NamedValue{Name = $"Ошибка {response.StatusDescription}"}
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Получение полной инфы по заявке
        /// </summary>
        /// <param name="id">id заявки</param>
        /// <returns>RequestContent</returns>
        public async Task<RequestContent> GetRequestsDetailList(string id)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(REQUEST_DETAIL_LIST + "/" + id, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = await restClientMp.ExecuteTaskAsync<RequestContent>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new RequestContent()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }
        public async Task<RequestContent> GetRequestsDetailListTech(string phone, string messageId = null, bool isId = false)
        {
            RestClient restClientMp = new RestClient(GET_TECH_MESSAGE);
            RestRequest restRequest = new RestRequest("", Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddParameter("phone", phone);
            restRequest.AddParameter("database", SERVER_ADDR.Split("/")[3]);
            restRequest.AddParameter("messageId", messageId);
            restRequest.AddParameter("ident", GetIdent());
            if(isId) restRequest.AddParameter("deviceId", App.DeviceId);
            restRequest.AddParameter("os", Device.RuntimePlatform);
            restRequest.AddParameter("appVersion", Xamarin.Essentials.AppInfo.VersionString);
            // ident - номер л/сч
            // os - ОС
            // appVersion - версия МП
            // info - доп инфо

            Analytics.TrackEvent("Запрос:\n" + "messageId: " + messageId +
                                 "database: " + SERVER_ADDR.Split("/")[3] +
                                 "phone: " + phone);
            var response = await restClientMp.ExecuteTaskAsync<RequestContent>(restRequest);
            Analytics.TrackEvent("Ответ: " + response.ErrorMessage + "\n" +
                                 response.ResponseUri);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new RequestContent()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }
        string GetIdent()
        {
            try
            {
                string ls = "";
                foreach (var each in Settings.Person.Accounts)
                {
                    ls += each.Ident + ", ";
                }
                return ls;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public async Task<RequestContent> GetRequestsDetailListConst(string id)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(REQUEST_DETAIL_LIST_CONST + "/" + id, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = await restClientMp.ExecuteTaskAsync<RequestContent>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new RequestContent()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Запрос об изменении заявок
        /// </summary>
        /// <param name="updateKey">ключ обновления</param>
        /// <param name="currentRequestId">id заявки (не обязательно)</param>
        /// <returns>RequestsUpdate</returns>
        public async Task<RequestsUpdate> GetRequestsUpdates(string updateKey, string currentRequestId)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(REQUEST_UPDATES, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                updateKey,
                currentRequestId
            });
            var response = await restClientMp.ExecuteTaskAsync<RequestsUpdate>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new RequestsUpdate()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<RequestsUpdate> GetRequestsUpdatesConst(string UpdateKey, string currentRequestId)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(REQUEST_UPDATES_CONST, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                UpdateKey,
                currentRequestId
            });
            var response = await restClientMp.ExecuteTaskAsync<RequestsUpdate>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new RequestsUpdate()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Отправка сообщения по заявке
        /// </summary>
        /// <param name="text">текст сообщения</param>
        /// <param name="requestId">id заявки</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> AddMessage(string text, string requestId)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(ADD_MESSAGE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                text,
                requestId
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }
        public async Task<IsSucceed> AddMessageTech(string text, string Phone, bool isId = false)
        {
            string Database = SERVER_ADDR.Split("/")[3];
            RestClient restClientMp = new RestClient(ADD_TECH_MESSAGE);
            RestRequest restRequest = new RestRequest("", Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            string deviceId = null;
            if (isId) deviceId = App.DeviceId;
            restRequest.AddBody(new
            {
                text,
                Phone,
                Database,
                DeviceId = deviceId
            });
            var response = await restClientMp.ExecuteTaskAsync<IsSucceed>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new IsSucceed()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<CommonResult> AddMessageConst(string text, string requestId, bool IsHidden)
        {
#if DEBUG
            Console.WriteLine($"вызов добавления комента {text} id={requestId} hidden={IsHidden}");
#endif

            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(ADD_MESSAGE_CONST, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                text,
                requestId,
                IsHidden
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<CommonResult> AddFileApps(string requestId, string name, byte[] source, string path)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(ADD_FILE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddParameter("requestId", requestId);
            restRequest.AddFile(path, source, name);
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }
        public async Task<CommonResult> AddFileAppsTech(string phone, string name, byte[] source, string path, bool isId = false)
        {
            string Database = SERVER_ADDR.Split("/")[3];
            RestClient restClientMp = new RestClient(ADD_TECH_FILE);
            RestRequest restRequest = new RestRequest("", Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddParameter("Database", Database);
            restRequest.AddParameter("phone", phone);
            if(isId) restRequest.AddParameter("deviceId", App.DeviceId);
            restRequest.AddFile(path, source, name);
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }


        public async Task<CommonResult> SetNewReadFlag(int ID)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(READ_NEW, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ID
            });

            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }


        public async Task<CommonResult> AddFileAppsConst(string requestId, string name, byte[] source, string path)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(ADD_FILE_CONST, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddParameter("requestId", requestId);
            restRequest.AddFile(path, source, name);
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<byte[]> GetFileAPP(string id)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_FILE_APP + "/" + id, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = restClientMp.Execute(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return response.RawBytes;
        } 
        public async Task<byte[]> GetCheckPP(string id, int inJpg=0)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_CHECH + "/" + id, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddParameter("inJpg", inJpg);
            var response = restClientMp.Execute(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return response.RawBytes;
        } 
        public async Task<byte[]> GetFileAPP_Tech(string id)
        {
            RestClient restClientMp = new RestClient(GET_TECH_FILE);
            RestRequest restRequest = new RestRequest("", Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddParameter("id", id);
            var response = restClientMp.Execute(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return response.RawBytes;
        }

        public async Task<byte[]> GetFileAPPConst(string id)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_FILE_APP_CONST + "/" + id, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = restClientMp.Execute(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return response.RawBytes;
        }

        /// <summary>
        /// Получение полной инфы по новостям
        /// </summary>
        /// <param name="id">id новости</param>
        /// <returns>NewsInfoFull</returns>
        public async Task<NewsInfoFull> GetNewsFull(string id)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_NEWS_FULL + "/" + id, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = await restClientMp.ExecuteTaskAsync<NewsInfoFull>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new NewsInfoFull()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Получение картинки доп услуги
        /// </summary>
        /// <param name="id">id доп услуги</param>
        /// <returns>Массив байтотв изображения</returns>
        public async Task<byte[]> GetPhotoAdditional(string id)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_PHOTO_ADDITIONAL + "/" + id, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = restClientMp.Execute(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return response.RawBytes;
        }
        public async Task<byte[]> GetPhotoAdditionalDop(string id)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_PHOTO_ADDITIONAL_DOP + "/" + id, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = restClientMp.Execute(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return response.RawBytes;
        }

        /// <summary>
        /// Получение картинки новости
        /// </summary>
        /// <param name="id">id новости</param>
        /// <returns>Картинка в байтах</returns>
        public async Task<MemoryStream> GetNewsImage(string id)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_NEWS_IMAGE + "/" + id, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = restClientMp.Execute(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return new MemoryStream(response.RawBytes);
        }

        public async Task<byte[]> DownloadFileAsync(string id, int inJpg = 0)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_FILE_BILLS + "/" + id, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddParameter("inJpg", inJpg);
            var response = restClientMp.Execute(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return response.RawBytes;
        }

        /// <summary>
        /// Получение 3 последних показаний по приборам
        /// </summary>
        /// <returns>AccountAccountingInfo</returns>
        public async Task<ItemsList<MeterInfo>> GetThreeMeters()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_METERS_THREE, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);

            var response = await restClientMp.ExecuteTaskAsync<ItemsList<MeterInfo>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<MeterInfo>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Получить историю показаний по прибору учета
        /// </summary>
        /// <param name="meterUniqueNum">Уникальный id прибора</param>
        /// <returns>Список показаний</returns>
        public async Task<ItemsList<MeterValueInfo>> MeterValues(string meterUniqueNum)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_METER_VALUE, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddParameter("meterUniqueNum", meterUniqueNum);
            var response = await restClientMp.ExecuteTaskAsync<ItemsList<MeterValueInfo>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<MeterValueInfo>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }
            return response.Data;
        }

        /// <summary>
        /// Обновление информации по профилю
        /// </summary>
        /// <param name="email">E-mail</param>
        /// <param name="fio">ФИО</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> UpdateProfile(string email, string fio)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(UPDATE_PROFILE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                email,
                fio
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<CommonResult> UpdateProfileConst(string email, string fio)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(UPDATE_PROFILE_CONST, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                email,
                fio
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Сохранение показаний счетчика
        /// </summary>
        /// <param name="email">E-mail</param>
        /// <param name="fio">ФИО</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> SaveMeterValue(string MeterId, string Value, string ValueT2, string ValueT3)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            string url = Settings.Person.IsDispatcher ? SAVE_VALUE_DISPATCH_METER : SAVE_METER_VALUE;
            RestRequest restRequest = new RestRequest(url, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                MeterId,
                Value,
                ValueT2,
                ValueT3
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Смена произвольного имени прибора
        /// </summary>
        /// <param name="MeterUniqueNumber">уникальный номер прибора</param>
        /// <param name="CustomName">произвольное имя прибора</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> SetMeterCustomName(string MeterUniqueNumber, string CustomName)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(SET_METER_NAME, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                MeterUniqueNumber,
                CustomName
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Удаляет значение показаний прибора учета
        /// </summary>
        /// <param name="MeterUniqueNumber"> ID прибора учета</param>
        /// <returns></returns>
        public async Task<CommonResult> DeleteMeterValue(int MeterId)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(DELETE_METER_VALUE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                MeterId
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<CommonResult> SetPaidRequestStatusOnTheWay(string RequestId)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(TRANSIT_ORDER, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                RequestId
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Получение товаров по магазину
        /// </summary>
        /// <returns>список товаров</returns>
        public async Task<ItemsList<Goods>> GetShopGoods(int? id)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_SHOPS_GOODS, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddParameter("shopId", id);
            var response = await restClientMp.ExecuteTaskAsync<ItemsList<Goods>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<Goods>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<MemoryStream> GetImageGoods(string id)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_SHOPS_GOODS_IMAGE + "/" + id, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = restClientMp.Execute(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return new MemoryStream(response.RawBytes);
        }

        public async Task<AddAccountResult> AddIdent(string Ident, bool Confirm = false, string PinCode = null)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(ADD_IDENT_PROFILE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                Ident,
                Confirm,
                PinCode
            });
            var response = await restClientMp.ExecuteTaskAsync<AddAccountResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new AddAccountResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<CommonResult> DellIdent(string Ident)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(DEL_IDENT_PROFILE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                Ident
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<CommonResult> SaveResultPolls(PollingResult pollingResult)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(SAVE_RESULT_POLL, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                pollingResult.PollId,
                pollingResult.ExtraInfo,
                pollingResult.Answers
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Получить список ОСС
        /// </summary>
        /// <param name="getArchive">- 1-получать архивные данные, 0-нет (по умолчанию 1) </param>
        /// <returns>OSS</returns>
        public async Task<ItemsList<OSS>> GetOss(int getArchive = 1)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_OSS, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                getArchive
            });
            var response = await restClientMp.ExecuteTaskAsync<ItemsList<OSS>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<OSS>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }


        public async Task<ItemsList<OSSBase>> GetOss()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_OSS_BASE, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);

            var response = await restClientMp.ExecuteTaskAsync<ItemsList<OSSBase>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<OSSBase>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<OSS> GetOssById(string id)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_OSS_BY_ID + "/" + id, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);

            var response = await restClientMp.ExecuteTaskAsync<OSS>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new OSS()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Проверка пин-кода аккаунта.
        /// </summary>
        /// <param name="PinCode">пин-код</param>
        /// <returns></returns>
        public async Task<CommonResult> OSSCheckPin(string PinCode)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            //RestRequest restRequest = new RestRequest(OSS_CHECK_PIN + "?PinCode=" + pinCode, Method.POST);
            RestRequest restRequest = new RestRequest(OSS_CHECK_PIN, Method.POST);

            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);

            restRequest.AddBody(new
            {
                PinCode
            });

            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Запрос проверочного кода
        /// </summary>
        /// <param name="phone">номер телефона пользователя</param>
        /// <param name="ident">номер л/сч</param>        
        /// <returns></returns>
        public async Task<CommonResult> OSSCheckCode(string Phone, string Ident)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(OSS_CHECK_CODE, Method.POST);

            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                Phone,
                Ident
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Проверка кода из смс и установка пин-кода аккаунта (если проверка пройдена)
        /// </summary>
        /// <param name="phone">номер телефона пользователя</param>
        /// <param name="code">код</param>
        /// <param name="pinCode">пин-код</param>
        /// <returns></returns>
        public async Task<CommonResult> OSSSavePin(string Phone, string Code, string PinCode)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(OSS_SAVE_PIN, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                Phone,
                Code,
                PinCode
            });

            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }


        /// <summary>
        /// Получение данных о физ лице по номеру л/сч
        /// </summary>
        /// <param name="Ident"> Лицевой счет</param>
        /// <returns></returns>
        public async Task<NaturalPerson> GetPersonalDataByIdent(string Ident)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_PERSONAL_DATA, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                Ident
            });
            var response = await restClientMp.ExecuteTaskAsync<NaturalPerson>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new NaturalPerson()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Добавление/обновление информации о физ лице
        /// </summary>
        /// <param name="ID">id</param>
        /// <param name="Ident"> ЛС</param>
        /// <param name="FIO">ФИО</param>
        /// <param name="DateOfBirth">Дата рождения</param>
        /// <param name="stringPlaceOfBirth">Место рождения</param>
        /// <param name="PassportSerie">Серия</param>
        /// <param name="PassportNumber">Номер</param>
        /// <param name="PassportDate">Дата выдачи</param>
        /// <param name="PassportIssuedBy">Выдал</param>
        /// <param name="RegistrationAddress">Адрес регистрации</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> AddPersonalData(string ID, string Ident, string FIO, string DateOfBirth,
            string stringPlaceOfBirth, string PassportSerie, string PassportNumber, string PassportDate,
            string PassportIssuedBy, string RegistrationAddress)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(ADD_PERSONAL_DATA, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ID,
                Ident,
                FIO,
                DateOfBirth,
                stringPlaceOfBirth,
                PassportSerie,
                PassportNumber,
                PassportDate,
                PassportIssuedBy,
                RegistrationAddress
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Регистрация устройства
        /// </summary>
        /// <param name="isCons"></param>
        /// <returns></returns>
        public async Task<CommonResult> RegisterDevice(bool isCons = false)
        {
            Analytics.TrackEvent($"Регистрация токена устройства {App.Model} пользователя {Preferences.Get("login", "")}");
            string OS = Device.RuntimePlatform;
            if (OS.ToLower() == "ios")
                await Task.Delay(500);

            string Version = Device.RuntimePlatform == Device.Android ? App.version : Xamarin.Essentials.DeviceInfo.VersionString;
            string Model = Device.RuntimePlatform == Device.Android ? App.model : Xamarin.Essentials.DeviceInfo.Model;
            string DeviceId = App.token;
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            string url = isCons ? REGISTR_DISPATCHER_DEVICE : REGISTR_DEVICE;
            RestRequest restRequest = new RestRequest(url, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                DeviceId,
                Model,
                OS,
                Version
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var rcr = new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
                return rcr;
            }

            return response.Data;
        }
        public async Task<CommonResult> RegisterDeviceNotAvtorization(string Phone)
        {
            string OS = Device.RuntimePlatform;
            if (OS.ToLower() == "ios")
                await Task.Delay(500);

            string Version = App.version;
            string Model = App.model;
            string DeviceId = App.token;
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(REGISTR_DEVICE_NOT_AVTORIZATION, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                DeviceId,
                Model,
                OS,
                Version,
                Phone
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Отправка ответа на вопрос ОСС
        /// </summary>
        /// <param name="ossAnswer">Ответ</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> SaveAnswer(OSSAnswer ossAnswer)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(SAVE_ANSWER_OSS, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(
                ossAnswer
            );
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Завершение голосования
        /// </summary>
        /// <param name="ID">id - голосования</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> CompleteVote(int ID)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(FINISH_OSS, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ID
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Записывает в лог, что участник голосования ознакомился с повесткой собрания
        /// </summary>
        /// <param name="ID">id голосования</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> SetAcquainted(int ID)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(SET_ACQUAINTED_OSS, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ID
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Записывает в лог, что участник начал голосование 
        /// </summary>
        /// <param name="ID">id голосования</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> SetStartVoiting(int ID)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(SET_START_OSS, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ID
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Отправка кода подтверждения
        /// </summary>
        /// <param name="Phone">телефон пользователя</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> SendCheckCode(string Phone, bool isWhatsApp = false, bool DontCheck = true)
        {
            string url = SEND_CHECK_CODE;
            if (isWhatsApp)
            {
                url = SEND_CHECK_CODE_WHATSAPP;
            }
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(url, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddBody(new
            {
                Phone,
                DontCheck
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Проверка кода
        /// </summary>
        /// <param name="Phone">Телефон</param>
        /// <param name="Code">Код</param>
        /// <returns></returns>
        public async Task<IsCorrected> ValidateCheckCode(string Phone, string Code)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(VALIDATE_CHECK_CODE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddBody(new
            {
                Phone,
                Code
            });
            var response = await restClientMp.ExecuteTaskAsync<IsCorrected>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new IsCorrected()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<CommonResult> CloseApp(string RequestId, string Text, string Mark)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(CLOSE_APP, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                RequestId,
                Text,
                Mark
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Отправка пин-кода в SMS по платной заявке после онлайн оплаты
        ///  Отправка пин-кода в SMS по платной заявке после онлайн оплаты
        /// </summary>
        /// <param name="RequestId"> ID платной заявки</param>
        /// <param name="Phone">номер телефона</param>
        /// <param name="isCard">Онлайн оплата или наличные</param>
        /// <returns></returns>
        public async Task<CommonResult> SendPaidRequestCompleteCodeOnlineAndCah(int? RequestId, string Phone, bool isCard = true)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            string url = isCard ? PAY_CARD_VALIDATE_CODE : PAY_CASH_VALIDATE_CODE;
            RestRequest restRequest = new RestRequest(url, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                RequestId,
                Phone
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<CommonResult> CloseAppConst(string RequestId)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(CLOSE_APP_CONST, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                RequestId
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }
        public async Task<CommonResult> CancelRequestByUser(string RequestId)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(CLOSE_APP_FOR_USER, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                RequestId
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<CommonResult> LockAppConst(string RequestId)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(LOCK_APP_CONST, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                RequestId
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<CommonResult> PerformAppConst(string RequestId)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(PERFORM_APP_CONST, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                RequestId
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<CommonResult> ChangeDispatcherConst(string RequestId, string NewDispatcherId)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(CHANGE_DISPATCHER_CONST, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                RequestId,
                NewDispatcherId
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }
        public async Task<CommonResult> ChangeConsultant(int RequestID , long? DispatcherID = null, int? PoolID = null, int? DivisionID = null  )
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(CHANGE_CONSULTANT, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                RequestID ,
                DispatcherID,
                PoolID,
                DivisionID
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<PayService> GetPayLink(string accountID, decimal Sum, bool PayInsurance = false, string PaymentSystem = null,  List<int> Services = null)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(PAY_ONLINE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                accountID,
                Sum,
                Services,
                PayInsurance,
                PaymentSystem
            });
            var response = await restClientMp.ExecuteTaskAsync<PayService>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new PayService()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }
        public async Task<List<PaymentSystem>> GetPaymentSystemsList ()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_PAYMENT_LIST, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = await restClientMp.ExecuteTaskAsync<List<PaymentSystem>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return response.Data;
        }
        public async Task<byte[]> PaymentSystemImage(string name)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_PAYMENT_IMAGE + "/" + name, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = restClientMp.Execute(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return response.RawBytes;
        } 
        public async Task<PayService> GetPayLink(int? PaidRequestId, decimal Sum, string? PaymentSystem = null)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(PAY_ONLINE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                Sum,
                PaidRequestId,
                PaymentSystem
            });
            var response = await restClientMp.ExecuteTaskAsync<PayService>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new PayService()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<PayResult> GetPayResult(string url)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(url, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            var response = await restClientMp.ExecuteTaskAsync<PayResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new PayResult()
                {
                    error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Возвращает список районов.
        /// </summary>
        /// <returns> Возвращаемый результат – ItemsList<NamedValue></returns>
        public async Task<ItemsList<NamedValue>> GetHouseGroups()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_HOUSES_GROUP, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);

            var response = await restClientMp.ExecuteTaskAsync<ItemsList<NamedValue>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<NamedValue>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Возвращает список домов. 
        /// </summary>
        /// <param name="street">название улицы для фильтра (необязательный параметр)</param>
        /// <returns>Возвращаемый результат – ItemsList<HouseProfile>. </returns>
        public async Task<ItemsList<HouseProfile>> GetHouse(string street = null)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_HOUSES, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);

            var response = await restClientMp.ExecuteTaskAsync<ItemsList<HouseProfile>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<HouseProfile>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Возвращает список помещений и л/сч по дому
        /// </summary>
        /// <param name="id"> ID дома</param>
        /// <param name="loadAllAccs">по умолчанию 0, если 1 - подгружаются все л/сч</param>
        /// <returns></returns>
        public async Task<ItemsList<R731PremiseWithAccounts>> GetHouseData(string id, string loadAllAccs = "1")
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_HOUSE_DATA, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddParameter("id", id);
            restRequest.AddParameter("loadAllAccs", loadAllAccs);
            var response = await restClientMp.ExecuteTaskAsync<ItemsList<R731PremiseWithAccounts>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<R731PremiseWithAccounts>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<ItemsList<string>> VehicleMarks()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_CAR_BRAND, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = await restClientMp.ExecuteTaskAsync<ItemsList<string>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<string>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }
        /// <summary>
        /// Закрытие списка заявки.
        /// </summary>
        /// <param name="Requests">Список id заявок для закрытия</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> CloseList(List<int> Requests)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(CLOSE_APP_LIST_DISP, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                Requests
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Выполнение списка заявок.
        /// </summary>
        /// <param name="Requests">Список id заявок для выполнения</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> PerformList(List<int> Requests)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(PERFORM_APP_LIST_DISP, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                Requests
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }
        /// <summary>
        /// Возвращает статистику по заявкам. 
        /// </summary>
        /// <param name="districtId">ид района</param>
        /// <param name="houseId">ид дома</param>
        /// <returns>объект со статистикой</returns>
        public async Task<ItemsList<RequestStats>> RequestStats(int? districtId = null, int? houseId = -1, string customPeriodStart = null,
            string customPeriodEnd = null)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_REQUESTS_STATS, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddParameter("districtId", districtId);
            restRequest.AddParameter("houseId", houseId);
            restRequest.AddParameter("customPeriodStart", customPeriodStart);
            restRequest.AddParameter("customPeriodEnd", customPeriodEnd);
            var response = await restClientMp.ExecuteTaskAsync<ItemsList<RequestStats>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ItemsList<RequestStats>()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Получить сумму комиссии
        /// </summary>
        /// <param name="sum">сумма оплаты</param>
        /// <returns></returns>
        public async Task<ComissionModel> GetSumWithComission(string sum, string accountID)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_SUM_COMISSION, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddParameter("sum", sum.Replace(",", "."));
            restRequest.AddParameter("accountID", accountID);
            var response = await restClientMp.ExecuteTaskAsync<ComissionModel>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new ComissionModel()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<Bonus> GetAccountBonusBalance(int id)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_POINS_BALANCE, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddParameter("id", id);
            var response = await restClientMp.ExecuteTaskAsync<Bonus>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new Bonus()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<bool> SendCodeRequestForpaidService(PaidRequestCodeModel data)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(SEND_CODE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                data.RequestId,
                data.Code
            });
            var response = await restClientMp.ExecuteTaskAsync<PaidRequestResponse>(restRequest);
            return response.Data.IsCorrect;
        }

        public async Task<CommonResult> UpdateReceipt(RequestsReceiptItemsList data)
        {
            RestClient client = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(UPDATE_RECEIPT, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                data.RequestId,
                data.ReceiptItems
            });
            var response = await client.ExecuteTaskAsync<CommonResult>(restRequest);
            return response.Data;
        }


        public async Task<ItemsList<CameraModel>> GetCameras(int all = 0)
        {
            RestClient client = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(CAMERAS_LIST, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = await client.ExecuteTaskAsync<ItemsList<CameraModel>>(restRequest);
            return response.Data;
        }

        public async Task<ItemsList<BonusCashFlow>> GetBonusHistory(string id = "0001")
        {
            RestClient client = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(BONUS_HISTORY, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddParameter("id", id);
            var response = await client.ExecuteTaskAsync<ItemsList<BonusCashFlow>>(restRequest);
            return response.Data;
        } 
        /// <summary>
        /// Отключает страхование для указанного лицевого счета
        /// </summary>
        /// <param name="accountID">ID лицевого счета</param>
        /// <returns></returns>
        public async Task<CommonResult> DisableAccountInsurance (string accountID)
        {
            RestClient client = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(DISABLE_INSURANCE_ACC, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                accountID
            });
            var response = await client.ExecuteTaskAsync<CommonResult>(restRequest);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }
            return response.Data;
        }

        /// <summary>
        /// Метод, который устанавливает что заявка прочитана сотрудником
        /// </summary>
        /// <param name="RequestId"> ID заявки</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> SetReadedFlag(int RequestId, bool isDispatcher = true)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(isDispatcher ? SET_READED_APP : READ_REQUEST, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                RequestId
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }


        public async Task<CommonResult> SetNotificationReadFlag(int ID)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(READ_NOTIFICATION, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ID
            });

            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<CommonResult> SetPollReadFlag(int ID)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(READ_POLL, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ID
            });

            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<List<NewsInfo>> AllNews()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_ALL_NEWS, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);

            var response = await restClientMp.ExecuteTaskAsync<List<NewsInfo>>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new List<NewsInfo>()
                {
                    // Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<IDResult> NewAnnouncement(AnnouncementArguments announcementArguments)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(CREATE_PUSH, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                announcementArguments.ID,
                announcementArguments.ShowOnMainPage,
                announcementArguments.Header,
                announcementArguments.Text,
                announcementArguments.id_homegroup,
                announcementArguments.ActiveFrom,
                announcementArguments.ActiveTo,
                announcementArguments.ForAccountsWithDebtOver,
                announcementArguments.Ident,
                announcementArguments.id_AdditionalService,
                announcementArguments.id_QuestionGroup,
                announcementArguments.OS,
                announcementArguments.Houses
            });
            var response = await restClientMp.ExecuteTaskAsync<IDResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new IDResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<CommonResult> SendAnnouncement(int ID)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(SEND_PUSH, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ID
            });
            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }


        public async Task<CommonResult> SendGeolocation(double Lat, double Lng)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GEOLOCATION, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                Lat,
                Lng
            });

            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }
        
        public async Task<CommonResult> ChangeMessageVisibility(string RequestID, string MessageID, bool IsHidden)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(CHANGE_MESS_VISIBILITY, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                RequestID,
                MessageID,
                IsHidden
            });

            var response = await restClientMp.ExecuteTaskAsync<CommonResult>(restRequest);

            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new CommonResult()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        public async Task<ItemsList<NamedValue>> GetAreaGroups()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_AREA_GROUPS, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);

            var response = await restClientMp.ExecuteAsync<ItemsList<NamedValue>>(restRequest);

            return response.Data;
        }

        public async Task<ItemsList<RequestStats>> GetMultipleStats(List<xamarinJKH.Server.RequestModel.Monitor.RequestStatsQuerySettings> Queries)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(REQUEST_MULTIPLE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("client", Device.RuntimePlatform);
            restRequest.AddHeader("CurrentLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                Queries
            });

            //if(Device.RuntimePlatform == Device.iOS)
            //{
            //    var response = restClientMp.ExecuteAsync<ItemsList<RequestStats>>(restRequest).Result.Data;

            //    return response;
            //}
            //else
            {
                var response = await restClientMp.ExecuteAsync<ItemsList<RequestStats>>(restRequest);

                return response.Data;
            }
            
        }
    }
}