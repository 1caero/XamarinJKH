﻿using Foundation;
using System;
using System.Diagnostics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using xamarinJKH.iOS.CustomRenderers;
using xamarinJKH.iOS.Extensions;
using xamarinJKH.Utils;

[assembly: ExportRenderer(typeof(xamarinJKH.Tech.AppPage), typeof(KeyboardOverlapRendererTechAppPage))]
namespace xamarinJKH.iOS.CustomRenderers
{
    [Preserve(AllMembers = true)]
    public class KeyboardOverlapRendererTechAppPage : PageRenderer
    {
        NSObject _keyboardShowObserver;
        NSObject _keyboardHideObserver;
        private bool _pageWasShiftedUp;
        private double _activeViewBottom;
        private bool _isKeyboardShown;

        public static void Init()
        {
            var now = DateTime.Now;
            Debug.WriteLine("Keyboard Overlap plugin initialized {0}", now);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            var page = Element as ContentPage;

            if (page != null)
            {
                var contentScrollView = page.Content as ScrollView;

                if (contentScrollView != null)
                    return;

                RegisterForKeyboardNotifications();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            UnregisterForKeyboardNotifications();
        }

        void RegisterForKeyboardNotifications()
        {
            if (_keyboardShowObserver == null)
                _keyboardShowObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardShow);
            if (_keyboardHideObserver == null)
                _keyboardHideObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardHide);
        }

        void UnregisterForKeyboardNotifications()
        {
            _isKeyboardShown = false;
            if (_keyboardShowObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardShowObserver);
                _keyboardShowObserver.Dispose();
                _keyboardShowObserver = null;
            }

            if (_keyboardHideObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardHideObserver);
                _keyboardHideObserver.Dispose();
                _keyboardHideObserver = null;
            }
        }

        protected virtual void OnKeyboardShow(NSNotification notification)
        {
            if (!IsViewLoaded || _isKeyboardShown)
                return;

            _isKeyboardShown = true;
            var activeView = View.FindFirstResponder();

            if (activeView == null)
                return;

            var keyboardFrame = UIKeyboard.FrameEndFromNotification(notification);
            var isOverlapping = activeView.IsKeyboardOverlapping(View, keyboardFrame);

            if (!isOverlapping)
                return;

            if (isOverlapping)
            {
                _activeViewBottom = activeView.GetViewRelativeBottom(View);
                ShiftPageUp(keyboardFrame.Height, _activeViewBottom);
            }
        }

        private void OnKeyboardHide(NSNotification notification)
        {
            if (!IsViewLoaded)
                return;

            _isKeyboardShown = false;
            var keyboardFrame = UIKeyboard.FrameEndFromNotification(notification);

            if (_pageWasShiftedUp)
            {
                ShiftPageDown(keyboardFrame.Height, _activeViewBottom);
            }
        }

        private void ShiftPageUp(nfloat keyboardHeight, double activeViewBottom)
        {
            var pageFrame = Element.Bounds;

            var newY = pageFrame.Y + CalculateShiftByAmount(pageFrame.Height, keyboardHeight, activeViewBottom);

            Element.LayoutTo(new Rectangle(pageFrame.X, newY,
                pageFrame.Width, pageFrame.Height));

            _pageWasShiftedUp = true;
        }

        private void ShiftPageDown(nfloat keyboardHeight, double activeViewBottom)
        {
            var pageFrame = Element.Bounds;

            var newY = pageFrame.Y - CalculateShiftByAmount(pageFrame.Height, keyboardHeight, activeViewBottom);
            //var newY = 0;
            Element.LayoutTo(new Rectangle(pageFrame.X, newY,
                pageFrame.Width, pageFrame.Height));

            _pageWasShiftedUp = false;
        }

        private double CalculateShiftByAmount(double pageHeight, nfloat keyboardHeight, double activeViewBottom)
        {            
            double r;
            
            if (App.isStart)
                r = (pageHeight - activeViewBottom) - keyboardHeight + (int)UIApplication.SharedApplication.StatusBarFrame.Height;
            else
                r = (pageHeight - activeViewBottom) - keyboardHeight - 20;
            return r;
        }
    }
}