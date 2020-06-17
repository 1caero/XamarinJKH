﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Plugin.FilePicker.Abstractions;
using Plugin.Media.Abstractions;
using xamarinJKH.Server.RequestModel;
using RestSharp;
using xamarinJKH.Utils;

namespace xamarinJKH.Server
{
    public class RestClientMP
    {
        public const string SERVER_ADDR = "https://api.sm-center.ru/test_erc_udm"; // ОСС
        // public const string SERVER_ADDR = "https://api.sm-center.ru/komfortnew"; // Гранель
        // public const string SERVER_ADDR = "https://api.sm-center.ru/water"; // Тихая гавань

        public const string LOGIN_DISPATCHER = "auth/loginDispatcher"; // Аутификация сотрудника
        public const string LOGIN = "auth/Login"; // Аунтификация пользователя
        public const string REQUEST_CODE = "auth/RequestAccessCode"; // Запрос кода подтверждения
        public const string REQUEST_CHECK_CODE = "auth/CheckAccessCode"; // Подтверждение кода подтверждения
        public const string REGISTR_BY_PHONE = "auth/RegisterByPhone"; // Регистрация по телефону
        public const string SEND_CHECK_CODE = "auth/SendCheckCode"; // Запрос проверочного кода
        public const string VALIDATE_CHECK_CODE = "auth/ValidateCheckCode "; // Проверка кода из смс

        public const string GET_MOBILE_SETTINGS = "Config/MobileAppSettings "; // Регистрация по телефону
        public const string GET_EVENT_BLOCK_DATA = "Common/EventBlockData"; // Блок события
        public const string GET_PHOTO_ADDITIONAL = "AdditionalServices/logo"; // Картинка доп услуги
        public const string GET_ACCOUNTING_INFO = "Accounting/Info"; // инфомация о начислениях
        public const string GET_FILE_BILLS = "Bills/Download"; // Получить квитанцию

        public const string REQUEST_LIST = "Requests/List"; // Заявки
        public const string REQUEST_DETAIL_LIST = "Requests/Details"; // Заявки
        public const string REQUEST_UPDATES = "Requests/GetUpdates"; // Обновление заявок
        public const string ADD_MESSAGE = "Requests/AddMessage"; // Отправка сообщения
        public const string ADD_FILE = "Requests/AddFile "; // Отправка файла
        public const string NEW_APP = "Requests/New"; // Добавление заявки
        public const string GET_TYPE = "Requests/RequestTypes"; // Получение типов заявок
        public const string GET_FILE_APP = "Requests/File"; // Получение типов заявок
        public const string CLOSE_APP = "Requests/Close "; // Закрытие заявки

        public const string UPDATE_PROFILE = "User/UpdateProfile"; // Обновить данные профиля
        public const string ADD_IDENT_PROFILE = "User/AddAccountByIdent"; // Привязать ЛС к профилю

        public const string
            GET_PERSONAL_DATA = "User/GetPersonalDataByIdent"; // Получение данных о физ лице по номеру л/сч

        public const string ADD_PERSONAL_DATA = "User/AddPersonalData"; // Добавление/обновление информации о физ лице

        public const string GET_METERS_THREE = "Meters/List"; // Получить последние 3 показания по приборам
        public const string SAVE_METER_VALUE = "Meters/SaveMeterValue"; // Получить полную инфу по новости

        public const string GET_NEWS_FULL = "News/Content"; // Получить полную инфу по новости
        public const string GET_NEWS_IMAGE = "News/Image"; // Получить полную инфу по новости

        public const string GET_SHOPS_GOODS = "Shops/Goods"; // Получить товары магазина
        public const string GET_SHOPS_GOODS_IMAGE = "Shops/GoodsImage"; // Получить картинку товара
        public const string SAVE_RESULT_POLL = "Polls/SaveResult"; // Получить картинку товара

