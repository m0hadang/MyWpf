using System.Windows;

namespace Notification.Helper
{
    public interface IAttachedObject
    {
        void Attach(DependencyObject dependencyObject);
        void Detach();

        DependencyObject AssociatedObject { get; }
    }
}
