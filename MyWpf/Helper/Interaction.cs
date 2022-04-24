using System;
using System.Windows;
using Notification.TriggerSrc;
using TriggerCollection = Notification.TriggerSrc.TriggerCollection;
using BehaviorCollection = Notification.BehaviorSrc.BehaviorCollection;

namespace Notification.Helper
{
    public static class Interaction
    {
        private static readonly DependencyProperty TriggersProperty =
            DependencyProperty.RegisterAttached("ShadowTriggers", typeof(TriggerCollection), typeof(Interaction),
                new FrameworkPropertyMetadata(OnTriggersChanged));

        private static readonly DependencyProperty BehaviorsProperty =
            DependencyProperty.RegisterAttached("ShadowBehaviors", typeof(BehaviorCollection), typeof(Interaction),
                new FrameworkPropertyMetadata(OnBehaviorsChanged));

        internal static bool ShouldRunInDesignMode { get; set; }

        public static TriggerCollection GetTriggers(DependencyObject obj)
        {
            var triggerCollection = (TriggerCollection)obj.GetValue(TriggersProperty);
            if (triggerCollection == null)
            {
                triggerCollection = new TriggerCollection();
                obj.SetValue(TriggersProperty, triggerCollection);
            }
            return triggerCollection;
        }

        // StackPanel 전달됨
        public static BehaviorCollection GetBehaviors(DependencyObject obj)
        {
            // StackPanel에 ShadowBehaviors Property 반환
            var behaviorCollection = (BehaviorCollection)obj.GetValue(BehaviorsProperty);
            if (behaviorCollection == null)
            {
                // StackPanel에 ShadowBehaviors 설정 안되어 있으면 ShadowBehaviors 설정
                behaviorCollection = new BehaviorCollection();
                obj.SetValue(BehaviorsProperty, behaviorCollection);
            }
            return behaviorCollection;
        }

        private static void OnBehaviorsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var oldValue = (BehaviorCollection)args.OldValue;
            var newValue = (BehaviorCollection)args.NewValue;
            if (Equals(oldValue, newValue))
                return;
            if (oldValue?.AssociatedObject != null)
                oldValue.Detach();
            if (newValue == null || obj == null)
                return;
            if (newValue.AssociatedObject != null)
                throw new InvalidOperationException(ExceptionStringTable
                    .CannotHostBehaviorCollectionMultipleTimesExceptionMessage);
            newValue.Attach(obj);
        }

        private static void OnTriggersChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var oldValue = args.OldValue as TriggerCollection;
            var newValue = args.NewValue as TriggerCollection;
            if (Equals(oldValue, newValue))
                return;
            if (oldValue?.AssociatedObject != null)
                oldValue.Detach();
            if (newValue == null || obj == null)
                return;
            if (newValue.AssociatedObject != null)
                throw new InvalidOperationException(ExceptionStringTable
                    .CannotHostTriggerCollectionMultipleTimesExceptionMessage);
            newValue.Attach(obj);
        }

        internal static bool IsElementLoaded(FrameworkElement element)
        {
            return element.IsLoaded;
        }
    }

}