        public const string GET_OSS = "OSS/GetOSS"; // Получить список ОСС. 
        public const string SAVE_ANSWER_OSS = "OSS/SaveAnswer"; // Сохранить ответ на вопрос.
        public const string FINISH_OSS = "OSS/CompleteVote"; // Завершить голосование 


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
            Console.WriteLine("Запрос кода подтверждения");
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(REQUEST_CODE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
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

            Console.WriteLine(response.Data.Error);
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
            Console.WriteLine("Запрос кода подтверждения");
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(REQUEST_CHECK_CODE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
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
            Console.WriteLine("Запрос кода подтверждения");
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_MOBILE_SETTINGS, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
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

        /// <summary>
        /// Возвращает данные для болка события мообильного приложения: новости, объявления, опросы, доп. услуги.
        /// </summary>
        /// <returns>EventBlockData</returns>
        public async Task<EventBlockData> GetEventBlockData()
        {
            Console.WriteLine("Запрос кода подтверждения");
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_EVENT_BLOCK_DATA, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
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
        public async Task<IDResult> newApp(string ident, string typeID, string Text)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(NEW_APP, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ident,
                typeID,
                Text,
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
            string paidServiceText)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(NEW_APP, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ident,
                typeID,
                Text,
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
            restRequest.AddHeader("acx", Settings.Person.acx);

            var response = await restClientMp.ExecuteTaskAsync<RequestList>(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new RequestList()
                {
                    Error = $"Ошибка {response.StatusDescription}"
                };
            }

            return response.Data;
        }

        /// <summary>
        /// Получение типов заявок
        /// </summary>
        /// <returns></returns>
        public async Task<ItemsList<NamedValue>> GetRequestsTypes()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_TYPE, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
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
        /// Получение полной инфы по заявке
        /// </summary>
        /// <param name="id">id заявки</param>
        /// <returns>RequestContent</returns>
        public async Task<RequestContent> GetRequestsDetailList(string id)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(REQUEST_DETAIL_LIST + "/" + id, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
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

        public async Task<CommonResult> AddFileApps(string requestId, string name, byte[] source, string path)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(ADD_FILE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
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
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = restClientMp.Execute(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return new MemoryStream(response.RawBytes);
        }

        public async Task<byte[]> DownloadFileAsync(string id)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_FILE_BILLS + "/" + id, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
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
        /// Получение 3 последних показаний по приборам
        /// </summary>
        /// <returns>AccountAccountingInfo</returns>
        public async Task<ItemsList<MeterInfo>> GetThreeMeters()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_METERS_THREE, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
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
            RestRequest restRequest = new RestRequest(SAVE_METER_VALUE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
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
        /// Получение товаров по магазину
        /// </summary>
        /// <returns>список товаров</returns>
        public async Task<ItemsList<Goods>> GetShopGoods()
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(GET_SHOPS_GOODS, Method.GET);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("acx", Settings.Person.acx);
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
            restRequest.AddHeader("acx", Settings.Person.acx);
            var response = restClientMp.Execute(restRequest);
            // Проверяем статус
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return new MemoryStream(response.RawBytes);
        }

        public async Task<AddAccountResult> AddIdent(string Ident, bool Confirm = false)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(ADD_IDENT_PROFILE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                Ident,
                Confirm
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

        public async Task<CommonResult> SaveResultPolls(PollingResult pollingResult)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(SAVE_RESULT_POLL, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
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
        /// Отправка ответа на вопрос ОСС
        /// </summary>
        /// <param name="ossAnswer">Ответ</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> SaveAnswer(OSSAnswer ossAnswer)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(SAVE_ANSWER_OSS, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddHeader("acx", Settings.Person.acx);
            restRequest.AddBody(new
            {
                ossAnswer
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
        /// Завершение голосования
        /// </summary>
        /// <param name="ID">id - голосования</param>
        /// <returns>CommonResult</returns>
        public async Task<CommonResult> CompleteVote(int ID)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(FINISH_OSS, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
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
        public async Task<CommonResult> SendCheckCode(string Phone)
        {
            RestClient restClientMp = new RestClient(SERVER_ADDR);
            RestRequest restRequest = new RestRequest(SEND_CHECK_CODE, Method.POST);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.AddBody(new
            {
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
    }
}