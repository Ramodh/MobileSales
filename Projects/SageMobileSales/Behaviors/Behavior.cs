﻿using System;
using System.Reflection;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace SageMobileSales.Behaviors
{
    public abstract class Behavior<T> : DependencyObject, IBehavior where T : DependencyObject
    {
        public T AssociatedObject { get; private set; }

        DependencyObject IBehavior.AssociatedObject
        {
            get { return AssociatedObject; }
        }

        public void Attach(DependencyObject associatedObject)
        {
            if (associatedObject != null &&
                !typeof (T).GetTypeInfo().IsAssignableFrom(associatedObject.GetType().GetTypeInfo()))
                throw new Exception(
                    string.Format(ResourceLoader.GetForCurrentView("Resources").GetString("associatedObjectException"),
                        typeof (T)));

            AssociatedObject = associatedObject as T;
            OnAttached();
        }

        public void Detach()
        {
            OnDetached();
            AssociatedObject = null;
        }

        protected abstract void OnAttached();
        protected abstract void OnDetached();
    }
}