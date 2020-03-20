using System.Windows;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using WpfCoreCopier;

namespace WpfCoreCopier
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<ViewModel>();
            Messenger.Default.Register<NotificationMessage>(this, NotifyUserMethod);
        }


        public ViewModel ViewModel
        {
            get { return ServiceLocator.Current.GetInstance<ViewModel>(); }
        }
        private void NotifyUserMethod(NotificationMessage message)
        {
            MessageBox.Show(message.Notification);
        }
    }
}