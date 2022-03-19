﻿using System.Windows;

namespace Notification.Src
{
    public interface IAttachedObject
    {
        void Attach(DependencyObject dependencyObject);
        void Detach();

        DependencyObject AssociatedObject { get; }
    }
}
