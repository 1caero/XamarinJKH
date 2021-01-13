using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Xamarin.Forms;
using xamarinJKH.DialogViews;
using xamarinJKH.Main;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.Utils
{
    public static class Settings
    {
        public static MobileSettings MobileSettings = new MobileSettings();
        public static LoginResult Person = new LoginResult();
        public static List<RequestType> TypeApp = new List<RequestType>();
        public static List<string> BrandCar= new List<string>();
        public static HashSet<Page> AppPAge = new HashSet<Page>();
        public static string UpdateKey = "";
        public static string DateUniq = "";
        public static string AppVersion = "3.2";
        public static bool TimerStart = false;
        public static bool AppIsVisible = true;
        public static bool NotifVisible = true;
        public static bool QuestVisible = true;
        public static bool AddVisible = true;
        public static bool GoodsIsVisible = false;
        public static int TimerTime = 59;

        public static bool IsFirsStart = true;
        public static bool ConstAuth = false;
        public static EventBlockData EventBlockData = new EventBlockData();

        public static string[] months =
        {
            "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь",
            "Декабрь"
        };

        public static Page mainPage;
        public static Page Page;
        public static bool? isSelf = null;

        public static bool OnTimerTick()
        {
            if (TimerStart)
            {
                TimerTime -= 1;
                if (TimerTime < 0)
                {
                    TimerStart = false;
                    TimerTime = 59;
                }
            }

            return TimerStart;
        }

        public static AnnouncementInfo getNotif(String id)
        {
            foreach (var each in EventBlockData.Announcements)
            {
                if (each.ID.Equals(id))
                {
                    return each;
                }
            }

            return null;
        }

        public static AdditionalService GetAdditionalService(int id)
        {
            foreach (var each in EventBlockData.AdditionalServices)
            {
                if (each.ID == id)
                {
                    return each;
                }
            }

            return null;
        }

        public static PollInfo GetPollInfo(int id)
        {
            foreach (var each in EventBlockData.Polls)
            {
                if (each.ID == id)
                {
                    return each;
                }
            }

            return null;
        }

        public static async Task StartProgressBar(string title = "Загрузка", double opacity = 0.6)
        {
            // Loading settings
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = Color.FromHex(MobileSettings.color),
                OverlayColor = Color.Black,
                Opacity = opacity,
                DefaultMessage = title,
            };

            await Loading.Instance.StartAsync(async progress =>
            {
                // some heavy process.
                // for (var i = 0; i < 100; i++)
                // {
                //     await Task.Delay(70);
                //     // can send progress to the dialog with the IProgress.
                //     // progress.Report((i + 1) * 0.01d);
                // }
            });
        }

        public static async Task StartOverlayBackground(Color hex)
        {
            // Loading settings
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = Color.Transparent,
                OverlayColor = Color.Black,
                Opacity = 0.8,
                DefaultMessage = "",
            };

            await Loading.Instance.StartAsync(async progress =>
            {
                // some heavy process.
                var ret = await Dialog.Instance.ShowAsync<RatingBarView>(new
                {
                    HexColor = hex
                });
            });
        }

        public static string GetStatusIcon(int id)
        {
            switch (id)
            {
                case 6:
                case 7: return "ic_status_done";
                case 1: return "ic_status_new";
                default: return "ic_status_wait";
            }
        }
    }
}