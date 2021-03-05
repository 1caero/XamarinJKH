using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;

namespace xamarinJKH.ViewModels.Main
{
    public class AppsPageViewModel : BaseViewModel
    {
        ObservableCollection<RequestInfo> _requests;
        
        public RequestInfo SelectedRequest { get; set; }
        
        public ObservableCollection<RequestInfo> Requests
        {
            get => _requests;
            set
            {
                if (value != null) _requests = value;
                OnPropertyChanged("Requests");
            }
        }
        private bool _isRefreshing = false;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }
        public List<RequestInfo> AllRequests { get; set; }
        public Command LoadRequests { get; set; }
        public Command UpdateRequests { get; set; }
        public Command OpenApp { get; set; }
        bool _showClosed;
        public bool ShowClosed
        {
            get => _showClosed;
            set
            {
                _showClosed = value;
                if (_showClosed)
                {
                    if (AllRequests != null)
                    {
                        Requests = new ObservableCollection<RequestInfo>();
                        foreach (var App in AllRequests.Where(x => x.IsClosed).ToList())
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                if (!Requests.Any(o => o.ID == App.ID))
                                    Requests.Add(App);
                            });
                        }
                        Empty = AllRequests.Count(x => x.IsClosed)==0;
                    }
                }
                else
                {
                    if (AllRequests != null)
                    {
                        Requests = new ObservableCollection<RequestInfo>();
                        foreach (var App in AllRequests.Where(x => !x.IsClosed).ToList())
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                if (!Requests.Any(o => o.ID == App.ID))
                                    Requests.Add(App);
                            });
                        }
                        Empty = AllRequests.Count(x => !x.IsClosed)==0;
                    }
                }
                OnPropertyChanged("ShowClosed");
            }
        }

        bool empty;
        public bool Empty
        {
            get => empty;
            set
            {
                empty = value;
                OnPropertyChanged("Empty");
            }
        }
        public AppsPageViewModel()
        {
            Requests = new ObservableCollection<RequestInfo>();
            AllRequests = new List<RequestInfo>();
            MessagingCenter.Subscribe<Object, int>(this, "SetAppRead", (sender, args) =>
            {
                IEnumerable<RequestInfo> requestInfos = _requests.Where(x => x.ID == args);
                foreach (var each in requestInfos)
                {
                    each.IsReadedByClient = false;
                }
            });
            OpenApp = new Command(async () =>
            {
                if (SelectedRequest != null)
                {
                    MessagingCenter.Send<Object, int>(this, "OpenApp", SelectedRequest.ID);
                }
            });
            LoadRequests = new Command(async (isRef) =>
            {
                if (isRef != null)
                {
                    IsRefreshing = false;
                }
                else
                {
                    IsRefreshing = true;
                }
                var response = await Server.GetRequestsList();
                //AllRequests = new List<RequestInfo>();
                if (response.Error != null)
                {
                    ShowError(response.Error);
                    IsRefreshing = false;
                    return;
                }
                else
                {
                    if (Settings.UpdateKey != response.UpdateKey)
                        Settings.UpdateKey = response.UpdateKey;
                    Device.BeginInvokeOnMainThread(() => {
                        if (response.Requests != null)
                        {
                            MessagingCenter.Send<Object, int>(this, "SetRequestsAmount", response.Requests.Where(x => !x.IsReadedByClient && x.StatusID != 6).Count());
                            AllRequests.AddRange(response.Requests);
                            if (Requests == null)
                            {
                                Empty = Requests.Count == 0;
                                Requests = new ObservableCollection<RequestInfo>();
                            }
                            //Device.BeginInvokeOnMainThread(() =>
                            //{
                            Requests.Clear();
                            foreach (var App in AllRequests.Where(x => x.IsClosed == ShowClosed))
                            {
                                    if (!Requests.Where(o => o.ID == App.ID).Any())
                                        Requests.Add(App);
                            }
                            //});
                        }

                        MessagingCenter.Subscribe<Object, string>(this, "AddIdent", (sender, args) =>
                        LoadRequests.Execute(null));
                        MessagingCenter.Send<Object>(this, "EndRefresh");
                        IsRefreshing = false;
                    });
                }
            });
        }
        
        public async Task UpdateTask()
        {
            var response = await Server.GetRequestsList();
            if (response.Error == null)
            {
                MessagingCenter.Send<Object, int>(this, "SetRequestsAmount", response.Requests.Where(x => !x.IsReadedByClient && x.StatusID != 6).Count());
                if (AllRequests != null)
                {
                    if (ShowClosed)
                    {
                        Empty = response.Requests.Count(x => x.IsClosed) == 0;
                    }
                    else
                    {
                        Empty = response.Requests.Count(x => !x.IsClosed) == 0;
                    }
                    
                    var ids = AllRequests.Select(x => x.ID);
                    var newRequests = response.Requests.Where(x => !ids.Contains(x.ID)).ToList();
                    foreach (var newApp in newRequests)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            AllRequests.Insert(0, newApp);
                            if (!Requests.Contains(newApp))
                                Requests.Insert(0, newApp);
                        });
                    }
                }

            }
        }
    }
}
