using System;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;

namespace SageMobileSales.Behaviors
{
    public class NavigateWithEventArgsToPageAction : DependencyObject, IAction
    {
        public string TargetPage { get; set; }
        public string EventArgsParameterPath { get; set; }

        object IAction.Execute(object sender, object parameter)
        {
            //Walk the ParameterPath for nested properties.
            string[] propertyPathParts = EventArgsParameterPath.Split('.');
            object propertyValue = parameter;
            foreach (string propertyPathPart in propertyPathParts)
            {
                PropertyInfo propInfo = propertyValue.GetType().GetTypeInfo().GetDeclaredProperty(propertyPathPart);
                propertyValue = propInfo.GetValue(propertyValue);
            }

            Type pageType = Type.GetType(TargetPage);

            Frame frame = GetFrame(sender as DependencyObject);
            return frame.Navigate(pageType, propertyValue);
        }

        private Frame GetFrame(DependencyObject dependencyObject)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(dependencyObject);
            var parentFrame = parent as Frame;
            if (parentFrame != null) return parentFrame;
            return GetFrame(parent);
        }
    }
}